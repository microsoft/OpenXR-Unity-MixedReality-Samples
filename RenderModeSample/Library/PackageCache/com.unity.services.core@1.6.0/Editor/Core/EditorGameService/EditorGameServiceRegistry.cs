using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Editor
{
    /// <summary>
    /// Base implementation of the <see cref="IEditorGameServiceRegistry"/>
    /// </summary>
    public sealed class EditorGameServiceRegistry : IEditorGameServiceRegistry
    {
        internal readonly Dictionary<string, IEditorGameService> Services;
        readonly IProjectStateRequest m_ProjectStateRequest;
        readonly IProjectStateHelper m_ProjectStateHelper;
        readonly IServiceFlagRequest m_ServiceFlagRequest;
        ProjectState? m_CachedProjectState;

        internal UserRoleHandler UserRoleHandler { get; }

        /// <summary>
        /// Default constructor for the registry.
        /// </summary>
        internal EditorGameServiceRegistry(
            IProjectStateRequest projectStateRequest = null, IProjectStateHelper projectStateHelper = null,
            IServiceFlagRequest serviceFlagRequest = null, UserRoleHandler userRoleHandler = null)
        {
            Services = new Dictionary<string, IEditorGameService>();
            m_ProjectStateRequest = projectStateRequest;
            m_CachedProjectState = m_ProjectStateRequest?.GetProjectState();
            m_ProjectStateHelper = projectStateHelper;
            m_ServiceFlagRequest = serviceFlagRequest;
            UserRoleHandler = userRoleHandler;
        }

        /// <summary>
        /// Access to the editor game service registry
        /// </summary>
        public static EditorGameServiceRegistry Instance { get; internal set; } = new EditorGameServiceRegistry(
            new ProjectStateRequest(), new ProjectStateHelper(), new ServiceFlagRequest(), new UserRoleHandler());

        [InitializeOnLoadMethod]
        static void RegisterAllServices()
        {
            var types = TypeCache.GetTypesDerivedFrom<IEditorGameService>().ToList();

            foreach (var type in types)
            {
                if (TryGetServiceFromType(type, out var service))
                {
                    // Check to see if the service has already been added
                    // There is a use-case where this method would be called via the [InitializeOnLoadMethod] attribute
                    // but the domain has not actually reloaded
                    // That is an intended Unity feature, and as such this is a safety check.
                    if (!Instance.Services.ContainsKey(service.Identifier.GetKey()))
                    {
                        Instance.RegisterService(service);
                    }
                }
                else
                {
                    CoreLogger.LogError($"Type `{type.FullName}` is not a valid service.");
                }
            }

            Instance.StartListeningToProjectStateChanges();
        }

        void StartListeningToProjectStateChanges()
        {
#if ENABLE_EDITOR_GAME_SERVICES
            CloudProjectSettingsEventManager.instance.projectStateChanged += VerifyIfProjectBindChanges;
            CloudProjectSettingsEventManager.instance.projectRefreshed += VerifyIfProjectBindChanges;
#endif
        }

        /// <summary>
        /// Destructor of the registry.
        /// </summary>
        ~EditorGameServiceRegistry()
        {
            StopListeningToProjectStateChanges();
            UserRoleHandler?.Dispose();
        }

        void StopListeningToProjectStateChanges()
        {
#if ENABLE_EDITOR_GAME_SERVICES
            CloudProjectSettingsEventManager.instance.projectStateChanged -= VerifyIfProjectBindChanges;
            CloudProjectSettingsEventManager.instance.projectRefreshed -= VerifyIfProjectBindChanges;
#endif
        }

        void VerifyIfProjectBindChanges()
        {
            if (m_ProjectStateRequest == null || m_ProjectStateHelper == null)
            {
                return;
            }

            var currentProjectState = m_ProjectStateRequest.GetProjectState();
            if (m_ProjectStateHelper.IsProjectOnlyPartiallyBound(currentProjectState))
            {
                return;
            }

            if (m_ProjectStateHelper.IsProjectBeingUnbound(m_CachedProjectState, currentProjectState))
            {
                UpdateServicesForProjectUnbinding();
            }
            else if (m_ProjectStateHelper.IsProjectBeingBound(m_CachedProjectState, currentProjectState))
            {
                UpdateServicesForProjectBinding();
            }

            m_CachedProjectState = currentProjectState;
        }

        void UpdateServicesForProjectBinding()
        {
            if (m_ServiceFlagRequest == null)
            {
                return;
            }

            var asyncOperation = m_ServiceFlagRequest.FetchServiceFlags();
            asyncOperation.Completed += UpdateServiceActivation;
        }

        void UpdateServiceActivation(IAsyncOperation<IDictionary<string, bool>> flagsAsyncOperation)
        {
            foreach (var service in Services.Values)
            {
                if (!(service.Enabler is EditorGameServiceFlagEnabler serviceFlagEnabler))
                {
                    continue;
                }

                var serviceFlags = flagsAsyncOperation.Result;
                var flagName = serviceFlagEnabler.GetFlagName();
                if (!(serviceFlags?.ContainsKey(flagName) ?? false))
                {
                    continue;
                }

                if (serviceFlags[flagName])
                {
                    serviceFlagEnabler.Enable();
                }
                else
                {
                    serviceFlagEnabler.Disable();
                }
            }
        }

        void UpdateServicesForProjectUnbinding()
        {
            var serviceEnablers = Services.Values
                .Where(service => service.Enabler != null)
                .Select(service => service.Enabler);
            foreach (var serviceEnabler in serviceEnablers)
            {
                if (serviceEnabler is EditorGameServiceFlagEnabler serviceFlagEnabler)
                {
                    serviceFlagEnabler.InternalDisableLocalSettings();
                }
                else
                {
                    serviceEnabler.Disable();
                }
            }
        }

        internal static bool TryGetServiceFromType(Type type, out IEditorGameService service)
        {
            bool output;
            service = null;

            try
            {
                service = (IEditorGameService)Activator.CreateInstance(type);
                output = IsIdentifierValid(service);
            }
            catch (MissingMethodException)
            {
                output = false;
            }

            if (!output)
            {
                service = null;
            }

            return output;
        }

        internal static bool IsIdentifierValid(IEditorGameService editorGameService)
        {
            if (editorGameService.Identifier == null)
            {
                throw new NoNullAllowedException($"The Identifier for the service {editorGameService.Name} is null. Cannot register the service.");
            }

            if (string.IsNullOrEmpty(editorGameService.Identifier.GetKey()))
            {
                throw new ArgumentException($"The Identifier key for the service {editorGameService.Name} is null or empty. Cannot register the service.");
            }

            return true;
        }

        internal void RegisterService(IEditorGameService editorGameService)
        {
            if (!IsIdentifierValid(editorGameService))
            {
                return;
            }

            if (Services.ContainsKey(editorGameService.Identifier.GetKey()))
            {
                throw new DuplicateNameException($"The Identifier key {editorGameService.Identifier.GetKey()} already exists in the registry. Cannot register the service.");
            }

            Services.Add(editorGameService.Identifier.GetKey(), editorGameService);
        }

        /// <summary>
        /// Get the instance of a registered <see cref="IEditorGameService"/>.
        /// </summary>
        /// <typeparam name="TIdentifier">The type of the identifier for a service</typeparam>
        /// <returns>Return the instance of the given <see cref="IEditorGameService"/> type if it has been registered.</returns>
        public IEditorGameService GetEditorGameService<TIdentifier>()
            where TIdentifier : struct, IEditorGameServiceIdentifier
        {
            IEditorGameService output = null;

            var serviceIdentifier = new TIdentifier();
            if (Services.ContainsKey(serviceIdentifier.GetKey()))
            {
                output = Services[serviceIdentifier.GetKey()];
            }

            return output;
        }
    }
}
