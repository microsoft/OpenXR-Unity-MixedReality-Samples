using System.ComponentModel;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the environment mode.
    /// </summary>
    public enum EnvironmentDepthMode
    {
        /// <summary>
        /// The environment depth is disabled and will not be generated.
        /// </summary>
        [Description("Disabled")]
        Disabled = 0,

        /// <summary>
        /// The environment depth is enabled and will be generated at the fastest resolution.
        /// </summary>
        /// <remarks>
        /// On <see cref="Fastest"/> mode, there is no smoothing or other post processing applied to the texture.
        /// </remarks>
        [Description("Fastest")]
        Fastest = 1,

        /// <summary>
        /// The environment depth is enabled and will be generated at the medium resolution.
        /// </summary>
        [Description("Medium")]
        Medium = 2,

        /// <summary>
        /// The environment depth is enabled and will be generated at the best resolution.
        /// </summary>
        [Description("Best")]
        Best = 3,
    }

    /// <summary>
    /// Extension for the <see cref="EnvironmentDepthMode"/>.
    /// </summary>
    public static class EnvironmentDepthModeExtension
    {
        /// <summary>
        /// Determine whether the environment depth mode is enabled.
        /// </summary>
        /// <param name="environmentDepthMode">The environment depth mode to check.</param>
        /// <returns>
        /// <c>true</c> if the environment depth mode is enabled. Otherwise, <c>false</c>.
        /// </returns>
        public static bool Enabled(this EnvironmentDepthMode environmentDepthMode)
            => environmentDepthMode != EnvironmentDepthMode.Disabled;
    }
}
