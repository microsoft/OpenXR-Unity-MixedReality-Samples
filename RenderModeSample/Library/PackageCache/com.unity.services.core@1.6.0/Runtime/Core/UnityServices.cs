using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Core
{
    /// <summary>
    /// Utility to initialize all Unity services from a single endpoint.
    /// </summary>
    public static class UnityServices
    {
        internal static IUnityServices Instance { get; set; }
        internal static TaskCompletionSource<object> InstantiationCompletion { get; set; }
        internal static ExternalUserIdProperty ExternalUserIdProperty = new ExternalUserIdProperty();

        /// <summary>
        /// Initialization state.
        /// </summary>
        public static ServicesInitializationState State
        {
            get
            {
                if (!UnityThreadUtils.IsRunningOnUnityThread)
                {
                    throw new ServicesInitializationException("You are attempting to access UnityServices.State from a non-Unity Thread." +
                        " UnityServices.State can only be accessed from Unity Thread");
                }

                if (Instance != null)
                {
                    return Instance.State;
                }

                if (InstantiationCompletion?.Task.Status == TaskStatus.WaitingForActivation)
                {
                    return ServicesInitializationState.Initializing;
                }

                return ServicesInitializationState.Uninitialized;
            }
        }

        /// <summary>
        /// ExternalUserId.
        /// Use this property to pass a user ID from a 3rd party identity provider to Unity Gaming Services
        /// </summary>
        public static string ExternalUserId
        {
            get => ExternalUserIdProperty.UserId;
            set => ExternalUserIdProperty.UserId = value;
        }

        /// <inheritdoc cref="InitializeAsync(InitializationOptions)"/>
        public static Task InitializeAsync()
        {
            return InitializeAsync(new InitializationOptions());
        }

        /// <summary>
        /// Single entry point to initialize all used services.
        /// </summary>
        /// <param name="options">
        /// The options to customize services initialization.
        /// </param>
        /// <exception cref="ServicesInitializationException">
        /// Exception when there's an error during services initialization
        /// </exception>
        /// <exception cref="UnityProjectNotLinkedException">
        /// Exception when the project is not linked to a cloud project id
        /// </exception>
        /// <exception cref="CircularDependencyException">
        /// Exception when two registered <see cref="IInitializablePackage"/> depend on the other.
        /// </exception>
        /// <returns>
        /// Return a handle to the initialization operation.
        /// </returns>
        [PreserveDependency("Register()", "Unity.Services.Core.Registration.CorePackageInitializer", "Unity.Services.Core.Registration")]
        [PreserveDependency("CreateStaticInstance()", "Unity.Services.Core.Internal.UnityServicesInitializer", "Unity.Services.Core.Internal")]
        [PreserveDependency("EnableServicesInitializationAsync()", "Unity.Services.Core.Internal.UnityServicesInitializer", "Unity.Services.Core.Internal")]
        [PreserveDependency("CaptureUnityThreadInfo()", "Unity.Services.Core.UnityThreadUtils", "Unity.Services.Core")]
        public static async Task InitializeAsync(InitializationOptions options)
        {
            if (!UnityThreadUtils.IsRunningOnUnityThread)
            {
                throw new ServicesInitializationException("You are attempting to initialize Unity Services from a non-Unity Thread." +
                    " Unity Services can only be initialized from Unity Thread");
            }

            if (!Application.isPlaying)
            {
                throw new ServicesInitializationException("You are attempting to initialize Unity Services in Edit Mode." +
                    " Unity Services can only be initialized in Play Mode");
            }

            if (Instance == null)
            {
                if (InstantiationCompletion == null)
                {
                    InstantiationCompletion = new TaskCompletionSource<object>();
                }

                await InstantiationCompletion.Task;
            }

            if (options is null)
            {
                options = new InitializationOptions();
            }

            await Instance.InitializeAsync(options);
        }
    }
}
