// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEditor;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    [CustomEditor(typeof(EyeLevelSceneOrigin))]
    internal class EyeLevelSceneOriginInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Enable this override if the XR experience assumes the scene origin at eye level.", MessageType.Info);
        }
    }
}
