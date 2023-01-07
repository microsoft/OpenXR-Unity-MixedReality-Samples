using System;
using System.Threading.Tasks;

namespace Unity.Services.Wire.Internal
{
    /// <summary>
    /// Channel object. Use <see cref="IWire.CreateChannel(IChannelTokenProvider)"/>
    /// to construct one. This object allows the subscription to a channel.
    /// </summary>
    public interface IChannel : IDisposable
    {
        /// <summary>
        /// Handler called each time the channel receives a message.
        /// </summary>
        event Action<string> MessageReceived;

        /// <summary>
        /// Handler called each time the channel receives a binary message.
        /// </summary>
        event Action<byte[]> BinaryMessageReceived;

        /// <summary>
        /// Handler called if the subscription gets terminated by the Wire server.
        /// </summary>
        event Action KickReceived;

        /// <summary>
        /// Handler called whenever the subscription reliability changes.
        /// </summary>
        event Action<SubscriptionState> NewStateReceived;

        /// <summary>
        /// Handler called whenever the subscription encounters an error.
        /// A description of the error is passed as an argument.
        /// </summary>
        event Action<string> ErrorReceived;

        /// <summary>
        /// SubscribeAsync will subscribe to the channel.
        /// Possible error codes are:
        /// * 23002 -> "CommandFailed"
        /// * 23003 -> "ConnectionFailed"
        /// * 23004 -> "InvalidToken"
        /// * 23005 -> "InvalidChannelName"
        /// * 23006 -> "TokenRetrieverFailed"
        /// * 23007 -> "Unauthorized"
        /// * 23008 -> "AlreadySubscribed"
        /// </summary>
        /// <returns>An awaitable task.</returns>
        /// <exception cref="RequestFailedException"/>
        Task SubscribeAsync();

        /// <summary>
        /// UnsubscribeAsync will stop the subscription, effective immediately.
        /// Possible error codes are:
        /// * 23002 -> "CommandFailed"
        /// * 23009 -> "AlreadyUnsubscribed"
        /// </summary>
        /// <returns>An awaitable task.</returns>
        /// <exception cref="RequestFailedException"/>
        Task UnsubscribeAsync();
    }
}
