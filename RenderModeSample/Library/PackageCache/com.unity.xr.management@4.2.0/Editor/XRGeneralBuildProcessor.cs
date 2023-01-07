using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;

[assembly: InternalsVisibleTo("Unity.XR.Management.EditorTests")]
namespace UnityEditor.XR.Management
{
    /// <summary>
    /// Small utility class for reading, updating and writing boot config.
    /// </summary>
    internal class BootConfig
    {
        static readonly string kXrBootSettingsKey = "xr-boot-settings";
        Dictionary<string, string> bootConfigSettings;

        BuildReport buildReport;
        string bootConfigPath;

        internal BootConfig(BuildReport report)
        {
            buildReport = report;
        }

        internal void ReadBootConfig()
        {
            bootConfigSettings = new Dictionary<string, string>();

            string buildTargetName = BuildPipeline.GetBuildTargetName(buildReport.summary.platform);
            string xrBootSettings = UnityEditor.EditorUserBuildSettings.GetPlatformSettings(buildTargetName, kXrBootSettingsKey);
            if (!String.IsNullOrEmpty(xrBootSettings))
            {
                // boot settings string format
                // <boot setting>:<value>[;<boot setting>:<value>]*
                var bootSettings = xrBootSettings.Split(';');
                foreach (var bootSetting in bootSettings)
                {
                    var setting = bootSetting.Split(':');
                    if (setting.Length == 2 && !String.IsNullOrEmpty(setting[0]) && !String.IsNullOrEmpty(setting[1]))
                    {
                        bootConfigSettings.Add(setting[0], setting[1]);
                    }
                }
            }

        }

        internal void SetValueForKey(string key, string value, bool replace = false)
        {
            if (bootConfigSettings.ContainsKey(key))
            {
                bootConfigSettings[key] = value;
            }
            else
            {
                bootConfigSettings.Add(key, value);
            }
        }

        internal void WriteBootConfig()
        {
            // boot settings string format
            // <boot setting>:<value>[;<boot setting>:<value>]*
            bool firstEntry = true;
            var sb = new System.Text.StringBuilder();
            foreach (var kvp in bootConfigSettings)
            {
                if (!firstEntry)
                {
                    sb.Append(";");
                }
                sb.Append($"{kvp.Key}:{kvp.Value}");
                firstEntry = false;
            }

            string buildTargetName = BuildPipeline.GetBuildTargetName(buildReport.summary.platform);
            EditorUserBuildSettings.SetPlatformSettings(buildTargetName, kXrBootSettingsKey, sb.ToString());
        }
    }

    class XRGeneralBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        static readonly string kPreInitLibraryKey = "xrsdk-pre-init-library";

        class PreInitInfo
        {
            public PreInitInfo(IXRLoaderPreInit loader, BuildTarget buildTarget, BuildTargetGroup buildTargetGroup)
            {
                this.loader = loader;
                this.buildTarget = buildTarget;
                this.buildTargetGroup = buildTargetGroup;
            }

            public IXRLoaderPreInit loader;
            public BuildTarget buildTarget;
            public BuildTargetGroup buildTargetGroup;
        }

        public int callbackOrder
        {
            get { return 0;  }
        }

