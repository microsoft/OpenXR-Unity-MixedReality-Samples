namespace Unity.Services.Core
{
    /// <summary>
    /// Initialization state of Unity Services
    /// </summary>
    public enum ServicesInitializationState
    {
        /// <summary>
        /// Initialization has not been started
        /// </summary>
        Uninitialized,
        /// <summary>
        /// Initialization in progress
        /// </summary>
        Initializing,
        /// <summary>
        /// Initialization has been successfully completed
        /// </summary>
        Initialized,
    }
}
