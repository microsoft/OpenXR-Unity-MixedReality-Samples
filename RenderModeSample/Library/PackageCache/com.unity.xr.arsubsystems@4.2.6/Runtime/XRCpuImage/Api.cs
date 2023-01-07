using System;

namespace UnityEngine.XR.ARSubsystems
{
    public partial struct XRCpuImage
    {
        /// <summary>
        /// An API for interacting with <see cref="XRCpuImage"/>s.
        /// </summary>
        /// <remarks>
        /// This interface is intended to be implemented by AR platform providers (for example, ARCore, ARKit, Magic Leap,
        /// and HoloLens). The <see cref="XRCpuImage"/> uses it to make platform-specific API calls. Unity developers
        /// don't need to interact directly with this class; use the <see cref="XRCpuImage"/> instead.
        /// </remarks>
        public abstract class Api
        {
            /// <summary>
            /// Callback from native code for when the asynchronous conversion is complete.
            /// </summary>
            /// <param name="status">The status of the conversion operation.</param>
            /// <param name="conversionParams">The parameters for the conversion.</param>
            /// <param name="dataPtr">The native pointer to the converted data.</param>
            /// <param name="dataLength">The memory size of the converted data.</param>
            /// <param name="context">The native context for the conversion operation.</param>
            public delegate void OnImageRequestCompleteDelegate(
                AsyncConversionStatus status,
                ConversionParams conversionParams,
                IntPtr dataPtr,
                int dataLength,
                IntPtr context);

