using System.ComponentModel;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the light estimation mode. This is deprecated.
    /// </summary>
    public enum LightEstimationMode
    {
        /// <summary>
        /// Light estimation is disabled.
        /// </summary>
        [Description("Disabled")]
        Disabled = 0,

        /// <summary>
        /// Ambient lighting will be estimated as a single-value intensity.
        /// </summary>
        [Description("AmbientIntensity")]
        AmbientIntensity = 1,

        /// <summary>
        /// Scene lighting will be estimated using Environmental HDR.
        /// </summary>
        [Description("EnvironmentalHDR")]
        EnvironmentalHDR = 2,
    }
}
