// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using static UnityEngine.XR.OpenXR.Features.OpenXRFeature;

namespace Microsoft.MixedReality.OpenXR
{
    internal class MixedRealityFeatureValidator
    {
        internal static void GetValidationChecks(OpenXRFeature feature, List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            if (targetGroup == BuildTargetGroup.WSA)
            {
                results.Add(new ValidationRule(feature)
                {
                    message = "Windows Mixed Reality support may need the WebCam capability for the locatable camera feature.",
                    error = false,
                    checkPredicate = () => PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.WebCam),
                    fixIt = () => PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.WebCam, true)
                });
                results.Add(new ValidationRule(feature)
                {
                    message = "Windows Mixed Reality support may need the SpatialPerception capability for plane detection.",
                    error = false,
                    checkPredicate = () => PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.SpatialPerception),
                    fixIt = () => PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.SpatialPerception, true)
                });
            }

            results.Add(new ValidationRule(feature)
            {
                message = "The Mixed Reality OpenXR package has been updated, and Unity must be restarted to complete the update.",
                error = true,
                errorEnteringPlaymode = true,
                checkPredicate = () =>
                {
                    // MixedRealityFeaturePlugin caches its version when it's loaded into the editor. This validation rule checks to make
                    // sure the currently installed package matches the version we've cached. If they don't match, we can assume the
                    // Mixed Reality OpenXR Plugin package has been updated during this editor session, which can lead to stale native
                    // DLL references and a crash on the next time the play button is pressed. Triggering a restart here prevents the crash.
                    UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(MixedRealityFeaturePlugin).Assembly);
                    FeatureHelpers.RefreshFeatures(BuildTargetGroup.Standalone);
                    MixedRealityFeaturePlugin mixedRealityFeaturePlugin = OpenXRSettings.Instance.GetFeature<MixedRealityFeaturePlugin>();
                    return mixedRealityFeaturePlugin != null && packageInfo != null && mixedRealityFeaturePlugin.versionInstalledOnLaunch == packageInfo.version;
                },
                fixIt = RequireRestart
            });

            results.Add(new ValidationRule(feature)
            {
                message = "\"Run in Background\" is necessary for XR Unity app to continue rendering when lost keyboard focus.",
                error = false,
                checkPredicate = () => PlayerSettings.runInBackground,  // Note: The settings for "run in background" are connected for both standalone and UWP.
                fixIt = () => PlayerSettings.runInBackground = true
            });
        }

        private static void RequireRestart()
        {
            if (!EditorUtility.DisplayDialog("Unity editor restart required", "The Unity editor must be restarted for this change to take effect.", "Apply", "Cancel"))
            {
                return;
            }

            RestartEditorAndRecompileScripts();
        }

        internal static void RestartEditorAndRecompileScripts()
        {
            typeof(EditorApplication).GetMethod("RestartEditorAndRecompileScripts", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
        }
    }
}

#endif
