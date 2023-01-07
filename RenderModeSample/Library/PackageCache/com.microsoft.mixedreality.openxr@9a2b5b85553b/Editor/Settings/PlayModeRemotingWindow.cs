// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Remoting;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    /// <summary>
    /// Represents a standalone window for accessing holographic remoting for play mode settings.
    /// </summary>
    internal class PlayModeRemotingWindow : EditorWindow
    {
        private const string ConnectionInfo = "Clicking the \"Play\" button will connect Unity editor to the Holographic Remoting Player running on above IP address.";

        private static readonly GUIContent FeatureEnabledLabel = EditorGUIUtility.TrTextContent($"Disable {PlayModeRemotingPlugin.featureName}");
        private static readonly GUIContent FeatureDisabledLabel = EditorGUIUtility.TrTextContent($"Enable {PlayModeRemotingPlugin.featureName}");
        private static readonly GUIContent FixLabel = EditorGUIUtility.TrTextContent("Fix");

        private UnityEditor.Editor m_playModeRemotingPluginEditor;
        private PlayModeRemotingPlugin m_feature;
        private Vector2 m_scrollPos;

        /// <summary>
        /// Initializes the Remoting Window class
        /// </summary>
        [MenuItem(PlayModeRemotingValidator.PlayModeRemotingMenuPath)]
        [MenuItem(PlayModeRemotingValidator.PlayModeRemotingMenuPath2)]
        private static void Init()
        {
            GetWindow<PlayModeRemotingWindow>(PlayModeRemotingPlugin.featureName);
        }

        private void OnGUI()
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(m_scrollPos))
            {
                m_scrollPos = scroll.scrollPosition;

                if (m_feature == null)
                {
                    FeatureHelpers.RefreshFeatures(BuildTargetGroup.Standalone);
                    m_feature = OpenXRSettings.Instance.GetFeature<PlayModeRemotingPlugin>();
                }

                UnityEditor.Editor.CreateCachedEditor(m_feature, null, ref m_playModeRemotingPluginEditor);

                if (m_playModeRemotingPluginEditor == null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox($"An instance of {PlayModeRemotingPlugin.featureName} could not be found. Please open Project Settings > XR Plug-in Management > OpenXR to ensure it's properly loaded.", MessageType.Error);
                    return;
                }

                EditorGUILayout.Space();
                m_playModeRemotingPluginEditor.OnInspectorGUI();

                if (m_feature != null && m_feature.enabled)
                {
                    EditorGUILayout.Space();
                    bool hasValidSettings = m_feature.HasValidSettings();
                    bool isLoaderAssigned = PlayModeRemotingValidator.IsLoaderAssigned();
                    bool areDependenciesEnabled = PlayModeRemotingValidator.AreDependenciesEnabled();

                    if (hasValidSettings && isLoaderAssigned && areDependenciesEnabled)
                    {
                        EditorGUILayout.HelpBox(ConnectionInfo, MessageType.Info);
                    }
                    else
                    {
                        if (!hasValidSettings)
                        {
                            EditorGUILayout.HelpBox(PlayModeRemotingValidator.RemotingNotConfigured, MessageType.Error);
                        }

                        if (!isLoaderAssigned)
                        {
                            EditorGUILayout.HelpBox(PlayModeRemotingValidator.OpenXRLoaderNotAssigned, MessageType.Error);
                            if (GUILayout.Button(FixLabel))
                            {
                                PlayModeRemotingValidator.AssignLoader();
                            }
                        }

                        if (!areDependenciesEnabled)
                        {
                            EditorGUILayout.HelpBox(PlayModeRemotingValidator.DependenciesNotEnabled, MessageType.Error);
                            if (GUILayout.Button(FixLabel))
                            {
                                PlayModeRemotingValidator.EnableDependencies();
                            }
                        }
                    }
                }

                // Disable the "enable/disable" button when editor is already playing
                using (new EditorGUI.DisabledScope(EditorApplication.isPlaying == true))
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button(m_feature.enabled ? FeatureEnabledLabel : FeatureDisabledLabel))
                    {
                        m_feature.enabled = !m_feature.enabled;
                        if (m_feature.enabled)
                        {
                            // If the user turned on the feature, try to enable dependencies as well.
                            PlayModeRemotingValidator.EnableDependencies();
                        }
                    }
                }
            }
        }
    }
}
