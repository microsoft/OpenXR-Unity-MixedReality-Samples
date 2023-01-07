using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Contains all of the data provided for an individual environment probe in an AR session.
    /// </summary>
    [StructLayout (LayoutKind.Sequential)]
    public struct XREnvironmentProbe : IEquatable<XREnvironmentProbe>, ITrackable
    {
        /// <summary>
        /// Creates an <see cref="XREnvironmentProbe"/> populated with default values.
        /// </summary>
        public static XREnvironmentProbe defaultValue => s_Default;

        static readonly XREnvironmentProbe s_Default = new XREnvironmentProbe
        {
            trackableId = TrackableId.invalidId,
            pose = Pose.identity
        };

        /// <summary>
        /// Uniquely identifies each environment probe in an AR session.
        /// </summary>
        /// <value>
        /// The unique trackable ID of the environment probe.
        /// </value>
        public TrackableId trackableId
        {
            get => m_TrackableId;
            private set => m_TrackableId = value;
        }
        TrackableId m_TrackableId;

        /// <summary>
        /// Contains the scale of the environment probe in the AR session.
        /// </summary>
        /// <value>
        /// The scale of the environment probe.
        /// </value>
        public Vector3 scale
        {
            get => m_Scale;
            private set => m_Scale = value;
        }
        Vector3 m_Scale;

        /// <summary>
        /// Contains the pose (position and rotation) of the environment probe in the AR session.
        /// </summary>
        /// <value>
        /// The pose (position and rotation) of the environment probe.
        /// </value>
        public Pose pose
        {
            get => m_Pose;
            private set => m_Pose = value;
        }
        Pose m_Pose;

        /// <summary>
        /// Specifies the volume size around the environment probe's position. This is used for
        /// for parallax correction when projecting the environment texture.
        /// </summary>
        /// <value>
        /// The bounding volume size of the environment probe.
        /// </value>
        /// <remarks>
        /// Note that <c>size</c> can be infinite. This is valid.
        /// </remarks>
        public Vector3 size
        {
            get => m_Size;
            private set => m_Size = value;
        }
        Vector3 m_Size;

        /// <summary>
        /// Contains the texture descriptor captured from the device.
        /// </summary>
        /// <value>
        /// The texture descriptor of the environment probe.
        /// </value>
        /// <remarks>
        /// The <c>environmentTextureData</c> value can be invalid, which indicates that the device has not captured an
        /// environment texture for this probe yet. Newly created environment probes have no environment texture. The
        /// <see cref="XRTextureDescriptor.valid" /> property should be used to determine whether the texture data
        /// is valid.
        /// </remarks>
        public XRTextureDescriptor textureDescriptor
        {
            get => m_TextureDescriptor;
            private set => m_TextureDescriptor = value;
        }
        XRTextureDescriptor m_TextureDescriptor;

        /// <summary>
        /// The <see cref="TrackingState"/> associated with this environment probe.
        /// </summary>
        public TrackingState trackingState
        {
            get => m_TrackingState;
            private set => m_TrackingState = value;
        }
        TrackingState m_TrackingState;

        /// <summary>
        /// A native pointer associated with this environment probe.
        /// The data pointed to by this pointer is implementation-defined. Its lifetime
        /// is also implementation-defined, but will be valid at least until the next
        /// call to <see cref="XREnvironmentProbeSubsystem.GetChanges(Unity.Collections.Allocator)"/>.
        /// </summary>
        public IntPtr nativePtr
        {
            get => m_NativePtr;
            private set => m_NativePtr = value;
        }
        IntPtr m_NativePtr;

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XREnvironmentProbe"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XREnvironmentProbe"/>, otherwise false.</returns>
        public bool Equals(XREnvironmentProbe other)
        {
            // Environment probes are equivalent if the trackable IDs match.
            return m_TrackableId.Equals(other.m_TrackableId);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XREnvironmentProbe"/> and
        /// <see cref="Equals(XREnvironmentProbe)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj) =>
            (obj is XREnvironmentProbe) && Equals((XREnvironmentProbe)obj);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XREnvironmentProbe)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XREnvironmentProbe lhs, XREnvironmentProbe rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XREnvironmentProbe)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XREnvironmentProbe lhs, XREnvironmentProbe rhs) => !(lhs == rhs);

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() => m_TrackableId.GetHashCode();

        /// <summary>
        /// Generates a string representation of this <see cref="XREnvironmentProbe"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="XREnvironmentProbe"/>.</returns>
        public override string ToString() => ToString("0.000");

        /// <summary>
        /// Generates a string representation of this <see cref="XREnvironmentProbe"/>. Floating point
        /// values use <paramref name="floatingPointformat"/> to generate their string representations.
        /// </summary>
        /// <param name="floatingPointformat">The format specifier used for floating point fields.</param>
        /// <returns>A string representation of this <see cref="XREnvironmentProbe"/>.</returns>
        public string ToString(string floatingPointformat)
        {
            return string.Format("{0} scale:{1} pose:{2} size:{3} environmentTextureData:{4} trackingState:{5} nativePtr:{6}",
                                 m_TrackableId.ToString(), m_Scale.ToString(floatingPointformat),
                                 m_Pose.ToString(floatingPointformat), m_Size.ToString(floatingPointformat),
                                 m_TextureDescriptor.ToString(), m_TrackingState.ToString(), m_NativePtr.ToString());
        }
    }
}
