namespace Unity.Services.Wire.Internal
{
    /// <summary>
    /// This structure represents the necessary data to perform a Wire subscription to an <see cref="IChannel"/>
    /// </summary>
    public struct ChannelToken
    {
        /// <summary>
        /// This is a string identifying a Channel on which a service publishes messages.
        /// </summary>
        public string ChannelName;
        /// <summary>
        /// This is an authorization token emitted by the service who owns the Channel for this specific UAS Id.
        /// </summary>
        public string Token;
    }
}
