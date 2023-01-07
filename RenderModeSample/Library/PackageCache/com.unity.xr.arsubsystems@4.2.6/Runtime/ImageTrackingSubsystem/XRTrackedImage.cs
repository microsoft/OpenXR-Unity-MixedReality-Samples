using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Contains low-level data for a tracked image in the environment.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRTrackedImage : ITrackable, IEquatable<XRTrackedImage>
    {
        /// <summary>
        /// Constructs an <see cref="XRTrackedImage"/>.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> associated with this tracked image.</param>
        /// <param name="sourceImageId">A <c>GUID</c> associated with the source image.</param>
        /// <param name="pose">The <c>Pose</c> associated with the detected image.</param>
        /// <param name="size">The size (dimensions) of the detected image.</param>
        /// <param name="trackingState">The <see cref="TrackingState"/> of the detected image.</param>
        /// <param name="nativePtr">A native pointer associated with the detected image.</param>
        public XRTrackedImage(
            TrackableId trackableId,
            Guid sourceImageId,
            Pose pose,
            Vector2 size,
            TrackingState trackingState,
            IntPtr nativePtr)
        {
            m_Id = trackableId;
            m_SourceImageId = sourceImageId;
            m_Pose = pose;
            m_Size = size;
            m_TrackingState = trackingState;
            m_NativePtr = nativePtr;
        }

        /// <summary>
        /// Generates a <see cref="XRTrackedImage"/> populated with default values.
        /// </summary>
        public static XRTrackedImage defaultValue => s_Default;

        static readonly XRTrackedImage s_Default = new XRTrackedImage
        {
            m_Id = TrackableId.invalidId,
            m_SourceImageId = Guid.Empty,
            m_Pose = Pose.identity,
        };

        /// <summary>
        /// The <see cref="TrackableId"/> associated with this tracked image.
        /// </summary>
        public TrackableId trackableId => m_Id;

        /// <summary>
        /// The <c>GUID</c> associated with the source image.
        /// </summary>
        public Guid sourceImageId => m_SourceImageId;

        /// <summary>
        /// The <c>Pose</c> associated with this tracked image.
        /// </summary>
        public Pose pose => m_Pose;

        /// <summary>
        /// The size (dimensions) of this tracked image.
        /// </summary>
        public Vector2 size => m_Size;

        /// <summary>
        /// The <see cref="TrackingState"/> associated with this tracked image.
        /// </summary>
        public TrackingState trackingState => m_TrackingState;

        /// <summary>
        /// A native pointer associated with this tracked image.
        /// The data pointed to by this pointer is implementation-defined.
        /// While its lifetime is also implementation-defined, it should be
        /// valid at least until the next call to
        /// <see cref="TrackingSubsystem{TTrackable, TSubsystem, TSubsystemDescriptor, TProvider}.GetChanges(Unity.Collections.Allocator)"/>.
        /// </summary>
        public IntPtr nativePtr => m_NativePtr;

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Id.GetHashCode();
                hashCode = hashCode * 486187739 + m_SourceImageId.GetHashCode();
                hashCode = hashCode * 486187739 + m_Pose.GetHashCode();
                hashCode = hashCode * 486187739 + m_Size.GetHashCode();
                hashCode = hashCode * 486187739 + ((int)m_TrackingState).GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRTrackedImage"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRTrackedImage"/>, otherwise false.</returns>
        public bool Equals(XRTrackedImage other)
        {
            return
                m_Id.Equals(other.m_Id) &&
                m_SourceImageId.Equals(other.m_SourceImageId) &&
                m_Pose.Equals(other.m_Pose) &&
                m_Size.Equals(other.m_Size) &&
                m_TrackingState == other.m_TrackingState;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRTrackedImage"/> and
        /// <see cref="Equals(XRTrackedImage)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is XRTrackedImage && Equals((XRTrackedImage)obj);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRTrackedImage)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator==(XRTrackedImage lhs, XRTrackedImage rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRTrackedImage)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator!=(XRTrackedImage lhs, XRTrackedImage rhs) => !lhs.Equals(rhs);

        TrackableId m_Id;
        Guid m_SourceImageId;
        Pose m_Pose;
        Vector2 m_Size;
        TrackingState m_TrackingState;
        IntPtr m_NativePtr;
    }
}
