using System;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Represents the result of a raycast intersection with a trackable.
    /// </summary>
    public struct ARRaycastHit : IEquatable<ARRaycastHit>, IComparable<ARRaycastHit>
    {
        /// <summary>
        /// Constructor invoked by <see cref="ARRaycastManager.Raycast(Vector2, System.Collections.Generic.List{ARRaycastHit}, TrackableType)"/>
        /// and <see cref="ARRaycastManager.Raycast(Ray, System.Collections.Generic.List{ARRaycastHit}, TrackableType)"/>.
        /// </summary>
        /// <param name="hit">Session-relative raycast hit data.</param>
        /// <param name="distance">The distance, in Unity world space, of the hit.</param>
        /// <param name="transform">The <c>Transform</c> that transforms from session space to world space.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="transform"/> is `null`.</exception>
        [Obsolete("Use ARRaycastHit(XRRaycastHit, float, Transform, ARTrackable) instead. (2020-10-09)")]
        public ARRaycastHit(XRRaycastHit hit, float distance, Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            m_Hit = hit;
            this.distance = distance;
            m_Transform = transform;
            trackable = null;
        }

        /// <summary>
        /// Constructor invoked by <see cref="ARRaycastManager.Raycast(Vector2, System.Collections.Generic.List{ARRaycastHit}, TrackableType)"/>
        /// and <see cref="ARRaycastManager.Raycast(Ray, System.Collections.Generic.List{ARRaycastHit}, TrackableType)"/>.
        /// </summary>
        /// <param name="hit">Session-relative raycast hit data.</param>
        /// <param name="distance">The distance, in Unity world space, of the hit.</param>
        /// <param name="transform">The <c>Transform</c> that transforms from session space to world space.</param>
        /// <param name="trackable">The trackable that was hit by this raycast, or `null` if no trackable was hit.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="transform"/> is `null`.</exception>
        public ARRaycastHit(XRRaycastHit hit, float distance, Transform transform, ARTrackable trackable)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            m_Hit = hit;
            this.distance = distance;
            m_Transform = transform;
            this.trackable = trackable;
        }

        /// <summary>
        /// The distance, in Unity world space, from the ray origin to the intersection point.
        /// </summary>
        public float distance { get; }

        /// <summary>
        /// The type of trackable hit by the raycast.
        /// </summary>
        public TrackableType hitType => m_Hit.hitType;

        /// <summary>
        /// The <c>Pose</c>, in Unity world space, of the intersection point.
        /// </summary>
        public Pose pose => m_Transform.TransformPose(sessionRelativePose);

        /// <summary>
        /// The session-unique identifier for the trackable that was hit.
        /// </summary>
        public TrackableId trackableId => m_Hit.trackableId;

        /// <summary>
        /// The <c>Pose</c>, in local (session) space, of the intersection point.
        /// </summary>
        public Pose sessionRelativePose => m_Hit.pose;

        /// <summary>
        /// The distance, in local (session) space, from the ray origin to the intersection point.
        /// </summary>
        public float sessionRelativeDistance => m_Hit.distance;

        /// <summary>
        /// The <see cref="ARTrackable"/> that this raycast hit, or `null` if no <see cref="ARTrackable"/> was hit.
        /// See <see cref="hitType"/> to determine what type of trackable, if any, was hit.
        /// </summary>
        public ARTrackable trackable { get; }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(
            m_Hit.GetHashCode(),
            distance.GetHashCode(),
            HashCodeUtil.ReferenceHash(m_Transform),
            HashCodeUtil.ReferenceHash(trackable));

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARRaycastHit"/> and
        /// <see cref="Equals(ARRaycastHit)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is ARRaycastHit other && Equals(other);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARRaycastHit"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARRaycastHit"/>, otherwise false.</returns>
        public bool Equals(ARRaycastHit other)
        {
            return
                m_Hit.Equals(other.m_Hit) &&
                distance.Equals(other.distance) &&
                m_Transform == other.m_Transform &&
                trackable == other.trackable;
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARRaycastHit)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARRaycastHit lhs, ARRaycastHit rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARRaycastHit)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARRaycastHit lhs, ARRaycastHit rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Used for sorting two raycast hits by distance. Uses [CompareTo](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable.compareto?view=netframework-4.8)
        /// on the raycasts' distance properties.
        /// </summary>
        /// <param name="other">The other <see cref="ARRaycastHit"/> to compare against.</param>
        /// <returns>A value less than zero if this raycast hit is closer than <paramref name="other"/>, zero if the distances
        /// are equal, and a positive value if <paramref name="other"/> is closer.</returns>
        public int CompareTo(ARRaycastHit other) => m_Hit.distance.CompareTo(other.m_Hit.distance);

        XRRaycastHit m_Hit;

        Transform m_Transform;
    }
}
