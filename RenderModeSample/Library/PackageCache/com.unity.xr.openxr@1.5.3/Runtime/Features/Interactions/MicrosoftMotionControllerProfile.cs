using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

#if UNITY_EDITOR
using UnityEditor;
#endif

using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;

namespace UnityEngine.XR.OpenXR.Features.Interactions
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of Microsoft Motion Controllers interaction profiles in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Microsoft Motion Controller Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA},
        Company = "Unity",
        Desc = "Allows for mapping input to the Microsoft Motion Controller interaction profile.",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/microsoftmotioncontrollerprofile.html",
        OpenxrExtensionStrings = "",
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class MicrosoftMotionControllerProfile : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.microsoftmotioncontroller";

        /// <summary>
        /// An Input System device based off the <a href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_microsoft_mixed_reality_motion_controller_profile">Microsoft Mixed Reality Motion Controller</a>.
        /// </summary>
        [Preserve, InputControlLayout(displayName = "Windows MR Controller (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" })]
        public class WMRSpatialController : XRControllerWithRumble
        {
            /// <summary>
            /// A [Vector2Control](xref:UnityEngine.InputSystem.Controls.Vector2Control) that represents the <see cref="MicrosoftMotionControllerProfile.thumbstick"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Primary2DAxis", "thumbstickaxes", "thumbstick" }, usage = "Primary2DAxis")]
            public Vector2Control joystick { get; private set; }

            /// <summary>
            /// A [Vector2Control](xref:UnityEngine.InputSystem.Controls.Vector2Control) that represents the <see cref="MicrosoftMotionControllerProfile.trackpad"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Secondary2DAxis", "touchpadaxes", "trackpad" }, usage = "Secondary2DAxis")]
            public Vector2Control touchpad { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MicrosoftMotionControllerProfile.squeeze"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "GripAxis", "squeeze" }, usage ="Grip")]
            public AxisControl grip { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MicrosoftMotionControllerProfile.squeeze"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "GripButton", "squeezeClicked" }, usage = "GripButton")]
            public ButtonControl gripPressed { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MicrosoftMotionControllerProfile.menu"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Primary", "menubutton" }, usage = "MenuButton")]
            public ButtonControl menu { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MicrosoftMotionControllerProfile.trigger"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "triggeraxis" }, usage = "Trigger")]
            public AxisControl trigger { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MicrosoftMotionControllerProfile.trigger"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias="triggerbutton", usage="TriggerButton")]
            public ButtonControl triggerPressed { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MicrosoftMotionControllerProfile.thumbstickClick"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] {"joystickClicked", "thumbstickpressed"}, usage = "Primary2DAxisClick")]
            public ButtonControl joystickClicked { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MicrosoftMotionControllerProfile.trackpadClick"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "joystickorpadpressed", "touchpadpressed", "trackpadClicked" }, usage = "Secondary2DAxisClick")]
            public ButtonControl touchpadClicked { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the <see cref="MicrosoftMotionControllerProfile.trackpadTouch"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "joystickorpadtouched", "touchpadtouched", "trackpadTouched" }, usage = "Secondary2DAxisTouch")]
            public ButtonControl touchpadTouched { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="MicrosoftMotionControllerProfile.grip"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" }, usage = "Device")]
            public PoseControl devicePose { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> that represents the <see cref="MicrosoftMotionControllerProfile.aim"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "aimPose" }, usage = "Pointer")]
            public PoseControl pointer { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping devicePose/isTracked.
            /// </summary>
            [Preserve, InputControl(offset = 32)]
            new public ButtonControl isTracked { get; private set; }

            /// <summary>
            /// A [IntegerControl](xref:UnityEngine.InputSystem.Controls.IntegerControl) required for backwards compatibility with the XRSDK layouts. This represents the bit flag set indicating what data is valid. This value is equivalent to mapping devicePose/trackingState.
            /// </summary>
            [Preserve, InputControl(offset = 36)]
            new public IntegerControl trackingState { get; private set; }

            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the device position, or grip position. This value is equivalent to mapping devicePose/position.
            /// </summary>
            [Preserve, InputControl(offset = 40, aliases = new[] { "gripPosition" })]
            new public Vector3Control devicePosition { get; private set; }

            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the device orientation, or grip orientation. This value is equivalent to mapping devicePose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 52, aliases = new[] { "gripOrientation" })]
            new public QuaternionControl deviceRotation { get; private set; }

            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for back compatibility with the XRSDK layouts. This is the pointer position. This value is equivalent to mapping pointerPose/position.
            /// </summary>
            [Preserve, InputControl(offset = 100)]
            public Vector3Control pointerPosition { get; private set; }

            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the pointer rotation. This value is equivalent to mapping pointerPose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 112, aliases = new[] { "pointerOrientation" })]
            public QuaternionControl pointerRotation { get; private set; }

            /// <summary>
            /// A <see cref="HapticControl"/> that represents the <see cref="MicrosoftMotionControllerProfile.haptic"/> binding.
            /// </summary>
            [Preserve, InputControl(usage = "Haptic")]
            public HapticControl haptic { get; private set; }

            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                joystick = GetChildControl<Vector2Control>("joystick");
                trigger = GetChildControl<AxisControl>("trigger");
                touchpad = GetChildControl<Vector2Control>("touchpad");
                grip = GetChildControl<AxisControl>("grip");
                gripPressed = GetChildControl<ButtonControl>("gripPressed");
                menu = GetChildControl<ButtonControl>("menu");
                joystickClicked = GetChildControl<ButtonControl>("joystickClicked");
                triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
                touchpadClicked = GetChildControl<ButtonControl>("touchpadClicked");
                touchpadTouched = GetChildControl<ButtonControl>("touchPadTouched");
                devicePose = GetChildControl<PoseControl>("devicePose");
                pointer = GetChildControl<PoseControl>("pointer");

                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
                pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
                pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");

                haptic = GetChildControl<HapticControl>("haptic");
            }
        }

        /// <summary>
        /// The interaction profile string used to reference the <a href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_microsoft_mixed_reality_motion_controller_profile">Microsoft Mixed Reality Motion Controller</a>.
        /// </summary>
        public const string profile = "/interaction_profiles/microsoft/motion_controller";

        /// <summary>
        /// Constant for a boolean interaction binding '.../input/menu/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string menu = "/input/menu/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/squeeze/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string squeeze = "/input/squeeze/click";
        /// <summary>
        /// Constant for a float interaction binding '.../input/trigger/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trigger = "/input/trigger/value";
        /// <summary>
        /// Constant for a Vector2 interaction binding '.../input/thumbstick' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbstick = "/input/thumbstick";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/thumbstick/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string thumbstickClick = "/input/thumbstick/click";
        /// <summary>
        /// Constant for a Vector2 interaction binding '.../input/trackpad' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trackpad = "/input/trackpad";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/trackpad/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trackpadClick = "/input/trackpad/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/trackpad/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trackpadTouch = "/input/trackpad/touch";
        /// <summary>
        /// Constant for a pose interaction binding '.../input/grip/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string grip = "/input/grip/pose";
        /// <summary>
        /// Constant for a pose interaction binding '.../input/aim/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string aim = "/input/aim/pose";
        /// <summary>
        /// Constant for a haptic interaction binding '.../output/haptic' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string haptic = "/output/haptic";

        private const string kDeviceLocalizedName = "Windows MR Controller OpenXR";

        /// <summary>
        /// Registers the <see cref="WMRSpatialController"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForEditorPlayMode())
                return;
