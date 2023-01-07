using System;

namespace Unity.Services.Wire.Internal
{
    /// <summary>
    /// The subscription current state.
    /// </summary>
    public enum SubscriptionState
    {
        /// <summary>
        /// The subscription is inactive.
        /// </summary>
        Unsubscribed,
        /// <summary>
        /// The subscription is active and synchronized to the server. You can trust that the last message received
        /// from the subscription is up to date.
        /// </summary>
        Synced,
        /// <summary>
        /// A connectivity issue prevents Wire from receiving update on this active subscription. As soon as Wire
        /// has reconnected, the subscription will receive the missing messages.
        /// </summary>
        Unsynced,
        /// <summary>
        /// The subscription encountered an error and is not active.
        /// </summary>
        Error,
        /// <summary>
        /// Wire is getting a token then sending a request to Wire to subscribe to a channel.
        /// </summary>
        Subscribing,
    }
}
