using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR
{
    public partial class OpenXRLoaderBase
    {
        private const string LibraryName = "UnityOpenXR";

        [DllImport(LibraryName, EntryPoint = "main_LoadOpenXRLibrary")]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool Internal_LoadOpenXRLibrary(byte[] loaderPath);

        [DllImport(LibraryName, EntryPoint = "main_UnloadOpenXRLibrary")]
        internal static extern void Internal_UnloadOpenXRLibrary();

        [DllImport(LibraryName, EntryPoint = "NativeConfig_SetCallbacks")]
        static extern void Internal_SetCallbacks(OpenXRLoader.ReceiveNativeEventDelegate callback);

        [DllImport(LibraryName, EntryPoint = "NativeConfig_SetApplicationInfo", CharSet = CharSet.Ansi)]
        static extern void Internal_SetApplicationInfo(string applicationName, string applicationVersion, uint applicationVersionHash, string engineVersion);

        // Session native imports
        [DllImport(LibraryName, EntryPoint = "session_RequestExitSession")]
        internal static extern void Internal_RequestExitSession();

        [DllImport(LibraryName, EntryPoint = "session_InitializeSession")]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool Internal_InitializeSession();

        [DllImport(LibraryName, EntryPoint = "session_CreateSessionIfNeeded")]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool Internal_CreateSessionIfNeeded();

        [DllImport(LibraryName, EntryPoint = "session_BeginSession")]
        internal static extern void Internal_BeginSession();

        [DllImport(LibraryName, EntryPoint = "session_EndSession")]
        internal static extern void Internal_EndSession();

        [DllImport(LibraryName, EntryPoint = "session_DestroySession")]
        internal static extern void Internal_DestroySession();

        [DllImport(LibraryName, EntryPoint = "messagepump_PumpMessageLoop")]
        static extern void Internal_PumpMessageLoop();

        [DllImport(LibraryName, EntryPoint = "unity_ext_RequestEnableExtensionString", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool Internal_RequestEnableExtensionString(string extensionString);
    }
}
