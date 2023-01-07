using System;

namespace Unity.Services.Core
{
    /// <summary>
    /// A base exception type for failed requests.
    /// </summary>
    public class RequestFailedException : Exception
    {
        /// <summary>
        /// Gets the error code for the failure.
        /// </summary>
        /// <remarks>
        /// See <see cref="CommonErrorCodes"/> for common error codes. Consult the
        /// service documentation for specific error codes various APIs can return.
        /// </remarks>
        public int ErrorCode { get; }

        /// <summary>
        /// Creates an exception.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     The exception message is typically the "detail" field from the error
        ///     response returned by the service when it is available.
        /// </para>
        /// <para>
        ///     The error code is the "code" field from the error response returned
        ///     by the service when it is available. See <see cref="CommonErrorCodes"/>
        ///     for common error codes.
        /// </para>
        /// </remarks>
        /// <param name="errorCode">The error code returned by the service.</param>
        /// <param name="message">A message describing the error.</param>
        public RequestFailedException(int errorCode, string message) : this(errorCode, message, null)
        {
        }

        /// <summary>
        /// Creates an exception.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     The exception message is typically the "detail" field from the error
        ///     response returned by the service when it is available.
        /// </para>
        /// <para>
        ///     The error code is the "code" field from the error response returned
        ///     by the service when it is available. See <see cref="CommonErrorCodes"/>
        ///     for common error codes.
        /// </para>
        /// </remarks>
        /// <param name="errorCode">The error code returned by the service.</param>
        /// <param name="message">A message describing the error.</param>
        /// <param name="innerException">The inner exception reference.</param>
        public RequestFailedException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }
    }
}
