using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.XR.ARSubsystems
{
    public partial struct XRCpuImage
    {
        /// <summary>
        /// Holds information related to an asynchronous camera image conversion request. Returned by
        /// <see cref="XRCpuImage.ConvertAsync(ConversionParams)"/>.
        /// </summary>
        public struct AsyncConversion : IDisposable, IEquatable<AsyncConversion>
        {
            XRCpuImage.Api m_Api;
            int m_RequestId;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle m_SafetyHandle;
#endif

            /// <summary>
            /// The <see cref="ConversionParams"/> used during the conversion.
            /// </summary>
            /// <value>
            /// The parameters used during the conversion.
            /// </value>
            public ConversionParams conversionParams { get; private set; }

            /// <summary>
            /// The status of the request.
            /// </summary>
            /// <value>
            /// The status of the request.
            /// </value>
            public AsyncConversionStatus status => m_Api?.GetAsyncRequestStatus(m_RequestId) ?? AsyncConversionStatus.Disposed;

            /// <summary>
            /// Start the image conversion using this class to interact with the asynchronous conversion and results.
            /// </summary>
            /// <param name="api">The CPU image API performing the image conversion.</param>
            /// <param name="nativeHandle">The native handle for the camera image.</param>
            /// <param name="conversionParams">The parameters for image conversion.</param>
            internal AsyncConversion(XRCpuImage.Api api, int nativeHandle, ConversionParams conversionParams)
            {
                m_Api = api;
                m_RequestId = m_Api.ConvertAsync(nativeHandle, conversionParams);
                this.conversionParams = conversionParams;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                m_SafetyHandle = AtomicSafetyHandle.Create();
#endif
            }

            /// <summary>
            /// Get the raw image data. The returned <c>NativeArray</c> is a direct view into the native memory. The
            /// memory is only valid until this <see cref="AsyncConversion"/> is disposed.
            /// </summary>
            /// <typeparam name="T">The type of data to return. No conversion is performed based on the type; this is
            /// only for access convenience.</typeparam>
            /// <returns>
            /// A new <c>NativeArray</c> representing the raw image data. This method might fail; use
            /// <c>NativeArray.IsCreated</c> to determine the validity of the data.
            /// </returns>
            /// <exception cref="System.InvalidOperationException">Thrown if the asynchronous conversion
            /// <see cref="status"/> is not <see cref="AsyncConversionStatus.Ready"/> or if the conversion is
            /// invalid.</exception>
            public unsafe NativeArray<T> GetData<T>() where T : struct
            {
                if (status != AsyncConversionStatus.Ready)
                    throw new InvalidOperationException("Async request is not ready.");

                IntPtr dataPtr;
                int dataLength;
                if (m_Api.TryGetAsyncRequestData(m_RequestId, out dataPtr, out dataLength))
                {
                    int stride = UnsafeUtility.SizeOf<T>();
                    var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                        (void*)dataPtr, dataLength / stride, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, m_SafetyHandle);
#endif
                    return array;
                }

                throw new InvalidOperationException("The XRAsyncCameraImageConversion is not valid.");
            }

            /// <summary>
            /// Dispose native resources associated with this request, including the raw image data. The `NativeArray`
            /// returned by <see cref="GetData{T}"/> is invalidated immediately after calling <see cref="Dispose"/>.
            /// </summary>
            public void Dispose()
            {
                if (m_Api == null || m_RequestId == 0)
                    return;

                m_Api.DisposeAsyncRequest(m_RequestId);
                m_Api = null;
                m_RequestId = 0;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.Release(m_SafetyHandle);
#endif
            }

            /// <summary>
            /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode() => HashCodeUtil.Combine(
                conversionParams.GetHashCode(),
                m_RequestId.GetHashCode(),
                HashCodeUtil.ReferenceHash(m_Api));

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="obj">The `object` to compare against.</param>
            /// <returns>`True` if <paramref name="obj"/> is of type <see cref="AsyncConversion"/> and
            /// <see cref="Equals(AsyncConversion)"/> also returns `true`; otherwise `false`.</returns>
            public override bool Equals(object obj)
            {
                return ((obj is AsyncConversion) && Equals((AsyncConversion)obj));
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="other">The other <see cref="AsyncConversion"/> to compare against.</param>
            /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="AsyncConversion"/>, otherwise false.</returns>
            public bool Equals(AsyncConversion other)
            {
                return
                    (conversionParams.Equals(other.conversionParams)) &&
                    (m_RequestId == other.m_RequestId) &&
                    (m_Api == other.m_Api);
            }

            /// <summary>
            /// Tests for equality. Same as <see cref="Equals(AsyncConversion)"/>.
            /// </summary>
            /// <param name="lhs">The <see cref="AsyncConversion"/> to compare with <paramref name="rhs"/>.</param>
            /// <param name="rhs">The <see cref="AsyncConversion"/> to compare with <paramref name="lhs"/>.</param>
            /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator ==(AsyncConversion lhs, AsyncConversion rhs) => lhs.Equals(rhs);

            /// <summary>
            /// Tests for inequality. Same as `!`<see cref="Equals(AsyncConversion)"/>.
            /// </summary>
            /// <param name="lhs">The <see cref="AsyncConversion"/> to compare with <paramref name="rhs"/>.</param>
            /// <param name="rhs">The <see cref="AsyncConversion"/> to compare with <paramref name="lhs"/>.</param>
            /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
            public static bool operator !=(AsyncConversion lhs, AsyncConversion rhs) => !lhs.Equals(rhs);

            /// <summary>
            /// Generates a string representation of this <see cref="AsyncConversion"/>.
            /// </summary>
            /// <returns>A string representation of the conversion parameters.</returns>
            public override string ToString() => $"ConversionParams: {conversionParams}";
        }
    }
}
