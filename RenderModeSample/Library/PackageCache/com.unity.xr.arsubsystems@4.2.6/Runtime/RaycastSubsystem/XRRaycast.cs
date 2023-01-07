using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the intersection of a raycast with a trackable.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRRaycast : ITrackable, IEquatable<XRRaycast>
    {
        static readonly XRRaycast s_Default = new XRRaycast(
            TrackableId.invalidId, Pose.identity, TrackingState.None, IntPtr.Zero, 0, TrackableId.invalidId);

        /// <summary>
        /// A default-initialized raycast.
        /// This can be different from a zero-initialized raycast.
        /// </summary>
        public static XRRaycast defaultValue => s_Default;

        /// <summary>
        /// A unique identifier for this raycast.
        /// </summary>
        public TrackableId trackableId => m_TrackableId;

        /// <summary>
        /// The session-space <c>Pose</c> of this raycast's intersection with a target.
        /// </summary>
        public Pose pose => m_Pose;

        /// <summary>
        /// The <see cref="TrackingState"/> of this raycast.
        /// </summary>
        public TrackingState trackingState => m_TrackingState;

        /// <summary>
        /// The pointer associated with this raycast. The data this pointer points to is implementation-defined.
        /// Refer to the platform-specific AR package for details.
        /// </summary>
        public IntPtr nativePtr => m_NativePtr;

        /// <summary>
        /// The session-space distance from the raycast origin to the intersection point.
        /// </summary>
        public float distance => m_Distance;

        /// <summary>
        /// The <see cref="TrackableId"/> of the trackable hit by this raycast, or
        /// <see cref="TrackableId.invalidId"/> if none.
        /// </summary>
        public TrackableId hitTrackableId => m_HitTrackableId;

        /// <summary>
        /// Constructs an <see cref="XRRaycast"/>.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> of the trackable which was hit.</param>
        /// <param name="pose">The session-space <c>Pose</c> of the intersection.</param>
        /// <param name="trackingState">The tracking state of this raycast.</param>
        /// <param name="nativePtr">A pointer into native memory for this raycast.</param>
        /// <param name="distance">The session-space distance from the raycast origin to the intersection point.</param>
        /// <param name="hitTrackableId">The <see cref="TrackableId"/> of the trackable hit by this raycast,
        /// or <see cref="TrackableId.invalidId"/> if none.</param>
        public XRRaycast(
            TrackableId trackableId, Pose pose, TrackingState trackingState, IntPtr nativePtr,  float distance,
            TrackableId hitTrackableId)
        {
            m_TrackableId = trackableId;
            m_Pose = pose;
            m_TrackingState = trackingState;
            m_NativePtr = nativePtr;
            m_Distance = distance;
            m_HitTrackableId = hitTrackableId;
        }

        /// <summary>
        /// Computes a hash suitable for use in a `Dictionary` or `HashSet`.
        /// </summary>
        /// <returns>A hash code computed from this raycast's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = m_TrackableId.GetHashCode();
                hash = hash * 486187739 + m_Pose.GetHashCode();
                hash = hash * 486187739 + ((int)m_TrackingState).GetHashCode();
                hash = hash * 486187739 + m_NativePtr.GetHashCode();
                hash = hash * 486187739 + m_Distance.GetHashCode();
                hash = hash * 486187739 + m_HitTrackableId.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Compares this raycast for equality with an `object`.
        /// </summary>
        /// <param name="obj">The object to compare for equality.</param>
        /// <returns>`True` if <paramref name="obj"/> is a <see cref="XRRaycast"/> and compares <see cref="Equals(XRRaycast)"/>.</returns>
        public override bool Equals(object obj) => (obj is XRRaycast) && Equals((XRRaycast)obj);

        /// <summary>
        /// Compares this raycast for equality with another raycast.
        /// </summary>
        /// <param name="other">The raycast with which to compare.</param>
        /// <returns>`True` if all fields of both raycasts are equal. Otherwise, `false`.</returns>
        public bool Equals(XRRaycast other) =>
            m_TrackableId.Equals(other.m_TrackableId) &&
            m_Pose.Equals(other.m_Pose) &&
            (m_TrackingState == other.m_TrackingState) &&
            (m_NativePtr == other.m_NativePtr) &&
            m_HitTrackableId.Equals(other.m_HitTrackableId) &&
            m_Distance.Equals(other.m_Distance);

        /// <summary>
        /// Tests for equality between two <see cref="XRRaycast"/>s. Same as <paramref name="lhs"/>.Equals(<paramref name="rhs"/>)
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if all fields of both <paramref name="lhs"/> and <paramref name="rhs"/> are equal.</returns>
        public static bool operator ==(XRRaycast lhs, XRRaycast rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality between two <see cref="XRRaycast"/>s. Same as !<paramref name="lhs"/>.Equals(<paramref name="rhs"/>)
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if any of the fields of <paramref name="lhs"/> and <paramref name="rhs"/> are not equal.</returns>
        public static bool operator !=(XRRaycast lhs, XRRaycast rhs) => !lhs.Equals(rhs);

        TrackableId m_TrackableId;

        Pose m_Pose;

        TrackingState m_TrackingState;

        IntPtr m_NativePtr;

        float m_Distance;

        TrackableId m_HitTrackableId;
    }
}
