using System.Threading.Tasks;

namespace Unity.Services.Core
{
    interface IUnityServices
    {
        ServicesInitializationState State { get; }

        InitializationOptions Options { get; }

        Task InitializeAsync(InitializationOptions options);
    }
}
