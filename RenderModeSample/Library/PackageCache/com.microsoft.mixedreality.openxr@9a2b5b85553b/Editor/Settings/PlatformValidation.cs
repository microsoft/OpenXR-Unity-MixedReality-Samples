// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using static Microsoft.MixedReality.OpenXR.MixedRealityFeaturePlugin;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    /// <summary>
    /// Provides a menu item for configuring settings according to specified OpenXR devices.
    /// </summary>
    internal static class PlatformValidation
    {
        private static readonly System.Type[] HoloLens2FeatureTypes = new System.Type[] {
            typeof(MixedRealityFeaturePlugin), typeof(MicrosoftHandInteraction),
            typeof(HandTrackingFeaturePlugin), typeof(EyeGazeInteraction) };

        private const string OpenXRProjectValidationSettingsPath = "Project/XR Plug-in Management/Project Validation";
        private const string PerformanceHelpLink = "https://aka.ms/HoloLens2PerfSettings";
        private const string SetupHelpLink = "https://aka.ms/HoloLens2OpenXRConfig";
        private static readonly string CannotAutoSetupForHL2 = "Could not automatically apply recommended settings for HoloLens 2. " +
            $"Please see {SetupHelpLink} for manual set up instructions";
        private static readonly string CannotAutoOptimizeForHL2 = "Could not automatically apply recommended settings for HoloLens 2. " +
            $"Please see {PerformanceHelpLink} for manual optimization instructions";

        [InitializeOnLoadMethod]
        private static void InitializePlatformValidation()
        {
            BuildValidationRule[] wsaValidationRules = new BuildValidationRule[] { GenerateRealtimeGIRule(), GenerateHL2QualityRule(), GenerateHL2BuildTargetRule(),
                GenerateUWPOpenXRLoaderRule(), GenerateHL2RenderAndDepthSubmissionModeRule(), GenerateHL2FeatureSetRule(), GenerateHL2FeaturesRule(), GenerateHL2CameraRule() };
            BuildValidator.AddRules(BuildTargetGroup.WSA, wsaValidationRules);
        }

        [MenuItem("Mixed Reality/Project/Apply recommended project settings for HoloLens 2", false)]
        private static void ApplyOpenXRSettings()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WSA, BuildTarget.WSAPlayer))
            {
                EditorUtility.DisplayDialog("UWP support not found", "The UWP build support is not currently installed. " +
                    "Please add the Universal Windows Platform Build Support module to your Unity installation.", "OK");
                return;
            }
            MixedRealityFeaturePlugin plugin = BuildProcessorHelpers.GetOpenXRFeature<MixedRealityFeaturePlugin>(BuildTargetGroup.WSA, false);
            if (plugin == null)
            {
                FeatureHelpers.RefreshFeatures(BuildTargetGroup.WSA);
                plugin = BuildProcessorHelpers.GetOpenXRFeature<MixedRealityFeaturePlugin>(BuildTargetGroup.WSA, false);
            }
            if (plugin != null)
            {
                plugin.ValidationRuleTarget = ValidationRuleTargetPlatform.HoloLens2;

                // Make sure to select WSA platform, which also select the UWP tab in validation window.
                // NOTE: must do this selected group change before switching build target below
                // otherwise this selection change will not function properly in Unity editor.
                if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.WSA)
                {
                    EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.WSA;
                }

                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WSAPlayer)
                {
                    if (EditorUtility.DisplayDialog("Target platform is not UWP", "You are currently targeting a non UWP platform. " +
                        "To build HoloLens 2 applications you need to switch the build target to UWP in Build Settings.\n\n" +
                        "Click `Continue` to switch build target platform to UWP and open the Project Validation window to review other validation messages.",
                        "Continue", "Cancel"))
                    {
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
                        ShowProjectValidationSettings();
                    }
                    return;
                }
                ShowProjectValidationSettings();
            }
            else
            {
                Debug.LogError(CannotAutoSetupForHL2);
            }
        }

        private static void ShowProjectValidationSettings()
        {
            // Need to call OpenProjectSettings twice since the first call may not properly bring up the requested page
            // Possibly due to the generation of XR settings related files on the fly
            SettingsService.OpenProjectSettings(OpenXRProjectValidationSettingsPath);
            EditorApplication.delayCall += () =>
            {
                SettingsService.OpenProjectSettings(OpenXRProjectValidationSettingsPath);
            };

        }

        #region HoloLens 2 rules

        private static BuildValidationRule GenerateRealtimeGIRule()
        {
            return new BuildValidationRule()
            {
                IsRuleEnabled = IsHoloLens2ValidationRuleTarget,
                Category = "MR OpenXR - HoloLens 2",
                Message = $"Realtime GI has a negative performance impact on HoloLens 2 applications.",
                CheckPredicate = () => !Lightmapping.TryGetLightingSettings(out LightingSettings lightingSettings) || !lightingSettings.realtimeGI,
                FixIt = () =>
                {
                    if (Lightmapping.TryGetLightingSettings(out LightingSettings lightingSettings))
                    {
                        lightingSettings.realtimeGI = false;
                        EditorUtility.SetDirty(lightingSettings);
                    }
                    else
                    {
                        Debug.LogError(CannotAutoOptimizeForHL2);
                    }
                },
                FixItMessage = $"Disable realtime GI in lighting settings",
                Error = false,
                HelpLink = PerformanceHelpLink
            };
        }

        private static BuildValidationRule GenerateHL2QualityRule()
        {
            return new BuildValidationRule()
            {
                // Currently this rule doesn't work as Unity use the "default quality" for a platform to determine the quality level to use
                // Setting the "current active quality" does not impact the application running on HoloLens 2
                IsRuleEnabled = () => false, //IsHoloLens2ValidationRuleTarget,
                Category = "MR OpenXR - HoloLens 2",
                Message = $"High quality settings have a negative performance impact on HoloLens 2 applications.",
                CheckPredicate = () => QualitySettings.GetQualityLevel() == 0,
                FixIt = () => QualitySettings.SetQualityLevel(0, true),
                FixItMessage = $"Set quality settings to very low",
                Error = false,
                HelpLink = PerformanceHelpLink
            };
        }

        private static BuildValidationRule GenerateHL2GPUSkinningRule()
        {
            return new BuildValidationRule()
            {
                IsRuleEnabled = IsHoloLens2ValidationRuleTarget,
                Category = "MR OpenXR - HoloLens 2",
                Message = $"GPU skinning negatively impacts the performance of HoloLens 2 applications.",
                CheckPredicate = () => !PlayerSettings.gpuSkinning,
                FixIt = () => PlayerSettings.gpuSkinning = false,
                FixItMessage = $"Disable GPU skinning",
                Error = false
            };
        }

        private static BuildValidationRule GenerateHL2BuildTargetRule()
        {
            return new BuildValidationRule()
            {
                IsRuleEnabled = IsHoloLens2ValidationRuleTarget,
                Category = "MR OpenXR - HoloLens 2",
                Message = $"The project needs to target the UWP platform to build applications for HoloLens 2.",
                CheckPredicate = () => EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer,
                FixIt = () => EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer),
                FixItMessage = $"Switch the build target to UWP",
                Error = true,
                FixItAutomatic = false
            };
        }

        private static BuildValidationRule GenerateHL2RenderAndDepthSubmissionModeRule()
        {
            return new BuildValidationRule()
            {
                IsRuleEnabled = IsHoloLens2ValidationRuleTarget,
                Category = "MR OpenXR - HoloLens 2",
                Message = $"Single pass instanced is recommended for render mode and depth 16 bit is recommended for depth submission mode settings.",
                CheckPredicate = () =>
                {
                    if (TryGetOpenXRSetting(BuildTargetGroup.WSA, out OpenXRSettings settings))
                    {
                        return settings.depthSubmissionMode == OpenXRSettings.DepthSubmissionMode.Depth16Bit && settings.renderMode == OpenXRSettings.RenderMode.SinglePassInstanced;
                    }
                    return true;
                },
                FixIt = () =>
                {
                    if (TryGetOpenXRSetting(BuildTargetGroup.WSA, out OpenXRSettings settings))
                    {
                        settings.depthSubmissionMode = OpenXRSettings.DepthSubmissionMode.Depth16Bit;
                        settings.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
                        EditorUtility.SetDirty(settings);
                    }
                    else
                    {
                        Debug.LogError(CannotAutoOptimizeForHL2);
                    }
                },
                FixItMessage = $"Switch the render mode to single pass instanced and depth submission mode to depth 16 bit",
                Error = false,
                HelpLink = PerformanceHelpLink
            };
        }

        private static BuildValidationRule GenerateUWPOpenXRLoaderRule()
        {
            return new BuildValidationRule()
            {
                // The OpenXR loader should always be enabled for UWP
                IsRuleEnabled = () => true,
                Category = "MR OpenXR - HoloLens 2",
                Message = $"The OpenXR loader must be enabled for UWP in XR plugin management settings.",
                CheckPredicate = () => XRPackageMetadataStore.IsLoaderAssigned(typeof(OpenXRLoader).FullName, BuildTargetGroup.WSA),
                FixIt = () => EnableOpenXRLoader(BuildTargetGroup.WSA),
                FixItMessage = $"Assign the OpenXR loader for UWP",
                Error = true,
                HelpLink = SetupHelpLink
            };
        }

        private static BuildValidationRule GenerateHL2FeatureSetRule()
        {
            return new BuildValidationRule()
            {
                IsRuleEnabled = IsHoloLens2ValidationRuleTarget,
                Category = "MR OpenXR - HoloLens 2",
                Message = $"The HoloLens feature set must be enabled for UWP in OpenXR settings.",
                CheckPredicate = () => CheckFeatureSet(BuildTargetGroup.WSA, HoloLensFeatureSet.featureSetId),
                FixIt = () => EnableFeatureSet(BuildTargetGroup.WSA, HoloLensFeatureSet.featureSetId),
                FixItMessage = $"Enable the HoloLens feature set for UWP",
                Error = true,
                HelpLink = SetupHelpLink
            };
        }

        private static BuildValidationRule GenerateHL2FeaturesRule()
        {
            return new BuildValidationRule()
            {
                IsRuleEnabled = IsHoloLens2ValidationRuleTarget,
                Category = "MR OpenXR - HoloLens 2",
                Message = $"HoloLens 2 related features must be enabled for UWP in OpenXR settings.",
                CheckPredicate = () => CheckFeatures(BuildTargetGroup.WSA, HoloLens2FeatureTypes),
                FixIt = () => EnableFeatures(BuildTargetGroup.WSA, HoloLens2FeatureTypes),
                FixItMessage = $"Enable HoloLens 2 related features for UWP",
                Error = true,
                HelpLink = SetupHelpLink
            };
        }

        private static BuildValidationRule GenerateHL2CameraRule()
        {
            return new BuildValidationRule()
            {
                IsRuleEnabled = IsHoloLens2ValidationRuleTarget,
                Category = "MR OpenXR - HoloLens 2 (Scene specific)",
                Message = $"It is recommended for the main camera to have a clear background color, a solid color clear flag and a TrackedPoseDriver component.",
                CheckPredicate = () =>
                {
                    if (Camera.main.clearFlags != CameraClearFlags.SolidColor || Camera.main.backgroundColor != Color.clear)
                    {
                        return false;
                    }
                    if (!Camera.main.gameObject.GetComponent<TrackedPoseDriver>()
#if USE_ARFOUNDATION
                        && !Camera.main.gameObject.GetComponent<UnityEngine.XR.ARFoundation.ARPoseDriver>()
#endif
                        )
                    {
                        return false;
                    }
                    return true;
                },
                FixIt = () =>
                {
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = Color.clear;
                    if (!Camera.main.gameObject.GetComponent<TrackedPoseDriver>()
#if USE_ARFOUNDATION
                        && !Camera.main.gameObject.GetComponent<UnityEngine.XR.ARFoundation.ARPoseDriver>()
#endif
                        )
                    {
                        Camera.main.gameObject.AddComponent<TrackedPoseDriver>();
                    }
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                },
                FixItMessage = $"Ensure the main camera has a clear background color, a solid color clear flag and a TrackedPoseDriver component",
                Error = false,
                FixItAutomatic = false,
                SceneOnlyValidation = true
            };
        }
        #endregion HoloLens 2 rules

        #region Helpers

        private static bool IsHoloLens2ValidationRuleTarget()
        {
            MixedRealityFeaturePlugin plugin = BuildProcessorHelpers.GetOpenXRFeature<MixedRealityFeaturePlugin>(BuildTargetGroup.WSA, false);
            return plugin != null && plugin.ValidationRuleTarget == ValidationRuleTargetPlatform.HoloLens2;
        }

        private static void EnableOpenXRLoader(BuildTargetGroup targetGroup)
        {
            if (XRSettingsHelpers.GetOrCreateXRManagerSettings(targetGroup) is XRManagerSettings settings && settings != null
            && XRPackageMetadataStore.AssignLoader(settings, nameof(OpenXRLoader), targetGroup))
            {
                EditorUtility.SetDirty(settings);
            }
            else
            {
                Debug.LogError(CannotAutoSetupForHL2);
            }
        }

        private static bool CheckFeatureSet(BuildTargetGroup targetGroup, string featureId)
        {
            if (XRSettingsHelpers.GetOrCreateXRManagerSettings(targetGroup) is XRManagerSettings settings && settings != null)
            {
                foreach (var featureSet in OpenXRFeatureSetManager.FeatureSetsForBuildTarget(targetGroup))
                {
                    if (featureSet.featureSetId == featureId && !featureSet.isEnabled)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void EnableFeatureSet(BuildTargetGroup targetGroup, string featureId)
        {
            foreach (var featureSet in OpenXRFeatureSetManager.FeatureSetsForBuildTarget(targetGroup))
            {
                if (featureSet.featureSetId == featureId && !featureSet.isEnabled)
                {
                    featureSet.isEnabled = true;
                }
            }
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(targetGroup);
        }

        private static bool TryGetOpenXRSetting(BuildTargetGroup targetGroup, out OpenXRSettings openXRSettings)
        {
            if (EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out Object obj) && obj is IPackageSettings packageSettings
                && packageSettings.GetSettingsForBuildTargetGroup(targetGroup) is OpenXRSettings settings && settings != null)
            {
                openXRSettings = settings;
                return true;
            }
            else
            {
                openXRSettings = null;
                return false;
            }
        }

        private static bool CheckFeatures(BuildTargetGroup targetGroup, IEnumerable<System.Type> featureTypes)
        {
            if (TryGetOpenXRSetting(targetGroup, out OpenXRSettings settings))
            {
                foreach (OpenXRFeature feature in settings.GetFeatures())
                {
                    if (featureTypes.Contains(feature.GetType()) && !feature.enabled)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void EnableFeatures(BuildTargetGroup targetGroup, IEnumerable<System.Type> featureTypes)
        {
            if (TryGetOpenXRSetting(targetGroup, out OpenXRSettings settings))
            {
                foreach (OpenXRFeature feature in settings.GetFeatures())
                {
                    if (featureTypes.Contains(feature.GetType()) && !feature.enabled)
                    {
                        feature.enabled = true;
                    }
                }
                EditorUtility.SetDirty(settings);
            }
        }
        #endregion Helpers
    }
}
