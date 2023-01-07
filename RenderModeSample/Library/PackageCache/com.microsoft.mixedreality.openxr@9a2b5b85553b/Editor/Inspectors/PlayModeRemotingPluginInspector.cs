// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Remoting;
using UnityEditor;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    [CustomEditor(typeof(PlayModeRemotingPlugin))]
    internal class PlayModeRemotingPluginInspector : UnityEditor.Editor
    {
        private PlayModeRemotingPlugin m_playModeRemotingPlugin;
        private UnityEditor.Editor m_remotingSettingsEditor;

        private void OnEnable()
        {
            m_playModeRemotingPlugin = target as PlayModeRemotingPlugin;
        }

        public override void OnInspectorGUI()
        {
            CreateCachedEditor(m_playModeRemotingPlugin.GetOrLoadRemotingSettings(), null, ref m_remotingSettingsEditor);

            if (m_remotingSettingsEditor == null)
            {
                return;
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            m_remotingSettingsEditor.OnInspectorGUI();
        }
    }
}
