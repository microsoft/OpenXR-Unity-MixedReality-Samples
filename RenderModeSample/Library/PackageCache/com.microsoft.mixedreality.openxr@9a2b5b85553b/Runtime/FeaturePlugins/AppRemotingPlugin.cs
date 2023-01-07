// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Microsoft.MixedReality.OpenXR.Remoting
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = featureName,
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA },
        Company = "Microsoft",
        Desc = "Feature to enable " + featureName + ".",
        DocumentationLink = "https://aka.ms/openxr-unity-app-remoting",
        OpenxrExtensionStrings = requestedExtensions,
        Category = FeatureCategory.Feature,
        Required = false,
        Priority = -100,    // hookup before other plugins so it affects json before GetProcAddr.
        FeatureId = featureId,
        Version = "1.7.0")]
#endif
    [NativeLibToken(NativeLibToken = NativeLibToken.Remoting)]
    internal class AppRemotingPlugin : OpenXRFeaturePlugin<AppRemotingPlugin>
    {
        internal const string featureId = "com.microsoft.openxr.feature.appremoting";
        internal const string featureName = "Holographic Remoting remote app";
        private const string requestedExtensions = "XR_MSFT_holographic_remoting XR_MSFT_holographic_remoting_speech";

        private OpenXRRuntimeRestartHandler m_restartHandler = null;
  
        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            if (enabled && AppRemotingSubsystem.GetCurrent().TryEnableRemotingOverride())
            {
                return AppRemotingSubsystem.GetCurrent().HookGetInstanceProcAddr(func);
            }
            else
            {
                return func;
            }
        }

        protected override void OnSubsystemCreate()
        {
            base.OnSubsystemCreate();

            if (enabled && m_restartHandler == null)
            {
                m_restartHandler = new OpenXRRuntimeRestartHandler(this, skipRestart: true, skipQuitApp: true);
            }
            else if (!enabled && m_restartHandler != null)
            {
                m_restartHandler.Dispose();
                m_restartHandler = null;
            }
        }

        protected override void OnInstanceDestroy(ulong instance)
        {
            if (enabled)
            {
                AppRemotingSubsystem.GetCurrent().ResetRemotingOverride();
            }

            Debug.Log($"[AppRemotingPlugin] OnInstanceDestroy, remotingState was {AppRemotingSubsystem.AppRemotingState}.");
            base.OnInstanceDestroy(instance);
        }

        protected override void OnSystemChange(ulong systemId)
        {
            base.OnSystemChange(systemId);

            if (systemId != 0)
            {
                Debug.Log($"[AppRemotingPlugin] OnSystemChange, systemId = {systemId}");
                if(enabled)
                {
                    AppRemotingSubsystem.GetCurrent().InitializeRemoting();
                }
            }
        }

        protected override void OnSessionStateChange(int oldState, int newState)
        {
            if ((XrSessionState)newState == XrSessionState.LossPending)
            {
                if(enabled)
                {
                    AppRemotingSubsystem.GetCurrent().OnSessionLossPending();
                }
            }
        }

#if UNITY_EDITOR
        protected override void GetValidationChecks(System.Collections.Generic.List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            AppRemotingValidator.GetValidationChecks(this, results, targetGroup);
        }
#endif
    }
}
