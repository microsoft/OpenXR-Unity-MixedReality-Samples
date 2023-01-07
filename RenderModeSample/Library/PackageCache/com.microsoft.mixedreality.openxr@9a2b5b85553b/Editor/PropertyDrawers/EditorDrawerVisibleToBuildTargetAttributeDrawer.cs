// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Editor
{
    [CustomPropertyDrawer(typeof(EditorDrawerVisibleToBuildTargetAttribute))]
    internal class EditorDrawerVisibleToBuildTargetAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorDrawerVisibleToBuildTargetAttribute buildTargetAttribute = attribute as EditorDrawerVisibleToBuildTargetAttribute;

            if (Array.Exists(buildTargetAttribute.BuildTargetGroups,
                x => x == OpenXRFeatureSetManager.activeBuildTarget))
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}
