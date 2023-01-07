using System;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Event arguments for the <see cref="XROrigin.TrackablesParentTransformChanged"/> event.
    /// </summary>
    public readonly struct ARTrackablesParentTransformChangedEventArgs : IEquatable<ARTrackablesParentTransformChangedEventArgs>
    {
        /// <summary>
        /// (Read Only) The <see cref="XROrigin"/> whose <see cref="XROrigin.TrackablesParent"/> has
        /// changed.
        /// </summary>
        public XROrigin Origin { get; }

        /// <summary>
        /// (Read Only) The parent transform for all
        /// [ARTrackable](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest?subfolder=/api/UnityEngine.XR.ARFoundation.ARTrackable)s
        /// under a <see cref="XROrigin"/>.
        /// </summary>
        public Transform TrackablesParent { get; }

        /// <summary>
        /// Constructs an <see cref="ARTrackablesParentTransformChangedEventArgs"/>.
        /// </summary>
        /// <param name="origin">The <see cref="XROrigin"/> whose
        ///     <see cref="XROrigin.TrackablesParent"/> has changed.</param>
        /// <param name="trackablesParent">The parent transform for all
        ///     [ARTrackable](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest?subfolder=/api/UnityEngine.XR.ARFoundation.ARTrackable)s
        ///     under the <paramref name="origin"/>.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="origin"/> is `null`.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="trackablesParent"/> is `null`.</exception>
        public ARTrackablesParentTransformChangedEventArgs(XROrigin origin, Transform trackablesParent)
        {
            if (origin == null)
                throw new ArgumentNullException(nameof(origin));

            if (trackablesParent == null)
                throw new ArgumentNullException(nameof(trackablesParent));

            this.Origin = origin;
            this.TrackablesParent = trackablesParent;
        }

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare against.</param>
        /// <returns>Returns `true` if each property in <paramref name="other"/> is equal (using `==`) to the
        ///     corresponding property in this one. Returns `false` otherwise.</returns>
        public bool Equals(ARTrackablesParentTransformChangedEventArgs other) =>
            Origin == other.Origin &&
            TrackablesParent == other.TrackablesParent;

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare against.</param>
        /// <returns>Returns `true` if <paramref name="obj"/> is an
        ///     <see cref="ARTrackablesParentTransformChangedEventArgs"/> and it compares equal using
        ///     <see cref="Equals(ARTrackablesParentTransformChangedEventArgs)"/>. Returns
        ///     `false` otherwise.</returns>
        public override bool Equals(object obj) =>
            obj is ARTrackablesParentTransformChangedEventArgs other && Equals(other);

        /// <summary>
        /// Generates a hash code suitable for using in a `HashSet` or `Dictionary`.
        /// </summary>
        /// <returns>Returns a hash code suitable for using in a `HashSet` or `Dictionary`.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(
            HashCodeUtil.ReferenceHash(Origin),
            HashCodeUtil.ReferenceHash(TrackablesParent));

        /// <summary>
        /// Compares for equality. Same as
        /// <see cref="Equals(ARTrackablesParentTransformChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare with <paramref name="rhs"/>.</param>
        /// <param name="rhs">The <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare with <paramref name="lhs"/>.</param>
        /// <returns>Returns `true` if <paramref name="lhs"/> is equal to <paramref name="rhs"/> using
        ///     <see cref="Equals(ARTrackablesParentTransformChangedEventArgs)"/>. Returns
        ///     `false` otherwise.</returns>
        public static bool operator ==(ARTrackablesParentTransformChangedEventArgs lhs, ARTrackablesParentTransformChangedEventArgs rhs)
            => lhs.Equals(rhs);

        /// <summary>
        /// Compares for inequality.
        /// </summary>
        /// <param name="lhs">The <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare with <paramref name="rhs"/>.</param>
        /// <param name="rhs">The <see cref="ARTrackablesParentTransformChangedEventArgs"/> to compare with <paramref name="lhs"/>.</param>
        /// <returns>Returns `false` if <paramref name="lhs"/> is equal to <paramref name="rhs"/> using
        ///     <see cref="Equals(ARTrackablesParentTransformChangedEventArgs)"/>. Returns
        ///     `true` otherwise.</returns>
        public static bool operator !=(ARTrackablesParentTransformChangedEventArgs lhs, ARTrackablesParentTransformChangedEventArgs rhs)
            => !lhs.Equals(rhs);
    }
}
