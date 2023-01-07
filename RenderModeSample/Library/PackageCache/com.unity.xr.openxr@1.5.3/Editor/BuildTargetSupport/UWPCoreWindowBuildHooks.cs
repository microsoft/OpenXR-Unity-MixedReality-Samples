using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR
{
    internal class UWPCoreWindowBuildHooks : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // After MixedRealityBuildProcessor, if it's in the project.
        public int callbackOrder => 2;

        private static readonly Dictionary<string, string> BootVars = new Dictionary<string, string>()
        {
            {"force-primary-window-holographic", "1"},
            {"vr-enabled", "1"},
            {"xrsdk-windowsmr-library", "UnityOpenXR.dll"},
            {"early-boot-windows-holographic", "1"},
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WSAPlayer)
                return;

            if (!BuildHelperUtils.HasLoader(BuildTargetGroup.WSA, typeof(OpenXRLoaderBase)))
                return;

            var bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            // MixedRealityBuildProcessor may skip setting `force-primary-window-holographic` in certain cases:
            //
            //     When AppRemoting is enabled, skip the flag to force primary corewindow to be holographic (it won't be).
            //     If this flag exist, Unity might hit a bug that it skips rendering into the CoreWindow on the desktop.
            var skipPrimaryWindowHolographic = bootConfig.TryGetValue("xrsdk-windowsmr-library", out var unused1) &&
                (bootConfig.TryGetValue("vr-enabled", out var vrEnabled) && vrEnabled == "1") &&
                (bootConfig.TryGetValue("early-boot-windows-holographic", out var earlyBoot) && earlyBoot == "1") &&
                (!bootConfig.TryGetValue("force-primary-window-holographic", out var forceHolographic) || forceHolographic == "0");

            foreach (var entry in BootVars)
            {
                if (entry.Key == "force-primary-window-holographic" && skipPrimaryWindowHolographic)
                    continue;

                bootConfig.SetValueForKey(entry.Key, entry.Value);
            }

            bootConfig.WriteBootConfig();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WSAPlayer)
                return;

            if (!BuildHelperUtils.HasLoader(BuildTargetGroup.WSA, typeof(OpenXRLoaderBase)))
                return;

            // Clean up boot settings after build
            BootConfig bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            foreach (KeyValuePair<string, string> entry in BootVars)
            {
                bootConfig.ClearEntryForKeyAndValue(entry.Key, entry.Value);
            }

            bootConfig.WriteBootConfig();
        }

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

            public bool TryGetValue(string key, out string value) => bootConfigSettings.TryGetValue(key, out value);

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
    }
}