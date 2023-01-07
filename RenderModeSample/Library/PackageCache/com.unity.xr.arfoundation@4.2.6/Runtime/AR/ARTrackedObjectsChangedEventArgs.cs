using System;
using System.Collections.Generic;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Event arguments for the <see cref="ARTrackedObjectManager.trackedObjectsChanged"/> event.
    /// </summary>
    public struct ARTrackedObjectsChangedEventArgs : IEquatable<ARTrackedObjectsChangedEventArgs>
    {
        /// <summary>
        /// The list of <see cref="ARTrackedObject"/>s added since the last event.
        /// </summary>
        public List<ARTrackedObject> added { get; private set; }

        /// <summary>
        /// The list of <see cref="ARTrackedObject"/>s udpated since the last event.
        /// </summary>
        public List<ARTrackedObject> updated { get; private set; }

        /// <summary>
        /// The list of <see cref="ARTrackedObject"/>s removed since the last event.
        /// </summary>
        public List<ARTrackedObject> removed { get; private set; }

        /// <summary>
        /// Constructs an <see cref="ARTrackedObjectsChangedEventArgs"/>.
        /// </summary>
        /// <param name="added">The list of <see cref="ARTrackedObject"/>s added since the last event.</param>
        /// <param name="updated">The list of <see cref="ARTrackedObject"/>s updated since the last event.</param>
        /// <param name="removed">The list of <see cref="ARTrackedObject"/>s removed since the last event.</param>
        public ARTrackedObjectsChangedEventArgs(
            List<ARTrackedObject> added,
            List<ARTrackedObject> updated,
            List<ARTrackedObject> removed)
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
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARTrackedObjectsChangedEventArgs"/> and
        /// <see cref="Equals(ARTrackedObjectsChangedEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ARTrackedObjectsChangedEventArgs))
                return false;

            return Equals((ARTrackedObjectsChangedEventArgs)obj);
        }

        /// <summary>
        /// Generates a string representation of this <see cref="ARTrackedObjectsChangedEventArgs"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="ARTrackedObjectsChangedEventArgs"/>.</returns>
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
        /// <param name="other">The other <see cref="ARTrackedObjectsChangedEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARTrackedObjectsChangedEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARTrackedObjectsChangedEventArgs other)
        {
            return
                (added == other.added) &&
                (updated == other.updated) &&
                (removed == other.removed);
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARTrackedObjectsChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARTrackedObjectsChangedEventArgs lhs, ARTrackedObjectsChangedEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARTrackedObjectsChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARTrackedObjectsChangedEventArgs lhs, ARTrackedObjectsChangedEventArgs rhs) => !lhs.Equals(rhs);
    }
}
