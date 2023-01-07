namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// Exception to use two registered <see cref="IInitializablePackage"/> depend on the other.
    /// </summary>
    public class CircularDependencyException : ServicesInitializationException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="ServicesInitializationException" /> class.
        /// </summary>
        public CircularDependencyException() {}

        /// <summary>
        /// Initialize a new instance of the <see cref="ServicesInitializationException" />
        /// class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        public CircularDependencyException(string message)
            : base(message) {}
    }
}
