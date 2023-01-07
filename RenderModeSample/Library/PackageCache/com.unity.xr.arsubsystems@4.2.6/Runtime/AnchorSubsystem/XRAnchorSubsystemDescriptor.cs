using System;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Describes the capabilities of an <see cref="XRAnchorSubsystem"/>.
    /// </summary>
    public class XRAnchorSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRAnchorSubsystem, XRAnchorSubsystem.Provider>
    {
        /// <summary>
        /// <c>true</c> if the subsystem supports attachments (that is, the ability to attach an anchor to a trackable).
        /// </summary>
        public bool supportsTrackableAttachments { get; private set; }

        /// <summary>
        /// Constructor info used to register a descriptor.
        /// </summary>
        public struct Cinfo : IEquatable<Cinfo>
        {
            /// <summary>
            /// The string identifier for this subsystem.
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
            /// The concrete <c>Type</c> of the subsystem which will be instantiated if a subsystem is created from this descriptor.
            /// </summary>
            [Obsolete("XRAnchorSubsystem no longer supports the deprecated set of base classes for subsystems as of Unity 2020.2. Use providerType and, optionally, subsystemTypeOverride instead.", true)]
            public Type subsystemImplementationType { get; set; }

            /// <summary>
            /// <c>true</c> if the subsystem supports attachments, i.e., the ability to attach an anchor to a trackable.
            /// </summary>
            public bool supportsTrackableAttachments { get; set; }

            /// <summary>
            /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = HashCodeUtil.ReferenceHash(id);
                    hash = 486187739 * hash + HashCodeUtil.ReferenceHash(providerType);
                    hash = 486187739 * hash + HashCodeUtil.ReferenceHash(subsystemTypeOverride);
                    hash = 486187738 * hash + supportsTrackableAttachments.GetHashCode();
                    return hash;
                }
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="obj">The `object` to compare against.</param>
            /// <returns>`True` if <paramref name="obj"/> is of type <see cref="Cinfo"/> and
            /// <see cref="Equals(Cinfo)"/> also returns `true`; otherwise `false`.</returns>
            public override bool Equals(object obj) => (obj is Cinfo other) && Equals(other);

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
                    supportsTrackableAttachments == other.supportsTrackableAttachments; // NB: before 2020.2, this was excluded - just trying to keep from changing behavior
            }

            /// <summary>
            /// Tests for equality. Same as <see cref="Equals(Cinfo)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator ==(Cinfo lhs, Cinfo rhs) => lhs.Equals(rhs);

            /// <summary>
            /// Tests for inequality. Same as `!`<see cref="Equals(Cinfo)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator !=(Cinfo lhs, Cinfo rhs) => !lhs.Equals(rhs);
        }

        /// <summary>
        /// Creates a new subsystem descriptor and registers it with the <c>SubsystemManager</c>.
        /// </summary>
        /// <param name="cinfo">Constructor info describing the descriptor to create.</param>
        public static void Create(Cinfo cinfo)
        {
            SubsystemDescriptorStore.RegisterDescriptor(new XRAnchorSubsystemDescriptor(cinfo));
        }

        XRAnchorSubsystemDescriptor(Cinfo cinfo)
        {
            id = cinfo.id;
            providerType = cinfo.providerType;
            subsystemTypeOverride = cinfo.subsystemTypeOverride;
            supportsTrackableAttachments = cinfo.supportsTrackableAttachments;
        }
    }
}
