using System;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Constructor info for the <see cref="XRHumanBodySubsystemDescriptor"/>.
    /// </summary>
    public struct XRHumanBodySubsystemCinfo : IEquatable<XRHumanBodySubsystemCinfo>
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
        /// Specifies the <c>XRHumanBodySubsystem</c>-derived type that forwards casted calls to its provider.
        /// </summary>
        /// <value>
        /// The type of the subsystem to use for instantiation. If null, <c>XRHumanBodySubsystem</c> will be instantiated.
        /// </value>
        public Type subsystemTypeOverride { get; set; }

        /// <summary>
        /// Specifies the provider implementation type to use for instantiation.
        /// </summary>
        /// <value>
        /// Specifies the provider implementation type to use for instantiation.
        /// </value>
        [Obsolete("XRHumanBodySubsystem no longer supports the deprecated set of base classes for subsystems as of Unity 2020.2. Use providerType and, optionally, subsystemTypeOverride instead.", true)]
        public Type implementationType { get; set; }

        /// <summary>
        /// Specifies if the current subsystem supports 2D human body pose estimation.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports 2D human body pose estimation. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsHumanBody2D { get; set; }

        /// <summary>
        /// Specifies if the current subsystem supports 3D human body pose estimation.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports 3D human body pose estimation. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsHumanBody3D { get; set; }

        /// <summary>
        /// Specifies if the current subsystem supports 3D human body scale estimation.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports 3D human body scale estimation. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsHumanBody3DScaleEstimation { get; set; }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRHumanBodySubsystemCinfo"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRHumanBodySubsystemCinfo"/>, otherwise false.</returns>
        public bool Equals(XRHumanBodySubsystemCinfo other)
        {
            return
                ReferenceEquals(id, other.id)
                && ReferenceEquals(providerType, other.providerType)
                && ReferenceEquals(subsystemTypeOverride, subsystemTypeOverride)
                && supportsHumanBody2D.Equals(other.supportsHumanBody2D)
                && supportsHumanBody3D.Equals(other.supportsHumanBody3D)
                && supportsHumanBody3DScaleEstimation.Equals(other.supportsHumanBody3DScaleEstimation);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRHumanBodySubsystemCinfo"/> and
        /// <see cref="Equals(XRHumanBodySubsystemCinfo)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj)
        {
            return ((obj is XRHumanBodySubsystemCinfo) && Equals((XRHumanBodySubsystemCinfo)obj));
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRHumanBodySubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRHumanBodySubsystemCinfo lhs, XRHumanBodySubsystemCinfo rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRHumanBodySubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRHumanBodySubsystemCinfo lhs, XRHumanBodySubsystemCinfo rhs)
        {
            return !lhs.Equals(rhs);
        }

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
                hashCode = (hashCode * 486187739) + supportsHumanBody2D.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsHumanBody3D.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsHumanBody3DScaleEstimation.GetHashCode();
            }
            return hashCode;
        }
    }

    /// <summary>
    /// The descriptor for the <see cref="XRHumanBodySubsystem"/>.
    /// </summary>
    public class XRHumanBodySubsystemDescriptor : SubsystemDescriptorWithProvider<XRHumanBodySubsystem, XRHumanBodySubsystem.Provider>
    {
        XRHumanBodySubsystemDescriptor(XRHumanBodySubsystemCinfo humanBodySubsystemCinfo)
        {
            id = humanBodySubsystemCinfo.id;
            providerType = humanBodySubsystemCinfo.providerType;
            subsystemTypeOverride = humanBodySubsystemCinfo.subsystemTypeOverride;
            supportsHumanBody2D = humanBodySubsystemCinfo.supportsHumanBody2D;
            supportsHumanBody3D = humanBodySubsystemCinfo.supportsHumanBody3D;
            supportsHumanBody3DScaleEstimation = humanBodySubsystemCinfo.supportsHumanBody3DScaleEstimation;
        }

        /// <summary>
        /// Specifies if the current subsystem supports 2D human body pose estimation.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports 2D human body pose estimation. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsHumanBody2D { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem supports 3D human body pose estimation.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports 3D human body pose estimation. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsHumanBody3D { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem supports 3D human body scale estimation.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports 3D human body scale estimation. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsHumanBody3DScaleEstimation { get; private set; }

        internal static XRHumanBodySubsystemDescriptor Create(XRHumanBodySubsystemCinfo humanBodySubsystemCinfo)
        {
            if (String.IsNullOrEmpty(humanBodySubsystemCinfo.id))
            {
                throw new ArgumentException("Cannot create human body subsystem descriptor because id is invalid",
                                            "humanBodySubsystemCinfo");
            }

            if (humanBodySubsystemCinfo.providerType == null
                || !humanBodySubsystemCinfo.providerType.IsSubclassOf(typeof(XRHumanBodySubsystem.Provider)))
            {
                throw new ArgumentException("Cannot create human body subsystem descriptor because providerType is invalid",
                                            "humanBodySubsystemCinfo");
            }

            if (humanBodySubsystemCinfo.subsystemTypeOverride != null
                && !humanBodySubsystemCinfo.subsystemTypeOverride.IsSubclassOf(typeof(XRHumanBodySubsystem)))
            {
                throw new ArgumentException("Cannot create human body subsystem descriptor because subsystemTypeOverride is invalid",
                                            "humanBodySubsystemCinfo");
            }

            return new XRHumanBodySubsystemDescriptor(humanBodySubsystemCinfo);
        }
    }
}
