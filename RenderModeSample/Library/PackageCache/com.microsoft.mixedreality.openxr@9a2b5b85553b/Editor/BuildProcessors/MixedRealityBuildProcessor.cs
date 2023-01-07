// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Remoting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    // Customize code for UWP (aka WSA platform)
    // Enable the "holographic window attachment" and slightly modify the app.cpp file in app.
    internal class MixedRealityBuildProcessor : OpenXRFeatureBuildHooks
    {
        private const string MixedRealityPluginName = "MicrosoftOpenXRPlugin.dll";
        private const string PerceptionDeviceName = "PerceptionDevice.dll";
        private const string RemotingRuntimeName = "Microsoft.Holographic.AppRemoting.OpenXr.dll";
        private const string RemotingSceneUnderstandingName = "Microsoft.Holographic.AppRemoting.OpenXr.SU.dll";

        private const string RemotingJsonName = "RemotingXR.json";
        private const string RemotingJsonGuid = "db1217138e9d063459fa78b3e75f4f93";
        private const string OpenXR = "com.microsoft.mixedreality.openxr";
        private const string SamplesRepoURL = "https://github.com/microsoft/OpenXR-Unity-MixedReality-Samples";

        private static readonly Dictionary<string, string> BootVars = new Dictionary<string, string>()
        {
            {"force-primary-window-holographic", "1"},
            {"vr-enabled", "1"},
            {"xrsdk-windowsmr-library", NativeLib.DllName + ".dll"},
            {"early-boot-windows-holographic", "1"},
        };

        public override void OnPreprocessBuild(BuildReport report)
        {
            base.OnPreprocessBuild(report);

            PluginImporter[] allPlugins = PluginImporter.GetAllImporters();
            foreach (PluginImporter plugin in allPlugins)
            {
                if (plugin.isNativePlugin && plugin.assetPath.Contains(OpenXR))
                {
                    if (plugin.assetPath.Contains(MixedRealityPluginName))
                    {
                        plugin.SetIncludeInBuildDelegate(IsMixedRealityNativePluginRequired);
                    }
                    else if (plugin.assetPath.Contains(RemotingRuntimeName))
                    {
                        plugin.SetIncludeInBuildDelegate(IsRemotingRuntimeRequired);
                    }
                    else if (plugin.assetPath.Contains(RemotingSceneUnderstandingName))
                    {
                        plugin.SetIncludeInBuildDelegate(IsRemotingRuntimeRequired);
                    }
                    else if (plugin.assetPath.Contains(PerceptionDeviceName))
                    {
                        plugin.SetIncludeInBuildDelegate(IsRemotingRuntimeRequired);
                    }
                }
            }
        }

        protected override void OnPreprocessBuildExt(BuildReport report)
        {
            if (report.summary.platformGroup == BuildTargetGroup.WSA)
            {
                PreprocessBuildForWSA(report);
            }
        }

        private void PreprocessBuildForWSA(BuildReport report)
        {
            // Write boot settings before build
            BootConfig bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            foreach (KeyValuePair<string, string> entry in BootVars)
            {
                if (entry.Key == "force-primary-window-holographic" && IsRemotingRuntimeRequired())
                {
                    // When AppRemoting is enabled, skip the flag to force primary corewindow to be holographic (it won't be).
                    // If this flag exist, Unity might hit a bug that it skips rendering into the CoreWindow on the desktop.
                    continue;

                }
                bootConfig.SetValueForKey(entry.Key, entry.Value);
            }

            bootConfig.WriteBootConfig();
        }

        protected override void OnPostprocessBuildExt(BuildReport report)
        {
            if (report.summary.platformGroup == BuildTargetGroup.WSA)
            {
                PostprocessBuildForWSA(report);
            }

            CheckRemotingJson(report, IsRemotingRuntimeRequired());
        }

        private void PostprocessBuildForWSA(BuildReport report)
        {
            // Clean up boot settings after build
            BootConfig bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            foreach (KeyValuePair<string, string> entry in BootVars)
            {
                bootConfig.ClearEntryForKeyAndValue(entry.Key, entry.Value);
            }

            bootConfig.WriteBootConfig();

            if (IsRemotingRuntimeRequired())
            {
                AddRemotingJsonToData(Path.Combine(report.summary.outputPath, PlayerSettings.productName));
                VerifyPackageManifestCapabilities(Path.Combine(report.summary.outputPath, PlayerSettings.productName));
            }

            RemoveSuppressSystemOverlays(report);
        }

        /// <summary>
        /// Copies RemotingXR.json to or deletes from the build folder, depending on shouldExist.
        /// </summary>
        /// <param name="report">The build report from Unity's build hooks events.</param>
        /// <param name="shouldExist">If the file should be present in the build.</param>
        private void CheckRemotingJson(BuildReport report, bool shouldExist)
        {
            string path = report.summary.outputPath;

            if (report.summary.platform == BuildTarget.WSAPlayer)
            {
                path = Path.Combine(path, PlayerSettings.productName, RemotingJsonName);
            }
            else if (report.summary.platform == BuildTarget.StandaloneWindows64)
            {
                string folderName = Path.GetFileNameWithoutExtension(report.summary.outputPath);

                // When building with "Create Visual Studio Solution", this API still reports a .exe
                // in the output path (but it doesn't exist, so we can assume we're building a .sln)
                if (path.EndsWith(".exe") && !File.Exists(path))
                {
                    path = Path.Combine(path, "..", "build", "bin");
                }
                else
                {
                    path = Path.Combine(path, "..");
                }

                path = Path.Combine(path, folderName + "_Data", "Plugins", "x86_64", RemotingJsonName);
            }
            else
            {
                // Other platforms aren't supported
                return;
            }

            string folderPath = Directory.GetParent(path).FullName;
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError($"The {nameof(MixedRealityBuildProcessor)} could not find the Plugins folder (looked in {folderPath}).\n" +
                    $"Please file an issue on {SamplesRepoURL}, as this is unexpected. Holographic Remoting may not work as a result.");
            }
            else if (shouldExist)
            {
                File.Copy(Path.GetFullPath(AssetDatabase.GUIDToAssetPath(RemotingJsonGuid)), path, true);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Adds a line in the Unity data file project to include the remoting file in the build.
        /// </summary>
        /// <param name="path">The path to the folder that contains Unity Data.vcxitems.</param>
        private void AddRemotingJsonToData(string path)
        {
            const string UnityDataPath = "Unity Data.vcxitems";
            path = Path.Combine(path, UnityDataPath);

            XElement root = XElement.Load(path);
            foreach (XElement itemGroup in root.Elements(root.GetDefaultNamespace() + "ItemGroup"))
            {
                foreach (XElement remotingDll in itemGroup.Elements(root.GetDefaultNamespace() + "None"))
                {
                    XAttribute includeDll = remotingDll.Attribute("Include");
                    if (includeDll != null && includeDll.Value.Contains(RemotingRuntimeName))
                    {
                        XElement jsonElement = new XElement(remotingDll);
                        // Update "Include" to point to the json, but leave "Condition" alone so it's still dependent on the remoting binary existing
                        jsonElement.Attribute("Include").Value = jsonElement.Attribute("Include").Value.Replace(RemotingRuntimeName, RemotingJsonName);
                        itemGroup.Add(jsonElement);
                        root.Save(path);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Verifies if the internet capabilities in Package.appxmanifest match the ones in the Unity Player Settings for Holographic App remoting to work properly.
        /// </summary>
        /// <param name="path">The path to the folder that contains Package.appxmanifest.</param>
        private void VerifyPackageManifestCapabilities(string path)
        {
            bool internetClientEnabled = false, internetClientServerEnabled = false, privateNetworkClientServerEnabled = false;

            const string PackageManifestPath = "Package.appxmanifest";
            path = Path.Combine(path, PackageManifestPath);
            XmlDocument manifest = new XmlDocument();
            manifest.Load(path);
            XmlNode root = manifest.DocumentElement;

            // Get the internet capabilities that are enabled from manifest
            foreach (XmlNode childNode in root.ChildNodes)
            {
                if (childNode.Name == "Capabilities")
                {
                    foreach (XmlNode capability in childNode.ChildNodes)
                    {
                        if (capability.Name == "Capability")
                        {
                            foreach (XmlAttribute attribute in capability.Attributes)
                            {
                                if (attribute.Name == "Name" && attribute.Value == "internetClient")
                                {
                                    internetClientEnabled = true;
                                }
                                if (attribute.Name == "Name" && attribute.Value == "internetClientServer")
                                {
                                    internetClientServerEnabled = true;
                                }
                                if (attribute.Name == "Name" && attribute.Value == "privateNetworkClientServer")
                                {
                                    privateNetworkClientServerEnabled = true;
                                }
                            }
                        }
                    }
                }
            }

            // Generate a warning, if the capabilities in the manifest don't match the ones in Unity Player Settings
            if (PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.InternetClient) != internetClientEnabled)
            {
                Debug.LogWarning("The InternetClient capability in the existing Package.appxmanifest does not match the capability in this project's Player settings." +
                " To update the existing Package.appxmanifest to reflect your project's current Player settings, please edit the file manually" +
                " or regenerate the Package.appxmanifest.");
            }
            if (PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.InternetClientServer) != internetClientServerEnabled)
            {
                Debug.LogWarning("The InternetClientServer capability in the existing Package.appxmanifest does not match the capability in this project's Player settings." +
                " To update the existing Package.appxmanifest to reflect your project's current Player settings, please edit the file manually" +
                " or regenerate the Package.appxmanifest.");
            }
            if (PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.PrivateNetworkClientServer) != privateNetworkClientServerEnabled)
            {
                Debug.LogWarning("The PrivateNetworkClientServer capability in the existing Package.appxmanifest does not match the capability in this project's Player settings." +
                " To update the existing Package.appxmanifest to reflect your project's current Player settings, please edit the file manually" +
                " or regenerate the Package.appxmanifest.");
            }

        }

        // Remove the deprecated usage of SuppressSystemOverlays from Unity UWP project template.
        private void RemoveSuppressSystemOverlays(BuildReport report)
        {
            string appCppPath = Path.Combine(report.summary.outputPath, PlayerSettings.productName, "App.cpp");

            // For certain types of builds, like XAML builds, App.cpp won't exist. App.xaml.cpp does though.
            if (!File.Exists(appCppPath))
            {
                appCppPath = Path.Combine(report.summary.outputPath, PlayerSettings.productName, "App.xaml.cpp");
            }

            string appCppLines = File.ReadAllText(appCppPath);
            const string Pattern = @"\r?\n.*SuppressSystemOverlays.*\r?\n";
            string modifiedAppCppLines = System.Text.RegularExpressions.Regex.Replace(appCppLines, Pattern, "");
            File.WriteAllText(appCppPath, modifiedAppCppLines);
        }

        private bool IsRemotingRuntimeRequired(string path = "") => BuildProcessorHelpers.IsFeatureEnabled<AppRemotingPlugin>();
        private bool IsMixedRealityNativePluginRequired(string path = "") =>
            IsRemotingRuntimeRequired(path) ||
            BuildProcessorHelpers.IsFeatureEnabled<MixedRealityFeaturePlugin>() ||
            BuildProcessorHelpers.IsFeatureEnabled<HandTrackingFeaturePlugin>() ||
            BuildProcessorHelpers.IsFeatureEnabled<MotionControllerFeaturePlugin>();

        /// <summary>
        /// Small utility class for reading, updating and writing boot config.
        /// </summary>
        private class BootConfig
        {
            private const string XrBootSettingsKey = "xr-boot-settings";

            private readonly Dictionary<string, string> bootConfigSettings;
            private readonly string buildTargetName;

            public BootConfig(BuildReport report)
            {
                bootConfigSettings = new Dictionary<string, string>();
                buildTargetName = BuildPipeline.GetBuildTargetName(report.summary.platform);
            }

            public void ReadBootConfig()
            {
                bootConfigSettings.Clear();

                string xrBootSettings = EditorUserBuildSettings.GetPlatformSettings(buildTargetName, XrBootSettingsKey);
                if (!string.IsNullOrEmpty(xrBootSettings))
                {
                    // boot settings string format
                    // <boot setting>:<value>[;<boot setting>:<value>]*
                    var bootSettings = xrBootSettings.Split(';');
                    foreach (var bootSetting in bootSettings)
                    {
                        var setting = bootSetting.Split(':');
                        if (setting.Length == 2 && !string.IsNullOrEmpty(setting[0]) && !string.IsNullOrEmpty(setting[1]))
                        {
                            bootConfigSettings.Add(setting[0], setting[1]);
                        }
                    }
                }
            }

            public void SetValueForKey(string key, string value) => bootConfigSettings[key] = value;

            public void ClearEntryForKeyAndValue(string key, string value)
            {
                if (bootConfigSettings.TryGetValue(key, out string dictValue) && dictValue == value)
                {
                    bootConfigSettings.Remove(key);
                }
            }

            public void WriteBootConfig()
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

                EditorUserBuildSettings.SetPlatformSettings(buildTargetName, XrBootSettingsKey, sb.ToString());
            }
        }

        public override int callbackOrder => 1;
        public override Type featureType => typeof(MixedRealityFeaturePlugin);

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
        }
    }
}
