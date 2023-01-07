using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR
{
    /// <summary>
    /// OpenXR Runtime access
    /// </summary>
    public static class OpenXRRuntime
    {
        /// <summary>
        /// Name of the current runtime.
        /// </summary>
        public static string name =>
            Internal_GetRuntimeName(out var runtimeNamePtr) ? Marshal.PtrToStringAnsi(runtimeNamePtr) : "";

        /// <summary>
        /// Version of the current runtime.
        /// </summary>
        public static string version =>
            Internal_GetRuntimeVersion(out var major, out var minor, out var patch) ? $"{major}.{minor}.{patch}" : "";

        /// <summary>
        /// Version of the current runtime API.
        /// </summary>
        public static string apiVersion =>
            Internal_GetAPIVersion(out var major, out var minor, out var patch) ? $"{major}.{minor}.{patch}" : "";

        /// <summary>
        /// Version of the current runtime plug-in.
        /// </summary>
        public static string pluginVersion =>
            Internal_GetPluginVersion(out var pluginVersionPtr) ? Marshal.PtrToStringAnsi(pluginVersionPtr) : "";

        /// <summary>
        /// Describes whether the OpenXR Extension with the given name is enabled.
        /// </summary>
        /// <param name="extensionName">Name of the extension</param>
        /// <returns>True if the extension matching the given name is enabled, false otherwise</returns>
        public static bool IsExtensionEnabled(string extensionName) => Internal_IsExtensionEnabled(extensionName);

        /// <summary>
        /// Returns the version number of the given extension.
        /// </summary>
        /// <param name="extensionName">Name of the extension</param>
        /// <returns>Version number of given extension, or zero if the extension was not found</returns>
        public static uint GetExtensionVersion(string extensionName) => Internal_GetExtensionVersion(extensionName);

        /// <summary>
        /// Returns the list of names of extensions enabled within the OpenXR runtime.
        /// </summary>
        /// <returns>Array of extension names or an empty array if no extensions are enabled.</returns>
        public static string[] GetEnabledExtensions()
        {
            var extensions = new string[Internal_GetEnabledExtensionCount()];
            for (var i = 0; i < extensions.Length; i++)
            {
                Internal_GetEnabledExtensionName((uint)i, out var extensionName);
                extensions[i] = $"{extensionName}";
            }

            return extensions;
        }

        /// <summary>
        /// Returns the list of names of extensions available within the OpenXR runtime.
        /// </summary>
        /// <returns>Array of extension names or an empty array if no extensions are available.</returns>
        public static string[] GetAvailableExtensions()
        {
            var extensions = new string[Internal_GetAvailableExtensionCount()];
            for (var i = 0; i < extensions.Length; i++)
            {
                Internal_GetAvailableExtensionName((uint)i, out var extensionName);
                extensions[i] = $"{extensionName}";
            }

            return extensions;
        }

        /// <summary>
        /// This event is raised when OpenXR wants the application to quit.
        ///
        /// Note that this event is raised before Application.wantsToQuit is raised and is generally
        /// raised because the Runtime has requested the application quit.
        ///
        /// Return true and the quit process will continue.  Return false and the quit process will cancel.
        /// </summary>
        public static event Func<bool> wantsToQuit;

        /// <summary>
        /// This event is raised when OpenXR Runtime wants to restart the XR loader by shutting down
        /// the loader and reinitializing it.
        ///
        /// Return true and the restart process will continue.  Return false and the XR loader will be
        /// unloaded and the quit process will begin.
        /// </summary>
        public static event Func<bool> wantsToRestart;

        /// <summary>
        /// Invokes the given event function and returns true if all invocations return true
        /// </summary>
        /// <param name="func">Event function</param>
        /// <returns>True if all event invocations return true</returns>
        private static bool InvokeEvent (Func<bool> func)
        {
            if (func == null)
                return true;

            foreach (Func<bool> invocation in func.GetInvocationList())
            {
                try
                {
                    if (!invocation())
                        return false;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            return true;
        }

#if UNITY_INCLUDE_TESTS
        internal static void ClearEvents ()
        {
            if(wantsToQuit != null)
                foreach (Func<bool> f in wantsToQuit.GetInvocationList()) wantsToQuit -= f;

            if(wantsToRestart != null)
                foreach (Func<bool> f in wantsToRestart.GetInvocationList()) wantsToRestart -= f;

            wantsToQuit = null;
            wantsToRestart = null;
        }
#endif

        internal static bool ShouldQuit() => InvokeEvent(wantsToQuit);
        internal static bool ShouldRestart() => InvokeEvent(wantsToRestart);

        private const string LibraryName = "UnityOpenXR";

        [DllImport(LibraryName, EntryPoint = "NativeConfig_GetRuntimeName")]
        private static extern bool Internal_GetRuntimeName(out IntPtr runtimeNamePtr);

        [DllImport(LibraryName, EntryPoint = "NativeConfig_GetRuntimeVersion")]
        private static extern bool Internal_GetRuntimeVersion(out ushort major, out ushort minor, out uint patch);

        [DllImport(LibraryName, EntryPoint = "NativeConfig_GetAPIVersion")]
        private static extern bool Internal_GetAPIVersion(out ushort major, out ushort minor, out uint patch);

        [DllImport(LibraryName, EntryPoint = "NativeConfig_GetPluginVersion")]
        private static extern bool Internal_GetPluginVersion(out IntPtr pluginVersionPtr);

        [DllImport(LibraryName, EntryPoint = "unity_ext_IsExtensionEnabled")]
        private static extern bool Internal_IsExtensionEnabled(string extensionName);

        [DllImport(LibraryName, EntryPoint = "unity_ext_GetExtensionVersion")]
        private static extern uint Internal_GetExtensionVersion(string extensionName);

        [DllImport(LibraryName, EntryPoint = "unity_ext_GetEnabledExtensionCount")]
        private static extern uint Internal_GetEnabledExtensionCount();

        [DllImport(LibraryName, EntryPoint = "unity_ext_GetEnabledExtensionName", CharSet = CharSet.Ansi)]
        private static extern bool Internal_GetEnabledExtensionNamePtr(uint index, out IntPtr outName);

        private static bool Internal_GetEnabledExtensionName(uint index, out string extensionName)
        {
            if (!Internal_GetEnabledExtensionNamePtr(index, out var extensionNamePtr))
            {
                extensionName = "";
                return false;
            }

            extensionName = Marshal.PtrToStringAnsi(extensionNamePtr);
            return true;
        }

        [DllImport(LibraryName, EntryPoint = "unity_ext_GetAvailableExtensionCount")]
        private static extern uint Internal_GetAvailableExtensionCount();

        [DllImport(LibraryName, EntryPoint = "unity_ext_GetAvailableExtensionName", CharSet = CharSet.Ansi)]
        private static extern bool Internal_GetAvailableExtensionNamePtr(uint index, out IntPtr extensionName);

        private static bool Internal_GetAvailableExtensionName(uint index, out string extensionName)
        {
            if (!Internal_GetAvailableExtensionNamePtr(index, out var extensionNamePtr))
            {
                extensionName = "";
                return false;
            }

            extensionName = Marshal.PtrToStringAnsi(extensionNamePtr);
            return true;
        }

        [DllImport(LibraryName, EntryPoint = "session_GetLastError", CharSet = CharSet.Ansi)]
        private static extern bool Internal_GetLastError(out IntPtr error);

        /// <summary>
        /// Returns the last error message that was issued in the native code
        /// </summary>
        /// <param name="error">Last error message</param>
        /// <returns>True if there was an error message, false if not</returns>
        internal static bool GetLastError(out string error)
        {
            if (!Internal_GetLastError(out var errorPtr))
            {
                error = "";
                return false;
            }

            error = Marshal.PtrToStringAnsi(errorPtr);
            return true;
        }

        /// <summary>
        /// Logs the last error message if there is one to the console
        /// </summary>
        internal static void LogLastError()
        {
            if (GetLastError(out var error))
            {
                Debug.LogError(error);
            }
        }
    }
}
