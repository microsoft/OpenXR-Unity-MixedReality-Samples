using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace UnityEditor.XR.ARFoundation
{
    [CustomEditor(typeof(ARSession))]
    class ARSessionEditor : Editor
    {
        SerializedProperty m_AttemptUpdate;
        SerializedProperty m_MatchFrameRate;
        SerializedProperty m_TrackingMode;

        static class Tooltips
        {
            public static readonly GUIContent attemptUpdate = new GUIContent(
                "Attempt Update",
                "If enabled, the session will attempt to update a supported device if its AR software is out of date.");

            public static readonly GUIContent matchFrameRate = new GUIContent(
                "Match Frame Rate",
                "If enabled, the Unity frame will be synchronized with the AR session. Otherwise, the AR session will be updated independently of the Unity frame.");

            public static readonly GUIContent trackingMode = new GUIContent(
                "Tracking Mode",
                "The requested tracking mode.");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_AttemptUpdate, Tooltips.attemptUpdate);

            EditorGUILayout.PropertyField(m_MatchFrameRate, Tooltips.matchFrameRate);
            if (m_MatchFrameRate.boolValue)
            {
                EditorGUILayout.HelpBox("'Match Frame Rate' does three things:\n" +
                    "- Blocks each render frame until the next AR frame is ready\n" +
                    "- Sets the target frame rate to the session's preferred update rate\n" +
                    "- Disables VSync\n" +
                    "These settings are not reverted when the ARSession is disabled.",
                    MessageType.Info);
            }

            EditorGUILayout.PropertyField(m_TrackingMode, Tooltips.trackingMode);

            serializedObject.ApplyModifiedProperties();
        }

        void OnEnable()
        {
            m_AttemptUpdate = serializedObject.FindProperty("m_AttemptUpdate");
            m_MatchFrameRate = serializedObject.FindProperty("m_MatchFrameRate");
            m_TrackingMode = serializedObject.FindProperty("m_TrackingMode");
        }
    }
}
