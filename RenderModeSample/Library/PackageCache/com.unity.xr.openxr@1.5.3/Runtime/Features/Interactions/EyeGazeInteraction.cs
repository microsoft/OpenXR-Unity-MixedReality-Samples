using System.Collections.Generic;
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
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of eye gaze interaction profiles in OpenXR. It enables <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_eye_gaze_interaction">XR_EXT_eye_gaze_interaction</see> in the underlying runtime.
    /// This creates a new <see cref="InputDevice"/> with the <see cref="InputDeviceCharacteristics.EyeTracking"/> characteristic. This new device has both <see cref="EyeTrackingUsages.gazePosition"/> and <see cref="EyeTrackingUsages.gazeRotation"/> input features, as well as <see cref="CommonUsages.isTracked"/> and <see cref="CommonUsages.trackingState"/> usages to determine if the gaze is available.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Eye Gaze Interaction Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.WSA, BuildTargetGroup.Standalone, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Support for enabling the eye tracking interaction profile. Will register the controller map for eye tracking if enabled.",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/eyegazeinteraction.html",
        Version = "0.0.1",
        OpenxrExtensionStrings = extensionString,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class EyeGazeInteraction : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.eyetracking";

        /// <summary>
        /// An Input System device based off the <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_eye_gaze_input">Eye Gaze Interaction Profile</see>. Enabled through <see cref="EyeGazeInteraction"/>.
        /// </summary>
        [Preserve, InputControlLayout(displayName = "Eye Gaze (OpenXR)", isGenericTypeOfDevice = true)]
        public class EyeGazeDevice : OpenXRDevice
        {
            /// <summary>
            /// A <see cref="PoseControl"/> representing the <see cref="EyeGazeInteraction.pose"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, usages = new [] {"Device", "gaze"})]
            public PoseControl pose { get; private set; }

            /// <inheritdoc/>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                pose = GetChildControl<PoseControl>("pose");
            }
        }

        /// <summary>
        /// The OpenXR constant that is used to reference an eye tracking supported input device. See <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#semantic-path-user">OpenXR Specification 6.3.1</see> for more information on user paths.
        /// </summary>
        private const string userPath = "/user/eyes_ext";

        /// <summary>The interaction profile string used to reference the eye gaze input device.</summary>
        private const string profile = "/interaction_profiles/ext/eye_gaze_interaction";

        /// <summary>
        /// Constant for a pose interaction binding '.../input/gaze_ext/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        private const string pose = "/input/gaze_ext/pose";

        private const string kDeviceLocalizedName = "Eye Tracking OpenXR";

        /// <summary>The OpenXR Extension string. This is used by OpenXR to check if this extension is available or enabled. See <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_eye_gaze_interaction">eye gaze interaction extension</see> documentation for more information on this OpenXR extension.</summary>
        public const string extensionString = "XR_EXT_eye_gaze_interaction";

#if UNITY_EDITOR
        protected internal override void GetValidationChecks(List<OpenXRFeature.ValidationRule> results, BuildTargetGroup target)
        {
            if (target == BuildTargetGroup.WSA)
            {
                results.Add( new ValidationRule(this){
                    message = "Eye Gaze support requires the Gaze Input capability.",
                    error = false,
                    checkPredicate = () => PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.GazeInput),
                    fixIt = () => PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.GazeInput, true)
                } );
            }
        }
#endif

        /// <inheritdoc/>
        protected internal override bool OnInstanceCreate(ulong instance)
        {
            // Requires the eye tracking extension
            if(!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            return base.OnInstanceCreate(instance);
        }

        /// <summary>
        /// Registers the <see cref="EyeGazeDevice"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForEditorPlayMode())
                return;
#endif
            InputSystem.InputSystem.RegisterLayout(typeof(EyeGazeDevice),
                        "EyeGaze",
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="EyeGazeDevice"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
#if UNITY_EDITOR
            if (!OpenXRLoaderEnabledForEditorPlayMode())
                return;
#endif
            InputSystem.InputSystem.RemoveLayout("EyeGaze");
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "eyegaze",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.HeadMounted,
                        userPath = userPath
                    }
                },
                actions = new List<ActionConfig>()
                {
                    // Pointer Pose
                    new ActionConfig()
                    {
                        name = "pose",
                        localizedName = "Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device",
                            "gaze"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pose,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }
    }

    /// <summary>
    /// Tags that can be used with <see cref="InputDevice.TryGetFeatureValue"/> to get eye tracking related input features.  See <seealso cref="CommonUsages"/> for additional usages.
    /// </summary>
    public static class EyeTrackingUsages
    {
        /// <summary>The origin position for the gaze. The gaze represents where a user is looking, and <see cref="gazePosition"/> represents the starting location, close to the eyes, from which to project a gaze ray from.</summary>
        public static InputFeatureUsage<Vector3> gazePosition = new InputFeatureUsage<Vector3>("gazePosition");
        /// <summary>The orientation of the gaze, such that the direction of the gaze is the same as <see cref="Vector3.forward "/> * gazeRotation. Use with <see cref="gazePosition"/> to create a gaze ray.</summary>
        public static InputFeatureUsage<Quaternion> gazeRotation = new InputFeatureUsage<Quaternion>("gazeRotation");
    }
}
