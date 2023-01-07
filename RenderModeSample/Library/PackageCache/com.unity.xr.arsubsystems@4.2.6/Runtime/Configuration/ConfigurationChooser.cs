using Unity.Collections;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// The base class for configuration choosers, which determines
    /// which configuration a session will use. Used by
    /// <see cref="XRSessionSubsystem.DetermineConfiguration(Feature)"/>.
    /// Use <see cref="XRSessionSubsystem.configurationChooser"/> to get or set
    /// the session's configuration choosers.
    /// </summary>
    public abstract class ConfigurationChooser
    {
        /// <summary>
        /// Chooses a configuration given a set of <see cref="ConfigurationDescriptor"/>s and requested <see cref="Feature"/>s.
        /// </summary>
        /// <param name="descriptors">The set of available configurations supported by the session.</param>
        /// <param name="requestedFeatures">The set of requested <see cref="Feature"/>s.</param>
        /// <returns>A <see cref="Configuration"/> the session should use.
        /// See <see cref="Configuration.Configuration(ConfigurationDescriptor, Feature)"/> for restrictions
        /// on the selected configuration.</returns>
        public abstract Configuration ChooseConfiguration(
            NativeSlice<ConfigurationDescriptor> descriptors,
            Feature requestedFeatures);
    }
}
