using System.ComponentModel;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the preference for how to occlude.
    /// </summary>
    public enum OcclusionPreferenceMode
    {
        /// <summary>
        /// The preference is to occlude using environment depth.
        /// </summary>
        [Description("PreferEnvironmentOcclusion")]
        PreferEnvironmentOcclusion = 0,

        /// <summary>
        /// The preference is to occlude using human segmentation stencil and depth.
        /// </summary>
        [Description("PreferHumanOcclusion")]
        PreferHumanOcclusion = 1,
    }
}
