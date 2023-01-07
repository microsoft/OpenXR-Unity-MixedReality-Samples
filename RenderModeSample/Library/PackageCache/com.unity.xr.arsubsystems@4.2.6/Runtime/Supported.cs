namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Used to indicate whether a feature or capability is supported.
    /// </summary>
    public enum Supported
    {
        /// <summary>
        /// Support is unknown. This could be because support is still being determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The feature or capability is not supported.
        /// </summary>
        Unsupported,

        /// <summary>
        /// The feature or capability is supported.
        /// </summary>
        Supported,
    }
}
