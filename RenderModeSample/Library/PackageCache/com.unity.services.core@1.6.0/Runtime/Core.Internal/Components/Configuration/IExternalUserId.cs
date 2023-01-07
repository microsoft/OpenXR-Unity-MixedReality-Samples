using System;
using Unity.Services.Core.Internal;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Services.Core.Configuration.Internal
{
    /// <summary>
    /// Component to provide external user ID, provided by a third party provider
    /// </summary>
#if UNITY_2020_2_OR_NEWER
    [RequireImplementors]
#endif
    public interface IExternalUserId : IServiceComponent
    {
        /// <summary>
        /// Get the external user id
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Event raised when the external id changed.
        /// </summary>
        event Action<string> UserIdChanged;
    }
}
