using UnityEngine.XR.Management;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A utility for interacting with an `XRLoader` from
    /// [XR Management](https://docs.unity3d.com/Packages/com.unity.xr.management@4.0/manual/index.html).
    /// </summary>
    /// <remarks>
    /// XR Management controls the lifecycle of subsystems. Components in AR Foundation, such as `ARSession` or
    /// `ARPlaneManager`, turn subsystems on and off, but do not create or destroy them. Therefore, subsystems
    /// can persist across many scenes. They are automatically created on app startup, but are not destroyed
    /// during a scene switch. This allows you to keep the same session alive between scenes, for example.
    /// </remarks>
    public static class LoaderUtility
    {
        /// <summary>
        /// Get the 'active' loader from XR Management.
        /// </summary>
        /// <returns>Returns the currently active `XRLoader`.</returns>
        public static XRLoader GetActiveLoader()
        {
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                return XRGeneralSettings.Instance.Manager.activeLoader;
            }

            return null;
        }

        /// <summary>
        /// Initializes the currently active `XR Loader`, if one exists. This creates all subsystems.
        /// </summary>
        /// <returns>Returns `true` if there is an active loader and its `Initialize` method returns `true`.
        /// Returns `false` otherwise.</returns>
        public static bool Initialize()
        {
            var loader = GetActiveLoader();
            return loader && loader.Initialize();
        }

        /// <summary>
        /// Deinitializes the currently active `XR Loader`, if one exists. This destroys all subsystems.
        /// </summary>
        /// <returns>Returns `true` if there is an active loader and its `Deinitialize` method returns `true`.
        /// Returns `false` otherwise.</returns>
        public static bool Deinitialize()
        {
            var loader = GetActiveLoader();
            return loader && loader.Deinitialize();
        }
    }
}
