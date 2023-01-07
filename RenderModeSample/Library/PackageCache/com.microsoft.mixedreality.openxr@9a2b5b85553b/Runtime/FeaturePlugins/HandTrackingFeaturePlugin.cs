// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.OpenXR.Input;
#if UNITY_EDITOR
using System.Linq;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
#endif
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Microsoft.MixedReality.OpenXR
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = featureName,
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android },
        Company = "Microsoft",
        Desc = "Supports articulated hand tracking with 26 hand joints.",
        DocumentationLink = "https://aka.ms/openxr-unity",
        CustomRuntimeLoaderBuildTargets = null,
        OpenxrExtensionStrings = requestedExtensions,
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "1.7.0")]
#endif
    [NativeLibToken(NativeLibToken = NativeLibToken.HandTracking)]
    internal class HandTrackingFeaturePlugin : OpenXRFeaturePlugin<HandTrackingFeaturePlugin>
    {
        internal const string featureId = "com.microsoft.openxr.feature.handtracking";
        internal const string featureName = "Hand Tracking";
        private const string requestedExtensions =
            "XR_EXT_hand_tracking " +
            "XR_EXT_hand_joints_motion_range " +
            "XR_MSFT_hand_tracking_mesh";

        [SerializeField]
        private HandTrackingOptions leftHandTrackingOptions = default;

        [SerializeField]
        private HandTrackingOptions rightHandTrackingOptions = default;

        internal enum QuestHandTracking
        {
            v1,
            v2,
        }

#if UNITY_EDITOR
        [EditorDrawerVisibleToBuildTarget(BuildTargetGroup.Android)]
#endif
        [SerializeField,
            Tooltip("Allows for toggling specific versions of the Quest hand tracking runtime."),
            LabelWidth(200f),
            DocURL("https://developer.oculus.com/blog/presence-platforms-hand-tracking-api-gets-an-upgrade/")]
        private QuestHandTracking questHandTrackingMode = QuestHandTracking.v2;

        internal QuestHandTracking QuestHandTrackingMode => questHandTrackingMode;

        private HandTrackingSubsystemController m_handTrackingSubsystemController;

        HandTrackingFeaturePlugin()
        {
            AddSubsystemController(m_handTrackingSubsystemController = new HandTrackingSubsystemController(this));
        }

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            return NativeLib.HookGetInstanceProcAddr(nativeLibToken, func);
        }

        protected override void OnSubsystemStart()
        {
            base.OnSubsystemStart();
            NativeLib.SetHandJointsMotionRange(nativeLibToken, Handedness.Left, leftHandTrackingOptions.MotionRange);
            NativeLib.SetHandJointsMotionRange(nativeLibToken, Handedness.Right, rightHandTrackingOptions.MotionRange);
        }

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
        /// <inheritdoc/>
        protected override bool OnInstanceCreate(ulong instance)
        {
            RegisterDeviceLayout();
            return base.OnInstanceCreate(instance);
        }

        [UnityEngine.Scripting.Preserve, InputControlLayout(displayName = featureName + " (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = true)]
        public class OpenXRHandTracking : OpenXRDevice
        {
            [InputControl]
            public ButtonControl isTracked { get; private set; }

            [InputControl]
            public IntegerControl trackingState { get; private set; }

            protected override void FinishSetup()
            {
                base.FinishSetup();

                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
            }
        }

        private static void RegisterDeviceLayout()
        {
            InputSystem.RegisterLayout(typeof(OpenXRHandTracking),
                matches: new InputDeviceMatcher()
                .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                .WithProduct("OpenXR (Right|Left) Hand"));
        }

        private static void UnregisterDeviceLayout()
        {
            InputSystem.RemoveLayout(nameof(OpenXRHandTracking));
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void OnEnabledChange()
        {
            base.OnEnabledChange();
            CheckRegistration();
        }

        /// <summary>
        /// In the editor, we need to make sure the device layout gets registered
        /// even if the user doesn't navigate to the project settings.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void CheckRegistration()
        {
            // Keep the layouts registered in the editor as long as at least one of the build target
            // groups has the feature enabled.
            EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out UnityEngine.Object obj);
            if (obj is IPackageSettings packageSettings)
            {
                if (packageSettings != null && packageSettings.GetFeatures<HandTrackingFeaturePlugin>().Any(f => f.feature.enabled))
                {
                    RegisterDeviceLayout();
                }
                else
                {
                    UnregisterDeviceLayout();
                }
            }
        }
#endif // UNITY_EDITOR
#endif // ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
    }
}
