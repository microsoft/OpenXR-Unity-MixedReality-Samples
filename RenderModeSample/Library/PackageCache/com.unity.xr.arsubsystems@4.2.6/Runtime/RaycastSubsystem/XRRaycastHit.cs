using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the intersection of a raycast with a trackable.
    /// </summary>
    /// <seealso cref="XRRaycastSubsystem.Raycast(Ray, TrackableType, Unity.Collections.Allocator)"/>
    /// <seealso cref="XRRaycastSubsystem.Raycast(Vector2, TrackableType, Unity.Collections.Allocator)"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRRaycastHit : IEquatable<XRRaycastHit>
    {
        static readonly XRRaycastHit s_Default = new XRRaycastHit(
            TrackableId.invalidId, Pose.identity, 0, TrackableType.None);

        /// <summary>
        /// A default-initialized raycast hit.
        /// This can be different from a zero-initialized raycast hit.
        /// </summary>
        public static XRRaycastHit defaultValue => s_Default;

        /// <summary>
        /// The <see cref="TrackableId"/> of the trackable which was hit. This can be <see cref="TrackableId.invalidId"/>
        /// as some trackables (for example, feature points) don't have ids.
        /// </summary>
        public TrackableId trackableId
        {
            get => m_TrackableId;
            set => m_TrackableId = value;
        }

        /// <summary>
        /// The session-space <c>Pose</c> of the intersection.
        /// </summary>
        public Pose pose
        {
            get => m_Pose;
            set => m_Pose = value;
        }

        /// <summary>
        /// The session-space distance from the raycast origin to the intersection point.
        /// </summary>
        public float distance
        {
            get => m_Distance;
            set => m_Distance = value;
        }

        /// <summary>
        /// The types of trackables which were hit by the ray.
        /// </summary>
        public TrackableType hitType
        {
            get => m_HitType;
            set => m_HitType = value;
        }

        /// <summary>
        /// Constructs an <see cref="XRRaycastHit"/>.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> of the trackable which was hit.</param>
        /// <param name="pose">The session-space <c>Pose</c> of the intersection.</param>
        /// <param name="distance">The session-space distance from the raycast origin to the intersection point.</param>
        /// <param name="hitType">The types of trackables which were hit by the ray.</param>
        public XRRaycastHit(
            TrackableId trackableId,
            Pose pose,
            float distance,
            TrackableType hitType)
        {
            m_TrackableId = trackableId;
            m_Pose = pose;
            m_Distance = distance;
            m_HitType = hitType;
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = m_TrackableId.GetHashCode();
                hash = hash * 486187739 + m_Pose.GetHashCode();
                hash = hash * 486187739 + m_Distance.GetHashCode();
                hash = hash * 486187739 + ((int)m_HitType).GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRRaycastHit"/> and
        /// <see cref="Equals(XRRaycastHit)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => (obj is XRRaycastHit other) && Equals(other);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRRaycastHit"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRRaycastHit"/>, otherwise false.</returns>
        public bool Equals(XRRaycastHit other)
        {
            return
                (m_TrackableId.Equals(other.m_TrackableId)) &&
                (m_Pose.Equals(other.m_Pose)) &&
                (m_Distance.Equals(other.m_Distance)) &&
                (m_HitType == other.m_HitType);
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRRaycastHit)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRRaycastHit lhs, XRRaycastHit rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRRaycastHit)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRRaycastHit lhs, XRRaycastHit rhs) => !lhs.Equals(rhs);

        TrackableId m_TrackableId;

        Pose m_Pose;

        float m_Distance;

        TrackableType m_HitType;
    }
}
