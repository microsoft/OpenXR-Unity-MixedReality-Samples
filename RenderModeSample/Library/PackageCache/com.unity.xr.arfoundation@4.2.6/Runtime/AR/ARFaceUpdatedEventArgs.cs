using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Holds data relevant to the <see cref="ARFace.updated"/> event.
    /// </summary>
    public struct ARFaceUpdatedEventArgs : IEquatable<ARFaceUpdatedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARFace"/> component that was updated.
        /// </summary>
        public ARFace face { get; private set; }

        /// <summary>
        /// Constructor invoked by the <see cref="ARFaceManager"/> which triggered the event.
        /// </summary>
        /// <param name="face">The <see cref="ARFace"/> component that was updated.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="face"/> is `null`.</exception>
        public ARFaceUpdatedEventArgs(ARFace face)
        {
            if (face == null)
                throw new ArgumentNullException(nameof(face));

            this.face = face;
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() => HashCodeUtil.ReferenceHash(face);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARFaceUpdatedEventArgs"/> and
        /// <see cref="Equals(ARFaceUpdatedEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is ARFaceUpdatedEventArgs other && Equals(other);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARFaceUpdatedEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARFaceUpdatedEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARFaceUpdatedEventArgs other) => face == other.face;

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARFaceUpdatedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator==(ARFaceUpdatedEventArgs lhs, ARFaceUpdatedEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARFaceUpdatedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator!=(ARFaceUpdatedEventArgs lhs, ARFaceUpdatedEventArgs rhs) => !lhs.Equals(rhs);
    }
}
