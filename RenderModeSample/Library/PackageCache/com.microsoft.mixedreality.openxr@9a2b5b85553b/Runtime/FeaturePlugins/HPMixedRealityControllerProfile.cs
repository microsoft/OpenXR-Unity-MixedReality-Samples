// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine.XR.OpenXR.Features;

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;

#if USE_INPUT_SYSTEM_POSE_CONTROL
using PoseControl = UnityEngine.InputSystem.XR.PoseControl;
#else
using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
#endif
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of the HP Mixed Reality Controller interaction profile in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "HP Reverb G2 Controller Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA },
        Company = "Microsoft",
        Desc = "Supports the input mapping to the HP Reverb G2 controller.",
        DocumentationLink = "https://aka.ms/openxr-unity",
        CustomRuntimeLoaderBuildTargets = null,
        OpenxrExtensionStrings = requestedExtensions,
        Required = false,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId,
        Version = "1.7.0")]
#endif
    internal class HPMixedRealityControllerProfile : OpenXRInteractionFeature
    {
        internal const string featureId = "com.microsoft.openxr.feature.interaction.hpmixedrealitycontroller";
        private const string requestedExtensions = "XR_EXT_hp_mixed_reality_controller";

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
        private const string kDeviceLocalizedName = "HP Reverb G2 Controller OpenXR";
        private const string kDeviceDisplayName = "HP Reverb G2 Controller (OpenXR)";

        /// <summary>
        /// An Input System device based off the hand interaction profile in the <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_hp_mixed_reality_controller">HP Reverb G2 Controller</see>.
        /// </summary>
        [Preserve, InputControlLayout(displayName = kDeviceDisplayName, commonUsages = new[] { "LeftHand", "RightHand" })]
        public class HPMixedRealityController : XRControllerWithRumble
        {
            /// <summary>
            /// A <see cref="Vector2Control"/> representing the <see cref="HPMixedRealityControllerProfile.thumbstick"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Primary2DAxis", "Joystick" })]
            public Vector2Control thumbstick { get; private set; }

            /// <summary>
            /// A <see cref="AxisControl"/> representing the <see cref="HPMixedRealityControllerProfile.squeeze"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "GripAxis", "squeeze" })]
            public AxisControl grip { get; private set; }

            /// <summary>
            /// A <see cref="ButtonControl"/> representing the <see cref="HPMixedRealityControllerProfile.squeeze"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "GripButton", "squeezeClicked" })]
            public ButtonControl gripPressed { get; private set; }

            /// <summary>
            /// A <see cref="ButtonControl"/> representing the <see cref="HPMixedRealityControllerProfile.menu"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "menuButton" })]
            public ButtonControl menu { get; private set; }

            /// <summary>
            /// A <see cref="ButtonControl"/> representing the <see cref="HPMixedRealityControllerProfile.buttonA"/> <see cref="HPMixedRealityControllerProfile.buttonX"/> OpenXR bindings, depending on handedness.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "A", "X", "buttonA", "buttonX" })]
            public ButtonControl primaryButton { get; private set; }

            /// <summary>
            /// A <see cref="ButtonControl"/> representing the <see cref="HPMixedRealityControllerProfile.buttonB"/> <see cref="HPMixedRealityControllerProfile.buttonY"/> OpenXR bindings, depending on handedness.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "B", "Y", "buttonB", "buttonY" })]
            public ButtonControl secondaryButton { get; private set; }

            /// <summary>
            /// A <see cref="AxisControl"/> representing the <see cref="HPMixedRealityControllerProfile.trigger"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl]
            public AxisControl trigger { get; private set; }

            /// <summary>
            /// A <see cref="ButtonControl"/> representing the <see cref="HPMixedRealityControllerProfile.trigger"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "indexButton", "indexTouched", "triggerbutton" })]
            public ButtonControl triggerPressed { get; private set; }

            /// <summary>
            /// A <see cref="ButtonControl"/> representing the <see cref="HPMixedRealityControllerProfile.thumbstickClick"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "joystickOrPadPressed" })]
            public ButtonControl thumbstickClicked { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> representing the <see cref="HPMixedRealityControllerProfile.grip"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" })]
            public PoseControl devicePose { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> representing the <see cref="HPMixedRealityControllerProfile.aim"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "aimPose" })]
            public PoseControl pointer { get; private set; }

            /// <summary>
            /// A <see cref="ButtonControl"/> required for back compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping devicePose/isTracked.
            /// </summary>
            [Preserve, InputControl(offset = 26)]
            new public ButtonControl isTracked { get; private set; }

            /// <summary>
            /// A <see cref="IntegerControl"/> required for back compatibility with the XRSDK layouts. This represents the bit flag set indicating what data is valid. This value is equivalent to mapping devicePose/trackingState.
            /// </summary>
            [Preserve, InputControl(offset = 28)]
            new public IntegerControl trackingState { get; private set; }

            /// <summary>
            /// A <see cref="Vector3Control"/> required for back compatibility with the XRSDK layouts. This is the device position. For the HP mixed reality controller, this is the grip position. This value is equivalent to mapping devicePose/position.
            /// </summary>
            [Preserve, InputControl(offset = 32, aliases = new[] { "gripPosition" })]
            new public Vector3Control devicePosition { get; private set; }

            /// <summary>
            /// A <see cref="QuaternionControl"/> required for back compatibility with the XRSDK layouts. This is the device orientation. For the HP mixed reality controller, this is the grip rotation. This value is equivalent to mapping devicePose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 44, aliases = new[] { "deviceOrientation", "gripRotation", "gripOrientation" })]
            new public QuaternionControl deviceRotation { get; private set; }

            /// <summary>
            /// A <see cref="Vector3Control"/> required for backwards compatibility with the XRSDK layouts. This is the pointer position. This value is equivalent to mapping pointerPose/position.
            /// </summary>
            [Preserve, InputControl(offset = 92)]
            public Vector3Control pointerPosition { get; private set; }

            /// <summary>
            /// A <see cref="QuaternionControl"/> required for backwards compatibility with the XRSDK layouts. This is the pointer rotation. This value is equivalent to mapping pointerPose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 104, aliases = new[] { "pointerOrientation" })]
            public QuaternionControl pointerRotation { get; private set; }

            /// <summary>
            /// Internal call used to assign controls to the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                thumbstick = GetChildControl<Vector2Control>("thumbstick");
                thumbstickClicked = GetChildControl<ButtonControl>("thumbstickClicked");
                trigger = GetChildControl<AxisControl>("trigger");
                triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
                grip = GetChildControl<AxisControl>("grip");
                gripPressed = GetChildControl<ButtonControl>("gripPressed");

                menu = GetChildControl<ButtonControl>("menu");
                primaryButton = GetChildControl<ButtonControl>("primaryButton");
                secondaryButton = GetChildControl<ButtonControl>("secondaryButton");

                devicePose = GetChildControl<PoseControl>("devicePose");
                pointer = GetChildControl<PoseControl>("pointer");

                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
                pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
                pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
            }
        }
