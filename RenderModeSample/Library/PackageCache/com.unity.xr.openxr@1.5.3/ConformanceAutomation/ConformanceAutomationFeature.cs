using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR.NativeTypes;
#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.ConformanceAutomation
{
    /// <summary>
    /// This OpenXRFeature implements XR_EXT_conformance_automation.
    /// See https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_conformance_automation
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Conformance Automation",
        Hidden = true,
        BuildTargetGroups = new []{UnityEditor.BuildTargetGroup.Standalone, UnityEditor.BuildTargetGroup.Android, UnityEditor.BuildTargetGroup.WSA },
        Company = "Unity",
        Desc = "The XR_EXT_conformance_automation allows conformance test and runtime developers to provide hints to the underlying runtime as to what input the test is expecting. This enables runtime authors to automate the testing of their runtime conformance.",
        DocumentationLink = Constants.k_DocumentationURL,
        OpenxrExtensionStrings = "XR_EXT_conformance_automation",
        Version = "0.0.1",
        FeatureId = featureId)]
#endif
    public class ConformanceAutomationFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.conformance";

        private static ulong xrInstance = 0ul;
        private static ulong xrSession = 0ul;

        /// <inheritdoc/>
        protected internal override bool OnInstanceCreate(ulong instance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled("XR_EXT_conformance_automation"))
            {
                Debug.LogError("XR_EXT_conformance_automation is not enabled. Disabling ConformanceAutomationExt");
                return false;
            }

            xrInstance = instance;
            xrSession = 0ul;

            initialize(xrGetInstanceProcAddr, xrInstance);
            return true;
        }

        /// <inheritdoc/>
        protected internal override void OnInstanceDestroy(ulong xrInstance)
        {
            base.OnInstanceDestroy(xrInstance);
            ConformanceAutomationFeature.xrInstance = 0ul;
        }

        /// <inheritdoc/>
        protected internal override void OnSessionCreate(ulong xrSessionId)
        {
            ConformanceAutomationFeature.xrSession = xrSessionId;
            base.OnSessionCreate(xrSession);
        }

        /// <inheritdoc/>
        protected internal override void OnSessionDestroy(ulong xrSessionId)
        {
            base.OnSessionDestroy(xrSessionId);
            ConformanceAutomationFeature.xrSession = 0ul;
        }

        /// <summary>
        /// Drive the xrSetInputDeviceActiveEXT function of the XR_EXT_conformance_automation.
        /// See https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_conformance_automation
        /// </summary>
        /// <param name="interactionProfile">An OpenXRPath that specifies the OpenXR Interaction Profile of the value to be changed (e.g. /interaction_profiles/khr/simple_controller).</param>
        /// <param name="topLevelPath">An OpenXRPath that specifies the OpenXR User Path of the value to be changed (e.g. /user/hand/left).</param>
        /// <param name="isActive">A boolean that specifies the desired state of the target.</param>
        /// <returns>Returns true if the state is set successfully, or false if there was an error.</returns>
        public static bool ConformanceAutomationSetActive(string interactionProfile, string topLevelPath, bool isActive)
        {
            return xrSetInputDeviceActiveEXT(
                xrSession,
                GetCurrentInteractionProfile(interactionProfile),
                StringToPath(topLevelPath),
                isActive);
        }

        /// <summary>
        /// Drive the xrSetInputDeviceStateBoolEXT function of the XR_EXT_conformance_automation.
        /// See https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_conformance_automation
        /// </summary>
        /// <param name="topLevelPath">An OpenXRPath that specifies the OpenXR User Path of the value to be changed (e.g. /user/hand/left).</param>
        /// <param name="inputSourcePath">An OpenXRPath that specifies the full path of the input component whose state you wish to set (e.g. /user/hand/left/input/select/click).</param>
        /// <param name="state">A boolean that specifies the desired state of the target.</param>
        /// <returns>Returns true if the state is set successfully, or false if there was an error.</returns>
        public static bool ConformanceAutomationSetBool(string topLevelPath, string inputSourcePath, bool state)
        {
            return xrSetInputDeviceStateBoolEXT(
                xrSession,
                StringToPath(topLevelPath),
                StringToPath(inputSourcePath),
                state);
        }

        /// <summary>
        /// Drive the xrSetInputDeviceStateFloatEXT function of the XR_EXT_conformance_automation.
        /// See https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_conformance_automation
        /// </summary>
        /// <param name="topLevelPath">>An OpenXRPath that specifies the OpenXR User Path of the value to be changed (e.g. /user/hand/left).</param>
        /// <param name="inputSourcePath">An OpenXRPath that specifies the full path of the input component whose state you wish to set (e.g. /user/hand/left/input/select/click).</param>
        /// <param name="state">A float that specifies the desired state of the target.</param>
        /// <returns>Returns true if the state is set successfully, or false if there was an error.</returns>
        public static bool ConformanceAutomationSetFloat(string topLevelPath, string inputSourcePath, float state)
        {
            return xrSetInputDeviceStateFloatEXT(
                xrSession,
                StringToPath(topLevelPath),
                StringToPath(inputSourcePath),
                state);
        }

        /// <summary>
        /// Drive the xrSetInputDeviceStateVector2fEXT function of the XR_EXT_conformance_automation.
        /// See https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_conformance_automation
        /// </summary>
        /// <param name="topLevelPath">An OpenXRPath that specifies the OpenXR User Path of the value to be changed (e.g. /user/hand/left).</param>
        /// <param name="inputSourcePath">An OpenXRPath that specifies the full path of the input component whose state you wish to set (e.g. /user/hand/left/input/select/click).</param>
        /// <param name="state">A Vector2 that specifies the desired state of the target.</param>
        /// <returns>Returns true if the state is set successfully, or false if there was an error.</returns>
        public static bool ConformanceAutomationSetVec2(string topLevelPath, string inputSourcePath, Vector2 state)
        {
            return xrSetInputDeviceStateVector2fEXT(
                xrSession,
                StringToPath(topLevelPath),
                StringToPath(inputSourcePath),
                new XrVector2f(state));
        }

        /// <summary>
        /// Drive the xrSetInputDeviceLocationEXT function of the XR_EXT_conformance_automation.
        /// See https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_conformance_automation
        /// </summary>
        /// <param name="topLevelPath">An OpenXRPath that specifies the OpenXR User Path of the value to be changed (e.g. /user/hand/left).</param>
        /// <param name="inputSourcePath">An OpenXRPath that specifies the full path of the input component whose state you wish to set (e.g. /user/hand/left/input/select/click).</param>
        /// <param name="position">A Vector3 that specifies the desired state of the target.</param>
        /// <param name="orientation">A Quaternion that specifies the desired state of the target.</param>
        /// <returns>Returns true if the state is set successfully, or false if there was an error.</returns>
        public static bool ConformanceAutomationSetPose(string topLevelPath, string inputSourcePath, Vector3 position, Quaternion orientation)
        {
            return xrSetInputDeviceLocationEXT(
                xrSession,
                StringToPath(topLevelPath),
                StringToPath(inputSourcePath),
                GetCurrentAppSpace(),
                new XrPosef(position, orientation));
        }

        /// <summary>
        /// Set the angular and linear velocity of a pose
        /// </summary>
        /// <param name="topLevelPath">An OpenXRPath that specifies the OpenXR User Path of the value to be changed (e.g. /user/hand/left).</param>
        /// <param name="inputSourcePath">An OpenXRPath that specifies the full path of the input component whose state you wish to set (e.g. /user/hand/left/input/select/click).</param>
        /// <param name="linearValid">True if the linear velocity is valid</param>
        /// <param name="linear">Linear velocity value</param>
        /// <param name="angularValid">True if the angular velocity is valid</param>
        /// <param name="angular">Angular velocity value</param>
        /// <returns></returns>
        public static bool ConformanceAutomationSetVelocity(string topLevelPath, string inputSourcePath, bool linearValid, Vector3 linear, bool angularValid, Vector3 angular)
        {
            return xrSetInputDeviceVelocityUNITY(
                xrSession,
                StringToPath(topLevelPath),
                StringToPath(inputSourcePath),
                linearValid,
                new XrVector3f(linear),
                angularValid,
                new XrVector3f(-1.0f * angular) // Angular velocity is multiplied by -1 in the OpenXR plugin so it must be negated here as well
            );
        }

        // Dll imports

        private const string ExtLib = "ConformanceAutomationExt";

        /// <summary>
        /// Set up function pointers for xrSetInputDevice... functions.
        /// </summary>
        /// <param name="xrGetInstanceProcAddr">This is an IntPtr to the current OpenXR process address.</param>
        /// <param name="xrInstance">This is a ulong handle for the current OpenXR xrInstance.</param>
        [DllImport(ExtLib, EntryPoint = "script_initialize")]
        private static extern void initialize(IntPtr xrGetInstanceProcAddr, ulong xrInstance);

        [DllImport(ExtLib, EntryPoint = "script_xrSetInputDeviceActiveEXT")]
        private static extern bool xrSetInputDeviceActiveEXT(ulong xrSession, ulong interactionProfile, ulong topLevelPath, bool isActive);

        [DllImport(ExtLib, EntryPoint = "script_xrSetInputDeviceStateBoolEXT")]
        private static extern bool xrSetInputDeviceStateBoolEXT(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, bool state);

        [DllImport(ExtLib, EntryPoint = "script_xrSetInputDeviceStateFloatEXT")]
        private static extern bool xrSetInputDeviceStateFloatEXT(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, float state);

        [DllImport(ExtLib, EntryPoint = "script_xrSetInputDeviceStateVector2fEXT")]
        private static extern bool xrSetInputDeviceStateVector2fEXT(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, XrVector2f state);

        [DllImport(ExtLib, EntryPoint = "script_xrSetInputDeviceLocationEXT")]
        private static extern bool xrSetInputDeviceLocationEXT(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, ulong space, XrPosef pose);

        [DllImport(ExtLib, EntryPoint = "script_xrSetInputDeviceVelocityUNITY")]
        private static extern bool xrSetInputDeviceVelocityUNITY(ulong xrSession, ulong topLevelPath, ulong inputSourcePath, bool linearValid, XrVector3f linear, bool angularValid, XrVector3f angular);
    }
}
