using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEditor.XR.ARFoundation
{
    [CustomEditor(typeof(AROcclusionManager))]
    class AROcclusionManagerEditor : Editor
    {
        SerializedProperty m_EnvironmentDepthMode;
        SerializedProperty m_EnvironmentDepthTemporalSmoothing;
        SerializedProperty m_OcclusionPreferenceMode;
        SerializedProperty m_HumanSegmentationStencilMode;
        SerializedProperty m_HumanSegmentationDepthMode;

        static class Styles
        {
            public static readonly GUIContent environmentDepthTemporalSmoothing =
                new GUIContent(
                    text: "Temporal Smoothing",
                    tooltip: "Whether temporal smoothing should be applied to the environment depth image.");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool isEnvDepthEnabled = ((EnvironmentDepthMode)m_EnvironmentDepthMode.enumValueIndex).Enabled();
            bool isHumanSegmentationStencilEnabled = ((HumanSegmentationStencilMode)m_HumanSegmentationStencilMode.enumValueIndex).Enabled();
            bool isHumanSegmentationDepthEnabled = ((HumanSegmentationDepthMode)m_HumanSegmentationDepthMode.enumValueIndex).Enabled();
            bool isHumanDepthEnabled = isHumanSegmentationStencilEnabled && isHumanSegmentationDepthEnabled;

            if (!isEnvDepthEnabled && !isHumanDepthEnabled)
            {
                EditorGUILayout.HelpBox("Automatic occlusion is disabled.",
                                        MessageType.Warning);
            }

            EditorGUILayout.LabelField("Environment Depth", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                EditorGUILayout.PropertyField(m_EnvironmentDepthMode);
                EditorGUILayout.PropertyField(m_EnvironmentDepthTemporalSmoothing, Styles.environmentDepthTemporalSmoothing);
            }

            EditorGUILayout.LabelField("Human Segmentation", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                EditorGUILayout.PropertyField(m_HumanSegmentationStencilMode);
                if (!isHumanSegmentationDepthEnabled && isHumanSegmentationStencilEnabled)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        EditorGUILayout.HelpBox($"Human occlusion also requires {m_HumanSegmentationDepthMode.displayName} to be enabled.",
                                                MessageType.Warning);
                    }
                }

                EditorGUILayout.PropertyField(m_HumanSegmentationDepthMode);
                if (!isHumanSegmentationStencilEnabled && isHumanSegmentationDepthEnabled)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        EditorGUILayout.HelpBox($"Human occlusion also requires {m_HumanSegmentationStencilMode.displayName} to be enabled.",
                                                MessageType.Warning);
                    }
                }
            }

            EditorGUILayout.PropertyField(m_OcclusionPreferenceMode);

            serializedObject.ApplyModifiedProperties();
        }

        void OnEnable()
        {
            m_EnvironmentDepthMode = serializedObject.FindProperty("m_EnvironmentDepthMode");
            m_EnvironmentDepthTemporalSmoothing = serializedObject.FindProperty("m_EnvironmentDepthTemporalSmoothing");
            m_HumanSegmentationStencilMode = serializedObject.FindProperty("m_HumanSegmentationStencilMode");
            m_HumanSegmentationDepthMode = serializedObject.FindProperty("m_HumanSegmentationDepthMode");
            m_OcclusionPreferenceMode = serializedObject.FindProperty("m_OcclusionPreferenceMode");
        }
    }
}
