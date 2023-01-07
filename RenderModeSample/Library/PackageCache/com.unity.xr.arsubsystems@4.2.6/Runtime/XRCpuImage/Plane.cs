using System;
using Unity.Collections;

namespace UnityEngine.XR.ARSubsystems
{
    public partial struct XRCpuImage
    {
        /// <summary>
        /// Information about the camera image planes. An image plane refers to an image channel used in video encoding.
        /// </summary>
        public struct Plane : IEquatable<Plane>
        {
            /// <summary>
            /// Container for the metadata that describes access to the raw camera image plane data.
            /// </summary>
            public struct Cinfo : IEquatable<Cinfo>
            {
                /// <summary>
                /// The pointer to the raw native image data.
                /// </summary>
                /// <value>
                /// The pointer to the raw native image data.
                /// </value>
                public IntPtr dataPtr => m_DataPtr;

                IntPtr m_DataPtr;

                /// <summary>
                /// The length of the native image data.
                /// </summary>
                /// <value>
                /// The length of the native image data.
                /// </value>
                public int dataLength => m_DataLength;

                int m_DataLength;

                /// <summary>
                /// The stride for iterating through the rows of the native image data.
                /// </summary>
                /// <value>
                /// The stride for iterating through the rows of the native image data.
                /// </value>
                public int rowStride => m_RowStride;

                int m_RowStride;

                /// <summary>
                /// The stride for iterating through the pixels of the native image data.
                /// </summary>
                /// <value>
                /// The stride for iterating through the pixels of the native image data.
                /// </value>
                public int pixelStride => m_PixelStride;

                int m_PixelStride;

                /// <summary>
                /// Constructs the camera image plane cinfo.
                /// </summary>
                /// <param name="dataPtr">The pointer to the raw native image data.</param>
                /// <param name="dataLength">The length of the native image data.</param>
                /// <param name="rowStride">The stride for iterating through the rows of the native image data.</param>
                /// <param name="pixelStride">The stride for iterating through the pixels of the native image data.</param>
                public Cinfo(IntPtr dataPtr, int dataLength, int rowStride, int pixelStride)
                {
                    this.m_DataPtr = dataPtr;
                    this.m_DataLength = dataLength;
                    this.m_RowStride = rowStride;
                    this.m_PixelStride = pixelStride;
                }

                /// <summary>
                /// Tests for equality.
                /// </summary>
                /// <param name="other">The other <see cref="Cinfo"/> to compare against.</param>
                /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="Cinfo"/>, otherwise false.</returns>
                public bool Equals(Cinfo other)
                {
                    return (dataPtr.Equals(other.dataPtr) && dataLength.Equals(other.dataLength)
                        && rowStride.Equals(other.rowStride) && pixelStride.Equals(other.pixelStride));
                }

                /// <summary>
                /// Tests for equality.
                /// </summary>
                /// <param name="obj">The `object` to compare against.</param>
                /// <returns>`True` if <paramref name="obj"/> is of type <see cref="Cinfo"/> and
                /// <see cref="Equals(Cinfo)"/> also returns `true`; otherwise `false`.</returns>
                public override bool Equals(System.Object obj) => obj is Cinfo other && Equals(other);

                /// <summary>
                /// Tests for equality. Same as <see cref="Equals(Cinfo)"/>.
                /// </summary>
                /// <param name="lhs">The left-hand side of the comparison.</param>
                /// <param name="rhs">The right-hand side of the comparison.</param>
                /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
                public static bool operator ==(Cinfo lhs, Cinfo rhs) => lhs.Equals(rhs);

                /// <summary>
                /// Tests for inequality. Same as `!`<see cref="Equals(Cinfo)"/>.
                /// </summary>
                /// <param name="lhs">The left-hand side of the comparison.</param>
                /// <param name="rhs">The right-hand side of the comparison.</param>
                /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
                public static bool operator !=(Cinfo lhs, Cinfo rhs) => !(lhs == rhs);

