using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Parameters of the Unity <c>Camera</c> that might be necessary or useful to the provider.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRCameraParams : IEquatable<XRCameraParams>
    {
        /// <summary>
        /// Distance from the camera to the near plane.
        /// </summary>
        /// <value>
        /// Distance from the camera to the near plane.
        /// </value>
        public float zNear
        {
            get => m_ZNear;
            set => m_ZNear = value;
        }
        float m_ZNear;

        /// <summary>
        /// Distance from the camera to the far plane.
        /// </summary>
        /// <value>
        /// Distance from the camera to the far plane.
        /// </value>
        public float zFar
        {
            get => m_ZFar;
            set => m_ZFar = value;
        }
        float m_ZFar;

        /// <summary>
        /// Width, in pixels, of the screen resolution.
        /// </summary>
        /// <value>
        /// Width, in pixels, of the screen resolution.
        /// </value>
        public float screenWidth
        {
            get => m_ScreenWidth;
            set => m_ScreenWidth = value;
        }
        float m_ScreenWidth;

        /// <summary>
        /// Height, in pixels, of the screen resolution.
        /// </summary>
        /// <value>
        /// Height, in pixels, of the screen resolution.
        /// </value>
        public float screenHeight
        {
            get => m_ScreenHeight;
            set => m_ScreenHeight = value;
        }
        float m_ScreenHeight;

        /// <summary>
        /// The orientation of the screen.
        /// </summary>
        /// <value>
        /// The orientation of the screen.
        /// </value>
        public ScreenOrientation screenOrientation
        {
            get => m_ScreenOrientation;
            set => m_ScreenOrientation = value;
        }
        ScreenOrientation m_ScreenOrientation;

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRCameraParams"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRCameraParams"/>, otherwise false.</returns>
        public bool Equals(XRCameraParams other)
        {
            return (m_ZNear.Equals(other.m_ZNear) && m_ZFar.Equals(other.m_ZFar)
                    && m_ScreenWidth.Equals(other.m_ScreenWidth) && m_ScreenHeight.Equals(other.m_ScreenHeight)
                    && (m_ScreenOrientation == other.m_ScreenOrientation));
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRCameraParams"/> and
        /// <see cref="Equals(XRCameraParams)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj)
        {
            return ((obj is XRCameraParams) && Equals((XRCameraParams)obj));
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRCameraParams)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRCameraParams lhs, XRCameraParams rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRCameraParams)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRCameraParams lhs, XRCameraParams rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            int hashCode = 486187739;
            unchecked
            {
                hashCode = (hashCode * 486187739) + m_ZNear.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ZFar.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ScreenWidth.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ScreenHeight.GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_ScreenOrientation).GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Generates a string representation of this <see cref="XRCameraParams"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="XRCameraParams"/>.</returns>
        public override string ToString()
        {
            return string.Format("zNear:{0} zFar:{1} screen:{2}x{3}({4})", m_ZNear.ToString("0.000"),
                                 m_ZFar.ToString("0.000"), m_ScreenWidth.ToString(), m_ScreenHeight.ToString(),
                                 m_ScreenOrientation.ToString());
        }
    }
}
