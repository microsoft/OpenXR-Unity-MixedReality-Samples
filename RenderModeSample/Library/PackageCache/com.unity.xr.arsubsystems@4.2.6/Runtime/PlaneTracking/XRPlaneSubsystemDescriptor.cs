using System;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Describes the capabilities of an <see cref="XRPlaneSubsystem"/>.
    /// </summary>
    public class XRPlaneSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRPlaneSubsystem, XRPlaneSubsystem.Provider>
    {
        /// <summary>
        /// <c>true</c> if the subsystem supports horizontal plane detection.
        /// </summary>
        public bool supportsHorizontalPlaneDetection { get; private set; }

        /// <summary>
        /// <c>true</c> if the subsystem supports vertical plane detection.
        /// </summary>
        public bool supportsVerticalPlaneDetection { get; private set; }

        /// <summary>
        /// <c>true</c> if the subsystem supports arbitrarily angled plane detection.
        /// </summary>
        public bool supportsArbitraryPlaneDetection { get; private set; }

        /// <summary>
        /// <c>true</c> if the subsystem supports boundary vertices for its planes.
        /// </summary>
        public bool supportsBoundaryVertices { get; private set; }

        /// <summary>
        /// <c>true</c> if the current subsystem supports plane classification. Otherwise, <c>false</c>.
        /// </summary>
        public bool supportsClassification { get; private set; }

        /// <summary>
        /// Constructor info used to register a descriptor.
        /// </summary>
        public struct Cinfo : IEquatable<Cinfo>
        {
            /// <summary>
            /// The string identifier for a specific implementation.
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
            /// Specifies the <c>XRPlaneSubsystem</c>-derived type that forwards casted calls to its provider.
            /// </summary>
            /// <value>
            /// The type of the subsystem to use for instantiation. If null, <c>XRPlaneSubsystem</c> will be instantiated.
            /// </value>
            public Type subsystemTypeOverride { get; set; }

            /// <summary>
            /// The concrete <c>Type</c> which will be instantiated if <c>Create</c> is called on this subsystem descriptor.
            /// </summary>
            [Obsolete("XRPlaneSubsystem no longer supports the deprecated set of base classes for subsystems as of Unity 2020.2. Use providerType and, optionally, subsystemTypeOverride instead.", true)]
            public Type subsystemImplementationType { get; set; }

            /// <summary>
            /// <c>true</c> if the subsystem supports horizontal plane detection.
            /// </summary>
            public bool supportsHorizontalPlaneDetection { get; set; }

            /// <summary>
            /// <c>true</c> if the subsystem supports vertical plane detection.
            /// </summary>
            public bool supportsVerticalPlaneDetection { get; set; }

            /// <summary>
            /// <c>true</c> if the subsystem supports arbitrarily angled plane detection.
            /// </summary>
            public bool supportsArbitraryPlaneDetection { get; set; }

            /// <summary>
            /// <c>true</c> if the subsystem supports boundary vertices for its planes.
            /// </summary>
            public bool supportsBoundaryVertices { get; set; }

            /// <summary>
            /// <c>true</c> if the subsystem supports boundary vertices for its planes.
            /// </summary>
            public bool supportsClassification { get; set; }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="other">The other <see cref="Cinfo"/> to compare against.</param>
            /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="Cinfo"/>, otherwise false.</returns>
            public bool Equals(Cinfo other)
            {
                return
                    ReferenceEquals(id, other.id) &&
                    ReferenceEquals(providerType, other.providerType) &&
                    ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride) &&
                    supportsHorizontalPlaneDetection == other.supportsHorizontalPlaneDetection &&
                    supportsVerticalPlaneDetection == other.supportsVerticalPlaneDetection &&
                    supportsArbitraryPlaneDetection == other.supportsArbitraryPlaneDetection &&
                    supportsClassification == other.supportsClassification &&
                    supportsBoundaryVertices == other.supportsBoundaryVertices;
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="obj">The `object` to compare against.</param>
            /// <returns>`True` if <paramref name="obj"/> is of type <see cref="Cinfo"/> and
            /// <see cref="Equals(Cinfo)"/> also returns `true`; otherwise `false`.</returns>
            public override bool Equals(object obj)
            {
                if (!(obj is Cinfo))
                    return false;

                return Equals((Cinfo)obj);
            }

            /// <summary>
            /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = HashCodeUtil.ReferenceHash(id);
                    hashCode = (hashCode * 486187739) + HashCodeUtil.ReferenceHash(providerType);
                    hashCode = (hashCode * 486187739) + HashCodeUtil.ReferenceHash(subsystemTypeOverride);
                    hashCode = (hashCode * 486187739) + supportsHorizontalPlaneDetection.GetHashCode();
                    hashCode = (hashCode * 486187739) + supportsVerticalPlaneDetection.GetHashCode();
                    hashCode = (hashCode * 486187739) + supportsArbitraryPlaneDetection.GetHashCode();
                    hashCode = (hashCode * 486187739) + supportsBoundaryVertices.GetHashCode();
                    hashCode = (hashCode * 486187739) + supportsClassification.GetHashCode();
                    return hashCode;
                }
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
        /// <param name="cinfo">Construction info for the descriptor.</param>
        public static void Create(Cinfo cinfo)
        {
            var descriptor = new XRPlaneSubsystemDescriptor(cinfo);
            SubsystemDescriptorStore.RegisterDescriptor(descriptor);
        }

        XRPlaneSubsystemDescriptor(Cinfo cinfo)
        {
            id = cinfo.id;
            providerType = cinfo.providerType;
            subsystemTypeOverride = cinfo.subsystemTypeOverride;
            supportsHorizontalPlaneDetection = cinfo.supportsHorizontalPlaneDetection;
            supportsVerticalPlaneDetection = cinfo.supportsVerticalPlaneDetection;
            supportsArbitraryPlaneDetection = cinfo.supportsArbitraryPlaneDetection;
            supportsBoundaryVertices = cinfo.supportsBoundaryVertices;
            supportsClassification = cinfo.supportsClassification;
        }
    }
}
