using System.Threading.Tasks;

namespace Unity.Services.Wire.Internal
{
    /// <summary>
    /// Must be implemented by the <see cref="IWire.CreateChannel(IChannelTokenProvider)"/> caller.
    /// This object responsibility is to provide an async method that returns a <see cref="ChannelToken"/> structure.
    /// </summary>
    public interface IChannelTokenProvider
    {
        /// <summary>
        /// This async method should implement whetever network transaction necessary to retrieve a token enabling a Wire subscription.
        /// This function can be called by the Wire SDK multiple times during the lifetime of the subscription.
        /// For example, whenever the token needs to be refreshed.
        /// </summary>
        /// <returns>A <see cref="ChannelToken"/> structure to be used at subscription time.</returns>
        Task<ChannelToken> GetTokenAsync();
    }
}
