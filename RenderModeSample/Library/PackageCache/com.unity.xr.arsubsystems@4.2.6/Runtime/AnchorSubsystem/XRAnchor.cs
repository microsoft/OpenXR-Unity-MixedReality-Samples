using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Describes session-relative data for an anchor.
    /// </summary>
    /// <seealso cref="XRAnchorSubsystem"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRAnchor : ITrackable, IEquatable<XRAnchor>
    {
        /// <summary>
        /// Gets a default-initialized <see cref="XRAnchor"/>. This may be
        /// different from the zero-initialized version (for example, the <see cref="pose"/>
        /// is <c>Pose.identity</c> instead of zero-initialized).
        /// </summary>
        public static XRAnchor defaultValue => s_Default;

        static readonly XRAnchor s_Default = new XRAnchor
        {
            m_Id = TrackableId.invalidId,
            m_Pose = Pose.identity,
            m_SessionId = Guid.Empty
        };

        /// <summary>
        /// Constructs the session-relative data for an anchor.
        /// This is typically provided by an implementation of the <see cref="XRAnchorSubsystem"/>
        /// and not invoked directly.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> associated with this anchor.</param>
        /// <param name="pose">The <c>Pose</c>, in session space, of the anchor.</param>
        /// <param name="trackingState">The <see cref="TrackingState"/> of the anchor.</param>
        /// <param name="nativePtr">A native pointer associated with the anchor. The data pointed to by
        /// this pointer is implementation-specific.</param>
        public XRAnchor(
            TrackableId trackableId,
            Pose pose,
            TrackingState trackingState,
            IntPtr nativePtr)
        {
            m_Id = trackableId;
            m_Pose = pose;
            m_TrackingState = trackingState;
            m_NativePtr = nativePtr;
            m_SessionId = Guid.Empty;
        }

        /// <summary>
        /// Constructs the session-relative data for anchor.
        /// This is typically provided by an implementation of the <see cref="XRAnchorSubsystem"/>
        /// and not invoked directly.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> associated with this anchor.</param>
        /// <param name="pose">The <c>Pose</c>, in session space, of the anchor.</param>
        /// <param name="trackingState">The <see cref="TrackingState"/> of the anchor.</param>
        /// <param name="nativePtr">A native pointer associated with the anchor. The data pointed to by
        /// this pointer is implementation-specific.</param>
        /// <param name="sessionId">The session from which this anchor originated.</param>
        public XRAnchor(
            TrackableId trackableId,
            Pose pose,
            TrackingState trackingState,
            IntPtr nativePtr,
            Guid sessionId)
        : this(trackableId, pose, trackingState, nativePtr)
        {
            m_SessionId = sessionId;
        }

        /// <summary>
        /// Get the <see cref="TrackableId"/> associated with this anchor.
        /// </summary>
        public TrackableId trackableId => m_Id;

        /// <summary>
        /// Get the <c>Pose</c>, in session space, for this anchor.
        /// </summary>
        public Pose pose => m_Pose;

        /// <summary>
        /// Get the <see cref="TrackingState"/> of this anchor.
        /// </summary>
        public TrackingState trackingState => m_TrackingState;

        /// <summary>
        /// A native pointer associated with the anchor.
        /// The data pointed to by this pointer is implementation-specific.
        /// </summary>
        public IntPtr nativePtr => m_NativePtr;

        /// <summary>
        /// The id of the session from which this anchor originated.
        /// </summary>
        public Guid sessionId => m_SessionId;

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Id.GetHashCode();
                hashCode = hashCode * 486187739 + m_Pose.GetHashCode();
                hashCode = hashCode * 486187739 + ((int)m_TrackingState).GetHashCode();
                hashCode = hashCode * 486187739 + m_NativePtr.GetHashCode();
                hashCode = hashCode * 486187739 + m_SessionId.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRAnchor"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRAnchor"/>, otherwise false.</returns>
        public bool Equals(XRAnchor other)
        {
            return
                m_Id.Equals(other.m_Id) &&
                m_Pose.Equals(other.m_Pose) &&
                m_TrackingState == other.m_TrackingState &&
                m_NativePtr == other.m_NativePtr &&
                m_SessionId.Equals(other.m_SessionId);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRAnchor"/> and
        /// <see cref="Equals(XRAnchor)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is XRAnchor && Equals((XRAnchor)obj);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRAnchor)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator==(XRAnchor lhs, XRAnchor rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRAnchor)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator!=(XRAnchor lhs, XRAnchor rhs) => !lhs.Equals(rhs);

        TrackableId m_Id;

        Pose m_Pose;

        TrackingState m_TrackingState;

        IntPtr m_NativePtr;

        Guid m_SessionId;
    }
}
