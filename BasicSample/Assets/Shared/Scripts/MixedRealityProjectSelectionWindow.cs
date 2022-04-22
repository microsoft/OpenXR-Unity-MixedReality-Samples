// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Remoting;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;


namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    enum MixedRealityProjectConfiguration
    {
        None,
        RunNativelyonPCVR,
        RunNativelyonHL2,
        RunRemotelyonUWP,
        RunRemotelyonWin32
    }
    public class MixedRealityProjectSelectionWindow : EditorWindow
    {
        private MixedRealityProjectConfiguration m_selectedMRConfiguration;
        private bool m_disablePopup;
        private const float Default_Window_Height = 300.0f;
        private const float Default_Window_Width = 400.0f;

        public static MixedRealityProjectSelectionWindow Instance { get; private set; }
        public static bool IsOpen => Instance != null;
        private static PopupUserSettings UserSettings;
        private const string SettingsFileName = "MixedRealityOpenXRProjectSelectionSettings.asset";
        private static string UserSettingsFolder => Path.Combine(Application.dataPath, "..", "UserSettings");
        private static string SettingsAssetPath => Path.Combine(UserSettingsFolder, SettingsFileName);

        [MenuItem("Mixed Reality/Quick Setup", false, 499)]
        private static void ShowWindowFromMenu()
        {
            ShowWindow(true);
        }

        public static void ShowWindow(bool showfromMenu)
        {
            GetUserSettings();
            // There should be only one configurator window open as a "pop-up". If already open, then just force focus on our instance
            if (IsOpen)
            {
                Instance.Focus();
            }
            else
            {
                if(!UserSettings.DisablePopup || showfromMenu)
                {
                    var window = CreateInstance<MixedRealityProjectSelectionWindow>();
                    window.titleContent = new GUIContent("MixedReality Project Selection Window", EditorGUIUtility.IconContent("_Popup").image);
                    window.position = new Rect(Screen.width / 2.0f, Screen.height / 2.0f, Default_Window_Height, Default_Window_Width);
                    window.ShowUtility();
                }

            }
        }

        internal static void GetUserSettings()
        {
            if(UserSettings == null)
            {
                UserSettings = CreateInstance<PopupUserSettings>();

                if (File.Exists(SettingsAssetPath))
                {
                    using (StreamReader settingsReader = new StreamReader(SettingsAssetPath))
                    {
                        JsonUtility.FromJsonOverwrite(settingsReader.ReadToEnd(), UserSettings);
                    }
                }
                else
                {
#pragma warning disable CS0618 // to use the obsolete fields to port to the new asset file
                    UserSettings.DisablePopup = false;
#pragma warning restore CS0618
                }
            }
        }

        private void SaveSettings()
        {
            if (UserSettings == null)
            {
                return;
            }
            
            if (!Directory.Exists(UserSettingsFolder))
            {
                Directory.CreateDirectory(UserSettingsFolder);
            }

            using (StreamWriter settingsWriter = new StreamWriter(SettingsAssetPath))
            {
                settingsWriter.Write(JsonUtility.ToJson(UserSettings, true));
            }
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 400;
            GUILayout.Space(15);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel) { fontStyle = FontStyle.Bold };
            GUILayout.Label("Welcome to the Mixed Reality OpenXR Samples!", titleStyle, GUILayout.Width(400));
            GUILayout.Space(10);
            GUILayout.Label("Change this project's settings for your Mixed Reality scenario:", GUILayout.Width(400));
            GUILayout.Space(5);

            m_selectedMRConfiguration = GUILayout.Button("Win32 app running on PC VR") ? MixedRealityProjectConfiguration.RunNativelyonPCVR :
                                        GUILayout.Button("UWP app running on HoloLens 2") ? MixedRealityProjectConfiguration.RunNativelyonHL2 :
                                        GUILayout.Button("Holographic Remoting remote UWP app") ? MixedRealityProjectConfiguration.RunRemotelyonUWP :
                                        GUILayout.Button("Holographic Remoting remote Win32 app") ? MixedRealityProjectConfiguration.RunRemotelyonWin32 :
                                        MixedRealityProjectConfiguration.None;
            m_disablePopup = GUILayout.Toggle(UserSettings.DisablePopup, "Don't show up this popup anymore");
            if(UserSettings.DisablePopup != m_disablePopup)
            {
                UserSettings.DisablePopup = m_disablePopup;
                SaveSettings();
            }
            ApplySelectedConfiguration(m_selectedMRConfiguration);
        }

        private void ApplySelectedConfiguration(MixedRealityProjectConfiguration selectedMRConfiguration)
        {
            bool remoting = false;
            BuildTargetGroup targetGroup;
            switch(selectedMRConfiguration)
            {
                case MixedRealityProjectConfiguration.RunNativelyonHL2:
                    targetGroup = BuildTargetGroup.WSA;
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
                    EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.DevicePortal;
                    EditorUserBuildSettings.wsaArchitecture = "ARM64";
                    break;
                case MixedRealityProjectConfiguration.RunNativelyonPCVR:
                    targetGroup = BuildTargetGroup.Standalone;
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                    break;
                case MixedRealityProjectConfiguration.RunRemotelyonUWP:
                    remoting = true;
                    targetGroup = BuildTargetGroup.WSA;
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
                    EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.LocalMachine;
                    EditorUserBuildSettings.wsaArchitecture = "Intel64";
                    // Player Capabilities
                    UnityEditor.PlayerSettings.WSA.SetCapability(UnityEditor.PlayerSettings.WSACapability.InternetClient, true);
                    UnityEditor.PlayerSettings.WSA.SetCapability(UnityEditor.PlayerSettings.WSACapability.InternetClientServer, true);
                    UnityEditor.PlayerSettings.WSA.SetCapability(UnityEditor.PlayerSettings.WSACapability.PrivateNetworkClientServer, true);
                    break;
                case MixedRealityProjectConfiguration.RunRemotelyonWin32:
                    remoting = true;
                    targetGroup = BuildTargetGroup.Standalone;
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                    break;
                default:
                    return;
            }

            const string AppRemotingPlugin = "Microsoft.MixedReality.OpenXR.Remoting.AppRemotingPlugin";
            Type appRemotingFeature = typeof(AppRemoting).Assembly.GetType(AppRemotingPlugin);
            if (appRemotingFeature == null)
            {
                Debug.LogError($"Could not find {AppRemotingPlugin}. Has this class been removed or renamed?");
                return;
            }

            FeatureHelpers.RefreshFeatures(targetGroup);
            OpenXRFeature feature = OpenXRSettings.ActiveBuildTargetInstance.GetFeature(appRemotingFeature);
            if (feature == null)
            {
                Debug.LogError($"Could not load {AppRemotingPlugin} as an OpenXR feature. Has this class been removed or renamed?");
                return;
            }
            feature.enabled = remoting? true:false;

            XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);
            if (settings != null)
            {
                settings.InitManagerOnStart = remoting? false:true;
            }
            Debug.Log($"Set up complete for {selectedMRConfiguration}");
        }
    }

    internal class PopupUserSettings : ScriptableObject
    {
        [field: SerializeField, Tooltip("Setting to disable MixedReality Project selection window popup")]
        public bool DisablePopup {get; set; } = false;
    }        
}