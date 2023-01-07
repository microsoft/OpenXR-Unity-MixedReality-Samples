using System;
using System.Collections.Generic;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Event arguments for the <see cref="AREnvironmentProbeManager.environmentProbesChanged"/> event.
    /// </summary>
    public struct AREnvironmentProbesChangedEvent : IEquatable<AREnvironmentProbesChangedEvent>
    {
        /// <summary>
        /// The list of <see cref="AREnvironmentProbe"/>s added since the last event.
        /// </summary>
        public List<AREnvironmentProbe> added { get; private set; }

        /// <summary>
        /// The list of <see cref="AREnvironmentProbe"/>s udpated since the last event.
        /// </summary>
        public List<AREnvironmentProbe> updated { get; private set; }

        /// <summary>
        /// The list of <see cref="AREnvironmentProbe"/>s removed since the last event.
        /// </summary>
        public List<AREnvironmentProbe> removed { get; private set; }

        /// <summary>
        /// Constructs an <see cref="AREnvironmentProbesChangedEvent"/>.
        /// </summary>
        /// <param name="added">The list of <see cref="AREnvironmentProbe"/>s added since the last event.</param>
        /// <param name="updated">The list of <see cref="AREnvironmentProbe"/>s updated since the last event.</param>
        /// <param name="removed">The list of <see cref="AREnvironmentProbe"/>s removed since the last event.</param>
        public AREnvironmentProbesChangedEvent(
            List<AREnvironmentProbe> added,
            List<AREnvironmentProbe> updated,
            List<AREnvironmentProbe> removed)
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
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="AREnvironmentProbesChangedEvent"/> and
        /// <see cref="Equals(AREnvironmentProbesChangedEvent)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => (obj is AREnvironmentProbesChangedEvent other) && Equals(other);

        /// <summary>
        /// Generates a string representation of this <see cref="AREnvironmentProbesChangedEvent"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="AREnvironmentProbesChangedEvent"/>.</returns>
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
        /// <param name="other">The other <see cref="AREnvironmentProbesChangedEvent"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="AREnvironmentProbesChangedEvent"/>, otherwise false.</returns>
        public bool Equals(AREnvironmentProbesChangedEvent other)
        {
            return
                ReferenceEquals(added, other.added) &&
                ReferenceEquals(updated, other.updated) &&
                ReferenceEquals(removed, other.removed);
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(AREnvironmentProbesChangedEvent)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(AREnvironmentProbesChangedEvent lhs, AREnvironmentProbesChangedEvent rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(AREnvironmentProbesChangedEvent)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(AREnvironmentProbesChangedEvent lhs, AREnvironmentProbesChangedEvent rhs) => !lhs.Equals(rhs);
    }
}