#endif
            InputSystem.InputSystem.RegisterLayout(typeof(WMRSpatialController),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="WMRSpatialController"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForEditorPlayMode())
                return;
#endif
            InputSystem.InputSystem.RemoveLayout(nameof(WMRSpatialController));
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "microsoftmotioncontroller",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "Microsoft",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
            {
                new DeviceConfig()
                {
                    characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left),
                    userPath = UserPaths.leftHand
                },
                new DeviceConfig()
                {
                    characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right),
                    userPath = UserPaths.rightHand
                }
            },
                actions = new List<ActionConfig>()
            {
                // Joystick
                new ActionConfig()
                {
                    name = "joystick",
                    localizedName = "Joystick",
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
                // Touchpad
                new ActionConfig()
                {
                    name = "touchpad",
                    localizedName = "Touchpad",
                    type = ActionType.Axis2D,
                    usages = new List<string>()
                    {
                        "Secondary2DAxis"
                    },
                    bindings = new List<ActionBinding>()
                    {
                        new ActionBinding()
                        {
                            interactionPath = trackpad,
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
                    name = "gripPressed",
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
                    name = "triggerPressed",
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
                //Joystick Clicked
                new ActionConfig()
                {
                    name = "joystickClicked",
                    localizedName = "JoystickClicked",
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
                //Touchpad Clicked
                new ActionConfig()
                {
                    name = "touchpadClicked",
                    localizedName = "Touchpad Clicked",
                    type = ActionType.Binary,
                    usages = new List<string>()
                    {
                        "Secondary2DAxisClick"
                    },
                    bindings = new List<ActionBinding>()
                    {
                        new ActionBinding()
                        {
                            interactionPath = trackpadClick,
                            interactionProfileName = profile,
                        }
                    }
                },
                //Touchpad Touched
                new ActionConfig()
                {
                    name = "touchpadTouched",
                    localizedName = "Touchpad Touched",
                    type = ActionType.Binary,
                    usages = new List<string>()
                    {
                        "Secondary2DAxisTouch"
                    },
                    bindings = new List<ActionBinding>()
                    {
                        new ActionBinding()
                        {
                            interactionPath = trackpadTouch,
                            interactionProfileName = profile,
                        }
                    }
                },
                // Device Pose
                new ActionConfig()
                {
                    name = "devicePose",
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
                    name = "haptic",
                    localizedName = "Haptic Output",
                    type = ActionType.Vibrate,
                    usages = new List<string>() { "Haptic" },
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
    }
}
