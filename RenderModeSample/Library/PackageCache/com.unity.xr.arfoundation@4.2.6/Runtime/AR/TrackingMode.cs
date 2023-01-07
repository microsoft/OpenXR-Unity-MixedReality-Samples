using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Represents the tracking mode for the session.
    /// </summary>
    public enum TrackingMode
    {
        /// <summary>
        /// The tracking mode is not specified and will be chosen automatically.
        /// </summary>
        DontCare = 0,

        /// <summary>
        /// 3 degrees of freedom for orientation only.
        /// </summary>
        RotationOnly = 1,

        /// <summary>
        /// 6 degrees of freedom including both orientation and position.
        /// </summary>
        PositionAndRotation = 2,
    }

    /// <summary>
    /// Extensions for the <see cref="TrackingMode"/> and <c>Feature</c> enums,
    /// allowing conversion between the two.
    /// </summary>
    public static class TrackingModeExtensions
    {
        /// <summary>
        /// Converts a <see cref="TrackingMode"/> to a <c>UnityEngine.XR.ARSubsystems.Feature</c>.
        /// </summary>
        /// <param name="self">The <see cref="TrackingMode"/> being extended.</param>
        /// <returns>A <c>Feature</c> with the appropriate tracking mode bits set.</returns>
        public static Feature ToFeature(this TrackingMode self)
        {
            switch (self)
            {
                case TrackingMode.RotationOnly: return Feature.RotationOnly;
                case TrackingMode.PositionAndRotation: return Feature.PositionAndRotation;
                default: return Feature.None;
            }
        }

        /// <summary>
        /// Converts a <c>UnityEngine.XR.ARSubsystems.Feature</c> to a <see cref="TrackingMode"/>.
        /// </summary>
        /// <param name="self">The <c>Feature</c> being extended.</param>
        /// <returns>The <see cref="TrackingMode"/> representation of <paramref name="self"/>.</returns>
        public static TrackingMode ToTrackingMode(this Feature self)
        {
            switch(self.TrackingModes())
            {
                case Feature.RotationOnly: return TrackingMode.RotationOnly;
                case Feature.PositionAndRotation: return TrackingMode.PositionAndRotation;
                default: return TrackingMode.DontCare;
            }
        }
    }
}
