namespace UnityEngine.XR.OpenXR
{
    /// <summary>
    /// Static constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Key used to store and retrieve custom configuration settings from EditorBuildSettings.
        /// </summary>
        public const string k_SettingsKey = "com.unity.xr.openxr.settings4";

#if UNITY_EDITOR
        /// <summary>
        /// Root URL for the OpenXR documentation
        /// </summary>
        public const string k_DocumentationManualURL = "https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.0/manual/";

        /// <summary>
        /// Main documentation URL for OpenXR
        /// </summary>
        public const string k_DocumentationURL = k_DocumentationManualURL + "index.html";
#endif
    }
}
