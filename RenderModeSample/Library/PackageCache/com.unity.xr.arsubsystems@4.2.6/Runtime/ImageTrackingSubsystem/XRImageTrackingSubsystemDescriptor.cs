using System;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Describes the capabilities of an <see cref="XRImageTrackingSubsystem"/>.
    /// </summary>
    public class XRImageTrackingSubsystemDescriptor : SubsystemDescriptorWithProvider<XRImageTrackingSubsystem, XRImageTrackingSubsystem.Provider>
    {
        /// <summary>
        /// Construction information for the <see cref="XRImageTrackingSubsystemDescriptor"/>.
        /// </summary>
        public struct Cinfo : IEquatable<Cinfo>
        {
            /// <summary>
            /// A string identifier used to name the subsystem provider.
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
            /// Specifies the <c>XRImageTrackingSubsystem</c>-derived type that forwards casted calls to its provider.
            /// </summary>
            /// <value>
            /// The type of the subsystem to use for instantiation. If null, <c>XRImageTrackingSubsystem</c> will be instantiated.
            /// </value>
            public Type subsystemTypeOverride { get; set; }

            /// <summary>
            /// The <c>System.Type</c> of the provider implementation, used to instantiate the class.
            /// </summary>
            [Obsolete("XRImageTrackingSubsystem no longer supports the deprecated set of base classes for subsystems as of Unity 2020.2. Use providerType and, optionally, subsystemTypeOverride instead.", true)]
            public Type subsystemImplementationType { get; set; }

            /// <summary>
            /// Whether the subsystem supports tracking the poses of moving images in realtime.
            /// </summary>
            /// <remarks>
            /// If <c>true</c>,
            /// <see cref="XRImageTrackingSubsystem.Provider.currentMaxNumberOfMovingImages"/> and
            /// <see cref="XRImageTrackingSubsystem.Provider.requestedMaxNumberOfMovingImages"/>
            /// should be implemented.
            /// </remarks>
            public bool supportsMovingImages { get; set; }

            /// <summary>
            /// Whether the subsystem requires physical image dimensions to be provided for all reference images.
            /// If <c>false</c>, specifying the physical dimensions is optional.
            /// </summary>
            public bool requiresPhysicalImageDimensions { get; set; }

            /// <summary>
            /// Whether the subsystem supports image libraries that can be mutated at runtime.
            /// </summary>
            /// <remarks>
            /// If <c>true</c>,
            /// <see cref="XRImageTrackingSubsystem.Provider.CreateRuntimeLibrary(XRReferenceImageLibrary)"/>
            /// must be implemented and
            /// <see cref="XRImageTrackingSubsystem.Provider.imageLibrary"/>
            /// will never be called.
            /// </remarks>
            /// <seealso cref="MutableRuntimeReferenceImageLibrary"/>
            public bool supportsMutableLibrary { get; set; }

            /// <summary>
            /// Whether the subsystem supports image validation (validating images before they are added to a
            /// <see cref="MutableRuntimeReferenceImageLibrary"/>).
            /// </summary>
            public bool supportsImageValidation { get; set; }

            /// <summary>
            /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = HashCodeUtil.ReferenceHash(id);
                    hashCode = hashCode * 486187739 + HashCodeUtil.ReferenceHash(providerType);
                    hashCode = hashCode * 486187739 + HashCodeUtil.ReferenceHash(subsystemTypeOverride);
                    hashCode = hashCode * 486187739 + supportsMovingImages.GetHashCode();
                    hashCode = hashCode * 486187739 + requiresPhysicalImageDimensions.GetHashCode();
                    hashCode = hashCode * 486187739 + supportsMutableLibrary.GetHashCode();
                    hashCode = hashCode * 486187739 + supportsImageValidation.GetHashCode();
                    return hashCode;
                }
            }

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
                    supportsMovingImages == other.supportsMovingImages &&
                    requiresPhysicalImageDimensions == other.requiresPhysicalImageDimensions &&
                    supportsMutableLibrary == other.supportsMutableLibrary &&
                    supportsImageValidation == other.supportsImageValidation;
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="obj">The `object` to compare against.</param>
            /// <returns>`True` if <paramref name="obj"/> is of type <see cref="Cinfo"/> and
            /// <see cref="Equals(Cinfo)"/> also returns `true`; otherwise `false`.</returns>
            public override bool Equals(object obj) => (obj is Cinfo) && Equals((Cinfo)obj);

            /// <summary>
            /// Tests for equality. Same as <see cref="Equals(Cinfo)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator==(Cinfo lhs, Cinfo rhs) => lhs.Equals(rhs);

            /// <summary>
            /// Tests for inequality. Same as `!`<see cref="Equals(Cinfo)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator!=(Cinfo lhs, Cinfo rhs) => !lhs.Equals(rhs);
        }

        /// <summary>
        /// <c>True</c> if the subsystem supports tracking the poses of moving images in real time.
        /// </summary>
        public bool supportsMovingImages { get; }

        /// <summary>
        /// <c>True</c> if the subsystem requires physical image dimensions to be provided for all reference images.
        /// If <c>false</c>, specifying the physical dimensions is optional.
        /// </summary>
        public bool requiresPhysicalImageDimensions { get; }

        /// <summary>
        /// Whether the subsystem supports <see cref="MutableRuntimeReferenceImageLibrary"/>, a reference
        /// image library which can modified at runtime, as opposed to the <see cref="XRReferenceImageLibrary"/>,
        /// which is generated at edit time and cannot be modified at runtime.
        /// </summary>
        /// <seealso cref="MutableRuntimeReferenceImageLibrary"/>
        /// <seealso cref="XRImageTrackingSubsystem.CreateRuntimeLibrary(XRReferenceImageLibrary)"/>
        public bool supportsMutableLibrary { get; }

        /// <summary>
        /// Whether the subsystem supports image validation (validating images before they are added to a
        /// <see cref="MutableRuntimeReferenceImageLibrary"/>).
        /// </summary>
        public bool supportsImageValidation { get; }

        /// <summary>
        /// Registers a new descriptor with the <c>SubsystemManager</c>.
        /// </summary>
        /// <param name="cinfo">The construction information for the new descriptor.</param>
        public static void Create(Cinfo cinfo)
        {
            SubsystemDescriptorStore.RegisterDescriptor(new XRImageTrackingSubsystemDescriptor(cinfo));
        }

        XRImageTrackingSubsystemDescriptor(Cinfo cinfo)
        {
            id = cinfo.id;
            providerType = cinfo.providerType;
            subsystemTypeOverride = cinfo.subsystemTypeOverride;
            supportsMovingImages = cinfo.supportsMovingImages;
            requiresPhysicalImageDimensions = cinfo.requiresPhysicalImageDimensions;
            supportsMutableLibrary = cinfo.supportsMutableLibrary;
            supportsImageValidation = cinfo.supportsImageValidation;
        }
    }
}
