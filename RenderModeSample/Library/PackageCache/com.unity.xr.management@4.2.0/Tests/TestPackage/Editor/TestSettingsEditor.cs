using UnityEditor;

namespace Unity.XR.Management.TestPackage.Editor
{
    [CustomEditor(typeof(TestSettings))]
    public class TestSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Test only...");
        }
    }
}
