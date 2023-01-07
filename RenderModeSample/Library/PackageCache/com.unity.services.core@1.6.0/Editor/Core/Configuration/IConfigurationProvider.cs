using Unity.Services.Core.Configuration.Internal;
using UnityEditor.Build;

namespace Unity.Services.Core.Configuration.Editor
{
    /// <summary>
    /// Contract for objects providing configuration values that needs to be passed
    /// to the <see cref="IProjectConfiguration"/> component at runtime.
    /// </summary>
    /// <remarks>
    /// Implementations must have a parameter-less constructor to be invoked.
    /// </remarks>
    public interface IConfigurationProvider : IOrderedCallback
    {
        /// <summary>
        /// Add configuration values to the given <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder used to create the runtime configuration data.
        /// Use it to set configuration values.
        /// </param>
        /// <remarks>
        /// All configuration values stored in the <paramref name="builder"/> will
        /// be persisted and included to the build in order to provide them to the
        /// <see cref="IProjectConfiguration"/> component at runtime.
        /// </remarks>
        void OnBuildingConfiguration(ConfigurationBuilder builder);
    }
}
