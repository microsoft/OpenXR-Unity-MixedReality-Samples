using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Contains a native texture object and includes various metadata about the texture.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRTextureDescriptor : IEquatable<XRTextureDescriptor>
    {
        /// <summary>
        /// A pointer to the native texture object.
        /// </summary>
        /// <value>
        /// A pointer to the native texture object.
        /// </value>
        public IntPtr nativeTexture
        {
            get { return m_NativeTexture; }
            private set { m_NativeTexture = value; }
        }
        IntPtr m_NativeTexture;

        /// <summary>
        /// Specifies the width dimension of the native texture object.
        /// </summary>
        /// <value>
        /// The width of the native texture object.
        /// </value>
        public int width
        {
            get { return m_Width; }
            private set { m_Width = value; }
        }
        int m_Width;

        /// <summary>
        /// Specifies the height dimension of the native texture object.
        /// </summary>
        /// <value>
        /// The height of the native texture object.
        /// </value>
        public int height
        {
            get { return m_Height; }
            private set { m_Height = value; }
        }
        int m_Height;

        /// <summary>
        /// Specifies the number of mipmap levels in the native texture object.
        /// </summary>
        /// <value>
        /// The number of mipmap levels in the native texture object.
        /// </value>
        public int mipmapCount
        {
            get { return m_MipmapCount; }
            private set { m_MipmapCount = value; }
        }
        int m_MipmapCount;

        /// <summary>
        /// Specifies the texture format of the native texture object.
        /// </summary>
        /// <value>
        /// The format of the native texture object.
        /// </value>
        public TextureFormat format
        {
            get { return m_Format; }
            private set { m_Format = value; }
        }
        TextureFormat m_Format;

        /// <summary>
        /// Specifies the unique shader property name ID for the material shader texture.
        /// </summary>
        /// <value>
        /// The unique shader property name ID for the material shader texture.
        /// </value>
        /// <remarks>
        /// Use the static method <c>Shader.PropertyToID(string name)</c> to get the unique identifier.
        /// </remarks>
        public int propertyNameId
        {
            get { return m_PropertyNameId; }
            private set { m_PropertyNameId = value; }
        }
        int m_PropertyNameId;

        /// <summary>
        /// Determines whether the texture data references a valid texture object with positive width and height.
        /// </summary>
        /// <value>
        /// <c>true</c> if the texture data references a valid texture object with positive width and height.
        /// Otherwise, <c>false</c>.
        /// </value>
        public bool valid
        {
            get { return (m_NativeTexture != IntPtr.Zero) && (m_Width > 0) && (m_Height > 0); }
        }

        /// <summary>
        /// This specifies the depth dimension of the native texture. For a 3D texture, depth is greater than zero.
        /// For any other kind of valid texture, depth is one.
        /// </summary>
        /// <value>
        /// The depth dimension of the native texture object.
        /// </value>
        public int depth
        {
            get => m_Depth;
            private set => m_Depth = value;
        }
        int m_Depth;

        /// <summary>
        /// Specifies the [texture dimension](https://docs.unity3d.com/ScriptReference/Rendering.TextureDimension.html) of the native texture object.
        /// </summary>
        /// <value>
        /// The texture dimension of the native texture object.
        /// </value>
        public TextureDimension dimension
        {
            get => m_Dimension;
            private set => m_Dimension = value;
        }
        TextureDimension m_Dimension;

        /// <summary>
        /// Determines whether the given texture descriptor has identical texture metadata (dimension, mipmap count,
        /// and format).
        /// </summary>
        /// <param name="other">The given texture descriptor with which to compare.</param>
        /// <returns>
        /// <c>true</c> if the texture metadata (dimension, mipmap count, and format) are identical between  the
        /// current and other texture descriptors. Otherwise, <c>false</c>.
        /// </returns>
        public bool hasIdenticalTextureMetadata(XRTextureDescriptor other)
        {
            return
                m_Width.Equals(other.m_Width) &&
                m_Height.Equals(other.m_Height) &&
                m_Depth.Equals(other.m_Depth) &&
                m_Dimension == other.m_Dimension &&
                m_MipmapCount.Equals(other.m_MipmapCount) &&
                (m_Format == other.m_Format);
        }

        /// <summary>
        /// Reset the texture descriptor back to default values.
        /// </summary>
        public void Reset()
        {
            m_NativeTexture = IntPtr.Zero;
            m_Width = 0;
            m_Height = 0;
            m_Depth = 0;
            m_Dimension = (TextureDimension)0;
            m_MipmapCount = 0;
            m_Format = (TextureFormat)0;
            m_PropertyNameId = 0;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRTextureDescriptor"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRTextureDescriptor"/>, otherwise `false`.</returns>
        public bool Equals(XRTextureDescriptor other)
        {
            return
                hasIdenticalTextureMetadata(other) &&
                m_PropertyNameId.Equals(other.m_PropertyNameId) &&
                (m_NativeTexture == other.m_NativeTexture);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRTextureDescriptor"/> and
        /// <see cref="Equals(XRTextureDescriptor)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj)
        {
            return ((obj is XRTextureDescriptor) && Equals((XRTextureDescriptor)obj));
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRTextureDescriptor)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRTextureDescriptor lhs, XRTextureDescriptor rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRTextureDescriptor)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRTextureDescriptor lhs, XRTextureDescriptor rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            int hashCode = 486187739;
            unchecked
            {
                hashCode = (hashCode * 486187739) + m_NativeTexture.GetHashCode();
                hashCode = (hashCode * 486187739) + m_Width.GetHashCode();
                hashCode = (hashCode * 486187739) + m_Height.GetHashCode();
                hashCode = (hashCode * 486187739) + m_Depth.GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_Dimension).GetHashCode();
                hashCode = (hashCode * 486187739) + m_MipmapCount.GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_Format).GetHashCode();
                hashCode = (hashCode * 486187739) + m_PropertyNameId.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Generates a string suitable for debugging purposes.
        /// </summary>
        /// <returns>A string suitable for debug logging.</returns>
        public override string ToString()
        {
            return $"0x{m_NativeTexture.ToString("X16")} {m_Width.ToString()}x{ m_Height.ToString()}"+
                    $"x{m_Depth.ToString()}-{m_MipmapCount.ToString()} dimension:{m_Dimension.ToString()}"+
                    $" format:{m_Format.ToString()} propertyNameId:{m_PropertyNameId.ToString()}";
        }
    }
}
