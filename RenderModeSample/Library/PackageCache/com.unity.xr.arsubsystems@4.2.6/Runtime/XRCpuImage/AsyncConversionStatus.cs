namespace UnityEngine.XR.ARSubsystems
{
    public partial struct XRCpuImage
    {
        /// <summary>
        /// Represents the status of an asynchronous camera image request.
        /// </summary>
        public enum AsyncConversionStatus
        {
            /// <summary>
            /// The request is not valid or has been disposed.
            /// </summary>
            Disposed,

            /// <summary>
            /// The request is waiting to be processed.
            /// </summary>
            Pending,

            /// <summary>
            /// The request is currently being processed.
            /// </summary>
            Processing,

            /// <summary>
            /// The request succeeded and image data is ready.
            /// </summary>
            Ready,

            /// <summary>
            /// The request failed. No data is available.
            /// </summary>
            Failed
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="XRCpuImage.AsyncConversionStatus"/> enum.
    /// </summary>
    public static class XRCpuImageAsyncConversionStatusExtensions
    {
        /// <summary>
        /// Whether the request has completed. It might have completed with an error or be
        /// an invalid or disposed request. See <see cref="IsError(XRCpuImage.AsyncConversionStatus)"/>.
        /// </summary>
        /// <param name="status">The <see cref="XRCpuImage.AsyncConversionStatus"/> being extended.</param>
        /// <returns><c>true</c> if the <see cref="XRCpuImage.AsyncConversionStatus"/> has completed.</returns>
        public static bool IsDone(this XRCpuImage.AsyncConversionStatus status)
        {
            switch (status)
            {
                case XRCpuImage.AsyncConversionStatus.Pending:
                case XRCpuImage.AsyncConversionStatus.Processing:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Whether the request status represents an error. If the request has been disposed, <c>IsError</c>
        /// will be <c>true</c>.
        /// </summary>
        /// <param name="status">The <see cref="XRCpuImage.AsyncConversionStatus"/> being extended.</param>
        /// <returns><c>true</c> if the <see cref="XRCpuImage.AsyncConversionStatus"/> represents an error.</returns>
        public static bool IsError(this XRCpuImage.AsyncConversionStatus status)
        {
            switch (status)
            {
                case XRCpuImage.AsyncConversionStatus.Pending:
                case XRCpuImage.AsyncConversionStatus.Processing:
                case XRCpuImage.AsyncConversionStatus.Ready:
                    return false;
                default:
                    return true;
            }
        }
    }
}
