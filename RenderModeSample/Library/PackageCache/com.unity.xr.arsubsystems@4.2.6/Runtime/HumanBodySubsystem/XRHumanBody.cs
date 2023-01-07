using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Container for the data that represents a trackable human body.
    /// </summary>
    [StructLayout (LayoutKind.Sequential)]
    public struct XRHumanBody : IEquatable<XRHumanBody>, ITrackable
    {
        /// <summary>
        /// The trackable identifier for the human body.
        /// </summary>
        /// <value>
        /// The trackable identifier for the human body.
        /// </value>
        public TrackableId trackableId
        {
            get => m_TrackableId;
            private set => m_TrackableId = value;
        }
        TrackableId m_TrackableId;

        /// <summary>
        /// The pose for the human body origin.
        /// </summary>
        /// <value>
        /// The pose for the human body origin.
        /// </value>
        public Pose pose
        {
            get => m_Pose;
            private set => m_Pose = value;
        }
        Pose m_Pose;

        /// <summary>
        /// The scale factor that relates the implementation's default body height to the estimated height.
        /// </summary>
        /// <value>
        /// The scale factor that relates the implementation's default body height to the estimated height.
        /// </value>
        public float estimatedHeightScaleFactor
        {
            get => m_EstimatedHeightScaleFactor;
            private set => m_EstimatedHeightScaleFactor = value;
        }
        float m_EstimatedHeightScaleFactor;

        /// <summary>
        /// The tracking state for the human body.
        /// </summary>
        /// <value>
        /// The tracking state for the human body.
        /// </value>
        public TrackingState trackingState
        {
            get => m_TrackingState;
            private set => m_TrackingState = value;
        }
        TrackingState m_TrackingState;

        /// <summary>
        /// The native pointer to the implementation-specific human body.
        /// </summary>
        /// <value>
        /// The native pointer to the implementation-specific human body.
        /// </value>
        public IntPtr nativePtr
        {
            get => m_NativePtr;
            private set => m_NativePtr = value;
        }
        IntPtr m_NativePtr;

        /// <summary>
        /// Get the default human body data.
        /// </summary>
        /// <returns>
        /// The default human body data.
        /// </returns>
        public static XRHumanBody defaultValue => s_Default;

        static readonly XRHumanBody s_Default = new XRHumanBody
        {
            trackableId = TrackableId.invalidId,
            pose = Pose.identity,
            estimatedHeightScaleFactor = 1.0f,
        };

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRHumanBody"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRHumanBody"/>, otherwise false.</returns>
        public bool Equals(XRHumanBody other) => m_TrackableId.Equals(other.m_TrackableId);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRHumanBody"/> and
        /// <see cref="Equals(XRHumanBody)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj) => ((obj is XRHumanBody) && Equals((XRHumanBody)obj));

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRHumanBody)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRHumanBody lhs, XRHumanBody rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRHumanBody)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRHumanBody lhs, XRHumanBody rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() => m_TrackableId.GetHashCode();
    }
}
