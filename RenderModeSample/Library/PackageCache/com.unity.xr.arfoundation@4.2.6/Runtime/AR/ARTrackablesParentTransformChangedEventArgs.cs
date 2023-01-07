using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Event arguments for the <see cref="ARSessionOrigin.trackablesParentTransformChanged"/> event.
    /// </summary>
    public readonly struct ARTrackablesParentTransformChangedEventArgs : IEquatable<ARTrackablesParentTransformChangedEventArgs>
    {
        /// <summary>
        /// (Read Only) The <see cref="ARSessionOrigin"/> whose <see cref="ARSessionOrigin.trackablesParent"/> has
        /// changed.
        /// </summary>
        public ARSessionOrigin sessionOrigin { get; }

        /// <summary>
        /// (Read Only) The parent transform for all <see cref="ARTrackable"/>s under a <see cref="ARSessionOrigin"/>.
        /// </summary>
        public Transform trackablesParent { get; }

        /// <summary>
        /// Constructs an <see cref="ARTrackablesParentTransformChangedEventArgs"/>.
        /// </summary>
        /// <param name="sessionOrigin">The <see cref="ARSessionOrigin"/> whose
        ///     <see cref="ARSessionOrigin.trackablesParent"/> has changed.</param>
        /// <param name="trackablesParent">The parent transform for all <see cref="ARTrackable"/>s under the
        ///     <paramref name="sessionOrigin"/>.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="sessionOrigin"/> is `null`.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="trackablesParent"/> is `null`.</exception>
        public ARTrackablesParentTransformChangedEventArgs(ARSessionOrigin sessionOrigin, Transform trackablesParent)
        {
            if (sessionOrigin == null)
                throw new ArgumentNullException(nameof(sessionOrigin));

            if (trackablesParent == null)
                throw new ArgumentNullException(nameof(trackablesParent));

            this.sessionOrigin = sessionOrigin;
            this.trackablesParent = trackablesParent;
        }

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare against.</param>
        /// <returns>Returns `true` if each property in <paramref name="other"/> is equal (using `==`) to the
        ///     corresponding property in this one. Returns `false` otherwise.</returns>
        public bool Equals(ARTrackablesParentTransformChangedEventArgs other) =>
            sessionOrigin == other.sessionOrigin &&
            trackablesParent == other.trackablesParent;

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare against.</param>
        /// <returns>Returns `true` if <paramref name="obj"/> is an
        ///     <see cref="ARTrackablesParentTransformChangedEventArgs"/> and it compares equal using
        ///     <see cref="Equals(UnityEngine.XR.ARFoundation.ARTrackablesParentTransformChangedEventArgs)"/>. Returns
        ///     `false` otherwise.</returns>
        public override bool Equals(object obj) =>
            obj is ARTrackablesParentTransformChangedEventArgs other && Equals(other);

        /// <summary>
        /// Generates a hash code suitable for using in a `HashSet` or `Dictionary`.
        /// </summary>
        /// <returns>Returns a hash code suitable for using in a `HashSet` or `Dictionary`.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(
            HashCodeUtil.ReferenceHash(sessionOrigin),
            HashCodeUtil.ReferenceHash(trackablesParent));

        /// <summary>
        /// Compares for equality. Same as
        /// <see cref="Equals(UnityEngine.XR.ARFoundation.ARTrackablesParentTransformChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare with <paramref name="rhs"/>.</param>
        /// <param name="rhs">The <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare with <paramref name="lhs"/>.</param>
        /// <returns>Returns `true` if <paramref name="lhs"/> is equal to <paramref name="rhs"/> using
        ///     <see cref="Equals(UnityEngine.XR.ARFoundation.ARTrackablesParentTransformChangedEventArgs)"/>. Returns
        ///     `false` otherwise.</returns>
        public static bool operator ==(ARTrackablesParentTransformChangedEventArgs lhs, ARTrackablesParentTransformChangedEventArgs rhs)
            => lhs.Equals(rhs);

        /// <summary>
        /// Compares for inequality.
        /// </summary>
        /// <param name="lhs">The <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare with <paramref name="rhs"/>.</param>
        /// <param name="rhs">The <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare with <paramref name="lhs"/>.</param>
        /// <returns>Returns `false` if <paramref name="lhs"/> is equal to <paramref name="rhs"/> using
        ///     <see cref="Equals(UnityEngine.XR.ARFoundation.ARTrackablesParentTransformChangedEventArgs)"/>. Returns
        ///     `true` otherwise.</returns>
        public static bool operator !=(ARTrackablesParentTransformChangedEventArgs lhs, ARTrackablesParentTransformChangedEventArgs rhs)
            => !lhs.Equals(rhs);
    }
}
