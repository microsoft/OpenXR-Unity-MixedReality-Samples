using System;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents a session configuration. It consists of a configuration <see cref="descriptor"/>, which
    /// contains information about the capabilities of the configuration, and the specific <see cref="features"/>
    /// which should be enabled by this configuration. Use <see cref="XRSessionSubsystem.DetermineConfiguration(Feature)"/>
    /// to get a <see cref="Configuration"/> given a set of features.
    /// </summary>
    /// <seealso cref="ConfigurationChooser"/>
    /// <seealso cref="XRSessionSubsystem.configurationChooser"/>
    /// <seealso cref="XRSessionSubsystem.DetermineConfiguration(Feature)"/>
    public struct Configuration : IEquatable<Configuration>
    {
        /// <summary>
        /// The descriptor contains information about the capabilities of a configuration.
        /// </summary>
        public ConfigurationDescriptor descriptor { get; private set; }

        /// <summary>
        /// The specific <see cref="Feature"/>(s) that should be enabled by this configuration.
        /// </summary>
        /// <remarks>
        /// You can ony enable exactly zero or one camera modes (see <see cref="Feature.UserFacingCamera"/> and <see cref="Feature.WorldFacingCamera"/>).
        /// If zero camera modes are enabled, no camera texture will be available. Some platforms might support a configuration that does
        /// not provide camera textures, which can be more performant if they are not necessary.
        /// All enabled features must be supported by the <see cref="descriptor"/>.
        /// </remarks>
        public Feature features { get; private set; }

        /// <summary>
        /// Constructs a <see cref="Configuration"/>.
        /// </summary>
        /// <param name="descriptor">A <see cref="ConfigurationDescriptor"/> for this configuration.</param>
        /// <param name="features">A set of <see cref="Feature"/>(s) that should be enabled for this configuration.
        /// You can only enable exactly zero or one camera modes (see <see cref="Feature.UserFacingCamera"/> and <see cref="Feature.WorldFacingCamera"/>).
        /// If zero camera modes are enabled, no camera texture will be available. Some platforms might support a configuration that does
        /// not provide camera textures, which can be more performant if they are not necessary.
        /// All <paramref name="features"/> must be supported by the <paramref name="descriptor"/>.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if multiple camera modes are enabled.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if multiple tracking modes are enabled.</exception>
        /// <exception cref="System.NotSupportedException">Thrown if the <paramref name="descriptor"/> does not support one or more <paramref name="features"/>.</exception>
        public Configuration(ConfigurationDescriptor descriptor, Feature features)
        {
            if (!descriptor.capabilities.All(features))
                throw new NotSupportedException($"The configuration does not support the following requested features: {features.SetDifference(descriptor.capabilities).ToStringList()}.");

            var cameraMode = features.Cameras();
            if (cameraMode.Any(Feature.AnyCamera))
            {
                if (cameraMode.Count() > 1)
                    throw new InvalidOperationException($"Either zero or one camera mode must be enabled. The following modes are enabled: {cameraMode.ToStringList()}");
            }

            var trackingMode = features.TrackingModes();
            if (trackingMode.Count() > 1)
                throw new InvalidOperationException($"Either zero or one tracking modes must be enabled. The following modes are enabled: {trackingMode.ToStringList()}");

            this.descriptor = descriptor;
            this.features = features;
        }

        /// <summary>
        /// Generates a hash code suitable for use in a Dictionary or HashSet.
        /// </summary>
        /// <returns>A hash code of this <see cref="Configuration"/>.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(descriptor.GetHashCode(), features.GetHashCode());

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="other">The other <see cref="Configuration"/> to compare against.</param>
        /// <returns><c>true</c> if the other <see cref="Configuration"/> is equal to this one.</returns>
        public bool Equals(Configuration other) => descriptor.Equals(other.descriptor) && (features == other.features);

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="obj">The <c>object</c> to compare against.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="Configuration"/> and <see cref="Equals(Configuration)"/> is <c>true</c>.</returns>
        public override bool Equals(object obj) => (obj is Configuration) && Equals((Configuration)obj);

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>The same as <see cref="Equals(Configuration)"/>.</returns>
        public static bool operator==(Configuration lhs, Configuration rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Compares for inequality.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>The negation of <see cref="Equals(Configuration)"/>.</returns>
        public static bool operator!=(Configuration lhs, Configuration rhs) => !lhs.Equals(rhs);
    }
}
