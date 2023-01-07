using System.ComponentModel;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the segmentation depth mode.
    /// </summary>
    public enum HumanSegmentationDepthMode
    {
        /// <summary>
        /// The segmentation depth is disabled and will not be generated.
        /// </summary>
        [Description("Disabled")]
        Disabled = 0,

        /// <summary>
        /// The segmentation depth is enabled and will be generated with no additional image filtering.
        /// </summary>
        [Description("Fastest")]
        Fastest = 1,

        /// <summary>
        /// The segmentation depth is enabled and will be generated with additional image filtering.
        /// </summary>
        [Description("Best")]
        Best = 2,
    }

    /// <summary>
    /// Extension for the <see cref="HumanSegmentationDepthMode"/>.
    /// </summary>
    public static class SegmentationDepthModeExtension
    {
        /// <summary>
        /// Determine whether the segmentation depth mode is enabled.
        /// </summary>
        /// <param name="segmentationDepthMode">The segmentation depth mode to check.</param>
        /// <returns>
        /// <c>true</c> if the segmentation depth mode is enabled. Otherwise, <c>false</c>.
        /// </returns>
        public static bool Enabled(this HumanSegmentationDepthMode segmentationDepthMode)
            => segmentationDepthMode != HumanSegmentationDepthMode.Disabled;
    }
}
