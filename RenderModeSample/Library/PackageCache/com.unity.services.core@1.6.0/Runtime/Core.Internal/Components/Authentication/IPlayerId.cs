using System;
using Unity.Services.Core.Internal;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Services.Authentication.Internal
{
    /// <summary>
    /// Contract for objects providing information with the player identification (PlayerID) for currently signed in player.
    /// </summary>
#if UNITY_2020_2_OR_NEWER
    [RequireImplementors]
#endif
    public interface IPlayerId : IServiceComponent
    {
        /// <summary>
        /// The ID of the player.
        /// </summary>
        string PlayerId { get; }

        /// <summary>
        /// Event raised when the player id changed.
        /// </summary>
        event Action<string> PlayerIdChanged;
    }
}
