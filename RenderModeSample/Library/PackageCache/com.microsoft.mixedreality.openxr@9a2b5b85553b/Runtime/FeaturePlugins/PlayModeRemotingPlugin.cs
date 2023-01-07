// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
#endif

namespace Microsoft.MixedReality.OpenXR.Remoting
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = featureName,
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone },
        Company = "Microsoft",
        Desc = featureName + " in Unity editor.",
        DocumentationLink = "https://aka.ms/openxr-unity-editor-remoting",
        OpenxrExtensionStrings = requestedExtensions,
        Category = FeatureCategory.Feature,
        Required = false,
        Priority = -100,    // hookup before other plugins so it affects json before GetProcAddr.
        FeatureId = featureId,
        Version = "1.7.0")]
#endif
    [NativeLibToken(NativeLibToken = NativeLibToken.Remoting)]
    internal class PlayModeRemotingPlugin : OpenXRFeaturePlugin<PlayModeRemotingPlugin>
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        internal const string featureId = "com.microsoft.openxr.feature.playmoderemoting";
        internal const string featureName = "Holographic Remoting for Play Mode";
        private const string requestedExtensions = "XR_MSFT_holographic_remoting XR_MSFT_holographic_remoting_speech";
        private const string SettingsFileName = "MixedRealityOpenXRRemotingSettings.asset";
        private static string UserSettingsFolder => Path.Combine(Application.dataPath, "..", "UserSettings");
        private static string SettingsAssetPath => Path.Combine(UserSettingsFolder, SettingsFileName);

        [SerializeField, Tooltip("The host name or IP address of the player running in network server mode to connect to."), Obsolete("Use the remotingSettings values instead")]
        private string m_remoteHostName = string.Empty;

        [SerializeField, Tooltip("The port number of the server's handshake port."), Obsolete("Use the remotingSettings values instead")]
        private ushort m_remoteHostPort = 8265;

        [SerializeField, Tooltip("The max bitrate in Kbps to use for the connection."), Obsolete("Use the remotingSettings values instead")]
        private uint m_maxBitrate = 20000;

        [SerializeField, Tooltip("The video codec to use for the connection."), Obsolete("Use the remotingSettings values instead")]
        private RemotingVideoCodec m_videoCodec = RemotingVideoCodec.Auto;

        [SerializeField, Tooltip("Enable/disable audio remoting."), Obsolete("Use the remotingSettings values instead")]
        private bool m_enableAudio = false;

        private readonly bool m_playModeRemotingIsActive =
#if UNITY_EDITOR
            true;
#else
            false;
#endif
        private RemotingSettings m_remotingSettings;

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            if (enabled && m_playModeRemotingIsActive)
            {
                if (AppRemotingSubsystem.GetCurrent().TryEnableRemotingOverride())
                {
                    return AppRemotingSubsystem.GetCurrent().HookGetInstanceProcAddr(func);
                }
            }
            return func;
        }

        protected override void OnInstanceDestroy(ulong instance)
        {
            if (enabled && m_playModeRemotingIsActive)
            {
                AppRemotingSubsystem.GetCurrent().ResetRemotingOverride();
            }
            base.OnInstanceDestroy(instance);
        }

        protected override void OnSystemChange(ulong systemId)
        {
            base.OnSystemChange(systemId);

            if (systemId != 0 && m_playModeRemotingIsActive)
            {
                RemotingSettings remotingSettings = GetOrLoadRemotingSettings();
                AppRemotingSubsystem.GetCurrent().InitializePlayModeRemoting(new RemotingConnectConfiguration
                {
                    RemoteHostName = remotingSettings.RemoteHostName,
                    RemotePort = remotingSettings.RemoteHostPort,
                    MaxBitrateKbps = remotingSettings.MaxBitrate,
                    VideoCodec = remotingSettings.VideoCodec,
                    EnableAudio = remotingSettings.EnableAudio,
                    secureConnectConfiguration = null
                });
            }
        }

        protected override void OnSessionStateChange(int oldState, int newState)
        {
            if (m_playModeRemotingIsActive && (XrSessionState)newState == XrSessionState.LossPending)
            {
                AppRemotingSubsystem.GetCurrent().OnSessionLossPending();
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#endif
            }
        }

        internal RemotingSettings GetOrLoadRemotingSettings()
        {
            if (m_remotingSettings == null)
            {
                // If this file doesn't yet exist, create it and port from the old values.
                m_remotingSettings = CreateInstance<RemotingSettings>();

                if (File.Exists(SettingsAssetPath))
                {
                    using (StreamReader settingsReader = new StreamReader(SettingsAssetPath))
                    {
                        JsonUtility.FromJsonOverwrite(settingsReader.ReadToEnd(), m_remotingSettings);
                    }
                }
                else
                {
#pragma warning disable CS0618 // to use the obsolete fields to port to the new asset file
                    m_remotingSettings.RemoteHostName = m_remoteHostName;
                    m_remotingSettings.RemoteHostPort = m_remoteHostPort;
                    m_remotingSettings.MaxBitrate = m_maxBitrate;
                    m_remotingSettings.VideoCodec = m_videoCodec;
                    m_remotingSettings.EnableAudio = m_enableAudio;
#pragma warning restore CS0618
                }
            }

            return m_remotingSettings;
        }

#if UNITY_EDITOR
        internal bool HasValidSettings() => !string.IsNullOrEmpty(GetOrLoadRemotingSettings().RemoteHostName);

        private void SaveSettings()
        {
            // Don't try to load the settings here. If this is null, then there's
            // no need to do extra work to load and save the same file.
            // When remoting is used, this is guaranteed to be non-null.
            if (m_remotingSettings == null)
            {
                return;
            }

            if (!Directory.Exists(UserSettingsFolder))
            {
                Directory.CreateDirectory(UserSettingsFolder);
            }

            using (StreamWriter settingsWriter = new StreamWriter(SettingsAssetPath))
            {
                settingsWriter.Write(JsonUtility.ToJson(m_remotingSettings, true));
            }
        }

        protected override void GetValidationChecks(System.Collections.Generic.List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
            PlayModeRemotingValidator.GetValidationChecks(this, results);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() => SaveSettings();

        void ISerializationCallbackReceiver.OnAfterDeserialize() { } // Can't call EnsureSettingsLoaded() here, since Application.dataPath can't be accessed during deserialization

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
        {
            StartOrStopXRHelper.OnEnterPlaymodeInEditor();
        }
#endif
    }

    internal class RemotingSettings : ScriptableObject
    {
        [field: SerializeField, Tooltip("The host name or IP address of the player running in network server mode to connect to.")]
        public string RemoteHostName { get; set; } = string.Empty;

        [field: SerializeField, Tooltip("The port number of the server's handshake port.")]
        public ushort RemoteHostPort { get; set; } = 8265;

        [field: SerializeField, Tooltip("The max bitrate in Kbps to use for the connection.")]
        public uint MaxBitrate { get; set; } = 20000;

        [field: SerializeField, Tooltip("The video codec to use for the connection.")]
        public RemotingVideoCodec VideoCodec { get; set; } = RemotingVideoCodec.Auto;

        [field: SerializeField, Tooltip("Enable/disable audio remoting.")]
        public bool EnableAudio { get; set; } = false;
    }
}
