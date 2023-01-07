// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;

namespace Microsoft.MixedReality.OpenXR
{
    internal static class XRSettingsHelpers
    {
        /// <summary>
        /// Provides the XRGeneralSettings corresponding to the specified BuildTargetGroup.
        /// If the XRGeneralSettings asset wasn't previously created, this ensures it's created.
        /// </summary>
        public static XRGeneralSettings GetOrCreateXRGeneralSettings(BuildTargetGroup targetGroup)
        {
            XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);

            if (settings == null)
            {
                XRGeneralSettingsPerBuildTarget generalSettings = GetXRGeneralSettingsPerBuildTarget();

                if (generalSettings != null && !generalSettings.HasSettingsForBuildTarget(targetGroup))
                {
                    generalSettings.CreateDefaultSettingsForBuildTarget(targetGroup);
                }

                settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);
            }

            return settings;
        }

        /// <summary>
        /// Provides the XRManagerSettings corresponding to the specified BuildTargetGroup.
        /// If the XRManagerSettings asset wasn't previously created, this ensures it's created.
        /// </summary>
        public static XRManagerSettings GetOrCreateXRManagerSettings(BuildTargetGroup targetGroup)
        {
            XRGeneralSettings settings = GetOrCreateXRGeneralSettings(targetGroup);

            if (settings != null && settings.AssignedSettings == null)
            {
                XRGeneralSettingsPerBuildTarget generalSettings = GetXRGeneralSettingsPerBuildTarget();

                if (generalSettings != null && !generalSettings.HasManagerSettingsForBuildTarget(targetGroup))
                {
                    generalSettings.CreateDefaultManagerSettingsForBuildTarget(targetGroup);
                }
            }

            return settings != null ? settings.AssignedSettings : null;
        }

        /// <summary>
        /// Tries to read out the XRGeneralSettingsPerBuildTarget from XRGeneralSettingsPerBuildTarget.
        /// If the config object hasn't been stored yet, the XR Plug-in Management window is opened to trigger its creation.
        /// </summary>
        private static XRGeneralSettingsPerBuildTarget GetXRGeneralSettingsPerBuildTarget()
        {
            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget generalSettings))
            {
                SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out generalSettings);
            }

            return generalSettings;
        }
    }
}

#endif // UNITY_EDITOR
