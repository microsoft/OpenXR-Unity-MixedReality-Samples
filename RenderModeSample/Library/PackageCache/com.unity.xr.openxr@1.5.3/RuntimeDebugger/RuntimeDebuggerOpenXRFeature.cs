using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine.Networking.PlayerConnection;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
using UnityEditor.XR.OpenXR.Features.RuntimeDebugger;
#endif

namespace UnityEngine.XR.OpenXR.Features.RuntimeDebugger
{
    /// <summary>
    /// A runtime debugger feature.  Intercepts all OpenXR calls and forwards them over player connection to an editor window.
    /// </summary>
    #if UNITY_EDITOR
    [OpenXRFeature(UiName = "Runtime Debugger",
        BuildTargetGroups = new []{BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android},
        Company = "Unity",
        Desc = "Enables debugging of OpenXR calls and dumping runtime info.",
        DocumentationLink = "",
        FeatureId = "com.unity.openxr.features.runtimedebugger",
        OpenxrExtensionStrings = "",
        Version = "1")]
    #endif
    public class RuntimeDebuggerOpenXRFeature : OpenXRFeature
    {
        internal static readonly Guid kEditorToPlayerRequestDebuggerOutput = new Guid("B3E6DED1-C6C7-411C-BE58-86031A0877E7");
        internal static readonly Guid kPlayerToEditorSendDebuggerOutput = new Guid("B3E6DED1-C6C7-411C-BE58-86031A0877E8");

        /// <summary>
        /// Size of main-thread cache on device for runtime debugger in bytes.
        /// </summary>
        public UInt32 cacheSize=1024*1024;

        /// <summary>
        /// Size of per-thread cache on device for runtime debugger in bytes.
        /// </summary>
        public UInt32 perThreadCacheSize=50*1024;

        private UInt32 lutOffset = 0;

        /// <inheritdoc/>
        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            #if !UNITY_EDITOR
            PlayerConnection.instance.Register(kEditorToPlayerRequestDebuggerOutput, RecvMsg);
            #endif

            // Reset
            Native_StartDataAccess();
            Native_EndDataAccess();
            lutOffset = 0;

            return Native_HookGetInstanceProcAddr(func, cacheSize, perThreadCacheSize);
        }

        internal void RecvMsg(MessageEventArgs args)
        {
            Native_StartDataAccess();

            // LUT for actions / handles
            Native_GetLUTData(out var lutPtr, out var lutSize, lutOffset);
            byte[] lutData = new Byte[lutSize];
            if (lutSize > 0)
            {
                lutOffset = lutSize;
                Marshal.Copy(lutPtr, lutData, 0, (int) lutSize);
            }

            // ring buffer on native side, so might get two chunks of data
            Native_GetDataForRead(out var ptr1, out var size1);
            Native_GetDataForRead(out var ptr2, out var size2);

            byte[] data = new byte[size1 + size2];
            Marshal.Copy(ptr1, data, 0, (int)size1);
            if (size2 > 0)
                Marshal.Copy(ptr2, data, (int)size1, (int)size2);

            Native_EndDataAccess();

            #if !UNITY_EDITOR
            PlayerConnection.instance.Send(kPlayerToEditorSendDebuggerOutput, lutData);
            PlayerConnection.instance.Send(kPlayerToEditorSendDebuggerOutput, data);
            #else
            DebuggerState.OnMessageEvent(new MessageEventArgs() {playerId = 0, data = lutData});
            DebuggerState.OnMessageEvent(new MessageEventArgs() { playerId = 0, data = data});
            #endif
        }

        private const string Library = "openxr_runtime_debugger";
        [DllImport(Library, EntryPoint = "HookXrInstanceProcAddr")]
        private static extern IntPtr Native_HookGetInstanceProcAddr(IntPtr func, UInt32 cacheSize, UInt32 perThreadCacheSize);

        [DllImport(Library, EntryPoint = "GetDataForRead")]
        private static extern bool Native_GetDataForRead(out IntPtr ptr, out UInt32 size);

        [DllImport(Library, EntryPoint = "GetLUTData")]
        private static extern void Native_GetLUTData(out IntPtr ptr, out UInt32 size, UInt32 offset);

        [DllImport(Library, EntryPoint = "StartDataAccess")]
        private static extern void Native_StartDataAccess();

        [DllImport(Library, EntryPoint = "EndDataAccess")]
        private static extern void Native_EndDataAccess();
    }
}

