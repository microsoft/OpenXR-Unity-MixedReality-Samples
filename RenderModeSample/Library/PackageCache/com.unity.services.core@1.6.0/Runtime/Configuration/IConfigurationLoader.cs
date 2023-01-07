using System.Threading.Tasks;

namespace Unity.Services.Core.Configuration
{
    interface IConfigurationLoader
    {
        Task<SerializableProjectConfiguration> GetConfigAsync();
    }
}
