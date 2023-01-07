using System;
using System.ComponentModel;
using System.Text;
using Unity.Services.Core.Internal;
using UnityEditor;
using UnityEngine.Networking;

namespace Unity.Services.Core.Editor
{
    /// <summary>
    /// Base class for services which require service flag handling when toggling.
    /// </summary>
    public abstract class EditorGameServiceFlagEnabler : IEditorGameServiceEnabler
    {
        ServiceFlagEndpoint m_ServiceFlagEndpoint;

        /// <summary>
        /// Name of the service in the services flags API
        /// </summary>
        protected abstract string FlagName { get; }

        /// <summary>
        /// <inheritdoc cref="IEditorGameServiceEnabler.Enable"/>
        /// Also sends an API request to disable the service on the dashboard.
        /// </summary>
        public void Enable()
        {
            SetFlagStatus(true);
            EnableLocalSettings();
        }

        /// <summary>
        /// <inheritdoc cref="IEditorGameServiceEnabler.Disable"/>
        /// Also sends an API request to disable the service on the dashboard.
        /// </summary>
        public void Disable()
        {
            SetFlagStatus(false);
            DisableLocalSettings();
        }

        internal string GetFlagName()
        {
            return FlagName;
        }

        internal void InternalDisableLocalSettings()
        {
            DisableLocalSettings();
        }

        /// <inheritdoc cref="IEditorGameServiceEnabler.Enable"/>
        protected abstract void EnableLocalSettings();

        /// <inheritdoc cref="IEditorGameServiceEnabler.Disable"/>
        protected abstract void DisableLocalSettings();

        /// <inheritdoc cref="IEditorGameServiceEnabler.IsEnabled"/>
        public abstract bool IsEnabled();

        /// <summary>
        /// The event fired when the web request that handles setting the service flag is complete
        /// </summary>
        /// <remarks>
        /// This event is only raised when the constant <code>ENABLE_EDITOR_GAME_SERVICES</code> is defined.
        /// Kept outside of this define to avoid an API breaking change.
        /// </remarks>
#if !ENABLE_EDITOR_GAME_SERVICES
#pragma warning disable CS0067
#endif
        public event Action ServiceFlagRequestComplete;
#if !ENABLE_EDITOR_GAME_SERVICES
#pragma warning restore CS0067
#endif

        void SetFlagStatus(bool status)
        {
#if ENABLE_EDITOR_GAME_SERVICES
            if (string.IsNullOrEmpty(FlagName))
            {
                throw new InvalidEnumArgumentException("FlagName is null or empty. Set a proper value to properly manage the service flags.");
            }

            if (!CloudProjectSettings.projectBound)
            {
                return;
            }

            if (m_ServiceFlagEndpoint == null)
            {
                m_ServiceFlagEndpoint = new ServiceFlagEndpoint();
            }

            var configAsyncOp = m_ServiceFlagEndpoint.GetConfiguration();
            configAsyncOp.Completed += OnEndpointConfigurationReceived;

            void OnEndpointConfigurationReceived(IAsyncOperation<ServiceFlagEndpointConfiguration> endpointResponse)
            {
                if (endpointResponse?.Result == null)
                {
                    return;
                }

                var uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(endpointResponse.Result.BuildPayload(FlagName, status)));
                var serviceFlagRequest = new UnityWebRequest(
                    url: endpointResponse.Result.BuildServiceFlagUrl(CloudProjectSettings.projectId),
                    UnityWebRequest.kHttpVerbPUT,
                    null,
                    uploadHandler);
                serviceFlagRequest.SetRequestHeader("AUTHORIZATION", $"Bearer {CloudProjectSettings.accessToken}");
                serviceFlagRequest.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");
                serviceFlagRequest.SendWebRequest().completed += OnServiceFlagRequestCompleted;
            }

            void OnServiceFlagRequestCompleted(UnityEngine.AsyncOperation operation)
            {
                ((UnityWebRequestAsyncOperation)operation).webRequest.Dispose();
                ServiceFlagRequestComplete?.Invoke();
            }

#endif
        }
    }
}
