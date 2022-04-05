// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Remoting;
using System;
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
    enum ConfigurationSelection
    {
        None,
        RunNativelyonHL2,
        RunNativelyonPCVR,
        RunRemotelyonUWP,
        RunRemotelyonWin32
    }
    public class ProjectConfiguratorWindow : EditorWindow
    {
        private ConfigurationSelection selectedConfiguration, previousSelectedConfiguration;
        private const float Default_Window_Height = 300.0f;
        private const float Default_Window_Width = 300.0f;

        public static ProjectConfiguratorWindow Instance { get; private set; }
        public static bool IsOpen => Instance != null;

        [MenuItem("ProjectConfigurator/Quick Setup", false, 499)]
        private static void ShowWindowFromMenu()
        {
            ShowWindow();
        }

        public static void ShowWindow()
        {
            // There should be only one configurator window open as a "pop-up". If already open, then just force focus on our instance
            if (IsOpen)
            {
                Instance.Focus();
            }
            else
            {
                var window = CreateInstance<ProjectConfiguratorWindow>();
                window.titleContent = new GUIContent("Basic Sample Project Quick Configurator", EditorGUIUtility.IconContent("_Popup").image);
                window.position = new Rect(Screen.width / 2.0f, Screen.height / 2.0f, Default_Window_Height, Default_Window_Width);
                window.ShowUtility();
            }
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 250;
            bool remoting = false;
            BuildTargetGroup targetGroup;
            previousSelectedConfiguration = selectedConfiguration;
            selectedConfiguration = (ConfigurationSelection)EditorGUILayout.EnumPopup("Select one of the following configurations:", selectedConfiguration);

            if(selectedConfiguration != previousSelectedConfiguration)
            {
                switch(selectedConfiguration)
                {
                    case ConfigurationSelection.RunNativelyonHL2:
                        targetGroup = BuildTargetGroup.WSA;
                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
                        EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.DevicePortal;
                        EditorUserBuildSettings.wsaArchitecture = "ARM64";
                        break;
                    case ConfigurationSelection.RunNativelyonPCVR:
                        targetGroup = BuildTargetGroup.Standalone;
                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                        break;
                    case ConfigurationSelection.RunRemotelyonUWP:
                        remoting = true;
                        targetGroup = BuildTargetGroup.WSA;
                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
                        EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.LocalMachine;
                        EditorUserBuildSettings.wsaArchitecture = "Intel64";
                        // TODO: need to set player capabilities
                        break;
                    case ConfigurationSelection.RunRemotelyonWin32:
                        remoting = true;
                        targetGroup = BuildTargetGroup.Standalone;
                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                        break;
                    default:
                        Debug.Log($"Could not find {selectedConfiguration}, setting default configuration as Standalone");
                        targetGroup = BuildTargetGroup.Standalone;
                        break;
                }

                Debug.Log($"Setting up for {selectedConfiguration}");
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
            }
        }
    }
}