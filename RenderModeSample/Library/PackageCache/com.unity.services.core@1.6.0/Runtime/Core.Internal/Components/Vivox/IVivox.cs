using Unity.Services.Core.Internal;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Services.Vivox.Internal
{
    /// <summary>
    /// Provides utilities for performing simple Vivox actions or overriding
    /// the <see cref="IVivoxTokenProviderInternal"/> with a custom implementation.
    /// </summary>
#if UNITY_2020_2_OR_NEWER
    [RequireImplementors]
#endif
    public interface IVivox : IServiceComponent
    {
        /// <summary>
        /// Registers an <see cref="IVivoxTokenProviderInternal"/> that will be used as the primary
        /// token generator for all Vivox actions.
        /// </summary>
        /// <param name="tokenProvider">
        /// Token provider to register.
        /// </param>
        void RegisterTokenProvider(IVivoxTokenProviderInternal tokenProvider);
    }
}
