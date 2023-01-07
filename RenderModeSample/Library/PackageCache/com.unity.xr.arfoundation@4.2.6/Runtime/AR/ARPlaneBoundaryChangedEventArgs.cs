using System;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Data associated with an <see cref="ARPlane.boundaryChanged" /> event.
    /// </summary>
    public struct ARPlaneBoundaryChangedEventArgs : IEquatable<ARPlaneBoundaryChangedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARPlane" /> which triggered the event.
        /// </summary>
        public ARPlane plane { get; private set; }

        /// <summary>
        /// Constructor for plane changed events.
        /// This is normally only used by the <see cref="ARPlane"/> component for <see cref="ARPlane.boundaryChanged"/> events.
        /// </summary>
        /// <param name="plane">The <see cref="ARPlane"/> that triggered the event.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="plane"/> is `null`.</exception>
        public ARPlaneBoundaryChangedEventArgs(ARPlane plane)
        {
            if (plane == null)
                throw new ArgumentNullException(nameof(plane));

            this.plane = plane;
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() => HashCodeUtil.ReferenceHash(plane);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARPlaneBoundaryChangedEventArgs"/> and
        /// <see cref="Equals(ARPlaneBoundaryChangedEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ARPlaneBoundaryChangedEventArgs))
                return false;

            return Equals((ARPlaneBoundaryChangedEventArgs)obj);
        }

        /// <summary>
        /// Generates a string representation fo this <see cref="ARPlaneBoundaryChangedEventArgs"/>.
        /// </summary>
        /// <returns>A string representation fo this <see cref="ARPlaneBoundaryChangedEventArgs"/>.</returns>
        public override string ToString() => $"ARPlane {(plane ? plane.trackableId.ToString() : "(null)")} boundary updated";

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARPlaneBoundaryChangedEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARPlaneBoundaryChangedEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARPlaneBoundaryChangedEventArgs other) => plane == other.plane;

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARPlaneBoundaryChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARPlaneBoundaryChangedEventArgs lhs, ARPlaneBoundaryChangedEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARPlaneBoundaryChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARPlaneBoundaryChangedEventArgs lhs, ARPlaneBoundaryChangedEventArgs rhs) => !lhs.Equals(rhs);
    }
}
