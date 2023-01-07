using System;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Descriptor for the <see cref="XRRaycastSubsystem"/>. Describes capabilities of a specific raycast provider.
    /// </summary>
    public sealed class XRRaycastSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRRaycastSubsystem, XRRaycastSubsystem.Provider>
    {
        /// <summary>
        /// Used to register a descriptor. See <see cref="RegisterDescriptor(Cinfo)"/>.
        /// </summary>
        public struct Cinfo : IEquatable<Cinfo>
        {
            /// <summary>
            /// A provider-specific identifier.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Specifies the provider implementation type to use for instantiation.
            /// </summary>
            /// <value>
            /// The provider implementation type to use for instantiation.
            /// </value>
            public Type providerType { get; set; }

            /// <summary>
            /// Specifies the <c>XRAnchorSubsystem</c>-derived type that forwards casted calls to its provider.
            /// </summary>
            /// <value>
            /// The type of the subsystem to use for instantiation. If null, <c>XRAnchorSubsystem</c> will be instantiated.
            /// </value>
            public Type subsystemTypeOverride { get; set; }

            /// <summary>
            /// The <c>Type</c> of the subsystem.
            /// </summary>
            [Obsolete("XRRaycastSubsystem no longer supports the deprecated set of base classes for subsystems as of Unity 2020.2. Use providerType and, optionally, subsystemTypeOverride instead.", true)]
            public Type subsystemImplementationType { get; set; }

            /// <summary>
            /// Whether the provider supports casting a ray from a screen point.
            /// </summary>
            public bool supportsViewportBasedRaycast { get; set; }

            /// <summary>
            /// Whether the provider supports casting an arbitrary ray.
            /// </summary>
            public bool supportsWorldBasedRaycast { get; set; }

            /// <summary>
            /// The types of trackables against which raycasting is supported.
            /// </summary>
            public TrackableType supportedTrackableTypes { get; set; }

            /// <summary>
            /// Whether tracked raycasts are supported. A tracked raycast is repeated
            /// over time and the results are updated automatically.
            /// </summary>
            public bool supportsTrackedRaycasts { get; set; }

            /// <summary>
            /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode() => HashCodeUtil.Combine(
                HashCodeUtil.ReferenceHash(id),
                HashCodeUtil.ReferenceHash(providerType),
                HashCodeUtil.ReferenceHash(subsystemTypeOverride),
                supportsViewportBasedRaycast.GetHashCode(),
                supportsWorldBasedRaycast.GetHashCode(),
                ((int)supportedTrackableTypes).GetHashCode(),
                supportsTrackedRaycasts.GetHashCode());

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="obj">The `object` to compare against.</param>
            /// <returns>`True` if <paramref name="obj"/> is of type <see cref="Cinfo"/> and
            /// <see cref="Equals(Cinfo)"/> also returns `true`; otherwise `false`.</returns>
            public override bool Equals(object obj) => (obj is Cinfo) && Equals((Cinfo)obj);

            /// <summary>
            /// Generates a string representation of this <see cref="Cinfo"/>.
            /// </summary>
            /// <returns>A string representation of this <see cref="Cinfo"/>.</returns>
            public override string ToString()
            {
                return string.Format("XRRaycastSubsystemDescriptor:\nsupportsViewportBasedRaycast: {0}\nsupportsWorldBasedRaycast: {1}",
                    supportsViewportBasedRaycast, supportsWorldBasedRaycast);
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="other">The other <see cref="Cinfo"/> to compare against.</param>
            /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="Cinfo"/>, otherwise false.</returns>
            public bool Equals(Cinfo other)
            {
                return
                    String.Equals(id, other.id) &&
                    ReferenceEquals(providerType, other.providerType) &&
                    ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride) &&
                    supportsViewportBasedRaycast == other.supportsViewportBasedRaycast &&
                    supportsWorldBasedRaycast == other.supportsWorldBasedRaycast &&
                    supportedTrackableTypes == other.supportedTrackableTypes;
            }

            /// <summary>
            /// Tests for equality. Same as <see cref="Equals(Cinfo)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator ==(Cinfo lhs, Cinfo rhs) { return lhs.Equals(rhs); }

            /// <summary>
            /// Tests for inequality. Same as `!`<see cref="Equals(Cinfo)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator !=(Cinfo lhs, Cinfo rhs) { return !lhs.Equals(rhs); }
        }

        /// <summary>
        /// Whether the provider supports casting a ray from a screen point.
        /// </summary>
        public bool supportsViewportBasedRaycast { get; private set; }

        /// <summary>
        /// Whether the provider supports casting an arbitrary ray.
        /// </summary>
        public bool supportsWorldBasedRaycast { get; private set; }

        /// <summary>
        /// The types of trackables against which raycasting is supported.
        /// </summary>
        public TrackableType supportedTrackableTypes { get; private set; }

        /// <summary>
        /// Whether "tracked" raycasts are supported. A tracked raycast is repeated
        /// over time and the results are updated automatically.
        /// </summary>
        public bool supportsTrackedRaycasts { get; set; }

        /// <summary>
        /// Registers a new descriptor. Should be called by provider implementations.
        /// </summary>
        /// <param name="cinfo"></param>
        public static void RegisterDescriptor(Cinfo cinfo)
        {
            SubsystemDescriptorStore.RegisterDescriptor(new XRRaycastSubsystemDescriptor(cinfo));
        }

        XRRaycastSubsystemDescriptor(Cinfo cinfo)
        {
            id = cinfo.id;
            providerType = cinfo.providerType;
            subsystemTypeOverride = cinfo.subsystemTypeOverride;
            supportsViewportBasedRaycast = cinfo.supportsViewportBasedRaycast;
            supportsWorldBasedRaycast = cinfo.supportsWorldBasedRaycast;
            supportedTrackableTypes = cinfo.supportedTrackableTypes;
            supportsTrackedRaycasts = cinfo.supportsTrackedRaycasts;
        }
    }
}
