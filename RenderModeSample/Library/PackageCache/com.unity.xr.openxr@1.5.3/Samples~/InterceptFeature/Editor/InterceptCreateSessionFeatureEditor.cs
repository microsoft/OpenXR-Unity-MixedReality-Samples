using UnityEngine;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Samples.InterceptFeature;

namespace UnityEditor.XR.OpenXR.Samples.InterceptFeature
{
    // see https://docs.unity3d.com/Manual/editor-CustomEditors.html for details on how to create custom editors
    [CustomEditor(typeof(InterceptCreateSessionFeature))]
    public class InterceptCreateSessionFeatureEditor : UnityEditor.Editor
    {
        private SerializedProperty message;

        void OnEnable()
        {
            // lookup any serialized properties you would want to draw in your UI.
            message = serializedObject.FindProperty("message");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // draws the default property field.
            EditorGUILayout.PropertyField(message);

            // https://docs.unity3d.com/ScriptReference/GUILayout.Button.html
            if (GUILayout.Button("Print This"))
            {
                message.stringValue = "Print This";
            }

            if (GUILayout.Button("Print That"))
            {
                message.stringValue = "Print That";
            }

            // See https://docs.unity3d.com/ScriptReference/GUILayout.html
            // and https://docs.unity3d.com/ScriptReference/EditorGUILayout.html
            // for advanced UI options.

            serializedObject.ApplyModifiedProperties();
        }
    }
}