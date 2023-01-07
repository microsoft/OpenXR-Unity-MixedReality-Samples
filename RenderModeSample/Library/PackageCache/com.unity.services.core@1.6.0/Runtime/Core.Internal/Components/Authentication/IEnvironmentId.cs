using Unity.Services.Core.Internal;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Services.Authentication.Internal
{
    /// <summary>
    /// Component providing the Environment Id
    /// </summary>
#if UNITY_2020_2_OR_NEWER
    [RequireImplementors]
#endif
    public interface IEnvironmentId : IServiceComponent
    {
        /// <summary>
        /// Returns the Environment ID when a sign in succeeds, otherwise null.
        /// </summary>
        string EnvironmentId { get; }
    }
}
