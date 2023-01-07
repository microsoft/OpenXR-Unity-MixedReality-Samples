using System;
using Unity.Services.Core.Internal;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Services.Wire.Internal
{
    /// <summary>
    /// IWire allows the creation of disposable <see cref="IChannel"/> objects.
    /// </summary>
#if UNITY_2020_2_OR_NEWER
    [RequireImplementors]
#endif
    public interface IWire : IServiceComponent
    {
        /// <summary>
        /// CreateChannel is a <see cref="IChannel"/> factory. It will generate an object enabling the subscription to a Wire channel.
        /// </summary>
        /// <param name="tokenProvider">Will be used to obtain a subscription token whenever the user calls <see cref="IChannel.SubscribeAsync"/>
        /// or Wire might choose to use it whenever a refreshed token is needed. Make sure that this <see cref="IChannelTokenProvider"/> can never provide
        /// an outdated or bad token.</param>
        /// <returns>A <see cref="IChannel"/> object.</returns>
        IChannel CreateChannel(IChannelTokenProvider tokenProvider);
    }
}
