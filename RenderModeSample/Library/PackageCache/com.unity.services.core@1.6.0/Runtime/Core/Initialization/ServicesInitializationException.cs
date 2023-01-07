using System;

namespace Unity.Services.Core
{
    /// <summary>
    /// Represents an error during services initialization
    /// </summary>
    public class ServicesInitializationException : Exception
    {
        /// <inheritdoc cref="ServicesInitializationException(string, Exception)"/>
        public ServicesInitializationException() {}

        /// <inheritdoc cref="ServicesInitializationException(string, Exception)"/>
        public ServicesInitializationException(string message)
            : base(message) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicesInitializationException" /> class.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, if any.
        /// </param>
        public ServicesInitializationException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}