            /// <summary>
            /// Method to be implemented by the provider to get information about an image plane from a native image
            /// handle by index.
            /// </summary>
            /// <param name="nativeHandle">A unique identifier for this camera image.</param>
            /// <param name="planeIndex">The index of the plane to get.</param>
            /// <param name="planeCinfo">The returned camera plane information if successful.</param>
            /// <returns>
            /// <c>true</c> if the image plane was acquired. Otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            public virtual bool TryGetPlane(
                int nativeHandle,
                int planeIndex,
                out Plane.Cinfo planeCinfo)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Method to be implemented by the provider to get the number of bytes required to store an image with the
            /// given dimensions and <c>TextureFormat</c>.
            /// </summary>
            /// <param name="nativeHandle">A unique identifier for the camera image to convert.</param>
            /// <param name="dimensions">The dimensions of the output image.</param>
            /// <param name="format">The <c>TextureFormat</c> for the image.</param>
            /// <param name="size">The number of bytes required to store the converted image.</param>
            /// <returns><c>true</c> if the output <paramref name="size"/> was set.</returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera image conversion.</exception>
            public virtual bool TryGetConvertedDataSize(int nativeHandle, Vector2Int dimensions, TextureFormat format, out int size)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Method to be implemented by the provider to convert the image with handle
            /// <paramref name="nativeHandle"/> using the provided <paramref cref="conversionParams"/>.
            /// </summary>
            /// <param name="nativeHandle">A unique identifier for the camera image to convert.</param>
            /// <param name="conversionParams">The parameters to use during the conversion.</param>
            /// <param name="destinationBuffer">A buffer to write the converted image to.</param>
            /// <param name="bufferLength">The number of bytes available in the buffer.</param>
            /// <returns>
            /// <c>true</c> if the image was converted and stored in <paramref name="destinationBuffer"/>.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            public virtual bool TryConvert(
                int nativeHandle,
                ConversionParams conversionParams,
                IntPtr destinationBuffer,
                int bufferLength)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Method to be implemented by the provider to create an asynchronous request to convert a camera image,
            /// similar to <see cref="TryConvert"/> except the conversion should happen on a thread other than the
            /// calling (main) thread.
            /// </summary>
            /// <param name="nativeHandle">A unique identifier for the camera image to convert.</param>
            /// <param name="conversionParams">The parameters to use during the conversion.</param>
            /// <returns>A unique identifier for this request.</returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            public virtual int ConvertAsync(
                int nativeHandle,
                ConversionParams conversionParams)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Method to be implemented by the provider to determine whether a native image handle is currently valid.
            /// An image can become invalid if it has been disposed.
            /// </summary>
            /// <remarks>
            /// If a handle is valid, <see cref="TryConvert"/> and <see cref="TryGetConvertedDataSize"/> should not fail.
            /// </remarks>
            /// <param name="nativeHandle">A unique identifier for the camera image in question.</param>
            /// <returns><c>true</c>, if it is a valid handle. Otherwise, <c>false</c>.</returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            /// <seealso cref="DisposeImage"/>
            public virtual bool NativeHandleValid(
                int nativeHandle)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Method to be implemented by the provider to get a pointer to the image data from a completed
            /// asynchronous request. This method should only succeed if <see cref="GetAsyncRequestStatus"/> returns
            /// <see cref="AsyncConversionStatus.Ready"/>.
            /// </summary>
            /// <param name="requestId">The unique identifier associated with a request.</param>
            /// <param name="dataPtr">A pointer to the native buffer containing the data.</param>
            /// <param name="dataLength">The number of bytes in <paramref name="dataPtr"/>.</param>
            /// <returns><c>true</c> if <paramref name="dataPtr"/> and <paramref name="dataLength"/> were set and point
            ///  to the image data.</returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            public virtual bool TryGetAsyncRequestData(int requestId, out IntPtr dataPtr, out int dataLength)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Method to be implemented by the provider. It is similar to
            /// <see cref="ConvertAsync(int, ConversionParams)"/> but takes a delegate to invoke when the
            /// request is complete, rather than returning a request id.
            /// </summary>
            /// <remarks>
            /// If the first parameter to <paramref name="callback"/> is
            /// <see cref="AsyncConversionStatus.Ready"/>, the <c>dataPtr</c> parameter must be valid
            /// for the duration of the invocation. The data can be destroyed immediately upon return. The
            /// <paramref name="context"/> parameter must be passed back to the <paramref name="callback"/>.
            /// </remarks>
            /// <param name="nativeHandle">A unique identifier for the camera image to convert.</param>
            /// <param name="conversionParams">The parameters to use during the conversion.</param>
            /// <param name="callback">A delegate which must be invoked when the request is complete, whether the
            /// conversion was successfully or not.</param>
            /// <param name="context">A native pointer which must be passed back unaltered to
            /// <paramref name="callback"/>.</param>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            public virtual void ConvertAsync(
                int nativeHandle,
                ConversionParams conversionParams,
                OnImageRequestCompleteDelegate callback,
                IntPtr context)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Method to be implemented by the provider to dispose an existing native image identified by
            /// <paramref name="nativeHandle"/>.
            /// </summary>
            /// <param name="nativeHandle">A unique identifier for this camera image.</param>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            public virtual void DisposeImage(int nativeHandle)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Method to be implemented by the provider to dispose an existing async conversion request.
            /// </summary>
            /// <param name="requestId">A unique identifier for the request.</param>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            /// <seealso cref="ConvertAsync(int, ConversionParams)"/>
            public virtual void DisposeAsyncRequest(int requestId)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Method to be implemented by the provider to get the status of an existing asynchronous conversion
            /// request.
            /// </summary>
            /// <param name="requestId">The unique identifier associated with a request.</param>
            /// <returns>The state of the request.</returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            /// <seealso cref="ConvertAsync(int, ConversionParams)"/>
            public virtual AsyncConversionStatus GetAsyncRequestStatus(int requestId)
            {
                throw new NotSupportedException("Camera image conversion is not supported by this implementation");
            }

            /// <summary>
            /// Determines whether a given
            /// [TextureFormat](https://docs.unity3d.com/ScriptReference/TextureFormat.html) is supported for image
            /// conversion.
            /// </summary>
            /// <param name="image">The <see cref="XRCpuImage"/> to convert.</param>
            /// <param name="format">The [`TextureFormat`](https://docs.unity3d.com/ScriptReference/TextureFormat.html)
            /// to test.</param>
            /// <returns>Returns `true` if <paramref name="image"/> can be converted to <paramref name="format"/>.
            /// Returns `false` otherwise.</returns>
            public virtual bool FormatSupported(XRCpuImage image, TextureFormat format) => false;
        }
    }
}
