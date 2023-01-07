using System;
using System.Collections.Generic;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Event arguments for the <see cref="ARReferencePointManager.referencePointsChanged"/> event.
    /// </summary>
    [Obsolete("ARReferencePointsChangedEventArgs has been deprecated. Use ARAnchorsChangedEventArgs instead (UnityUpgradable) -> UnityEngine.XR.ARFoundation.ARAnchorsChangedEventArgs", true)]
    public struct ARReferencePointsChangedEventArgs : IEquatable<ARReferencePointsChangedEventArgs>
    {
        /// <summary>
        /// The list of <see cref="ARReferencePoint"/>s added since the last event.
        /// </summary>
        public List<ARReferencePoint> added { get; private set; }

        /// <summary>
        /// The list of <see cref="ARReferencePoint"/>s udpated since the last event.
        /// </summary>
        public List<ARReferencePoint> updated { get; private set; }

        /// <summary>
        /// The list of <see cref="ARReferencePoint"/>s removed since the last event.
        /// At the time the event is invoked, the <see cref="ARReferencePoint"/>s in
        /// this list still exist. They are destroyed immediately afterward.
        /// </summary>
        public List<ARReferencePoint> removed { get; private set; }

        /// <summary>
        /// Constructs an <see cref="ARReferencePointsChangedEventArgs"/>.
        /// </summary>
        /// <param name="added">The list of <see cref="ARReferencePoint"/>s added since the last event.</param>
        /// <param name="updated">The list of <see cref="ARReferencePoint"/>s updated since the last event.</param>
        /// <param name="removed">The list of <see cref="ARReferencePoint"/>s removed since the last event.</param>
        public ARReferencePointsChangedEventArgs(
            List<ARReferencePoint> added,
            List<ARReferencePoint> updated,
            List<ARReferencePoint> removed)
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
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARReferencePointsChangedEventArgs"/> and
        /// <see cref="Equals(ARReferencePointsChangedEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ARReferencePointsChangedEventArgs))
                return false;

            return Equals((ARReferencePointsChangedEventArgs)obj);
        }

        /// <summary>
        /// Generates a string representation of this <see cref="ARReferencePointsChangedEventArgs"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="ARReferencePointsChangedEventArgs"/>.</returns>
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
        /// <param name="other">The other <see cref="ARReferencePointsChangedEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARReferencePointsChangedEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARReferencePointsChangedEventArgs other)
        {
            return
                (added == other.added) &&
                (updated == other.updated) &&
                (removed == other.removed);
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARReferencePointsChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARReferencePointsChangedEventArgs lhs, ARReferencePointsChangedEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARReferencePointsChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARReferencePointsChangedEventArgs lhs, ARReferencePointsChangedEventArgs rhs) => !lhs.Equals(rhs);
    }
}
