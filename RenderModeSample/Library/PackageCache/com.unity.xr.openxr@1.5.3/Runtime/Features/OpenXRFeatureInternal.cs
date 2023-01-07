using System;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features
{
    public abstract partial class OpenXRFeature
    {
        const string Library = "UnityOpenXR";

        [DllImport(Library, EntryPoint = "Internal_PathToString")]
        static extern bool Internal_PathToStringPtr(ulong pathId, out IntPtr path);

        [DllImport(Library, EntryPoint = "Internal_StringToPath")]
        static extern bool Internal_StringToPath([MarshalAs(UnmanagedType.LPStr)] string str, out ulong pathId);

        [DllImport(Library, EntryPoint = "Internal_GetCurrentInteractionProfile")]
        static extern bool Internal_GetCurrentInteractionProfile(ulong pathId, out ulong interactionProfile);

        [DllImport(Library, EntryPoint = "NativeConfig_GetFormFactor")]
        static extern int Internal_GetFormFactor();

        [DllImport(Library, EntryPoint = "NativeConfig_GetViewConfigurationType")]
        static extern int Internal_GetViewConfigurationType();

        [DllImport(Library, EntryPoint = "NativeConfig_GetViewTypeFromRenderIndex")]
        static extern int Internal_GetViewTypeFromRenderIndex(int renderPassIndex);

        [DllImport(Library, EntryPoint = "session_GetSessionState")]
        static extern void Internal_GetSessionState(out int oldState, out int newState);

        [DllImport(Library, EntryPoint = "NativeConfig_GetEnvironmentBlendMode")]
        static extern XrEnvironmentBlendMode Internal_GetEnvironmentBlendMode();

        [DllImport(Library, EntryPoint = "NativeConfig_SetEnvironmentBlendMode")]
        static extern void Internal_SetEnvironmentBlendMode(XrEnvironmentBlendMode xrEnvironmentBlendMode);

        [DllImport(Library, EntryPoint = "OpenXRInputProvider_GetAppSpace")]
        static extern bool Internal_GetAppSpace(out ulong appSpace);

        [DllImport(Library, EntryPoint = "NativeConfig_GetProcAddressPtr")]
        internal static extern IntPtr Internal_GetProcAddressPtr(bool loaderDefault);

        [DllImport(Library, EntryPoint = "NativeConfig_SetProcAddressPtrAndLoadStage1")]
        internal static extern void Internal_SetProcAddressPtrAndLoadStage1(IntPtr func);
    }
}
