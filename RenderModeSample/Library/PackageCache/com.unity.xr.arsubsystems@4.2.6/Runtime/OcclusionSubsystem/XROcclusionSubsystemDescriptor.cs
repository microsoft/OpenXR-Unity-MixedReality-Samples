using System;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Constructor parameters for the <see cref="XROcclusionSubsystemDescriptor"/>.
    /// </summary>
    public struct XROcclusionSubsystemCinfo : IEquatable<XROcclusionSubsystemCinfo>
    {
        /// <summary>
        /// Specifies an identifier for the provider implementation of the subsystem.
        /// </summary>
        /// <value>
        /// The identifier for the provider implementation of the subsystem.
        /// </value>
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
        /// Specifies the provider implementation type to use for instantiation.
        /// </summary>
        /// <value>
        /// Specifies the provider implementation type to use for instantiation.
        /// </value>
        [Obsolete("XROcclusionSubsystem no longer supports the deprecated set of base classes for subsystems as of Unity 2020.2. Use providerType and, optionally, subsystemTypeOverride instead.", true)]
        public Type implementationType { get; set; }

        /// <summary>
        /// Specifies if the current subsystem supports human segmentation stencil image. This property is deprecated.
        /// Use <see cref="humanSegmentationStencilImageSupportedDelegate"/> instead.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports human segmentation stencil image. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use " + nameof(humanSegmentationStencilImageSupportedDelegate) + " instead.")]
        public bool supportsHumanSegmentationStencilImage { get; set; }

        /// <summary>
        /// Specifies whether a subsystem supports human segmentation stencil image.
        /// </summary>
        public Func<Supported> humanSegmentationStencilImageSupportedDelegate { get; set; }

        /// <summary>
        /// Specifies if the current subsystem supports human segmentation depth image. This property is deprecated.
        /// Use <see cref="humanSegmentationDepthImageSupportedDelegate"/> instead.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports human segmentation depth image. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use " + nameof(humanSegmentationDepthImageSupportedDelegate) + " instead.")]
        public bool supportsHumanSegmentationDepthImage { get; set; }

        /// <summary>
        /// Specifies whether a subsystem supports human segmentation depth image.
        /// </summary>
        public Func<Supported> humanSegmentationDepthImageSupportedDelegate { get; set; }

        /// <summary>
        /// Specifies whether a subsystem supports temporal smoothing of the environment depth image.
        /// </summary>
        /// <value>A method delegate indicating support for temporal smoothing of the environment depth image.</value>
        public Func<Supported> environmentDepthTemporalSmoothingSupportedDelegate { get; set; }

        /// <summary>
        /// Query for whether the current subsystem supports environment depth image. This property is deprecated. Use
        /// <see cref="environmentDepthImageSupportedDelegate"/> instead.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports environment depth image. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use " + nameof(environmentDepthImageSupportedDelegate) + " instead.")]
        public Func<bool> queryForSupportsEnvironmentDepthImage { get; set; }

        /// <summary>
        /// Query for whether the current subsystem supports environment depth image.
        /// </summary>
        public Func<Supported> environmentDepthImageSupportedDelegate { get; set; }

        /// <summary>
        /// Specifies if the current subsystem supports environment depth confidence image. This property is deprecated.
        /// Use <see cref="environmentDepthConfidenceImageSupportedDelegate"/> instead.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports environment depth confidence image. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use " + nameof(environmentDepthConfidenceImageSupportedDelegate) + " instead.")]
        public Func<bool> queryForSupportsEnvironmentDepthConfidenceImage { get; set; }

        /// <summary>
        /// Specifies if the current subsystem supports environment depth confidence image.
        /// </summary>
        public Func<Supported> environmentDepthConfidenceImageSupportedDelegate { get; set; }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XROcclusionSubsystemCinfo"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XROcclusionSubsystemCinfo"/>, otherwise false.</returns>
        public bool Equals(XROcclusionSubsystemCinfo other)
        {
            return
                ReferenceEquals(id, other.id)
                && ReferenceEquals(providerType, other.providerType)
                && ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride)
#pragma warning disable CS0618 // obsolete
                && supportsHumanSegmentationStencilImage.Equals(other.supportsHumanSegmentationStencilImage)
                && supportsHumanSegmentationDepthImage.Equals(other.supportsHumanSegmentationDepthImage)
                && ReferenceEquals(queryForSupportsEnvironmentDepthImage, other.queryForSupportsEnvironmentDepthImage)
                && ReferenceEquals(queryForSupportsEnvironmentDepthConfidenceImage, other.queryForSupportsEnvironmentDepthConfidenceImage)
#pragma warning restore CS0618
                && humanSegmentationDepthImageSupportedDelegate == other.humanSegmentationDepthImageSupportedDelegate
                && humanSegmentationStencilImageSupportedDelegate == other.humanSegmentationStencilImageSupportedDelegate
                && environmentDepthImageSupportedDelegate == other.environmentDepthImageSupportedDelegate
                && environmentDepthTemporalSmoothingSupportedDelegate == other.environmentDepthTemporalSmoothingSupportedDelegate
                && environmentDepthConfidenceImageSupportedDelegate == other.environmentDepthConfidenceImageSupportedDelegate;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XROcclusionSubsystemCinfo"/> and
        /// <see cref="Equals(XROcclusionSubsystemCinfo)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj) => ((obj is XROcclusionSubsystemCinfo) && Equals((XROcclusionSubsystemCinfo)obj));

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XROcclusionSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XROcclusionSubsystemCinfo lhs, XROcclusionSubsystemCinfo rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XROcclusionSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XROcclusionSubsystemCinfo lhs, XROcclusionSubsystemCinfo rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            int hashCode = 486187739;
            unchecked
            {
                hashCode = (hashCode * 486187739) + HashCodeUtil.ReferenceHash(id);
                hashCode = (hashCode * 486187739) + HashCodeUtil.ReferenceHash(providerType);
                hashCode = (hashCode * 486187739) + HashCodeUtil.ReferenceHash(subsystemTypeOverride);
                hashCode = HashCodeUtil.Combine(hashCode,
#pragma warning disable CS0618 // obsolete
                    supportsHumanSegmentationStencilImage.GetHashCode(),
                    supportsHumanSegmentationDepthImage.GetHashCode(),
                    HashCodeUtil.ReferenceHash(queryForSupportsEnvironmentDepthImage),
                    HashCodeUtil.ReferenceHash(queryForSupportsEnvironmentDepthConfidenceImage),
#pragma warning restore CS0618
                    HashCodeUtil.ReferenceHash(humanSegmentationStencilImageSupportedDelegate),
                    HashCodeUtil.ReferenceHash(humanSegmentationDepthImageSupportedDelegate),
                    HashCodeUtil.ReferenceHash(environmentDepthImageSupportedDelegate),
                    HashCodeUtil.ReferenceHash(environmentDepthTemporalSmoothingSupportedDelegate),
                    HashCodeUtil.ReferenceHash(environmentDepthConfidenceImageSupportedDelegate));
            }
            return hashCode;
        }
    }

    /// <summary>
    /// Descriptor for the XROcclusionSubsystem.
    /// </summary>
    public class XROcclusionSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XROcclusionSubsystem, XROcclusionSubsystem.Provider>
    {
        XROcclusionSubsystemDescriptor(XROcclusionSubsystemCinfo occlusionSubsystemCinfo)
        {
            id = occlusionSubsystemCinfo.id;
            providerType = occlusionSubsystemCinfo.providerType;
            subsystemTypeOverride = occlusionSubsystemCinfo.subsystemTypeOverride;
#pragma warning disable CS0618 // obsolete
            m_SupportsHumanSegmentationStencilImage = occlusionSubsystemCinfo.supportsHumanSegmentationStencilImage;
            m_SupportsHumanSegmentationDepthImage = occlusionSubsystemCinfo.supportsHumanSegmentationDepthImage;
            m_QueryForSupportsEnvironmentDepthImage = occlusionSubsystemCinfo.queryForSupportsEnvironmentDepthImage;
            m_QueryForSupportsEnvironmentDepthConfidenceImage = occlusionSubsystemCinfo.queryForSupportsEnvironmentDepthConfidenceImage;
#pragma warning restore CS0618
            m_EnvironmentDepthImageSupportedDelegate = occlusionSubsystemCinfo.environmentDepthImageSupportedDelegate;
            m_EnvironmentDepthConfidenceImageSupportedDelegate = occlusionSubsystemCinfo.environmentDepthConfidenceImageSupportedDelegate;
            m_HumanSegmentationStencilImageSupportedDelegate = occlusionSubsystemCinfo.humanSegmentationStencilImageSupportedDelegate;
            m_HumanSegmentationDepthImageSupportedDelegate = occlusionSubsystemCinfo.humanSegmentationDepthImageSupportedDelegate;
            m_EnvironmentDepthTemporalSmoothingSupportedDelegate = occlusionSubsystemCinfo.environmentDepthTemporalSmoothingSupportedDelegate;
        }

        /// <summary>
        /// Query for whether environment depth is supported.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports environment depth image. Otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// On some platforms, this is a runtime check that requires an active session.
        /// </remarks>
        [Obsolete("Use " + nameof(m_EnvironmentDepthImageSupportedDelegate) + " instead.")]
        Func<bool> m_QueryForSupportsEnvironmentDepthImage;

        /// <summary>
        /// Query for whether environment depth is supported.
        /// </summary>
        Func<Supported> m_EnvironmentDepthImageSupportedDelegate;

        /// <summary>
        /// Query for whether environment depth confidence is supported.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports environment depth confidence image. Otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// On some platforms, this is a runtime check that requires an active session.
        /// </remarks>
        [Obsolete("Use " + nameof(m_EnvironmentDepthConfidenceImageSupportedDelegate) + " instead.")]
        Func<bool> m_QueryForSupportsEnvironmentDepthConfidenceImage;

        /// <summary>
        /// Query for whether environment depth confidence is supported.
        /// </summary>
        Func<Supported> m_EnvironmentDepthConfidenceImageSupportedDelegate;

        /// <summary>
        /// Specifies if the current subsystem supports human segmentation stencil image. This property is deprecated.
        /// Use <see cref="humanSegmentationStencilImageSupported"/> instead.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > This is a runtime check which might require some initialization to determine support. During this period,
        /// > this property may return `false` for a time before becoming `true`.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the current subsystem supports human segmentation stencil image. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use " + nameof(humanSegmentationStencilImageSupported) + " instead.")]
        public bool supportsHumanSegmentationStencilImage => humanSegmentationStencilImageSupported == Supported.Supported;
        bool m_SupportsHumanSegmentationStencilImage;

        /// <summary>
        /// (Read Only) Whether a subsystem supports human segmentation stencil image.
        /// </summary>
        public Supported humanSegmentationStencilImageSupported
        {
            get
            {
                if (m_HumanSegmentationStencilImageSupportedDelegate != null)
                {
                    return m_HumanSegmentationStencilImageSupportedDelegate();
                }

                return m_SupportsHumanSegmentationStencilImage ? Supported.Supported : Supported.Unsupported;
            }
        }
        Func<Supported> m_HumanSegmentationStencilImageSupportedDelegate;

        /// <summary>
        /// Specifies if the current subsystem supports human segmentation depth image. This property is deprecated.
        /// Use <see cref="humanSegmentationDepthImageSupported"/> instead.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > This is a runtime check which might require some initialization to determine support. During this period,
        /// > this property might return `false` for a time before becoming `true`.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the current subsystem supports human segmentation depth image. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use " + nameof(humanSegmentationDepthImageSupported) + " instead.")]
        public bool supportsHumanSegmentationDepthImage => humanSegmentationDepthImageSupported == Supported.Supported;
        bool m_SupportsHumanSegmentationDepthImage;

        /// <summary>
        /// (Read Only) Whether a subsystem supports human segmentation depth image.
        /// </summary>
        public Supported humanSegmentationDepthImageSupported
        {
            get
            {
                if (m_HumanSegmentationDepthImageSupportedDelegate != null)
                {
                    return m_HumanSegmentationDepthImageSupportedDelegate();
                }

                return m_SupportsHumanSegmentationDepthImage ? Supported.Supported : Supported.Unsupported;
            }
        }

        Func<Supported> m_HumanSegmentationDepthImageSupportedDelegate;

        /// <summary>
        /// Specifies if the current subsystem supports environment depth image. This property is deprecated.
        /// Use <see cref="environmentDepthImageSupported"/> instead.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > This is a runtime check which might require some initialization to determine support. During this period,
        /// > this property might return `false` for a time before becoming `true`.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the current subsystem supports environment depth image. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use " + nameof(environmentDepthImageSupported) + " instead.")]
        public bool supportsEnvironmentDepthImage => environmentDepthImageSupported == Supported.Supported;

        /// <summary>
        /// (Read Only) Whether the subsystem supports environment depth image.
        /// </summary>
        /// <remarks>
        /// The supported status might take time to determine. If support is still being determined, the value will be <see cref="Supported.Unknown"/>.
        /// </remarks>
        public Supported environmentDepthImageSupported
        {
            get
            {
                if (m_EnvironmentDepthImageSupportedDelegate != null)
                {
                    return m_EnvironmentDepthImageSupportedDelegate();
                }

#pragma warning disable CS0618 // obsolete
                // Check deprecated fallback
                return m_QueryForSupportsEnvironmentDepthImage?.Invoke() == true
                    ? Supported.Supported
                    : Supported.Unsupported;
#pragma warning restore CS0618
            }
        }

        Func<Supported> m_EnvironmentDepthTemporalSmoothingSupportedDelegate;

        /// <summary>
        /// Whether temporal smoothing of the environment image is supported.
        /// </summary>
        /// <value>Read Only.</value>
        public Supported environmentDepthTemporalSmoothingSupported =>
            m_EnvironmentDepthTemporalSmoothingSupportedDelegate?.Invoke() ?? Supported.Unsupported;

        /// <summary>
        /// Specifies if the current subsystem supports environment depth confidence image. This property is deprecated.
        /// Use <see cref="environmentDepthConfidenceImageSupported"/> instead.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > This is a runtime check which might require some initialization to determine support. During this period,
        /// > this property might return `false` for a time before becoming `true`.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the current subsystem supports environment depth confidence image. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use " + nameof(environmentDepthConfidenceImageSupported) + " instead.")]
        public bool supportsEnvironmentDepthConfidenceImage => environmentDepthConfidenceImageSupported == Supported.Supported;

        /// <summary>
        /// (Read Only) Whether the subsystem supports environment depth confidence image.
        /// </summary>
        /// <remarks>
        /// The supported status might take time to determine. If support is still being determined, the value will be <see cref="Supported.Unknown"/>.
        /// </remarks>
        public Supported environmentDepthConfidenceImageSupported
        {
            get
            {
                if (m_EnvironmentDepthConfidenceImageSupportedDelegate != null)
                {
                    return m_EnvironmentDepthConfidenceImageSupportedDelegate();
                }

#pragma warning disable CS0618 // obsolete
                // Check deprecated fallback
                return m_QueryForSupportsEnvironmentDepthConfidenceImage?.Invoke() == true
                    ? Supported.Supported
                    : Supported.Unsupported;
#pragma warning restore CS0618
            }
        }

        /// <summary>
        /// Creates the occlusion subsystem descriptor from the construction info.
        /// </summary>
        /// <param name="occlusionSubsystemCinfo">The occlusion subsystem descriptor constructor information.</param>
        internal static XROcclusionSubsystemDescriptor Create(XROcclusionSubsystemCinfo occlusionSubsystemCinfo)
        {
            if (string.IsNullOrEmpty(occlusionSubsystemCinfo.id))
            {
                throw new ArgumentException("Cannot create occlusion subsystem descriptor because id is invalid",
                                            nameof(occlusionSubsystemCinfo));
            }

            if (occlusionSubsystemCinfo.providerType == null
                || !occlusionSubsystemCinfo.providerType.IsSubclassOf(typeof(XROcclusionSubsystem.Provider)))
            {
                throw new ArgumentException("Cannot create occlusion subsystem descriptor because providerType is invalid",
                                            nameof(occlusionSubsystemCinfo));
            }

            if (occlusionSubsystemCinfo.subsystemTypeOverride == null
                || !occlusionSubsystemCinfo.subsystemTypeOverride.IsSubclassOf(typeof(XROcclusionSubsystem)))
            {
                throw new ArgumentException("Cannot create occlusion subsystem descriptor because subsystemTypeOverride is invalid",
                                            nameof(occlusionSubsystemCinfo));
            }

            return new XROcclusionSubsystemDescriptor(occlusionSubsystemCinfo);
        }
    }
}
