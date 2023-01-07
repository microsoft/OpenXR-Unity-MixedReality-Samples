// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR

using Microsoft.MixedReality.OpenXR.Remoting;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using static UnityEngine.XR.OpenXR.Features.OpenXRFeature;

namespace Microsoft.MixedReality.OpenXR
{
    internal class PlayModeRemotingValidator
    {
        internal const string RemotingNotConfigured = "Using Holographic Remoting requires the Remote Host Name in settings " +
            "to match the IP address displayed in the Holographic Remoting Player running on your HoloLens 2 device.";

        internal static readonly string DependenciesNotEnabled = "Using Holographic Remoting requires the following HoloLens features " +
            "to be enabled in the `PC, Mac & Linux Standalone settings` tab, because the Unity editor runs as a standalone XR application. " +
            "\n  - Eye Gaze Interaction Profile" +
            $"\n  - {HandTrackingFeaturePlugin.featureName}" +
            $"\n  - {MixedRealityFeaturePlugin.featureName}" +
            "\n  - Microsoft Hand Interaction Profile";

        internal const string OpenXRLoaderNotAssigned = "Using Holographic Remoting requires the OpenXR loader " +
            "to be enabled in the `PC, Mac & Linux Standalone settings` tab, because the Unity editor runs as a standalone XR application.";

        internal const string PlayModeRemotingMenuPath = "Mixed Reality/Remoting/" + PlayModeRemotingPlugin.featureName;
        internal const string PlayModeRemotingMenuPath2 = "Window/XR/" + PlayModeRemotingPlugin.featureName;

        internal const string CannotAutoConfigureRemoting = "Could not automatically apply recommended settings to enable " + PlayModeRemotingPlugin.featureName +
            ". Please see https://aka.ms/openxr-unity-editor-remoting for manual set up instructions.";

        internal static void GetValidationChecks(OpenXRFeature feature, List<ValidationRule> results)
        {
            results.Add(new ValidationRule(feature)
            {
                message = DependenciesNotEnabled,
                error = true,
                checkPredicate = () =>
                {
                    return AreDependenciesEnabled();
                },
                fixIt = () =>
                {
                    EnableDependencies();
                }
            });

            results.Add(new ValidationRule(feature)
            {
                message = OpenXRLoaderNotAssigned,
                error = true,
                checkPredicate = () =>
                {
                    return IsLoaderAssigned();
                },
                fixIt = () =>
                {
                    AssignLoader();
                }
            });

            results.Add(new ValidationRule(feature)
            {
                message = RemotingNotConfigured,
                error = true,
                fixItAutomatic = false,
                helpText = $"To open this feature's settings, click the \"Edit\" button here or click the settings icon to the right of the \"{PlayModeRemotingPlugin.featureName}\" feature in the XR Plug-in Management settings.",
                checkPredicate = () =>
                {
                    FeatureHelpers.RefreshFeatures(BuildTargetGroup.Standalone);
                    PlayModeRemotingPlugin remotingFeature = OpenXRSettings.Instance.GetFeature<PlayModeRemotingPlugin>();
                    return remotingFeature != null && remotingFeature.HasValidSettings();
                },
                fixIt = () =>
                {
                    EditorApplication.ExecuteMenuItem(PlayModeRemotingMenuPath);
                }
            });
        }

        internal static bool IsLoaderAssigned()
        {
            XRManagerSettings standaloneManagerSettings = XRSettingsHelpers.GetOrCreateXRManagerSettings(BuildTargetGroup.Standalone);
            return standaloneManagerSettings != null &&
                standaloneManagerSettings.activeLoaders.Any(l => l.GetType().Equals(typeof(OpenXRLoader)));
        }

        internal static bool AreDependenciesEnabled()
        {
            FeatureHelpers.RefreshFeatures(BuildTargetGroup.Standalone);
            OpenXRSettings openxrSettings = OpenXRSettings.Instance;
            return openxrSettings != null &&
                IsFeatureEnabled<MixedRealityFeaturePlugin>(openxrSettings) &&
                IsFeatureEnabled<HandTrackingFeaturePlugin>(openxrSettings) &&
                IsFeatureEnabled<EyeGazeInteraction>(openxrSettings) &&
                IsFeatureEnabled<MicrosoftHandInteraction>(openxrSettings);
        }

        internal static void AssignLoader()
        {
            // Workaround: when the XR Plug-in Management window is open, we cannot assign the loader properly
            SettingsService.OpenProjectSettings("Project/Editor");
            XRManagerSettings standaloneManagerSettings = XRSettingsHelpers.GetOrCreateXRManagerSettings(BuildTargetGroup.Standalone);
            if (standaloneManagerSettings == null ||
                !XRPackageMetadataStore.AssignLoader(standaloneManagerSettings, typeof(OpenXRLoader).Name, BuildTargetGroup.Standalone))
            {
                SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
                Debug.LogError(CannotAutoConfigureRemoting);
            }
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }

        internal static void EnableDependencies()
        {
            var buildTarget = BuildTargetGroup.Standalone;
            FeatureHelpers.RefreshFeatures(buildTarget);
            OpenXRSettings openxrSettings = OpenXRSettings.Instance;
            if (openxrSettings != null)
            {
                var featureSetId = "com.microsoft.openxr.featureset.wmr"; // must be same as in WMRFeatureSet.cs
                var featureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(buildTarget, featureSetId);

                featureSet.isEnabled = true;
                OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(buildTarget);

                EnableFeature<MixedRealityFeaturePlugin>(openxrSettings);
                EnableFeature<HandTrackingFeaturePlugin>(openxrSettings);
                EnableFeature<EyeGazeInteraction>(openxrSettings);
                EnableFeature<MicrosoftHandInteraction>(openxrSettings);
            }
            else
            {
                Debug.LogError(CannotAutoConfigureRemoting);
            }
        }

        private static bool IsFeatureEnabled<T>(OpenXRSettings openxrSettings) where T : OpenXRFeature
        {
            var feature = openxrSettings.GetFeature<T>();
            return feature != null && feature.enabled;
        }

        private static void EnableFeature<T>(OpenXRSettings openxrSettings) where T : OpenXRFeature
        {
            var feature = openxrSettings.GetFeature<T>();
            if (feature != null)
            {
                feature.enabled = true;
            }
        }
    }
}

#endif
