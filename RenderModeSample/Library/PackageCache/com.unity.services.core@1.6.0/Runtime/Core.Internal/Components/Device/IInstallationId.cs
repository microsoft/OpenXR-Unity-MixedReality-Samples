using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Device.Internal
{
    /// <summary>
    /// Component providing a Unity Installation Identifier
    /// </summary>
    public interface IInstallationId : IServiceComponent
    {
        /// <summary>
        /// Returns Unity Installation Identifier
        /// </summary>
        /// <returns>The Installation Identifier</returns>
        string GetOrCreateIdentifier();
    }
}
