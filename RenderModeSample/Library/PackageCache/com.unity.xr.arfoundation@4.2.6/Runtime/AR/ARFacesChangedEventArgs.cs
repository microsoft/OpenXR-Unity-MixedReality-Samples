using System;
using System.Collections.Generic;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Event arguments for the <see cref="ARFaceManager.facesChanged"/> event.
    /// </summary>
    public struct ARFacesChangedEventArgs : IEquatable<ARFacesChangedEventArgs>
    {
        /// <summary>
        /// The list of <see cref="ARFace"/>s added since the last event.
        /// </summary>
        public List<ARFace> added { get; private set; }

        /// <summary>
        /// The list of <see cref="ARFace"/>s updated since the last event.
        /// </summary>
        public List<ARFace> updated { get; private set; }

        /// <summary>
        /// The list of <see cref="ARFace"/>s removed since the last event.
        /// </summary>
        public List<ARFace> removed { get; private set; }

        /// <summary>
        /// Constructs an <see cref="ARFacesChangedEventArgs"/>.
        /// </summary>
        /// <param name="added">The list of <see cref="ARFace"/>s added since the last event.</param>
        /// <param name="updated">The list of <see cref="ARFace"/>s updated since the last event.</param>
        /// <param name="removed">The list of <see cref="ARFace"/>s removed since the last event.</param>
        public ARFacesChangedEventArgs(
            List<ARFace> added,
            List<ARFace> updated,
            List<ARFace> removed)
        {
            this.added = added;
            this.updated = updated;
            this.removed = removed;
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(
            HashCodeUtil.ReferenceHash(added),
            HashCodeUtil.ReferenceHash(updated),
            HashCodeUtil.ReferenceHash(removed));

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARFacesChangedEventArgs"/> and
        /// <see cref="Equals(ARFacesChangedEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is ARFacesChangedEventArgs other && Equals(other);

        /// <summary>
        /// Generates a string representation of this <see cref="ARFacesChangedEventArgs"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="ARFacesChangedEventArgs"/>.</returns>
        public override string ToString()
        {
            return string.Format("Added: {0}, Updated: {1}, Removed: {2}",
                added == null ? 0 : added.Count,
                updated == null ? 0 : updated.Count,
                removed == null ? 0 : removed.Count);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARFacesChangedEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARFacesChangedEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARFacesChangedEventArgs other)
        {
            return
                (added == other.added) &&
                (updated == other.updated) &&
                (removed == other.removed);
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARFacesChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARFacesChangedEventArgs lhs, ARFacesChangedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARFacesChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARFacesChangedEventArgs lhs, ARFacesChangedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
