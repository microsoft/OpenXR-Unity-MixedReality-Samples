using System;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Describes features of an <see cref="XRObjectTrackingSubsystem"/>.
    /// </summary>
    /// <remarks>
    /// Enumerate available subsystems with <c>SubsystemManager.GetSubsystemDescriptors</c> and instantiate one by calling
    /// <c>Create</c> on one of the descriptors.
    /// Subsystem implementors can register their subsystem with
    /// <see cref="XRObjectTrackingSubsystem.Register{T}(string, XRObjectTrackingSubsystemDescriptor.Capabilities)"/>.
    /// </remarks>
    public class XRObjectTrackingSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRObjectTrackingSubsystem, XRObjectTrackingSubsystem.Provider>
    {
        /// <summary>
        /// Describes the capabilities of an <see cref="XRObjectTrackingSubsystem"/> implementation.
        /// </summary>
        public Capabilities capabilities { get; private set; }

        /// <summary>
        /// Describes the capabilities of an <see cref="XRObjectTrackingSubsystem"/> implementation.
        /// </summary>
        public struct Capabilities : IEquatable<Capabilities>
        {
            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="other">The other <see cref="Capabilities"/> to compare against.</param>
            /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="Capabilities"/>, otherwise `false`.</returns>
            public bool Equals(Capabilities other) => true;

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="obj">The `object` to compare against.</param>
            /// <returns>`True` if <paramref name="obj"/> is of type <see cref="Capabilities"/> and
            /// <see cref="Equals(Capabilities)"/> also returns `true`; otherwise `false`.</returns>
            public override bool Equals(object obj) => (obj is Capabilities capabilities) && Equals(capabilities);

            /// <summary>
            /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode() => 0;

            /// <summary>
            /// Tests for equality. Same as <see cref="Equals(Capabilities)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator ==(Capabilities lhs, Capabilities rhs) => lhs.Equals(rhs);

            /// <summary>
            /// Tests for inequality. Same as `!`<see cref="Equals(Capabilities)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator !=(Capabilities lhs, Capabilities rhs) => !lhs.Equals(rhs);
        }

        internal XRObjectTrackingSubsystemDescriptor(string id, Type providerType, Type subsystemTypeOverride, Capabilities capabilities)
        {
            this.id = id;
            this.providerType = providerType;
            this.subsystemTypeOverride = subsystemTypeOverride;
            this.capabilities = capabilities;
        }
    }
}
