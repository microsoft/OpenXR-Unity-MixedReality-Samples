using UnityEngine;

using UnityEngine.XR.OpenXR.Features.OculusQuestSupport;

namespace UnityEditor.XR.OpenXR.Features.OculusQuestSupport
{
    [CustomEditor(typeof(OculusQuestFeature))]
    internal class OculusQuestFeatureEditor : Editor
    {
        private SerializedProperty targetQuest;
        private SerializedProperty targetQuest2;

        static GUIContent s_TargetQuestLabel = EditorGUIUtility.TrTextContent("Quest");
        static GUIContent s_TargetQuest2Label = EditorGUIUtility.TrTextContent("Quest 2");

        void OnEnable()
        {
            targetQuest = serializedObject.FindProperty("targetQuest");
            targetQuest2 = serializedObject.FindProperty("targetQuest2");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Target Devices", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(targetQuest, s_TargetQuestLabel);
            EditorGUILayout.PropertyField(targetQuest2, s_TargetQuest2Label);

            serializedObject.ApplyModifiedProperties();
        }
    }
}