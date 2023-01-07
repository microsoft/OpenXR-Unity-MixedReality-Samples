using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    public partial struct XRCpuImage
    {
        /// <summary>
        /// Describes a set of conversion parameters for use with <see cref="XRCpuImage"/>'s conversion methods.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ConversionParams : IEquatable<ConversionParams>
        {
            RectInt m_InputRect;
            Vector2Int m_OutputDimensions;
            TextureFormat m_Format;
            Transformation m_Transformation;

            /// <summary>
            /// The portion of the original image that will be used as input to the conversion.
            /// </summary>
            /// <remarks>
            /// The input rectangle must be completely contained inside the <c>XRCpuImage</c>
            /// <see cref="XRCpuImage.dimensions"/>.
            /// </remarks>
            /// <value>
            /// The portion of the original image that will be converted.
            /// </value>
            public RectInt inputRect
            {
                get => m_InputRect;
                set => m_InputRect = value;
            }

            /// <summary>
            /// The dimensions of the converted image. The output dimensions must be less than or equal to the
            /// <see cref="inputRect"/>'s dimensions. If the output dimensions are less than the <see cref="inputRect"/>'s
            /// dimensions, downsampling is performed using nearest neighbor.
            /// </summary>
            /// <value>
            /// The dimensions of the converted image.
            /// </value>
            public Vector2Int outputDimensions
            {
                get => m_OutputDimensions;
                set => m_OutputDimensions = value;
            }

            /// <summary>
            /// The <c>TextureFormat</c> to which to convert. See <see cref="XRCpuImage.FormatSupported"/> for a list of
            /// supported formats.
            /// </summary>
            /// <value>
            /// The <c>TextureFormat</c> to which to convert.
            /// </value>
            public TextureFormat outputFormat
            {
                get => m_Format;
                set => m_Format = value;
            }

            /// <summary>
            /// The transformation to apply to the image during conversion.
            /// </summary>
            /// <value>
            /// The transformation to apply to the image during conversion.
            /// </value>
            public Transformation transformation
            {
                get => m_Transformation;
                set => m_Transformation = value;
            }

            /// <summary>
            /// Constructs a <see cref="ConversionParams"/> using the <paramref name="image"/>'s full
            /// resolution. That is, it sets <see cref="inputRect"/> to <c>(0, 0, image.width, image.height)</c> and
            /// <see cref="outputDimensions"/> to <c>(image.width, image.height)</c>.
            /// </summary>
            /// <param name="image">The source <see cref="XRCpuImage"/>.</param>
            /// <param name="format">The <c>TextureFormat</c> to convert to.</param>
            /// <param name="transformation">An optional <see cref="Transformation"/> to apply.</param>
            public ConversionParams(
                XRCpuImage image,
                TextureFormat format,
                Transformation transformation = Transformation.None)
            {
                m_InputRect = new RectInt(0, 0, image.width, image.height);
                m_OutputDimensions = new Vector2Int(image.width, image.height);
                m_Format = format;
                m_Transformation = transformation;
            }

            /// <summary>
            /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode()
            {
                var hash = HashCodeUtil.Combine(inputRect.width.GetHashCode(), inputRect.height.GetHashCode(), inputRect.x.GetHashCode(), inputRect.y.GetHashCode());
                return HashCodeUtil.Combine(hash, outputDimensions.GetHashCode(), ((int)outputFormat).GetHashCode(), ((int)transformation).GetHashCode());
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="other">The other <see cref="ConversionParams"/> to compare against.</param>
            /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ConversionParams"/>, otherwise false.</returns>
            public bool Equals(ConversionParams other)
            {
                return (inputRect.Equals(other.inputRect) && outputDimensions.Equals(other.outputDimensions)
                    && (outputFormat == other.outputFormat) && (transformation == other.transformation));
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="obj">The `object` to compare against.</param>
            /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ConversionParams"/> and
            /// <see cref="Equals(ConversionParams)"/> also returns `true`; otherwise `false`.</returns>
            public override bool Equals(object obj)
            {
                return (obj is ConversionParams) && Equals((ConversionParams)obj);
            }

            /// <summary>
            /// Tests for equality. Same as <see cref="Equals(ConversionParams)"/>.
            /// </summary>
            /// <param name="lhs">The <see cref="ConversionParams"/> to compare with <paramref name="rhs"/>.</param>
            /// <param name="rhs">The <see cref="ConversionParams"/> to compare with <paramref name="lhs"/>.</param>
            /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator ==(ConversionParams lhs, ConversionParams rhs) => lhs.Equals(rhs);

            /// <summary>
            /// Tests for inequality. Same as `!`<see cref="Equals(ConversionParams)"/>.
            /// </summary>
            /// <param name="lhs">The <see cref="ConversionParams"/> to compare with <paramref name="rhs"/>.</param>
            /// <param name="rhs">The <see cref="ConversionParams"/> to compare with <paramref name="lhs"/>.</param>
            /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator !=(ConversionParams lhs, ConversionParams rhs) => !lhs.Equals(rhs);

            /// <summary>
            /// Generates a string representation of this <see cref="ConversionParams"/>.
            /// </summary>
            /// <returns>A string representation of this <see cref="ConversionParams"/>.</returns>
            public override string ToString()
                => $"inputRect: {inputRect} outputDimensions: {outputDimensions} format: {outputFormat} transformation: {transformation})";
        }
    }
}
