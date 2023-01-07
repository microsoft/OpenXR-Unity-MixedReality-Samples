using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// The arguments for the <see cref="ARPointCloud.updated"/>
    /// event. This is currently empty, but it might change in the future without the need to change the
    /// subscribers' method signatures.
    /// </summary>
    public struct ARPointCloudUpdatedEventArgs : IEquatable<ARPointCloudUpdatedEventArgs>
    {
        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() => 0;

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARPointCloudUpdatedEventArgs"/> and
        /// <see cref="Equals(ARPointCloudUpdatedEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ARPointCloudUpdatedEventArgs))
                return false;

            return Equals((ARPointCloudUpdatedEventArgs)obj);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARPointCloudUpdatedEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARPointCloudUpdatedEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARPointCloudUpdatedEventArgs other) => true;

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARPointCloudUpdatedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARPointCloudUpdatedEventArgs lhs, ARPointCloudUpdatedEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARPointCloudUpdatedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARPointCloudUpdatedEventArgs lhs, ARPointCloudUpdatedEventArgs rhs) => !lhs.Equals(rhs);
    }
}
