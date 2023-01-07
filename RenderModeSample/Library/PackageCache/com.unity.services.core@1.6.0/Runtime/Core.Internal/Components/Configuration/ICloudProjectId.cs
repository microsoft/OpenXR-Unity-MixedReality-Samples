using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Configuration.Internal
{
    /// <summary>
    /// Component to provide cloud project ID.
    /// </summary>
    public interface ICloudProjectId : IServiceComponent
    {
        /// <summary>
        /// Get cloud project ID at runtime.
        /// </summary>
        /// <returns>cloud project id</returns>
        string GetCloudProjectId();
    }
}
