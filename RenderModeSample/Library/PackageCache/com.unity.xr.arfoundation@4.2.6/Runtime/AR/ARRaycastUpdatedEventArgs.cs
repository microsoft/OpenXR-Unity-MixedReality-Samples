using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Provides arguments for the <see cref="ARRaycast.updated"/> event.
    /// </summary>
    public struct ARRaycastUpdatedEventArgs : IEquatable<ARRaycastUpdatedEventArgs>
    {
        /// <summary>
        /// The raycast that has been updated.
        /// </summary>
        public ARRaycast raycast { get; internal set; }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The event args to compare for equality.</param>
        /// <returns>`True` if all properties are the same; `false` otherwise.</returns>
        public bool Equals(ARRaycastUpdatedEventArgs other) => ReferenceEquals(raycast, other.raycast);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare with this object.</param>
        /// <returns>`True` if <paramref name="obj"/> is an <see cref="ARRaycastUpdatedEventArgs"/> and
        /// <see cref="Equals(UnityEngine.XR.ARFoundation.ARRaycastUpdatedEventArgs)"/> is `true`, otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is ARRaycastUpdatedEventArgs other && Equals(other);

        /// <summary>
        /// Computes a hash code from all properties suitable for use in a `Dictionary` or `HashSet`.
        /// </summary>
        /// <returns>A hashcode of this object.</returns>
        public override int GetHashCode() => raycast?.GetHashCode() ?? 0;

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(UnityEngine.XR.ARFoundation.ARRaycastUpdatedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Same as <see cref="Equals(UnityEngine.XR.ARFoundation.ARRaycastUpdatedEventArgs)"/></returns>
        public static bool operator ==(ARRaycastUpdatedEventArgs lhs, ARRaycastUpdatedEventArgs rhs) => lhs.Equals(rhs);
        
        /// <summary>
        /// Tests for inequality. Same as !<see cref="Equals(UnityEngine.XR.ARFoundation.ARRaycastUpdatedEventArgs)"/>
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>The negation of <see cref="Equals(UnityEngine.XR.ARFoundation.ARRaycastUpdatedEventArgs)"/></returns>
        public static bool operator !=(ARRaycastUpdatedEventArgs lhs, ARRaycastUpdatedEventArgs rhs) => !lhs.Equals(rhs);
    }
}
