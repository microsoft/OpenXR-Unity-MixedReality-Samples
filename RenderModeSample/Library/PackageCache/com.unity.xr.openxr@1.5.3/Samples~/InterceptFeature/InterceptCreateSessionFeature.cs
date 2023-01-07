using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using UnityEditor;
using UnityEngine.XR.OpenXR.Features;
#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Samples.InterceptFeature
{
    /// <summary>
    /// Example feature showing how to intercept a single OpenXR function.
    /// </summary>
    #if UNITY_EDITOR
    [OpenXRFeature(UiName = "Sample: Intercept Create Session",
        BuildTargetGroups = new []{BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android},
        Company = "Unity",
        Desc = "Example feature extension showing how to intercept a single OpenXR function.",
        DocumentationLink = Constants.k_DocumentationURL,
        OpenxrExtensionStrings = "XR_test", // this extension doesn't exist, a log message will be printed that it couldn't be enabled
        Version = "0.0.1",
        FeatureId = featureId)]
    #endif
    public class InterceptCreateSessionFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.example.intercept";

        /// <summary>
        /// Message to display upon interception.
        /// </summary>
        public string message = "Hello from C#!";

        /// <summary>
        /// Message received from native as a result of the intercepted xrCreateSession call.
        /// </summary>
        public string receivedMessage { get; private set; }

        /// <inheritdoc />
        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            Debug.Log("EXT: registering our own xrGetInstanceProcAddr");
            return intercept_xrCreateSession_xrGetInstanceProcAddr(func);
        }

        /// <inheritdoc />
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            Internal_SetCallback(OnMessage);

            // Example of sending data set by user down to native.
            // this is unsafe!  Don't do this. Just an example.
            Internal_SetMessage(message);

            // here's one way you can grab the instance
            Debug.Log($"EXT: Got xrInstance: {xrInstance}");
            return true;
        }

        private delegate void OnMessageDelegate(string message);
        [MonoPInvokeCallback(typeof(OnMessageDelegate))]
        private static void OnMessage(string message)
        {
            if (message == null)
                return;

            Debug.Log(message);

            var feature = OpenXRSettings.Instance.GetFeature<InterceptCreateSessionFeature>();
            if (null == feature)
                return;

            feature.receivedMessage = message;
        }

        /// <inheritdoc />
        protected override void OnSessionCreate(ulong xrSession)
        {
            // here's one way you can grab the session
            Debug.Log($"EXT: Got xrSession: {xrSession}");
        }

        /// <inheritdoc />
        protected override void OnSessionBegin (ulong xrSession)
        {
            Debug.Log($"EXT: xrBeginSession: {xrSession}");
        }

        /// <inheritdoc />
        protected override void OnSessionEnd(ulong xrSession)
        {
            Debug.Log($"EXT: about to xrEndSession: {xrSession}");
        }

        private const string ExtLib = "InterceptFeaturePlugin";
        [DllImport(ExtLib, EntryPoint = "script_intercept_xrCreateSession_xrGetInstanceProcAddr")]
        private static extern IntPtr intercept_xrCreateSession_xrGetInstanceProcAddr(IntPtr func);

        [DllImport(ExtLib, EntryPoint = "script_set_message", CharSet = CharSet.Ansi)]
        private static extern void Internal_SetMessage (string printString);

        internal delegate void ReceiveMessageDelegate (string message);

        [DllImport(ExtLib, EntryPoint = "script_set_callback")]
        static extern void Internal_SetCallback(ReceiveMessageDelegate callback);
    }
}
