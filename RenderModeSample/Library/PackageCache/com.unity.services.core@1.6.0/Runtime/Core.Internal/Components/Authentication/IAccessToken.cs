using Unity.Services.Core.Internal;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Services.Authentication.Internal
{
    /// <summary>
    /// Contract for objects providing an access token to access remote services.
    /// </summary>
#if UNITY_2020_2_OR_NEWER
    [RequireImplementors]
#endif
    public interface IAccessToken : IServiceComponent
    {
        /// <summary>
        /// The current token to use to access remote services.
        /// </summary>
        string AccessToken { get; }
    }
}