#endif

        /// <summary>
        /// The interaction profile string used to reference the <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_hp_mixed_reality_controller">HP Mixed Reality Controller</see>.
        /// </summary>
        public const string profile = "/interaction_profiles/hp/mixed_reality_controller";

        // Available Bindings
        // Left Hand Only
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Binary"/> interaction binding '.../input/x/click' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
        /// </summary>
        public const string buttonX = "/input/x/click";
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Binary"/> interaction binding '.../input/y/click' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.leftHand"/> user path.
        /// </summary>
        public const string buttonY = "/input/y/click";

        // Right Hand Only
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Binary"/> interaction binding '.../input/a/click' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
        /// </summary>
        public const string buttonA = "/input/a/click";
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Binary"/> interaction binding '..."/input/b/click' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs. This binding is only available for the <see cref="OpenXRInteractionFeature.UserPaths.rightHand"/> user path.
        /// </summary>
        public const string buttonB = "/input/b/click";

        // Both Hands
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Binary"/> interaction binding '.../input/menu/click' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string menu = "/input/menu/click";
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Axis1D"/> interaction binding '.../input/squeeze/value' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string squeeze = "/input/squeeze/value";
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Axis1D"/> interaction binding '.../input/trigger/value' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trigger = "/input/trigger/value";
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Axis2D"/> interaction binding '.../input/thumbstick' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbstick = "/input/thumbstick";
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Binary"/> interaction binding '.../input/thumbstick/click' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbstickClick = "/input/thumbstick/click";
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Pose"/> interaction binding '.../input/grip/pose' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string grip = "/input/grip/pose";
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Pose"/> interaction binding '.../input/aim/pose' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string aim = "/input/aim/pose";
        /// <summary>
        /// Constant for a <see cref="OpenXRInteractionFeature.ActionType.Vibrate"/> interaction binding '.../output/haptic' OpenXR Input Binding. Used by the input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string haptic = "/output/haptic";

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
        /// <inheritdoc/>
        protected override void RegisterDeviceLayout()
        {
            InputSystem.RegisterLayout(typeof(HPMixedRealityController),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <inheritdoc/>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.RemoveLayout(typeof(HPMixedRealityController).Name);
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "hpmixedrealitycontroller",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "HP",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left,
                        userPath = UserPaths.leftHand
                    },
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right,
                        userPath = UserPaths.rightHand
                    }
                },
                actions = new List<ActionConfig>()
                {
                    // Joystick
                    new ActionConfig()
                    {
                        name = "thumbstick",
                        localizedName = "Thumbstick",
                        type = ActionType.Axis2D,
                        usages = new List<string>()
                        {
                            "Primary2DAxis"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbstick,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // A / X Press
                    new ActionConfig()
                    {
                        name = "primarybutton",
                        localizedName = "Primary Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "PrimaryButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = buttonX,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.leftHand }
                            },
                            new ActionBinding()
                            {
                                interactionPath = buttonA,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.rightHand }
                            },
                        }
                    },
                    // B / Y Press
                    new ActionConfig()
                    {
                        name = "secondarybutton",
                        localizedName = "Secondary Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "SecondaryButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = buttonY,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.leftHand }
                            },
                            new ActionBinding()
                            {
                                interactionPath = buttonB,
                                interactionProfileName = profile,
                                userPaths = new List<string>() { UserPaths.rightHand }
                            },
                        }
                    },
                    // Menu
                    new ActionConfig()
                    {
                        name = "menu",
                        localizedName = "Menu",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "MenuButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = menu,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Grip
                    new ActionConfig()
                    {
                        name = "grip",
                        localizedName = "Grip",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Grip"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = squeeze,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Grip Pressed
                    new ActionConfig()
                    {
                        name = "grippressed",
                        localizedName = "Grip Pressed",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "GripButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = squeeze,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Trigger
                    new ActionConfig()
                    {
                        name = "trigger",
                        localizedName = "Trigger",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Trigger"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trigger,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Trigger Pressed
                    new ActionConfig()
                    {
                        name = "triggerpressed",
                        localizedName = "Trigger Pressed",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "TriggerButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trigger,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Thumbstick Clicked
                    new ActionConfig()
                    {
                        name = "thumbstickclicked",
                        localizedName = "Thumbstick Clicked",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "Primary2DAxisClick"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = thumbstickClick,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Device Pose
                    new ActionConfig()
                    {
                        name = "devicepose",
                        localizedName = "Device Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = grip,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Pointer Pose
                    new ActionConfig()
                    {
                        name = "pointer",
                        localizedName = "Pointer Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Pointer"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = aim,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Haptics
                    new ActionConfig()
                    {
                        name = "vibrate",
                        localizedName = "Vibrate",
                        type = ActionType.Vibrate,
                        usages = new List<string>(),
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = haptic,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }
#endif
    }
}
