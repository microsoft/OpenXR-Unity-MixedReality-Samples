using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Input;

#if UNITY_EDITOR
using UnityEditor;
#endif

using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;

namespace UnityEngine.XR.OpenXR.Tests
{
    /// <summary>
    /// Mock Interaction feature used in tests to test interaction features.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Mock Interaction Feature",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Mock interaction feature used for testing interactions",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/khrsimplecontrollerprofile.html",
        OpenxrExtensionStrings = "",
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        Hidden = true,
        Priority = int.MinValue,
        FeatureId = featureId)]
#endif
    internal class MockInteractionFeature : OpenXRInteractionFeature
    {
        public const string featureId = "com.unity.openxr.feature.input.mockinteraction";

        private const string kDeviceLocalizedName = "Mock Controller OpenXR";

        /// <summary>
        /// Mock controller used in testing of interaction features
        /// </summary>
        [Preserve, InputControlLayout(displayName = kDeviceLocalizedName, commonUsages = new[] { "LeftHand", "RightHand" })]
        public class MockController : XRControllerWithRumble
        {
            [Preserve, InputControl]
            public AxisControl trigger { get; private set; }

            [Preserve, InputControl]
            public ButtonControl triggerPressed { get; private set; }

            [Preserve, InputControl]
            public Vector2Control thumbstick { get; private set; }

            [Preserve, InputControl(offset = 0)]
            public PoseControl devicePose { get; private set; }

            [Preserve, InputControl(offset = 2)]
            public new ButtonControl isTracked { get; private set; }

            [Preserve, InputControl(offset = 4)]
            public new IntegerControl trackingState { get; private set; }

            [Preserve, InputControl]
            public new Vector3Control devicePosition { get; private set; }

            [Preserve, InputControl]
            public new QuaternionControl deviceRotation { get; private set; }

            [Preserve, InputControl]
            public HapticControl haptic { get; private set; }

            protected override void FinishSetup()
            {
                base.FinishSetup();

                trigger = GetChildControl<AxisControl>("trigger");
                triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
                thumbstick = GetChildControl<Vector2Control>("thumbstick");

                devicePose = GetChildControl<PoseControl>("devicePose");
                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");

                haptic = GetChildControl<HapticControl>("haptic");
            }
        }

        /// <summary>
        /// Get/Set the action map config that should be used to register actions for this profile. If no
        /// config is specified the default action map config will be used.
        /// </summary>
        public ActionMapConfig actionMapConfig { get; set; }

        /// <summary>
        /// Registers the <see cref="MockController"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            InputSystem.InputSystem.RegisterLayout(typeof(MockController),
                matches: new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                    .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="MockController"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.InputSystem.RemoveLayout(nameof(MockController));
        }

        /// <summary>
        /// Create a default action map config for the controller
        /// </summary>
        /// <returns></returns>
        public ActionMapConfig CreateDefaultActionMapConfig() =>
            new ActionMapConfig()
            {
                name = "mockcontroller",
                localizedName = kDeviceLocalizedName,
                manufacturer = "Unity",
                serialNumber = "",
                desiredInteractionProfile = "/interaction_profiles/unity/mock_controller",
                deviceInfos = new List<DeviceConfig>
                {
                    new DeviceConfig
                    {
                        userPath = UserPaths.leftHand,
                        characteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller
                    },
                    new DeviceConfig
                    {
                        userPath = UserPaths.rightHand,
                        characteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller
                    }
                },
                actions = new List<ActionConfig>
                {
                    new ActionConfig
                    {
                        name = nameof(MockController.triggerPressed),
                        localizedName = "Trigger Pressed",
                        type = ActionType.Binary,
                        bindings = new List<ActionBinding>
                        {
                            new ActionBinding
                            {
                                interactionPath = "/input/trigger/click"
                            }
                        }
                    },
                    new ActionConfig
                    {
                        name = nameof(MockController.trigger),
                        localizedName = "Trigger",
                        type = ActionType.Axis1D,
                        bindings = new List<ActionBinding>
                        {
                            new ActionBinding
                            {
                                interactionPath = "/input/trigger/value"
                            }
                        }
                    },
                    new ActionConfig
                    {
                        name = nameof(MockController.thumbstick),
                        localizedName = "Thumbstick",
                        type = ActionType.Axis2D,
                        bindings = new List<ActionBinding>
                        {
                            new ActionBinding
                            {
                                interactionPath = "/input/thumbstick/value"
                            }
                        }
                    },
                    new ActionConfig
                    {
                        name = nameof(MockController.devicePose),
                        localizedName = "Grip",
                        type = ActionType.Pose,
                        bindings = new List<ActionBinding>
                        {
                            new ActionBinding
                            {
                                interactionPath = "/input/grip/pose"
                            }
                        }
                    },
                    new ActionConfig
                    {
                        name = nameof(MockController.haptic),
                        localizedName = "Haptic Output",
                        type = ActionType.Vibrate,
                        bindings = new List<ActionBinding>
                        {
                            new ActionBinding
                            {
                                interactionPath = "/output/haptic"
                            }
                        }
                    }
                }
            };

        protected override void RegisterActionMapsWithRuntime()
        {
            AddActionMap(actionMapConfig ?? CreateDefaultActionMapConfig());
        }
    }
}