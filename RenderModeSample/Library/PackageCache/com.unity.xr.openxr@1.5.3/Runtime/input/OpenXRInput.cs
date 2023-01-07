using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.OpenXR.Features;
using UnityEditor;

namespace UnityEngine.XR.OpenXR.Input
{
    /// <summary>
    /// OpenXR Input related functionality.
    /// </summary>
    public static class OpenXRInput
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct SerializedGuid
        {
            [FieldOffset(0)]
            public Guid guid;
            [FieldOffset(0)]
            public ulong ulong1;
            [FieldOffset(8)]
            public ulong ulong2;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct SerializedBinding
        {
            /// <summary>
            /// Identifier of the action (created with CreateAction) to bind to
            /// </summary>
            public ulong actionId;

            /// <summary>
            /// OpenXR path to bind too (full path including user path)
            /// </summary>
            public string path;
        }

        /// <summary>
        /// Flags used to indicate which parts of the the input source name is being requested from <see cref="OpenXRInput.TryGetInputSourceName"/>
        /// </summary>
        [Flags]
        public enum InputSourceNameFlags
        {
            /// <summary>
            /// Request the localized name of the user path as part of the input source name
            /// </summary>
            UserPath = 1,

            /// <summary>
            /// Request the localized name of the interaction profile as part of the input source name
            /// </summary>
            InteractionProfile = 2,

            /// <summary>
            /// Request the localized name of the component as part of the input source name
            /// </summary>
            Component = 4,

            /// <summary>
            /// Request all components
            /// </summary>
            All = UserPath | InteractionProfile | Component
        }

        /// <summary>
        /// Dictionary that provides a conversion between InputSystem.ExpectedControlType to OpenXRInteractionFeature.ActionType
        /// </summary>
        private static readonly Dictionary<string, OpenXRInteractionFeature.ActionType> ExpectedControlTypeToActionType = new Dictionary<string, OpenXRInteractionFeature.ActionType>
        {
            // Binary
            ["Digital"] = OpenXRInteractionFeature.ActionType.Binary,
            ["Button"] = OpenXRInteractionFeature.ActionType.Binary,

            // Axis1D
            ["Axis"] = OpenXRInteractionFeature.ActionType.Axis1D,
            ["Integer"] = OpenXRInteractionFeature.ActionType.Axis1D,
            ["Analog"] = OpenXRInteractionFeature.ActionType.Axis1D,

            // Axis2D
            ["Vector2"] = OpenXRInteractionFeature.ActionType.Axis2D,
            ["Dpad"] = OpenXRInteractionFeature.ActionType.Axis2D,
            ["Stick"] = OpenXRInteractionFeature.ActionType.Axis2D,

            // Pose
            ["Pose"] = OpenXRInteractionFeature.ActionType.Pose,
            ["Vector3"] = OpenXRInteractionFeature.ActionType.Pose,
            ["Quaternion"] = OpenXRInteractionFeature.ActionType.Pose,

            // Haptics
            ["Haptic"] = OpenXRInteractionFeature.ActionType.Vibrate
        };

        private const string s_devicePoseActionName = "devicepose";

        private const string s_pointerActionName = "pointer";

        /// <summary>
        /// Dictionary used to map virtual controls to concrete controls.
        /// </summary>
        private static readonly Dictionary<string, string> kVirtualControlMap = new Dictionary<string, string>
        {
            ["deviceposition"] = s_devicePoseActionName,
            ["devicerotation"] = s_devicePoseActionName,
            ["trackingstate"] = s_devicePoseActionName,
            ["istracked"] = s_devicePoseActionName,
            ["pointerposition"] = s_pointerActionName,
            ["pointerrotation"] = s_pointerActionName
        };

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void RegisterFeatureLayouts()
        {
            static void OnFirstFrame()
            {
                EditorApplication.update -= OnFirstFrame;

                // In the editor we need to make sure the OpenXR layouts get registered even if the user doesn't
                // navigate to the project settings.  The following code will register the base layouts as well
                // as any enabled interaction features.
                RegisterLayouts();
            }

            // LoadAssetFromPath is not supported from within InitializeOnLoad.  To work around this we register
            // an update callback and wait for the first frame before registering our feature layouts.
            EditorApplication.update += OnFirstFrame;
        }
#endif

        internal static void RegisterLayouts()
        {
            InputSystem.InputSystem.RegisterLayout<HapticControl>("Haptic");
            InputSystem.InputSystem.RegisterLayout<PoseControl>("Pose");
            InputSystem.InputSystem.RegisterLayout<OpenXRDevice>();
            InputSystem.InputSystem.RegisterLayout<OpenXRHmd>(matches: new InputDeviceMatcher()
                .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                .WithProduct(@"Head Tracking - OpenXR")
                .WithManufacturer(@"OpenXR"));

            OpenXRInteractionFeature.RegisterLayouts();
        }

        /// <summary>
        /// Validates a given ActionMapConfig to ensure that it is generally set up correctly.
        /// </summary>
        /// <param name="interactionFeature">InteractionFeature the ActionMapConfig belongs to</param>
        /// <param name="actionMapConfig">ActionMapConfig to validate</param>
        /// <returns>True if the action map config is valid</returns>
        private static bool ValidateActionMapConfig(OpenXRInteractionFeature interactionFeature, OpenXRInteractionFeature.ActionMapConfig actionMapConfig)
        {
            var valid = true;
            if (actionMapConfig.deviceInfos == null || actionMapConfig.deviceInfos.Count == 0)
            {
                Debug.LogError($"ActionMapConfig contains no `deviceInfos` in InteractionFeature '{interactionFeature.GetType()}'");
                valid = false;
            }

            if (actionMapConfig.actions == null || actionMapConfig.actions.Count == 0)
            {
                Debug.LogError($"ActionMapConfig contains no `actions` in InteractionFeature '{interactionFeature.GetType()}'");
                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Attach all Unity actions to OpenXR
        /// Note: this should not be called more than once per session
        /// </summary>
        internal static void AttachActionSets()
        {
            var actionMaps = new List<OpenXRInteractionFeature.ActionMapConfig>();
            foreach(var interactionFeature in OpenXRSettings.Instance.features.OfType<OpenXRInteractionFeature>().Where(f => f.enabled))
            {
                var start = actionMaps.Count;
                interactionFeature.CreateActionMaps(actionMaps);

                for (var index = actionMaps.Count - 1; index >= start; index--)
                {
                    if (!ValidateActionMapConfig(interactionFeature, actionMaps[index]))
                        actionMaps.RemoveAt(index);
                }
            }

            foreach (var actionMap in actionMaps)
            {
                foreach (var deviceInfo in actionMap.deviceInfos)
                {
                    var localizedName = actionMap.desiredInteractionProfile == null ? UserPathToDeviceName(deviceInfo.userPath) : actionMap.localizedName;
                    if (0 == Internal_RegisterDeviceDefinition(deviceInfo.userPath, actionMap.desiredInteractionProfile, (uint) deviceInfo.characteristics, localizedName, actionMap.manufacturer, actionMap.serialNumber))
                    {
                        OpenXRRuntime.LogLastError();
                        return;
                    }
                }
            }

            var interactionProfiles = new Dictionary<string,List<SerializedBinding>>();
            foreach (var actionMap in actionMaps)
            {
                string actionMapLocalizedName = SanitizeStringForOpenXRPath(actionMap.localizedName);
                var actionSetId = Internal_CreateActionSet(SanitizeStringForOpenXRPath(actionMap.name), actionMapLocalizedName, new SerializedGuid());
                if (0 == actionSetId)
                {
                    OpenXRRuntime.LogLastError();
                    return;
                }

                // User paths specified in the deviceInfo
                var deviceUserPaths = actionMap.deviceInfos.Select(d => d.userPath).ToList();

                foreach (var action in actionMap.actions)
                {
                    // User paths specified in the bindings
                    var bindingUserPaths = action.bindings.Where(b => b.userPaths != null).SelectMany(b => b.userPaths).Distinct().ToList();

                    // Combination of all user paths
                    var allUserPaths = bindingUserPaths.Union(deviceUserPaths).ToArray();

                    var actionId = Internal_CreateAction(
                        actionSetId,
                        SanitizeStringForOpenXRPath(action.name),
                        action.localizedName,
                        (uint) action.type,
                        new SerializedGuid(),
                        allUserPaths, (uint) allUserPaths.Length,
                        action.usages?.ToArray(), (uint)(action.usages?.Count ?? 0));

                    if (actionId == 0)
                    {
                        OpenXRRuntime.LogLastError();
                        return;
                    }

                    foreach (var binding in action.bindings)
                    {
                        foreach (var userPath in binding.userPaths ?? deviceUserPaths)
                        {
                            var interactionProfile = binding.interactionProfileName ?? actionMap.desiredInteractionProfile;
                            if (!interactionProfiles.TryGetValue(interactionProfile, out var bindings))
                            {
                                bindings = new List<SerializedBinding>();
                                interactionProfiles[interactionProfile] = bindings;
                            }

                            bindings.Add(new SerializedBinding {actionId = actionId, path = userPath + binding.interactionPath});
                        }
                    }
                }
            }

            // Suggest bindings
            foreach(var kv in interactionProfiles)
            {
                if (!Internal_SuggestBindings(kv.Key, kv.Value.ToArray(), (uint) kv.Value.Count))
                    OpenXRRuntime.LogLastError();
            }

            // Attach actions sets to commit all bindings
            if (!Internal_AttachActionSets())
                OpenXRRuntime.LogLastError();
        }

        /// <summary>
        /// Sanitize the given character for use as an OpenXR Path
        /// </summary>
        /// <param name="c">Character to sanitize</param>
        /// <returns>The sanitized character or 0 if the character should be excluded</returns>
        private static char SanitizeCharForOpenXRPath (char c)
        {
            if (char.IsLower(c) || char.IsDigit(c))
                return c;

            if (char.IsUpper(c))
                return char.ToLower(c);

            if (c == '-' || c == '.' || c == '_' || c == '/')
                return c;

            return (char) 0;
        }

        /// <summary>
        /// OpenXR names can only contain certain characters. see https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#well-formed-path-strings
        /// </summary>
        /// <param name="input">the string we'll convert to a valid OpenXR path</param>
        private static string SanitizeStringForOpenXRPath(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // Find the first character that is not sanitized
            var i = 0;
            for (; i < input.Length && SanitizeCharForOpenXRPath(input[i]) == input[i]; ++i);

            // Already clean
            if (i == input.Length)
                return input;

            // Build the rest of the string by sanitizing each character but start with the
            // portion of the string we already know is sanitized
            var sb = new StringBuilder(input, 0, i, input.Length);
            for (; i < input.Length; ++i)
            {
                var c = SanitizeCharForOpenXRPath(input[i]);
                if (c != 0)
                    sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the name of the control's action handle.
        /// </summary>
        /// <param name="control">The input control</param>
        /// <returns>The name of the action handle.</returns>
        private static string GetActionHandleName(InputControl control)
        {
            // Extract the name of the action from the control path.
            // Example: /EyeTrackingOpenXR/pose/isTracked --> action is pose.
            InputControl inputControl = control;
            while (inputControl.parent != null && inputControl.parent.parent != null)
            {
                inputControl = inputControl.parent;
            }

            string controlName = SanitizeStringForOpenXRPath(inputControl.name);
            if (kVirtualControlMap.TryGetValue(controlName, out var virtualControlName))
            {
                return virtualControlName;
            }

            return controlName;
        }

        /// <summary>
        /// Send a haptic impulse using an action reference
        /// </summary>
        /// <param name="actionRef">Action Reference to send haptic impulse through</param>
        /// <param name="amplitude">Amplitude of the impulse [0-1]</param>
        /// <param name="duration">Duration of the impulse [0-]</param>
        /// <param name="inputDevice">Optional device to limit haptic impulse to</param>
        public static void SendHapticImpulse(InputActionReference actionRef, float amplitude, float duration, InputSystem.InputDevice inputDevice = null) =>
            SendHapticImpulse(actionRef, amplitude, 0.0f, duration, inputDevice);

        /// <summary>
        /// Send a haptic impulse using an action reference
        /// </summary>
        /// <param name="actionRef">Action Reference to send haptic impulse through</param>
        /// <param name="amplitude">Amplitude of the impulse [0-1]</param>
        /// <param name="frequency">Frequency of the impulse in hertz (Hz). (Typical frequency values are between 0 and 300Hz) (0 = default).  Note that not all runtimes support frequency.</param>
        /// <param name="duration">Duration of the impulse [0-]</param>
        /// <param name="inputDevice">Optional device to limit haptic impulse to</param>
        public static void SendHapticImpulse(InputActionReference actionRef, float amplitude, float frequency, float duration, InputSystem.InputDevice inputDevice = null) =>
            SendHapticImpulse(actionRef.action, amplitude, frequency, duration, inputDevice);

        /// <summary>
        /// Send a haptic impulse using the given action
        /// </summary>
        /// <param name="action">Action to send haptic impulse through</param>
        /// <param name="amplitude">Amplitude of the impulse [0-1]</param>
        /// <param name="duration">Duration of the impulse [0-]</param>
        /// <param name="inputDevice">Optional device to limit haptic impulse to</param>
        public static void SendHapticImpulse(InputAction action, float amplitude, float duration, InputSystem.InputDevice inputDevice = null) =>
            SendHapticImpulse(action, amplitude, 0.0f, duration, inputDevice);

        /// <summary>
        /// Send a haptic impulse using the given action
        /// </summary>
        /// <param name="action">Action to send haptic impulse through</param>
        /// <param name="amplitude">Amplitude of the impulse [0-1]</param>
        /// <param name="frequency">Frequency of the impulse in hertz (Hz). (Typical frequency values are between 0 and 300Hz) (0 = default).  Note that not all runtimes support frequency.</param>
        /// <param name="duration">Duration of the impulse [0-]</param>
        /// <param name="inputDevice">Optional device to limit haptic impulse to</param>
        public static void SendHapticImpulse(InputAction action, float amplitude, float frequency, float duration, InputSystem.InputDevice inputDevice = null)
        {
            if (action == null)
                return;

            var actionHandle = GetActionHandle(action, inputDevice);
            if (actionHandle == 0)
                return;

            amplitude = Mathf.Clamp(amplitude, 0, 1);
            duration = Mathf.Max(duration, 0);

            Internal_SendHapticImpulse(GetDeviceId(inputDevice), actionHandle, amplitude, frequency, duration);
        }

        /// <summary>
        /// Stop any haptics playing for the given action reference
        /// </summary>
        /// <param name="actionRef">Action reference to stop the haptics on.</param>
        /// <param name="inputDevice">Optional device filter for actions bound to multiple devices.</param>
        public static void StopHaptics (InputActionReference actionRef, InputSystem.InputDevice inputDevice = null)
        {
            if (actionRef == null)
                return;

            StopHaptics(actionRef.action, inputDevice);
        }

        /// <summary>
        /// Stop any haptics playing for the given action
        /// </summary>
        /// <param name="inputAction">Input action to stop haptics for</param>
        /// <param name="inputDevice">Optional device filter for actions bound to multiple defices</param>
        public static void StopHaptics (InputAction inputAction, InputSystem.InputDevice inputDevice = null)
        {
            if (inputAction == null)
                return;

            var actionHandle = GetActionHandle(inputAction, inputDevice);
            if (actionHandle == 0)
                return;

            Internal_StopHaptics(GetDeviceId(inputDevice), actionHandle);
        }

        /// <summary>
        /// Return the name of the input source bound to the given action
        /// </summary>
        /// <param name="inputAction">Input Action</param>
        /// <param name="index">Index of the input source in the case of multiple bindings.</param>
        /// <param name="name">Name of the input source if an input source was found or an empty string if none was found</param>
        /// <param name="flags">Flags that indicate which parts of the input source name are requested.</param>
        /// <param name="inputDevice">Optional input device to limit search to</param>
        /// <returns>True if an input source was found</returns>
        public static bool TryGetInputSourceName (
            InputAction inputAction,
            int index,
            out string name,
            InputSourceNameFlags flags = InputSourceNameFlags.All,
            InputSystem.InputDevice inputDevice = null
            )
        {
            name = "";

            if (index < 0)
                return false;

            var actionHandle = GetActionHandle(inputAction, inputDevice);
            if (actionHandle == 0)
                return false;

            return Internal_TryGetInputSourceName(GetDeviceId(inputDevice), actionHandle, (uint) index, (uint) flags, out name);
        }

        [StructLayout(LayoutKind.Explicit, Size = k_Size)]
        private struct GetInternalDeviceIdCommand : IInputDeviceCommandInfo
        {
            private static FourCC Type => new FourCC('X', 'R', 'D', 'I');
            private const int k_BaseCommandSizeSize = 8;
            private const int k_Size = k_BaseCommandSizeSize + sizeof(uint);

            [FieldOffset(0)] private InputDeviceCommand baseCommand;
            [FieldOffset(8)] public readonly uint deviceId;

            public FourCC typeStatic => Type;

            public static GetInternalDeviceIdCommand Create() =>
                new GetInternalDeviceIdCommand { baseCommand = new InputDeviceCommand(Type, k_Size) };
        }

        /// <summary>
        /// Returns the OpenXR action handle for the given input action
        /// </summary>
        /// <param name="inputAction">Source InputAction</param>
        /// <param name="inputDevice">Optional InputDevice to filter by</param>
        /// <returns>OpenXR handle that is associated with the given InputAction or 0 if not found</returns>
        internal static ulong GetActionHandle(InputAction inputAction, InputSystem.InputDevice inputDevice = null)
        {
            if (inputAction == null || inputAction.controls.Count == 0)
                return 0;

            foreach (var control in inputAction.controls)
            {
                if (inputDevice != null && control.device != inputDevice || control.device == null)
                    continue;

                var deviceId = GetDeviceId(control.device);
                if (deviceId == 0)
                    continue;

                var controlName = GetActionHandleName(control);

                // Populate the action handles list and make sure we dont overflow
                var xrAction = Internal_GetActionId(deviceId, controlName);

                if (xrAction != 0)
                    return xrAction;
            }

            return 0;
        }

        /// <summary>
        /// Return the OpenXR device identifier for the given input device
        /// </summary>
        /// <param name="inputDevice">Input device to return identifier for</param>
        /// <returns>Identifier the OpenXR plugin uses for this device.  If a device could not be found an invalid device id of 0 will be returned</returns>
        private static uint GetDeviceId(InputSystem.InputDevice inputDevice)
        {
            if (inputDevice == null)
                return 0;

            var command = GetInternalDeviceIdCommand.Create();
            var result = inputDevice.ExecuteCommand(ref command);
            return result == 0 ? 0 : command.deviceId;
        }

        /// <summary>
        /// Convert a user path into a device name
        /// </summary>
        /// <param name="userPath">User Path</param>
        /// <returns>Device name that represents the given user path</returns>
        private static string UserPathToDeviceName(string userPath)
        {
            // Build the device name from the user path
            var parts = userPath.Split('/', '_');
            var nameBuilder = new StringBuilder("OXR");
            foreach (var part in parts)
            {
                if (part.Length == 0)
                    continue;

                var sanitizedPart = SanitizeStringForOpenXRPath(part);
                nameBuilder.Append(char.ToUpper(sanitizedPart[0]));
                nameBuilder.Append(sanitizedPart.Substring(1));
            }

            return nameBuilder.ToString();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        private const string Library = "UnityOpenXR";

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_SendHapticImpulse", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Internal_SendHapticImpulse(uint deviceId, ulong actionId, float amplitude, float frequency, float duration);

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_StopHaptics", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Internal_StopHaptics (uint deviceId, ulong actionId);

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_GetActionIdByControl")]
        private static extern ulong Internal_GetActionId(uint deviceId, string name);

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_TryGetInputSourceName", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Internal_TryGetInputSourceNamePtr(uint deviceId, ulong actionId, uint index, uint flags, out IntPtr outName);

        internal static bool Internal_TryGetInputSourceName(uint deviceId, ulong actionId, uint index, uint flags, out string outName)
        {
            if (!Internal_TryGetInputSourceNamePtr(deviceId, actionId, index, flags, out var outNamePtr))
            {
                outName = "";
                return false;
            }

            outName = Marshal.PtrToStringAnsi(outNamePtr);
            return true;
        }

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_RegisterDeviceDefinition", CharSet = CharSet.Ansi)]
        private static extern ulong Internal_RegisterDeviceDefinition(string userPath, string interactionProfile, uint characteristics, string name, string manufacturer, string serialNumber);

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_CreateActionSet", CharSet = CharSet.Ansi)]
        private static extern ulong Internal_CreateActionSet(string name, string localizedName, SerializedGuid guid);

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_CreateAction", CharSet = CharSet.Ansi)]
        private static extern ulong Internal_CreateAction(ulong actionSetId, string name, string localizedName, uint actionType, SerializedGuid guid, string[] userPaths, uint userPathCount, string[] usages, uint usageCount);

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_SuggestBindings", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool Internal_SuggestBindings(string interactionProfile, SerializedBinding[] serializedBindings, uint serializedBindingCount);

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_AttachActionSets", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool Internal_AttachActionSets ();
    }
}
