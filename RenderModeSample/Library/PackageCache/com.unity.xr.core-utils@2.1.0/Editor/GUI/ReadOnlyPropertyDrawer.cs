using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.GUI.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
}
