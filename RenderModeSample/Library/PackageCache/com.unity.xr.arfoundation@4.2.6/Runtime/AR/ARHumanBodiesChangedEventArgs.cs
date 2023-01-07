using System;
using System.Collections.Generic;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Container for the changed <see cref="ARHumanBodyManager.humanBodiesChanged"/> of the event.
    /// </summary>
    public struct ARHumanBodiesChangedEventArgs : IEquatable<ARHumanBodiesChangedEventArgs>
    {
        /// <summary>
        /// The list of <see cref="ARHumanBody"/>s added since the last event.
        /// </summary>
        public List<ARHumanBody> added { get; private set; }

        /// <summary>
        /// The list of <see cref="ARHumanBody"/>s udpated since the last event.
        /// </summary>
        public List<ARHumanBody> updated { get; private set; }

        /// <summary>
        /// The list of <see cref="ARHumanBody"/>s removed since the last event.
        /// </summary>
        public List<ARHumanBody> removed { get; private set; }

        /// <summary>
        /// Constructs an <see cref="ARHumanBodiesChangedEventArgs"/>.
        /// </summary>
        /// <param name="added">The list of <see cref="ARHumanBody"/>s added since the last event.</param>
        /// <param name="updated">The list of <see cref="ARHumanBody"/>s updated since the last event.</param>
        /// <param name="removed">The list of <see cref="ARHumanBody"/>s removed since the last event.</param>
        public ARHumanBodiesChangedEventArgs(
            List<ARHumanBody> added,
            List<ARHumanBody> updated,
            List<ARHumanBody> removed)
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
        /// <param name="other">The other <see cref="ARHumanBodiesChangedEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARHumanBodiesChangedEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARHumanBodiesChangedEventArgs other) =>
            (added == other.added) &&
            (updated == other.updated) &&
            (removed == other.removed);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARHumanBodiesChangedEventArgs"/> and
        /// <see cref="Equals(ARHumanBodiesChangedEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => (obj is ARHumanBodiesChangedEventArgs other) && Equals(other);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARHumanBodiesChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARHumanBodiesChangedEventArgs lhs, ARHumanBodiesChangedEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARHumanBodiesChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARHumanBodiesChangedEventArgs lhs, ARHumanBodiesChangedEventArgs rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Generates a string representation of this <see cref="ARHumanBodiesChangedEventArgs"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="ARHumanBodiesChangedEventArgs"/>.</returns>
        public override string ToString()
        {
            return string.Format("Added: {0}, Updated: {1}, Removed: {2}",
                added == null ? 0 : added.Count,
                updated == null ? 0 : updated.Count,
                removed == null ? 0 : removed.Count);

        }
    }
}
