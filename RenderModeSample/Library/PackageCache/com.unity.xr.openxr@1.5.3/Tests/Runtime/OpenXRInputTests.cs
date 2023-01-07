using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.TestTools;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.ConformanceAutomation;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class OpenXRInputTestsBase : OpenXRLoaderSetup
    {
        private static readonly List<XRNodeState> s_NodeStates = new List<XRNodeState>();

        protected static bool IsNodeTracked(XRNode node)
        {
            s_NodeStates.Clear();
            InputTracking.GetNodeStates(s_NodeStates);
            return s_NodeStates.Where(s => s.nodeType == node).Select(s => s.tracked).FirstOrDefault();
        }

        /// <summary>
        /// List of all known interaction features and their associated devices for testing
        /// </summary>
        protected static readonly (Type featureType, Type layoutType, string layoutNameOverride) [] s_InteractionFeatureLayouts = {
            (typeof(OculusTouchControllerProfile), typeof(OculusTouchControllerProfile.OculusTouchController), null),
            (typeof(EyeGazeInteraction), typeof(EyeGazeInteraction.EyeGazeDevice), "EyeGaze"),
            (typeof(MicrosoftHandInteraction), typeof(MicrosoftHandInteraction.HoloLensHand), null),
            (typeof(KHRSimpleControllerProfile), typeof(KHRSimpleControllerProfile.KHRSimpleController), null),
#if !UNITY_ANDROID
            (typeof(HTCViveControllerProfile), typeof(HTCViveControllerProfile.ViveController), null),
            (typeof(MicrosoftMotionControllerProfile), typeof(MicrosoftMotionControllerProfile.WMRSpatialController), null),
            (typeof(ValveIndexControllerProfile), typeof(ValveIndexControllerProfile.ValveIndexController), null)
#endif
        };

        /// <summary>
        /// List of interaction features that should not be tested.
        /// </summary>
        protected static readonly Type[] s_IgnoreInteractionFeatures = {
            typeof(MockInteractionFeature)
        };

        /// <summary>
        /// Return true if the given layout is registered.
        /// </summary>
        /// <param name="layoutName">Name of the layout</param>
        /// <returns>True if the layout is registered with the input system</returns>
        protected static bool IsLayoutRegistered(string layoutName)
        {
            // Force an input system update first to make sure all registrations are committed.
            InputSystem.InputSystem.Update();

            try
            {
                return InputSystem.InputSystem.LoadLayout(layoutName) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }

    internal class OpenXRInputTests : OpenXRInputTestsBase
    {
        protected override void QueryBuildFeatures(List<Type> featureTypes)
        {
            base.QueryBuildFeatures(featureTypes);
            featureTypes.AddRange(s_InteractionFeatureLayouts.Select(i => i.featureType));
            featureTypes.Add(typeof(MockInteractionFeature));
            featureTypes.Add(typeof(ConformanceAutomationFeature));
        }

        /// <summary>
        /// Tests whether or not the device layout for an interaction feature is registered at runtime
        /// </summary>
        [UnityTest]
        public IEnumerator DeviceLayoutRegistration([ValueSource(nameof(s_InteractionFeatureLayouts))] (Type featureType, Type layoutType, string layoutNameOverride) interactionFeature)
        {
            var layoutName = interactionFeature.layoutNameOverride ?? interactionFeature.layoutType.Name;

            // Make sure the layout is not registered as it would give the test a false positive
            InputSystem.InputSystem.RemoveLayout(layoutName);
            Assert.IsFalse(IsLayoutRegistered(layoutName), "Layout is still registered, test will give a false positive");

            // Starting OpenXR should register all layouts from interaction features.  Make sure that the
            // layout is registered after starting.
            EnableFeature(interactionFeature.featureType);
            InitializeAndStart();
            yield return new WaitForXrFrame(2);
            Assert.IsTrue(IsLayoutRegistered(layoutName), "Layout was not registered during Initialization");
        }

        /// <summary>
        /// Validate that data flows through the given OpenXR interaction path to the give action.
        /// </summary>
        /// <param name="inputAction">Input action that should receive the data</param>
        /// <param name="userPath">OpenXR User Path</param>
        /// <param name="interactionPath">OpenXR interaction path</param>
        /// <param name="value">Value to verify</param>
        /// <returns></returns>
        private static IEnumerator ValidateInputAction (InputAction inputAction, string userPath, string interactionPath, bool value)
        {
            ConformanceAutomationFeature.ConformanceAutomationSetBool(userPath, interactionPath, value);
            yield return new WaitForXrFrame(2);
            var actualValue = inputAction.ReadValue<float>() > 0.0f;
            Assert.IsTrue(actualValue==value, $"Expected '{value}' but received '{actualValue}' from '{inputAction}' bound to '{interactionPath}'");
        }

        /// <summary>
        /// Validate that data flows through the given OpenXR interaction path to the give action.
        /// </summary>
        /// <param name="inputAction">Input action that should receive the data</param>
        /// <param name="userPath">OpenXR User Path</param>
        /// <param name="interactionPath">OpenXR interaction path</param>
        /// <param name="value">Value to verify</param>
        /// <returns></returns>
        private static IEnumerator ValidateInputAction (InputAction inputAction, string userPath, string interactionPath, float value)
        {
            ConformanceAutomationFeature.ConformanceAutomationSetFloat(userPath, interactionPath, value);
            yield return new WaitForXrFrame(2);
            var actualValue = inputAction.ReadValue<float>();
            Assert.IsTrue(actualValue >= value - float.Epsilon && actualValue <= value + float.Epsilon, $"Expected '{value}' but received '{actualValue}' from '{inputAction}' bound to '{interactionPath}'");
        }

        /// <summary>
        /// Validate that data flows through the given OpenXR interaction path to the give action.
        /// </summary>
        /// <param name="inputAction">Input action that should receive the data</param>
        /// <param name="userPath">OpenXR User Path</param>
        /// <param name="interactionPath">OpenXR interaction path</param>
        /// <param name="value">Value to verify</param>
        /// <returns></returns>
        private static IEnumerator ValidateInputAction (InputAction inputAction, string userPath, string interactionPath, Vector2 value)
        {
            ConformanceAutomationFeature.ConformanceAutomationSetVec2(userPath, interactionPath, value);
            yield return new WaitForXrFrame(2);
            var actualValue = inputAction.ReadValue<Vector2>();
            Assert.IsTrue(
                actualValue.x >= value.x - float.Epsilon && actualValue.x <= value.x + float.Epsilon &&
                actualValue.y >= value.y - float.Epsilon && actualValue.y <= value.y + float.Epsilon,
                $"Expected '{value}' but received '{actualValue}' from '{inputAction}' bound to '{interactionPath}'"
                );
        }

        /// <summary>
        /// Validate that data flows through the given OpenXR interaction path to the give action.
        /// </summary>
        /// <param name="inputAction">Input action that should receive the data</param>
        /// <param name="userPath">OpenXR User Path</param>
        /// <param name="interactionPath">OpenXR interaction path</param>
        /// <param name="expected">Value to verify</param>
        /// <returns></returns>
        private static IEnumerator ValidateInputAction (InputAction inputAction, string userPath, string interactionPath, OpenXR.Input.Pose expected)
        {
            ConformanceAutomationFeature.ConformanceAutomationSetPose(userPath, interactionPath, expected.position, expected.rotation);
            ConformanceAutomationFeature.ConformanceAutomationSetVelocity(
                userPath,
                interactionPath,
                ((expected.trackingState & InputTrackingState.Velocity) == InputTrackingState.Velocity),
                expected.velocity,
                ((expected.trackingState & InputTrackingState.AngularVelocity) == InputTrackingState.AngularVelocity),
                expected.angularVelocity);
            ConformanceAutomationFeature.ConformanceAutomationSetActive(null, userPath, expected.isTracked);
            yield return new WaitForXrFrame(2);

            switch (inputAction.expectedControlType)
            {
                case "Vector3":
                {
                    var received = inputAction.ReadValue<Vector3>();
                    Assert.IsTrue(received == expected.position, $"Action '{inputAction.bindings[0].path}' bound to '{interactionPath}' expected '{expected.position} but received '{received}'");
                    break;
                }

                case "Quaternion":
                {
                    var received = inputAction.ReadValue<Quaternion>();
                    Assert.IsTrue(received == expected.rotation, $"Action '{inputAction.bindings[0].path}' bound to '{interactionPath}' expected '{expected.rotation}' but received '{received}'");
                    break;
                }

                case "Button":
                {
                    var received = inputAction.ReadValue<float>() > 0.0f;
                    Assert.IsTrue(received == expected.isTracked, $"Action '{inputAction.bindings[0].path}' bound to '{interactionPath}' expected '{expected.isTracked}' but received '{received}'");
                    break;
                }

                case "Integer":
                {
                    var received = inputAction.ReadValue<int>();
                    Assert.IsTrue(received == (int)expected.trackingState, $"Action '{inputAction.bindings[0].path}' bound to '{interactionPath}' expected '{expected.trackingState}' but received '{(InputTrackingState)received}'");
                    break;
                }

                case "Pose":
                {
                    var received = inputAction.ReadValue<Input.Pose>();
                    Assert.IsTrue(received.isTracked == expected.isTracked, $"Action '{inputAction.bindings[0].path}/isTracked' bound to '{interactionPath}' expected '{expected.isTracked}' but received '{received.isTracked}'");
                    Assert.IsTrue(received.trackingState == expected.trackingState, $"Action '{inputAction.bindings[0].path}/trackingState' bound to '{interactionPath}' expected '{expected.trackingState}' but received '{received.trackingState}'");

                    if (received.isTracked)
                    {
                        Assert.IsTrue(received.position == expected.position, $"Action '{inputAction.bindings[0].path}/position' bound to '{interactionPath}' expected '{expected.position}' but received '{received.position}'");
                        Assert.IsTrue(received.rotation == expected.rotation, $"Action '{inputAction.bindings[0].path}/rotation' bound to '{interactionPath}' expected '{expected.rotation}' but received '{received.rotation}'");

                        if((received.trackingState & InputTrackingState.Velocity ) == InputTrackingState.Velocity)
                            Assert.IsTrue(received.velocity == expected.velocity, $"Action '{inputAction.bindings[0].path}/position' bound to '{interactionPath}' expected '{expected.velocity}' but received '{received.velocity}'");
                        else
                            Assert.IsTrue(received.velocity == Vector3.zero, $"Action '{inputAction.bindings[0].path}/position' bound to '{interactionPath}' expected '{Vector3.zero}' but received '{received.velocity}'");

                        if((received.trackingState & InputTrackingState.AngularVelocity) == InputTrackingState.AngularVelocity)
                            Assert.IsTrue(received.angularVelocity == expected.angularVelocity, $"Action '{inputAction.bindings[0].path}/position' bound to '{interactionPath}' expected '{expected.angularVelocity}' but received '{received.angularVelocity}'");
                        else
                            Assert.IsTrue(received.angularVelocity == Vector3.zero, $"Action '{inputAction.bindings[0].path}/position' bound to '{interactionPath}' expected '{Vector3.zero}' but received '{received.angularVelocity}'");
                    }
                    break;
                }
            }

            yield return null;
        }

        /// <summary>
        /// Validate that the haptic associated with an input action fires
        /// </summary>
        /// <param name="inputAction">Input action</param>
        /// <param name="amplitude">Amplitude for haptic</param>
        /// <param name="duration">Duration for haptic</param>
        /// <param name="inputDevice">Device to filter with</param>
        private static IEnumerator ValidateHaptic(InputAction inputAction, float amplitude, float duration, InputSystem.InputDevice inputDevice = null)
        {
            var hapticImpulseCount = 0;
            var hapticStopCount = 0;
            void OnHapticOutput(MockRuntime.ScriptEvent evt, ulong param)
            {
                hapticImpulseCount += evt == MockRuntime.ScriptEvent.HapticImpulse ? 1 : 0;
                hapticStopCount += evt == MockRuntime.ScriptEvent.HapticStop ? 1 : 0;
            }

            MockRuntime.onScriptEvent += OnHapticOutput;

            if (null == inputAction)
            {
                Assert.IsNotNull(inputDevice);

                if (inputDevice is XRControllerWithRumble rumble)
                    rumble.SendImpulse(amplitude, duration);
            }
            else
                OpenXRInput.SendHapticImpulse(inputAction, amplitude, duration, inputDevice);

            // Give some time for the haptic event to make its way to our callback
            yield return new WaitForXrFrame(2);

            if (null != inputAction)
                OpenXRInput.StopHaptics(inputAction, inputDevice);

            yield return new WaitForXrFrame(2);

            MockRuntime.onScriptEvent -= OnHapticOutput;

            Assert.IsTrue(hapticImpulseCount == 1, null == inputAction ?
                $"Haptic impulse failed for XRControllerWithRumble '{inputDevice.name}" :
                $"Haptic impulse failed for action '{inputAction}'");

            Assert.IsTrue(inputAction == null || hapticStopCount == 1, $"Haptic stop failed for action '{inputAction}'");
        }

        /// <summary>
        /// Validate that data flows from OpenXR to the InputSystem through the given OpenXR interaction path to
        /// the given input ControlItem
        /// </summary>
        /// <param name="localizedActionMapName">Device layout name to validate</param>
        /// <param name="control">Control within the device layout to validate</param>
        /// <param name="userPath">OpenXR User path to bind to</param>
        /// <param name="interactionPath">OpenXR interaction path to bind to</param>
        /// <param name="controlLayoutOverride">Optional override for the control layout</param>
        /// <param name="usageOverride">Optional usage override for the binding</param>
        /// <returns></returns>
        private static IEnumerator ValidateLayoutControl(InputControlLayout layout, InputControlLayout.ControlItem control, string userPath, string interactionPath, string controlLayoutOverride = null, string usageOverride = null)
        {
            // Convert the user path to a usage to limit the bound action
            var usage = userPath switch
            {
                "/user/hand/left" => "{LeftHand}",
                "/user/hand/right" => "{RightHand}",
                _ => ""
            };

            // Create an action bound to the control
            var action = new InputAction(
                null,
                InputActionType.Value,
                $"<{layout.name}>{usage}/{(usageOverride != null ? $"{{{usageOverride}}}" : control.name)}",
                null,
                null,
                control.layout);

            action.Enable();

            // Make sure the input system updates and wait a frame to ensure the action is properly bound before testing with it
            InputSystem.InputSystem.Update();
            yield return new WaitForXrFrame(1);

            // Use the usage to find the device for the action
            var inputDevice  = !string.IsNullOrEmpty(usage) ?
                InputSystem.InputSystem.GetDevice<InputSystem.InputDevice>(usage.Substring(1,usage.Length-2)) :
                null;

            // Check input TryGetInputSourceName
            Assert.IsTrue(
                OpenXRInput.TryGetInputSourceName(action, 0, out var actionName, OpenXRInput.InputSourceNameFlags.All, inputDevice),
                $"Failed to retrieve input source for action '{action}'.");
            Assert.IsNotEmpty(actionName, $"Input source name for action '{action}' should not be empty");

            switch (controlLayoutOverride ?? control.layout)
            {
                case "Button":
                {
                    yield return ValidateInputAction(action, userPath, interactionPath, true);
                    yield return ValidateInputAction(action, userPath, interactionPath, false);
                    break;
                }

                case "Axis":
                {
                    yield return ValidateInputAction(action, userPath, interactionPath, 1.0f);
                    yield return ValidateInputAction(action, userPath, interactionPath, 0.0f);
                    // TODO: Disabled this because the Microsoft Motion Controller and the HTC Vive controller specify Axis1D controls that are not actually 1DAxis controls
                    //yield return ValidateInputAction(action, userPath, interactionPath, 0.5f);
                    break;
                }

                case "Vector2":
                {
                    yield return ValidateInputAction(action, userPath, interactionPath, Vector2.one);
                    yield return ValidateInputAction(action, userPath, interactionPath, Vector2.zero);
                    yield return ValidateInputAction(action, userPath, interactionPath, new Vector2(1.0f,0.0f));
                    yield return ValidateInputAction(action, userPath, interactionPath, new Vector2(0.0f,1.0f));
                    break;
                }

                case "Pose":
                {
                    yield return ValidateInputAction(action, userPath, interactionPath, new Input.Pose
                    {
                        position = Vector3.one,
                        rotation = Quaternion.identity,
                        isTracked = true,
                        trackingState = InputTrackingState.Position | InputTrackingState.Rotation
                    });

                    yield return ValidateInputAction(action, userPath, interactionPath, new Input.Pose
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity,
                        isTracked = false,
                        trackingState = InputTrackingState.None
                    });

                    yield return ValidateInputAction(action, userPath, interactionPath, new Input.Pose
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.Euler(90,0,0),
                        isTracked = true,
                        trackingState = InputTrackingState.Position | InputTrackingState.Rotation
                    });

                    // Velocity only
                    yield return ValidateInputAction(action, userPath, interactionPath, new Input.Pose
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity,
                        isTracked = true,
                        trackingState = InputTrackingState.Position | InputTrackingState.Rotation | InputTrackingState.Velocity,
                        velocity = new Vector3(1,2,3)
                    });

                    // AngularVelocity only
                    yield return ValidateInputAction(action, userPath, interactionPath, new Input.Pose
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.Euler(90,0,0),
                        isTracked = true,
                        trackingState = InputTrackingState.Position | InputTrackingState.Rotation | InputTrackingState.AngularVelocity,
                        angularVelocity = new Vector3(1,2,3)
                    });

                    // Velocity and AngularVelocity
                    yield return ValidateInputAction(action, userPath, interactionPath, new Input.Pose
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.Euler(90,0,0),
                        isTracked = true,
                        trackingState = InputTrackingState.Position | InputTrackingState.Rotation | InputTrackingState.Velocity | InputTrackingState.AngularVelocity,
                        velocity = new Vector3(1,2,3),
                        angularVelocity = new Vector3(3,2,1)
                    });

                    break;
                }

                case "Haptic":
                {
                    // Validate haptics through the action
                    yield return ValidateHaptic(action, 1.0f, 1.0f, inputDevice);

                    // Validate haptics through a rumble controller
                    if (inputDevice is XRControllerWithRumble)
                        yield return ValidateHaptic(null, 1.0f, 1.0f, inputDevice);

                    break;
                }

                default:
                    Assert.Fail($"Unknown control type `{control.layout}`");
                    break;
            }
        }

        /// <summary>
        /// Tests all controls of all interaction features to ensure data flows through properly.
        /// </summary>
        [UnityTest]
        public IEnumerator ValidateControls ([ValueSource(nameof(s_InteractionFeatureLayouts))] (Type featureType, Type layoutType, string layoutNameOverride) interactionFeature)
        {
            // Enable the needed features
            EnableMockRuntime();
            EnableFeature<ConformanceAutomationFeature>();
            var feature = EnableFeature(interactionFeature.featureType) as OpenXRInteractionFeature;

            // Make sure all the devices are registered with the input system
            InputSystem.InputSystem.Update();

            var actionMaps = new List<OpenXRInteractionFeature.ActionMapConfig>();
            feature.CreateActionMaps(actionMaps);

            base.InitializeAndStart();
            yield return new WaitForXrFrame(2);

            var layoutName = interactionFeature.layoutNameOverride ?? interactionFeature.layoutType.Name;
            var layout = InputSystem.InputSystem.LoadLayout(layoutName);
            Assert.IsNotNull(layout, $"Missing layout '{layoutName}'");

            // Get list of all known user paths supported by this action map
            var userPaths = actionMaps.SelectMany(m => m.deviceInfos.Select(d => d.userPath)).Distinct().ToList();

            var actionMapCoverage = new HashSet<OpenXRInteractionFeature.ActionConfig>();

            foreach (var control in layout.controls)
            {
                // Find the ActionConfig that matches the given control name
                var actionConfigs = actionMaps.SelectMany(m => m.actions).Where(a => a.name == control.name).ToArray();

                // Control should not be specified in more than one action map config.  If there is a future reason for this then this
                // test will need to be extended to accomodate that.
                Assert.IsTrue(actionConfigs.Length < 2, $"Control '{control.name}' with type '{control.layout}' is specified in more than one ActionConfig");

                var actionConfig = actionConfigs.Length == 1 ? actionConfigs[0] : null;

                // Controls with offsets that are not-zero should not be in the action config as they are "virtual" controls.
                if (control.offset != uint.MaxValue && control.offset != 0)
                {
                    // Any controls with offsets should not be in the ActionConfig
                    Assert.IsNull(actionConfig, $"Control '{control.name}' with type '{control.layout}' has offset and should not be included in the ActionMapConfig");

                    foreach (var userPath in userPaths)
                    {
                        switch (control.name)
                        {
                            case "isTracked":
                            case "trackingState":
                            case "devicePosition":
                            case "deviceRotation":
                                yield return ValidateLayoutControl(layout, control, userPath, $"{userPath}/input/grip/pose", "Pose");
                                break;

                            case "pointerPosition":
                            case "pointerRotation":
                                yield return ValidateLayoutControl(layout, control, userPath, $"{userPath}/input/aim/pose", "Pose");
                                break;

                            default:
                                Assert.Fail($"Unknown control '{control.name}' with non-zero offset");
                                break;
                        }
                    }

                    continue;
                }

                // Control must be in the action map config if it does not have a non-zero offset
                Assert.IsNotNull(actionConfig, $"Control '{control.name}' with type '{control.layout}' is missing from the ActionMapConfig");

                Assert.IsTrue(
                    actionConfig.usages.Count == control.usages.Count &&
                    actionConfig.usages.Intersect(control.usages.Select(u => u.ToString())).Count() == actionConfig.usages.Count,
                    $"ActionConfig usage list for control `{control.name}` does not match ControlItem usage list");

                actionMapCoverage.Add(actionConfig);

                foreach (var binding in actionConfig.bindings)
                foreach (var userPath in (binding.userPaths ?? userPaths))
                {
                    yield return ValidateLayoutControl(layout, control, userPath, $"{userPath}{binding.interactionPath}");

                    // Ensure the usages all map correctly to the data as well
                    foreach (var usage in actionConfig.usages)
                    {
                        yield return ValidateLayoutControl(layout, control, userPath, $"{userPath}{binding.interactionPath}", null, usage);
                    }
                }
            }

            // Make sure that there are no action maps that reference controls that were not paired up
            foreach (var actionConfig in actionMaps.SelectMany(m => m.actions))
            {
                Assert.IsTrue(actionMapCoverage.Contains(actionConfig), $"Action config '{actionConfig.name}' does not have a matching control in the parent layout");
            }
        }

        private static readonly Regex k_ErrorNoDevices = new Regex("ActionMapConfig contains no `deviceInfos`.*");
        private static readonly Regex k_ErrorInvalidDeviceName = new Regex(@".*Invalid device name.*");
        private static readonly Regex k_ErrorInvalidInteractionProfile = new Regex(@".*Invalid interaction profile.*");
        private static readonly Regex k_ErrorInvalidUserPath = new Regex(@".*Invalid user path.*");
        private static readonly Regex k_ErrorInvalidUsage = new Regex(@".*Invalid Usage.*");
        private static readonly Regex k_ErrorInvalidActionSetName = new Regex(@".*Invalid ActionSet name.*");
        private static readonly Regex k_ErrorInvalidActionType = new Regex(@".*Invalid action type \'\d*' for action '.*'");

        private static readonly (Action<OpenXRInteractionFeature.ActionMapConfig> filter, Regex expectLog, Regex expectReport)[] s_ActionMapTests =
        {
            // One or more device infos must be specified
            ((c) => c.deviceInfos = null, k_ErrorNoDevices, null),
            ((c) => c.deviceInfos = new List<OpenXRInteractionFeature.DeviceConfig>(), k_ErrorNoDevices, null),

            // Desired interaction profile must be specified and be a valid path
            ((c) => c.desiredInteractionProfile = "", k_ErrorInvalidInteractionProfile, k_ErrorInvalidInteractionProfile),
            ((c) => c.desiredInteractionProfile = "bad", k_ErrorInvalidInteractionProfile, k_ErrorInvalidInteractionProfile),
            ((c) => c.desiredInteractionProfile = new String('a', 500), k_ErrorInvalidInteractionProfile, k_ErrorInvalidInteractionProfile),

            // Device user path must be specified and be a valid path
            ((c) => c.deviceInfos[0].userPath = null, k_ErrorInvalidUserPath, k_ErrorInvalidUserPath),
            ((c) => c.deviceInfos[0].userPath = "", k_ErrorInvalidUserPath, k_ErrorInvalidUserPath),
            ((c) => c.deviceInfos[0].userPath = "bad", k_ErrorInvalidUserPath, k_ErrorInvalidUserPath),
            ((c) => c.deviceInfos[0].userPath = "/user/" + new String('a', 500), k_ErrorInvalidUserPath, k_ErrorInvalidUserPath),

            // Name must be valid
            ((c) => c.name = null, k_ErrorInvalidActionSetName, k_ErrorInvalidActionSetName),
            ((c) => c.name = "", k_ErrorInvalidActionSetName, k_ErrorInvalidActionSetName),
            ((c) => c.name = new String('a', 500), k_ErrorInvalidActionSetName, k_ErrorInvalidActionSetName),

            // Localized name
            ((c) => c.localizedName = null, k_ErrorInvalidDeviceName, k_ErrorInvalidDeviceName),
            ((c) => c.localizedName = "", k_ErrorInvalidDeviceName, k_ErrorInvalidDeviceName),
            ((c) => c.localizedName = new String('a', 500), k_ErrorInvalidDeviceName, k_ErrorInvalidDeviceName),

            // Manufacturer or serial number should be allowed to be null or empty
            ((c) => c.manufacturer = "", null, null),
            ((c) => c.manufacturer = null, null, null),
            ((c) => c.serialNumber = "", null, null),
            ((c) => c.serialNumber = null, null, null),

            // Invalid action type
            ((c) => c.actions[0].type = (OpenXRInteractionFeature.ActionType)100, k_ErrorInvalidActionType, k_ErrorInvalidActionType),

            // Action Usages
            ((c) => c.actions[0].usages = new List<string> {""}, k_ErrorInvalidUsage, k_ErrorInvalidUsage),
            ((c) => c.actions[0].usages = new List<string> {null}, k_ErrorInvalidUsage, k_ErrorInvalidUsage),
            ((c) => c.actions[0].usages = new List<string> {new string('a', 500)}, k_ErrorInvalidUsage, k_ErrorInvalidUsage),

            // Invalid user path on binding
            ((c) => c.actions[0].bindings[0].userPaths = new List<string> {"bad", "bad"}, k_ErrorInvalidUserPath, k_ErrorInvalidUserPath),
            ((c) => c.actions[0].bindings[0].userPaths = new List<string> {null, null}, k_ErrorInvalidUserPath, k_ErrorInvalidUserPath),
            ((c) => c.actions[0].bindings[0].userPaths = new List<string> {"/" + new string('a', 500)}, k_ErrorInvalidUserPath, k_ErrorInvalidUserPath),

            // Invalid interaction profile on bindings
            ((c) => c.actions[0].bindings[0].interactionProfileName = null, null, null),
            ((c) => c.actions[0].bindings[0].interactionProfileName = "", k_ErrorInvalidInteractionProfile, k_ErrorInvalidInteractionProfile),
            ((c) => c.actions[0].bindings[0].interactionProfileName = "/" + new string('a', 500), k_ErrorInvalidInteractionProfile, k_ErrorInvalidInteractionProfile),
            ((c) => c.actions[0].bindings[0].interactionProfileName = "bad", k_ErrorInvalidInteractionProfile, k_ErrorInvalidInteractionProfile),
        };

        [UnityTest]
        public IEnumerator ValidateActionMapConfig([ValueSource(nameof(s_ActionMapTests))] (Action<OpenXRInteractionFeature.ActionMapConfig> filter, Regex expectLog, Regex expectReport) test)
        {
            var feature = EnableFeature<MockInteractionFeature>();

            // Set an action map config for the feature that has a bad interaction profile
            var actionMapConfig = feature.CreateDefaultActionMapConfig();
            test.filter(actionMapConfig);
            feature.actionMapConfig = actionMapConfig;

            InitializeAndStart();

            yield return new WaitForXrFrame(2);

            if (test.expectLog != null)
                LogAssert.Expect(LogType.Error, test.expectLog);

            if (test.expectReport != null)
                Assert.IsTrue(DoesDiagnosticReportContain(test.expectReport), "Missing report entry");
        }

        /// <summary>
        /// Defines a list of OpenXR API methods to test failure with
        /// </summary>
        private static readonly (string function, XrResult result, Regex expectLog)[] s_RuntimeFailureTests =
        {
            ("xrSuggestInteractionProfileBindings", XrResult.FeatureUnsupported, new Regex(@".*Failed to suggest bindings for interaction profile.*XR_ERROR_FEATURE_UNSUPPORTED.*")),
            ("xrCreateActionSet", XrResult.FeatureUnsupported, new Regex(@".*Failed to create ActionSet.*XR_ERROR_FEATURE_UNSUPPORTED.*")),
            ("xrCreateAction", XrResult.FeatureUnsupported, new Regex(@".*Failed to create Action.*XR_ERROR_FEATURE_UNSUPPORTED.*")),
            ("xrAttachSessionActionSets", XrResult.FeatureUnsupported, new Regex(@".*Failed to attach ActionSets.*XR_ERROR_FEATURE_UNSUPPORTED.*")),
        };

        /// <summary>
        /// Test a failure in suggested bindings.
        /// </summary>
        [UnityTest]
        public IEnumerator RuntimeMethodFailure([ValueSource(nameof(s_RuntimeFailureTests))] (string function, XrResult result, Regex expectLog) test)
        {
            MockRuntime.SetFunctionCallback(test.function, (name) => test.result);

            EnableFeature<OculusTouchControllerProfile>();
            InitializeAndStart();
            yield return new WaitForXrFrame(1);
            StopAndShutdown();
            LogAssert.Expect(LogType.Error, test.expectLog);
            Assert.IsTrue(DoesDiagnosticReportContain(test.expectLog));
        }

        /// <summary>
        /// Ensures that the `interactionFeatureLayouts` list is not missing any entries
        /// </summary>
        [Test]
        public void AllInteractionFeaturesCovered()
        {
            // Array of all known interaction features
            var knownInteractionFeatures = OpenXRSettings.Instance.GetFeatures<OpenXRInteractionFeature>()
                .Select(f => f.GetType())
                .Where(f => !s_IgnoreInteractionFeatures.Contains(f))
                .ToArray();

            // Array of interaction features being tested
            var testedFeatures = s_InteractionFeatureLayouts.Select(l => l.featureType).ToArray();

            // Make sure the two arrays are equal
            Assert.IsTrue(knownInteractionFeatures.Length == testedFeatures.Length && knownInteractionFeatures.Intersect(testedFeatures).Count() == knownInteractionFeatures.Length,
                "One or more interaction features has not been added to the testable interaction feature list.");
        }

        /// <summary>
        /// Ensures that EyeGaze isTracked, position, rotation features map correctly to action handles.
        /// (Since the EyeGaze features use pose instead of devicePose)
        /// </summary>
        [UnityTest]
        public IEnumerator EyeGazeFeatureTest()
        {
            EnableFeature<EyeGazeInteraction>();
            InitializeAndStart();
            yield return new WaitForXrFrame(1);

            InputAction inputAction = new InputAction(null, InputActionType.Value, "<XRInputV1::EyeTrackingOpenXR>/pose/isTracked");
            InputControl control = inputAction.controls[0];

            var isTrackedHandle = OpenXRInput.GetActionHandle(new InputAction(null, InputActionType.Value, "<XRInputV1::EyeTrackingOpenXR>/pose/isTracked"));
            Assert.IsTrue(isTrackedHandle != 0);
            var positionHandle = OpenXRInput.GetActionHandle(new InputAction(null, InputActionType.Value, "<XRInputV1::EyeTrackingOpenXR>/pose/position"));
            Assert.IsTrue(positionHandle != 0);
            var rotationHandle = OpenXRInput.GetActionHandle(new InputAction(null, InputActionType.Value, "<XRInputV1::EyeTrackingOpenXR>/pose/rotation"));
            Assert.IsTrue(rotationHandle != 0);
        }

        [UnityTest]
        public IEnumerator InputTrackingAquiredAndLost ()
        {
            EnableFeature<OculusTouchControllerProfile>();

            var tracked = false;
            InputTracking.trackingAcquired += (ns) =>
            {
                if (ns.nodeType == XRNode.LeftHand)
                    tracked = ns.tracked;
            };
            InputTracking.trackingLost += (ns) =>
            {
                if (ns.nodeType == XRNode.LeftHand)
                    tracked = ns.tracked;
            };

            // Node should be untracked before we start
            Assert.IsFalse(IsNodeTracked(XRNode.LeftHand));

            // Node should be tracked after we start
            InitializeAndStart();
            yield return new WaitForXrFrame(1);
            Assert.IsTrue(tracked);

            // Clear the space location flags for the node which should switch to the untracked state
            var gripAction = OpenXRInput.GetActionHandle(new InputAction(null, InputActionType.Value, "<XRInputV1::Oculus::OculusTouchControllerOpenXR>{LeftHand}/devicePose"));
            var aimAction = OpenXRInput.GetActionHandle(new InputAction(null, InputActionType.Value, "<XRInputV1::Oculus::OculusTouchControllerOpenXR>{LeftHand}/pointer"));
            MockRuntime.SetSpace(gripAction, Vector3.zero, Quaternion.identity, XrSpaceLocationFlags.None);
            MockRuntime.SetSpace(aimAction, Vector3.zero, Quaternion.identity, XrSpaceLocationFlags.None);
            yield return new WaitForXrFrame(2);
            Assert.IsFalse(tracked);

            // Reset the space location flags to make sure it goes back to tracked state
            var trackedFlags = XrSpaceLocationFlags.PositionValid | XrSpaceLocationFlags.OrientationValid | XrSpaceLocationFlags.PositionTracked | XrSpaceLocationFlags.OrientationTracked;
            MockRuntime.SetSpace(gripAction, Vector3.zero, Quaternion.identity, trackedFlags);
            MockRuntime.SetSpace(aimAction, Vector3.zero, Quaternion.identity, trackedFlags);
            yield return new WaitForXrFrame(2);
            Assert.IsTrue(tracked);
        }
    }
}
