using AOT;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents a single, raw image from a device camera. Provides access to the raw image plane data, as well as
    /// conversion methods to convert to color and grayscale formats. See <see cref="Convert"/> and
    /// <see cref="ConvertAsync(XRCpuImage.ConversionParams)"/>. Use
    /// <see cref="XRCameraSubsystem.TryAcquireLatestCpuImage"/> to get a <see cref="XRCpuImage"/>.
    /// </summary>
    /// <remarks>
    /// Each <see cref="XRCpuImage"/> must be explicitly disposed. Failing to do so will leak resources and could prevent
    /// future camera image access.
    /// </remarks>
    public partial struct XRCpuImage : IDisposable, IEquatable<XRCpuImage>
    {
        int m_NativeHandle;

        Api m_Api;

        static Api.OnImageRequestCompleteDelegate s_OnAsyncConversionCompleteDelegate = OnAsyncConversionComplete;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_SafetyHandle;
#endif

        /// <summary>
        /// Container for native camera image construction metadata.
        /// </summary>
        public struct Cinfo : IEquatable<Cinfo>
        {
            /// <summary>
            /// The handle representing the camera image on the native level.
            /// </summary>
            /// <value>
            /// The handle representing the camera image on the native level.
            /// </value>
            public int nativeHandle => m_NativeHandle;
            int m_NativeHandle;

            /// <summary>
            /// The dimensions of the camera image.
            /// </summary>
            /// <value>
            /// The dimensions of the camera image.
            /// </value>
            public Vector2Int dimensions => m_Dimensions;
            Vector2Int m_Dimensions;

            /// <summary>
            /// The number of video planes in the camera image.
            /// </summary>
            /// <value>
            /// The number of video planes in the camera image.
            /// </value>
            public int planeCount => m_PlaneCount;
            int m_PlaneCount;

            /// <summary>
            /// The timestamp for when the camera image was captured.
            /// </summary>
            /// <value>
            /// The timestamp for when the camera image was captured.
            /// </value>
            public double timestamp => m_Timestamp;
            double m_Timestamp;

            /// <summary>
            /// The format of the camera image.
            /// </summary>
            /// <value>
            /// The format of the camera image.
            /// </value>
            public Format format => m_Format;
            Format m_Format;

            /// <summary>
            /// Constructs the camera image `Cinfo`.
            /// </summary>
            /// <param name="nativeHandle">The handle representing the camera image on the native level.</param>
            /// <param name="dimensions">The dimensions of the camera image.</param>
            /// <param name="planeCount">The number of video planes in the camera image.</param>
            /// <param name="timestamp">The timestamp for when the camera image was captured.</param>
            /// <param name="format">The format of the camera image.</param>
            public Cinfo(int nativeHandle, Vector2Int dimensions, int planeCount, double timestamp, Format format)
            {
                m_NativeHandle = nativeHandle;
                m_Dimensions = dimensions;
                m_PlaneCount = planeCount;
                m_Timestamp = timestamp;
                m_Format = format;
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="other">The other <see cref="Cinfo"/> to compare against.</param>
            /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="Cinfo"/>, otherwise `false`.</returns>
            public bool Equals(Cinfo other)
            {
                return (nativeHandle.Equals(other.nativeHandle) && dimensions.Equals(other.dimensions)
                        && planeCount.Equals(other.planeCount) && timestamp.Equals(other.timestamp)
                        && (format == other.format));
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
            /// <param name="lhs">The <see cref="Cinfo"/> to compare with <paramref name="rhs"/>.</param>
            /// <param name="rhs">The <see cref="Cinfo"/> to compare with <paramref name="lhs"/>.</param>
            /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator ==(Cinfo lhs, Cinfo rhs) => lhs.Equals(rhs);

            /// <summary>
            /// Tests for inequality. Same as `!`<see cref="Equals(Cinfo)"/>.
            /// </summary>
            /// <param name="lhs">The <see cref="Cinfo"/> to compare with <paramref name="rhs"/>.</param>
            /// <param name="rhs">The <see cref="Cinfo"/> to compare with <paramref name="lhs"/>.</param>
            /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator !=(Cinfo lhs, Cinfo rhs) => !(lhs == rhs);

            /// <summary>
            /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode() => HashCodeUtil.Combine(
                nativeHandle.GetHashCode(),
                dimensions.GetHashCode(),
                planeCount.GetHashCode(),
                timestamp.GetHashCode(),
                ((int)format).GetHashCode());

            /// <summary>
            /// Generates a string representation of this object suitable for debugging.
            /// </summary>
            /// <returns>Returns a string representation of this <see cref="Cinfo"/>.</returns>
            public override string ToString() => $"nativeHandle: {nativeHandle} dimensions:{dimensions} planes:{planeCount} timestamp:{timestamp} format:{format}";
        }

        /// <summary>
        /// The dimensions (width and height) of the image.
        /// </summary>
        /// <value>
        /// The dimensions (width and height) of the image.
        /// </value>
        public Vector2Int dimensions { get; private set; }

        /// <summary>
        /// The image width.
        /// </summary>
        /// <value>
        /// The image width.
        /// </value>
        public int width => dimensions.x;

        /// <summary>
        /// The image height.
        /// </summary>
        /// <value>
        /// The image height.
        /// </value>
        public int height => dimensions.y;

        /// <summary>
        /// The number of image planes. A plane in this context refers to a channel in the raw video format,
        /// not a physical surface.
        /// </summary>
        /// <value>
        /// The number of image planes.
        /// </value>
        public int planeCount { get; private set; }

        /// <summary>
        /// The format used by the image planes. You will only need this if you plan to interpret the raw plane data.
        /// </summary>
        /// <value>
        /// The format used by the image planes.
        /// </value>
        public Format format { get; private set; }

        /// <summary>
        /// The timestamp, in seconds, associated with this camera image
        /// </summary>
        /// <value>
        /// The timestamp, in seconds, associated with this camera image
        /// </value>
        public double timestamp { get; private set; }

        /// <summary>
        /// Whether this <c>XRCpuImage</c> represents a valid image (that is, it has not been Disposed).
        /// </summary>
        /// <value>
        /// <c>true</c> if this <c>XRCpuImage</c> represents a valid image. Otherwise, <c>false</c>.
        /// </value>
        public bool valid => (m_Api != null) && m_Api.NativeHandleValid(m_NativeHandle);

        /// <summary>
        /// Construct the <c>XRCpuImage</c> with the given native image information.
        /// </summary>
        /// <param name="api">The camera subsystem to use for interacting with the native image.</param>
        /// <param name="cinfo">Construction information. See <see cref="Cinfo"/>.</param>
        internal XRCpuImage(XRCpuImage.Api api, Cinfo cinfo)
        {
            m_Api = api;
            m_NativeHandle = cinfo.nativeHandle;
            this.dimensions = cinfo.dimensions;
            this.planeCount = cinfo.planeCount;
            this.timestamp = cinfo.timestamp;
            this.format = cinfo.format;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_SafetyHandle = AtomicSafetyHandle.Create();
#endif
        }

        /// <summary>
        /// Determines whether the given [TextureFormat](https://docs.unity3d.com/ScriptReference/TextureFormat.html)
        /// is supported for conversion.
        /// </summary>
        /// <param name="format">A [TextureFormat](https://docs.unity3d.com/ScriptReference/TextureFormat.html) to
        /// test.</param>
        /// <returns>Returns `true` if <paramref name="format"/> is supported by the various conversion
        /// methods.</returns>
        public bool FormatSupported(TextureFormat format) => m_Api?.FormatSupported(this, format) == true;

        /// <summary>
        /// Get an image plane. A plane in this context refers to a channel in the raw video format, not a physical
        /// surface.
        /// </summary>
        /// <param name="planeIndex">The index of the plane to get.</param>
        /// <returns>A <see cref="Plane"/> describing the plane.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="planeIndex"/> is not within
        /// the range [0, <see cref="planeCount"/>).</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the requested plane is not valid.</exception>
        public unsafe Plane GetPlane(int planeIndex)
        {
            ValidateNativeHandleAndThrow();
            if (planeIndex < 0 || planeIndex >= planeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(planeIndex), $"{nameof(planeIndex)} must be in the range 0 to {planeCount - 1}");
            }

            if (!m_Api.TryGetPlane(m_NativeHandle, planeIndex, out Plane.Cinfo imagePlaneCinfo))
            {
                throw new InvalidOperationException("The requested plane is not valid for this XRCpuImage.");
            }

            var data = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                (void*)imagePlaneCinfo.dataPtr, imagePlaneCinfo.dataLength, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref data, m_SafetyHandle);
#endif

            return new Plane
            {
                rowStride = imagePlaneCinfo.rowStride,
                pixelStride = imagePlaneCinfo.pixelStride,
                data = data
            };
        }

        /// <summary>
        /// Get the number of bytes required to store a converted image with the given parameters.
        /// </summary>
        /// <param name="dimensions">The desired dimensions of the converted image.</param>
        /// <param name="format">The desired <c>TextureFormat</c> for the converted image.</param>
        /// <returns>The number of bytes required to store the converted image.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the desired <paramref name="format"/> is not
        /// supported.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the desired <paramref name="dimensions"/>
        /// exceed the native image dimensions.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the image is invalid.</exception>
        /// <seealso cref="FormatSupported"/>
        public int GetConvertedDataSize(Vector2Int dimensions, TextureFormat format)
        {
            ValidateNativeHandleAndThrow();

            if (dimensions.x > this.dimensions.x)
            {
                throw new ArgumentOutOfRangeException(nameof(dimensions),
                    $"Converted image width must be less than or equal to native image width. {dimensions.x} > {this.dimensions.x}");
            }

            if (dimensions.y > this.dimensions.y)
            {
                throw new ArgumentOutOfRangeException(nameof(dimensions),
                    $"Converted image height must be less than or equal to native image height. {dimensions.y} > {this.dimensions.y}");
            }

            if (!FormatSupported(format))
            {
                throw new ArgumentException("Invalid texture format.", nameof(format));
            }

            if (!m_Api.TryGetConvertedDataSize(m_NativeHandle, dimensions, format, out int size))
            {
                throw new InvalidOperationException("XRCpuImage is not valid.");
            }

            return size;
        }

        /// <summary>
        /// Get the number of bytes required to store a converted image with the given parameters.
        /// </summary>
        /// <param name="conversionParams">The desired conversion parameters.</param>
        /// <returns>The number of bytes required to store the converted image.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the desired format is not supported.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the desired dimensions exceed the native
        /// image dimensions.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the image is invalid.</exception>
        /// <seealso cref="FormatSupported"/>
        public int GetConvertedDataSize(ConversionParams conversionParams)
        {
            return GetConvertedDataSize(
                conversionParams.outputDimensions,
                conversionParams.outputFormat);
        }

        /// <summary>
        /// Convert the `XRCpuImage` to one of the supported formats using the specified
        /// <paramref name="conversionParams"/>.
        /// </summary>
        /// <param name="conversionParams">The parameters for the image conversion.</param>
        /// <param name="destinationBuffer">A pointer to memory to which to write the converted image.</param>
        /// <param name="bufferLength">The number of bytes pointed to by <paramref name="destinationBuffer"/>. Must be
        /// greater than or equal to the value returned by
        /// <see cref="GetConvertedDataSize(ConversionParams)"/>.</param>
        /// <exception cref="System.ArgumentException">Thrown if the <paramref name="bufferLength"/> is smaller than
        /// the data size required by the conversion.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the conversion failed.</exception>
        /// <seealso cref="FormatSupported"/>
        public void Convert(ConversionParams conversionParams, IntPtr destinationBuffer, int bufferLength)
        {
            ValidateNativeHandleAndThrow();
            ValidateConversionParamsAndThrow(conversionParams);
            int requiredDataSize = GetConvertedDataSize(conversionParams);

            if (bufferLength < requiredDataSize)
            {
                throw new ArgumentException($"Conversion requires {requiredDataSize} bytes but only provided {bufferLength} bytes.", nameof(bufferLength));
            }

            if (!m_Api.TryConvert(m_NativeHandle, conversionParams, destinationBuffer, bufferLength))
            {
                throw new InvalidOperationException("Conversion failed.");
            }
        }

        /// <summary>
        /// Convert the `XRCpuImage` to one of the supported formats using the specified
        /// <paramref name="conversionParams"/>.
        /// </summary>
        /// <param name="conversionParams">The parameters for the image conversion.</param>
        /// <param name="destinationBuffer">The destination buffer for the converted data. The buffer must be
        /// large enough to hold the converted data, that is, at least as large as the value returned by
        /// <see cref="GetConvertedDataSize(ConversionParams)"/>.</param>
        /// <exception cref="System.ArgumentException">Thrown if the <paramref name="destinationBuffer"/>
        /// has insufficient space for the converted data.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the conversion failed.</exception>
        /// <seealso cref="FormatSupported"/>
        public unsafe void Convert(ConversionParams conversionParams, NativeSlice<byte> destinationBuffer)
        {
            Convert(conversionParams, new IntPtr(destinationBuffer.GetUnsafePtr()), destinationBuffer.Length);
        }

        /// <summary>
        /// Convert the <c>XRCpuImage</c> to one of the supported formats using the specified
        /// <paramref name="conversionParams"/>. The conversion is performed asynchronously. Use the returned
        /// <see cref="AsyncConversion"/> to check for the status of the conversion, and retrieve the data
        /// when complete.
        /// </summary>
        /// <remarks>
        /// It is safe to <c>Dispose</c> the <c>XRCpuImage</c> before the asynchronous operation has completed.
        /// </remarks>
        /// <param name="conversionParams">The parameters to use for the conversion.</param>
        /// <returns>A <see cref="AsyncConversion"/> which can be used to check the status of the
        /// conversion operation and get the resulting data.</returns>
        /// <seealso cref="FormatSupported"/>
        public AsyncConversion ConvertAsync(ConversionParams conversionParams)
        {
            ValidateNativeHandleAndThrow();
            ValidateConversionParamsAndThrow(conversionParams);

            return new AsyncConversion(m_Api, m_NativeHandle, conversionParams);
        }

        /// <summary>
        /// <para>Convert the <c>XRCpuImage</c> to one of the supported formats using the specified
        /// <paramref name="conversionParams"/>. The conversion is performed asynchronously, and
        /// <paramref name="onComplete"/> is invoked when the conversion is complete, whether successful or not.</para>
        /// <para>The <c>NativeArray</c> provided in the <paramref name="onComplete"/> delegate is only valid during
        /// the invocation and is disposed immediately upon return.</para>
        /// </summary>
        /// <param name="conversionParams">The parameters to use for the conversion.</param>
        /// <param name="onComplete">A delegate to invoke when the conversion operation completes. The delegate is
        /// always invoked, regardless of whether the conversion succeeded.</param>
        /// <seealso cref="FormatSupported"/>
        public void ConvertAsync(
            ConversionParams conversionParams,
            Action<AsyncConversionStatus, ConversionParams, NativeArray<byte>> onComplete)
        {
            ValidateNativeHandleAndThrow();
            ValidateConversionParamsAndThrow(conversionParams);

            var handle = GCHandle.Alloc(onComplete);
            var context = GCHandle.ToIntPtr(handle);
            m_Api.ConvertAsync(m_NativeHandle, conversionParams, s_OnAsyncConversionCompleteDelegate, context);
        }

        /// <summary>
        /// Callback from native code for when the asynchronous conversion is complete.
        /// </summary>
        /// <param name="status">The status of the conversion operation.</param>
        /// <param name="conversionParams">The parameters for the conversion.</param>
        /// <param name="dataPtr">The native pointer to the converted data.</param>
        /// <param name="dataLength">The memory size of the converted data.</param>
        /// <param name="context">The native context for the conversion operation.</param>
        [MonoPInvokeCallback(typeof(Api.OnImageRequestCompleteDelegate))]
        static unsafe void OnAsyncConversionComplete(
            AsyncConversionStatus status, ConversionParams conversionParams, IntPtr dataPtr,
            int dataLength, IntPtr context)
        {
            var handle = GCHandle.FromIntPtr(context);
            var onComplete = (Action<AsyncConversionStatus, ConversionParams, NativeArray<byte>>)handle.Target;

            if (onComplete != null)
            {
                var data = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                    (void*)dataPtr, dataLength, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                var safetyHandle = AtomicSafetyHandle.Create();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref data, safetyHandle);
#endif

                onComplete(status, conversionParams, data);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.Release(safetyHandle);
#endif
            }

            handle.Free();
        }

        /// <summary>
        /// Ensure the image is valid.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the image is invalid.</exception>
        void ValidateNativeHandleAndThrow()
        {
            if (!valid)
            {
                throw new InvalidOperationException("XRCpuImage is not valid.");
            }
        }

        /// <summary>
        /// Ensure the conversion parameters are valid.
        /// </summary>
        /// <param name="conversionParams">The conversion parameters to validate.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the input image rect exceeds the actual
        /// image dimensions or if the output dimensions exceed the input dimensions.</exception>
        /// <exception cref="System.ArgumentException">Thrown if the texture format is not suppported.</exception>
        /// <seealso cref="FormatSupported"/>
        void ValidateConversionParamsAndThrow(ConversionParams conversionParams)
        {
            if ((conversionParams.inputRect.x + conversionParams.inputRect.width > width) ||
                (conversionParams.inputRect.y + conversionParams.inputRect.height > height))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(conversionParams),
                    "Input rect must be completely within the original image.");
            }

            if ((conversionParams.outputDimensions.x > conversionParams.inputRect.width) ||
                (conversionParams.outputDimensions.y > conversionParams.inputRect.height))
            {
                throw new ArgumentOutOfRangeException($"Output dimensions must be less than or equal to the inputRect's dimensions: ({conversionParams.outputDimensions.x}x{conversionParams.outputDimensions.y} > {conversionParams.inputRect.width}x{conversionParams.inputRect.height}).");
            }

            if (!FormatSupported(conversionParams.outputFormat))
            {
                throw new ArgumentException("TextureFormat not supported.", nameof(conversionParams));
            }
        }

        /// <summary>
        /// Dispose native resources associated with this request, including the raw image data. Any
        /// <see cref="Plane"/>s returned by <see cref="GetPlane"/> are invalidated immediately after
        /// calling <c>Dispose</c>.
        /// </summary>
        public void Dispose()
        {
            if (m_Api == null || m_NativeHandle == 0)
            {
                return;
            }

            m_Api.DisposeImage(m_NativeHandle);
            m_NativeHandle = 0;
            m_Api = null;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(m_SafetyHandle);
#endif
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(
            dimensions.GetHashCode(),
            planeCount.GetHashCode(),
            m_NativeHandle.GetHashCode(),
            ((int)format).GetHashCode(),
            HashCodeUtil.ReferenceHash(m_Api));

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRCpuImage"/> and
        /// <see cref="Equals(XRCpuImage)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is XRCpuImage other && Equals(other);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRCpuImage"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRCpuImage"/>, otherwise false.</returns>
        public bool Equals(XRCpuImage other) =>
            dimensions.Equals(other.dimensions) &&
            (planeCount == other.planeCount) &&
            (format == other.format) &&
            (m_NativeHandle == other.m_NativeHandle) &&
            (m_Api == other.m_Api);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRCpuImage)"/>.
        /// </summary>
        /// <param name="lhs">The <see cref="XRCpuImage"/> to compare with <paramref name="rhs"/>.</param>
        /// <param name="rhs">The <see cref="XRCpuImage"/> to compare with <paramref name="lhs"/>.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRCpuImage lhs, XRCpuImage rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRCpuImage)"/>.
        /// </summary>
        /// <param name="lhs">The <see cref="XRCpuImage"/> to compare with <paramref name="rhs"/>.</param>
        /// <param name="rhs">The <see cref="XRCpuImage"/> to compare with <paramref name="lhs"/>.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRCpuImage lhs, XRCpuImage rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Generates a string representation of this <see cref="XRCpuImage"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="XRCpuImage"/>.</returns>
        public override string ToString() => $"(Width: {width}, Height: {height}, PlaneCount: {planeCount}, Format: {format})";
    }
}
