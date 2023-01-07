using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// The session-relative data associated with a plane.
    /// </summary>
    /// <seealso cref="XRPlaneSubsystem"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundedPlane : ITrackable, IEquatable<BoundedPlane>
    {
        static readonly BoundedPlane s_Default = new BoundedPlane(
                TrackableId.invalidId,
                TrackableId.invalidId,
                Pose.identity,
                Vector2.zero,
                Vector2.zero,
                PlaneAlignment.None,
                TrackingState.None,
                IntPtr.Zero,
                PlaneClassification.None);

        /// <summary>
        /// Gets a default-initialized <see cref="BoundedPlane"/>. This can be
        /// different from the zero-initialized version, e.g., the <see cref="pose"/>
        /// is <c>Pose.identity</c> instead of zero-initialized.
        /// </summary>
        public static BoundedPlane defaultValue => s_Default;

        /// <summary>
        /// Constructs a new <see cref="BoundedPlane"/>. This is just a data container
        /// for a plane's session relative data. These are typically created by
        /// <see cref="XRPlaneSubsystem.GetChanges(Unity.Collections.Allocator)"/>.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> associated with the plane.</param>
        /// <param name="subsumedBy">The plane which subsumed this one. Use <see cref="TrackableId.invalidId"/> if it has not been subsumed.</param>
        /// <param name="pose">The <c>Pose</c> associated with the plane.</param>
        /// <param name="center">The center of the plane, in plane space (relative to <paramref name="pose"/>).</param>
        /// <param name="size">The dimensions associated with the plane.</param>
        /// <param name="alignment">The <see cref="PlaneAlignment"/> associated with the plane.</param>
        /// <param name="trackingState">The <see cref="TrackingState"/> associated with the plane.</param>
        /// <param name="nativePtr">The native pointer associated with the plane.</param>
        /// <param name="classification">The <see cref="PlaneClassification"/> associated with the plane.</param>
        public BoundedPlane(
            TrackableId trackableId,
            TrackableId subsumedBy,
            Pose pose,
            Vector2 center,
            Vector2 size,
            PlaneAlignment alignment,
            TrackingState trackingState,
            IntPtr nativePtr,
            PlaneClassification classification)
        {
            m_TrackableId = trackableId;
            m_SubsumedById = subsumedBy;
            m_Pose = pose;
            m_Center = center;
            m_Size = size;
            m_Alignment = alignment;
            m_TrackingState = trackingState;
            m_NativePtr = nativePtr;
            m_Classification = classification;
        }

        /// <summary>
        /// The <see cref="TrackableId"/> associated with this plane.
        /// </summary>
        public TrackableId trackableId => m_TrackableId;

        /// <summary>
        /// The <see cref="TrackableId"/> associated with the plane which subsumed this one. Will be <see cref="TrackableId.invalidId"/>
        /// if this plane has not been subsumed.
        /// </summary>
        public TrackableId subsumedById => m_SubsumedById;

        /// <summary>
        /// The <c>Pose</c>, in session space, of the plane.
        /// </summary>
        public Pose pose => m_Pose;

        /// <summary>
        /// The center of the plane in plane space (relative to its <see cref="pose"/>).
        /// </summary>
        public Vector2 center => m_Center;

        /// <summary>
        /// The extents of the plane (half dimensions) in meters.
        /// </summary>
        public Vector2 extents => m_Size * 0.5f;

        /// <summary>
        /// The size (dimensions) of the plane in meters.
        /// </summary>
        public Vector2 size => m_Size;

        /// <summary>
        /// The <see cref="PlaneAlignment"/> of the plane.
        /// </summary>
        public PlaneAlignment alignment => m_Alignment;

        /// <summary>
        /// The <see cref="TrackingState"/> of the plane.
        /// </summary>
        public TrackingState trackingState => m_TrackingState;

        /// <summary>
        /// A native pointer associated with this plane.
        /// The data pointer to by this pointer is implementation defined.
        /// </summary>
        public IntPtr nativePtr => m_NativePtr;

        /// <summary>
        /// The <see cref="PlaneClassification"/> of the plane.
        /// </summary>
        public PlaneClassification classification => m_Classification;

        /// <summary>
        /// The width of the plane in meters.
        /// </summary>
        public float width => m_Size.x;

        /// <summary>
        /// The height (or depth) of the plane in meters.
        /// </summary>
        public float height => m_Size.y;

        /// <summary>
        /// The normal of the plane in session space.
        /// </summary>
        public Vector3 normal => m_Pose.up;

        /// <summary>
        /// Gets an infinite plane in session space.
        /// </summary>
        public Plane plane => new Plane(normal, center);

        /// <summary>
        /// Get the four corners of the plane in session space, in clockwise order.
        /// </summary>
        /// <param name="p0">The first vertex.</param>
        /// <param name="p1">The second vertex.</param>
        /// <param name="p2">The third vertex.</param>
        /// <param name="p3">The fourth vertex.</param>
        public void GetCorners(
            out Vector3 p0,
            out Vector3 p1,
            out Vector3 p2,
            out Vector3 p3)
        {
            var sessionCenter = m_Pose.rotation * center + m_Pose.position;
            var sessionHalfX = (m_Pose.right) * (width * .5f);
            var sessionHalfZ = (m_Pose.forward) * (height * .5f);
            p0 = sessionCenter - sessionHalfX - sessionHalfZ;
            p1 = sessionCenter - sessionHalfX + sessionHalfZ;
            p2 = sessionCenter + sessionHalfX + sessionHalfZ;
            p3 = sessionCenter + sessionHalfX - sessionHalfZ;
        }

        /// <summary>
        /// Generates a new string that describes the plane's properties, suitable for debugging purposes.
        /// </summary>
        /// <returns>A string that describes the plane's properties.</returns>
        public override string ToString()
        {
            return string.Format(
                "Plane:\n\ttrackableId: {0}\n\tsubsumedById: {1}\n\tpose: {2}\n\tcenter: {3}\n\tsize: {4}\n\talignment: {5}\n\tclassification: {6}\n\ttrackingState: {7}\n\tnativePtr: {8:X16}",
                trackableId,
                subsumedById,
                pose,
                center,
                size,
                alignment,
                classification,
                trackingState,
                nativePtr.ToInt64());
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="BoundedPlane"/> and
        /// <see cref="Equals(BoundedPlane)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => (obj is BoundedPlane other) && Equals(other);

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_TrackableId.GetHashCode();
                hashCode = (hashCode * 486187739) + m_SubsumedById.GetHashCode();
                hashCode = (hashCode * 486187739) + m_Pose.GetHashCode();
                hashCode = (hashCode * 486187739) + m_Center.GetHashCode();
                hashCode = (hashCode * 486187739) + m_Size.GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_Alignment).GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_Classification).GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_TrackingState).GetHashCode();
                hashCode = (hashCode * 486187739) + m_NativePtr.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(BoundedPlane)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(BoundedPlane lhs, BoundedPlane rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(BoundedPlane)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(BoundedPlane lhs, BoundedPlane rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="BoundedPlane"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="BoundedPlane"/>, otherwise false.</returns>
        public bool Equals(BoundedPlane other)
        {
            return
                m_TrackableId.Equals(other.m_TrackableId) &&
                m_SubsumedById.Equals(other.m_SubsumedById) &&
                m_Pose.Equals(other.m_Pose) &&
                m_Center.Equals(other.m_Center) &&
                m_Size.Equals(other.m_Size) &&
                (m_Alignment == other.m_Alignment) &&
                (m_Classification == other.m_Classification) &&
                (m_TrackingState == other.m_TrackingState) &&
                (m_NativePtr == other.m_NativePtr);
        }

        TrackableId m_TrackableId;

        TrackableId m_SubsumedById;

        Vector2 m_Center;

        Pose m_Pose;

        Vector2 m_Size;

        PlaneAlignment m_Alignment;

        TrackingState m_TrackingState;

        IntPtr m_NativePtr;

        PlaneClassification m_Classification;
    }
}
