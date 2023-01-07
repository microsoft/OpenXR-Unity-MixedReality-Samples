// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR

using Microsoft.MixedReality.OpenXR.Remoting;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using static UnityEngine.XR.OpenXR.Features.OpenXRFeature;

namespace Microsoft.MixedReality.OpenXR
{
    internal class AppRemotingValidator
    {
        internal static void GetValidationChecks(OpenXRFeature feature, List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            results.Add(new ValidationRule(feature)
            {
                message = $"\"{AppRemotingPlugin.featureName}\" and \"Initialize XR on Startup\" are both enabled. XR initialization should be delayed until a specific IP address is entered.",
                error = true,
                checkPredicate = () =>
                {
                    XRGeneralSettings settings = XRSettingsHelpers.GetOrCreateXRGeneralSettings(targetGroup);
                    return settings != null && !settings.InitManagerOnStart;
                },
                fixIt = () =>
                {
                    XRGeneralSettings settings = XRSettingsHelpers.GetOrCreateXRGeneralSettings(targetGroup);
                    if (settings != null)
                    {
                        settings.InitManagerOnStart = false;
                    }
                }
            });

            if (targetGroup == BuildTargetGroup.WSA)
            {
                results.Add(new ValidationRule(feature)
                {
                    message = "Required InternetClient capabilty in Unity PlayerSettings is not enabled for Holographic Application Remoting to work properly",
                    error = true,
                    checkPredicate = () => PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.InternetClient),
                    fixIt = () =>
                    {
                        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.InternetClient, true);
                    }
                });

                results.Add(new ValidationRule(feature)
                {
                    message = "Required InternetClientServer, PrivateNetworkClientServer capabilties in Unity PlayerSettings are not enabled for Holographic Application Remoting to work properly",
                    error = false,
                    checkPredicate = () => PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.InternetClientServer) && 
                                           PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.PrivateNetworkClientServer),
                    fixIt = () =>
                    {
                        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.InternetClientServer, true);
                        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.PrivateNetworkClientServer, true);
                    }
                });
            }

        }
    }
}

#endif
