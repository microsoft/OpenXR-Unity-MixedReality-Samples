
namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the reason tracking was lost.
    /// </summary>
    public enum NotTrackingReason
    {
        /// <summary>
        /// Tracking is working normally.
        /// </summary>
        None,

        /// <summary>
        /// Tracking is being initialized.
        /// </summary>
        Initializing,

        /// <summary>
        /// Tracking is resuming after an interruption.
        /// </summary>
        Relocalizing,

        /// <summary>
        /// Tracking was lost due to poor lighting conditions.
        /// </summary>
        InsufficientLight,

        /// <summary>
        /// Tracking was lost due to insufficient visual features.
        /// </summary>
        InsufficientFeatures,

        /// <summary>
        /// Tracking was lost due to excessive motion.
        /// </summary>
        ExcessiveMotion,

        /// <summary>
        /// Tracking lost reason is not supported.
        /// </summary>
        Unsupported,

        /// <summary>
        /// The camera is in use by another application. Tracking can resume once the app regains access to the camera.
        /// </summary>
        CameraUnavailable
    }
}

