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
    public class ProjectConfiguratorWindow : EditorWindow
    {
        bool RunNativelyonHL2 = false;
        bool RunNativelyonPCVR = false;
        bool RunRemotelyonUWP = false;
        bool RunRemotelyonWin32 = false;

        private const float Default_Window_Height = 500.0f;
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
                window.titleContent = new GUIContent("Basic Sample Project Configurator", EditorGUIUtility.IconContent("_Popup").image);
                window.position = new Rect(Screen.width / 2.0f, Screen.height / 2.0f, Default_Window_Height, Default_Window_Width);
                window.ShowUtility();
            }
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 250;
            RunNativelyonHL2 = EditorGUILayout.Toggle("Setup to run natively on Hololens2 (UWP)", RunNativelyonHL2);
            RunNativelyonPCVR = EditorGUILayout.Toggle("Setup to run natively on PC VR (Win32)", RunNativelyonPCVR);
            RunRemotelyonUWP = EditorGUILayout.Toggle("Setup to run remotely on UWP", RunRemotelyonUWP);
            RunRemotelyonWin32 = EditorGUILayout.Toggle("Setup to run remotely on Win32", RunRemotelyonWin32);
            BuildTargetGroup targetGroup = RunNativelyonPCVR || RunRemotelyonWin32  ? BuildTargetGroup.Standalone : BuildTargetGroup.WSA;

            Debug.Log("entered onGUi");

            if (RunNativelyonHL2)
            {

            }
            else if (RunNativelyonPCVR)
            {

            } 
            else if (RunRemotelyonUWP || RunRemotelyonWin32)
            {
                Debug.Log($"Setting up app remoting for {targetGroup}");

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
                feature.enabled = true;

                XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);
                if (settings != null)
                {
                    Debug.Log($"changed initmanageronstart");
                    settings.InitManagerOnStart = false;
                }
            }
        }
    }
}