        void CleanOldSettings()
        {
            BuildHelpers.CleanOldSettings<XRGeneralSettings>();
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            // Always remember to cleanup preloaded assets after build to make sure we don't
            // dirty later builds with assets that may not be needed or are out of date.
            CleanOldSettings();

            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
                return;

            XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(report.summary.platformGroup);
            if (settings == null)
                return;

            XRManagerSettings loaderManager = settings.AssignedSettings;

            if (loaderManager != null)
            {
                var loaders = loaderManager.activeLoaders;
                // If there are no loaders present in the current manager instance, then the settings will not be included in the current build.
                if (loaders.Count == 0)
                    return;

                var summary = report.summary;

                XRManagementAnalytics.SendBuildEvent(summary.guid, summary.platform, summary.platformGroup, loaders);

                // chances are that our devices won't fall back to graphics device types later in the list so it's better to assume the device will be created with the first gfx api in the list.
                // furthermore, we have no way to influence falling back to other graphics API types unless we automatically change settings underneath the user which is no good!
                GraphicsDeviceType[] deviceTypes = PlayerSettings.GetGraphicsAPIs(report.summary.platform);
                if (deviceTypes.Length > 0)
                {
                    VerifyGraphicsAPICompatibility(loaderManager, deviceTypes[0]);
                }
                else
                {
                    Debug.LogWarning("No Graphics APIs have been configured in Player Settings.");
                }

                PreInitInfo preInitInfo = null;
                if (loaders.Count >= 1)
                {
                    preInitInfo = new PreInitInfo(loaders[0] as IXRLoaderPreInit, report.summary.platform, report.summary.platformGroup);
                }

                var loader = preInitInfo?.loader ?? null;
                if (loader != null)
                {
                    BootConfig bootConfig = new BootConfig(report);
                    bootConfig.ReadBootConfig();
                    string preInitLibraryName = loader.GetPreInitLibraryName(preInitInfo.buildTarget, preInitInfo.buildTargetGroup);
                    bootConfig.SetValueForKey(kPreInitLibraryKey, preInitLibraryName);
                    bootConfig.WriteBootConfig();
                }
            }

            UnityEngine.Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
            var settingsIncludedInPreloadedAssets = preloadedAssets.Contains(settings);

            // If there are no loaders present in the current manager instance, then the settings will not be included in the current build.
            if (!settingsIncludedInPreloadedAssets && loaderManager.activeLoaders.Count > 0)
            {
                var assets = preloadedAssets.ToList();
                assets.Add(settings);
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
            else
            {
                CleanOldSettings();
            }
        }

        public static void VerifyGraphicsAPICompatibility(XRManagerSettings loaderManager, GraphicsDeviceType selectedDeviceType)
        {
                HashSet<GraphicsDeviceType> allLoaderGraphicsDeviceTypes = new HashSet<GraphicsDeviceType>();
                foreach (var loader in loaderManager.activeLoaders)
                {
                    List<GraphicsDeviceType> supporteDeviceTypes = loader.GetSupportedGraphicsDeviceTypes(true);
                    // To help with backward compatibility, if we find that any of the compatibility lists are empty we assume that at least one of the loaders does not implement the GetSupportedGraphicsDeviceTypes method
                    // Therefore we revert to the previous behavior of building the app regardless of gfx api settings.
                    if (supporteDeviceTypes.Count == 0)
                    {
                        allLoaderGraphicsDeviceTypes.Clear();
                        break;
                    }
                    foreach (var supportedGraphicsDeviceType in supporteDeviceTypes)
                    {
                        allLoaderGraphicsDeviceTypes.Add(supportedGraphicsDeviceType);
                    }
                }


                if (allLoaderGraphicsDeviceTypes.Count > 0 && !allLoaderGraphicsDeviceTypes.Contains(selectedDeviceType))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendFormat(
                            "The selected grpahics API, {0}, is not supported by any of the current loaders. Please change the preferred Graphics API setting in Player Settings.\n",
                            selectedDeviceType);

                    foreach (var loader in loaderManager.activeLoaders)
                    {
                        stringBuilder.AppendLine(loader.name + " supports:");
                        foreach (var supportedGraphicsDeviceType in loader.GetSupportedGraphicsDeviceTypes(true))
                        {
                            stringBuilder.AppendLine("\t -" + supportedGraphicsDeviceType);
                        }
                    }
                    throw new BuildFailedException(stringBuilder.ToString());
                }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            // Always remember to cleanup preloaded assets after build to make sure we don't
            // dirty later builds with assets that may not be needed or are out of date.
            CleanOldSettings();
        }

    }
}

