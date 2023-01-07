// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    [CustomPropertyDrawer(typeof(DocURLAttribute))]
    internal class DocURLAttributeDrawer : PropertyDrawer
    {
        private static readonly GUIContent ButtonContent = new GUIContent(
            string.Empty, EditorGUIUtility.IconContent("_Help").image, "Click for documentation");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DocURLAttribute labelWidthAttribute = attribute as DocURLAttribute;

            if (!string.IsNullOrEmpty(labelWidthAttribute.Url))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    const float Spacing = 5f;
                    Vector2 size = EditorStyles.label.CalcSize(ButtonContent);
                    position.width -= size.x + Spacing;

                    EditorGUI.PropertyField(position, property, label);

                    position.x = position.width + Spacing;
                    position.width = size.x;

                    if (GUI.Button(position, ButtonContent, EditorStyles.label))
                    {
                        Help.BrowseURL(labelWidthAttribute.Url);
                    }
                }
            }
        }
    }
}
