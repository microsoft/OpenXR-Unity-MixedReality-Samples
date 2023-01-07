using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Environments.Internal
{
    /// <summary>
    /// Component providing the Unity Service Environment
    /// </summary>
    public interface IEnvironments : IServiceComponent
    {
        /// <summary>
        /// Returns the name of the currently used Unity Service Environment
        /// </summary>
        string Current { get; }
    }
}