                /// <summary>
                /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
                /// </summary>
                /// <returns>A hash code generated from this object's fields.</returns>
                public override int GetHashCode() => HashCodeUtil.Combine(
                    dataPtr.GetHashCode(),
                    dataLength.GetHashCode(),
                    rowStride.GetHashCode(),
                    pixelStride.GetHashCode());

                /// <summary>
                /// Generates a string suitable for debugging.
                /// </summary>
                /// <returns>A string representation of this <see cref="Cinfo"/>.</returns>
                public override string ToString()
                    => $"dataPtr: 0x{dataPtr.ToInt64():x} length:{dataLength} rowStride:{rowStride} pixelStride:{pixelStride}";
            }

            /// <summary>
            /// The number of bytes per row for this plane.
            /// </summary>
            /// <value>
            /// The number of bytes per row for this plane.
            /// </value>
            public int rowStride { get; internal set; }

            /// <summary>
            /// The number of bytes per pixel for this plane.
            /// </summary>
            /// <value>
            /// The number of bytes per pixel for this plane.
            /// </value>
            public int pixelStride { get; internal set; }

            /// <summary>
            /// A view into the platform-specific plane data. It is an error to access <c>data</c> after the owning
            /// <see cref="XRCpuImage"/> has been disposed.
            /// </summary>
            /// <value>
            /// The platform-specific plane data.
            /// </value>
            public NativeArray<byte> data { get; internal set; }

            /// <summary>
            /// Constructs an <see cref="Plane"/>.
            /// </summary>
            /// <param name="rowStride">The number of bytes per row for this plane.</param>
            /// <param name="pixelStride">The number of bytes per pixel for this plane.</param>
            /// <param name="data">The platform-specific plane data.</param>
            public Plane(int rowStride, int pixelStride, NativeArray<byte> data)
            {
                this.rowStride = rowStride;
                this.pixelStride = pixelStride;
                this.data = data;
            }

            /// <summary>
            /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode() => HashCodeUtil.Combine(
                data.GetHashCode(),
                rowStride.GetHashCode(),
                pixelStride.GetHashCode());

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="obj">The `object` to compare against.</param>
            /// <returns>`True` if <paramref name="obj"/> is of type <see cref="Plane"/> and
            /// <see cref="Equals(Plane)"/> also returns `true`; otherwise `false`.</returns>
            public override bool Equals(object obj) => obj is Plane other && Equals(other);

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="other">The other <see cref="Plane"/> to compare against.</param>
            /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="Plane"/>, otherwise false.</returns>
            public bool Equals(Plane other) =>
                (data.Equals(other.data)) &&
                (rowStride == other.rowStride) &&
                (pixelStride == other.pixelStride);

            /// <summary>
            /// Tests for equality. Same as <see cref="Equals(Plane)"/>.
            /// </summary>
            /// <param name="lhs">The <see cref="Plane"/> to compare with <paramref name="rhs"/>.</param>
            /// <param name="rhs">The <see cref="Plane"/> to compare with <paramref name="lhs"/>.</param>
            /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator ==(Plane lhs, Plane rhs) => lhs.Equals(rhs);

            /// <summary>
            /// Tests for inequality. Same as `!`<see cref="Equals(Plane)"/>.
            /// </summary>
            /// <param name="lhs">The <see cref="Plane"/> to compare with <paramref name="rhs"/>.</param>
            /// <param name="rhs">The <see cref="Plane"/> to compare with <paramref name="lhs"/>.</param>
            /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator !=(Plane lhs, Plane rhs) => !lhs.Equals(rhs);

            /// <summary>
            /// Generates a string representation of this <see cref="Plane"/>.
            /// </summary>
            /// <returns>A string representation of this <see cref="Plane"/>.</returns>
            public override string ToString() => $"({data.Length} bytes, Row Stride: {rowStride}, Pixel Stride: {pixelStride})";
        }
    }
}
