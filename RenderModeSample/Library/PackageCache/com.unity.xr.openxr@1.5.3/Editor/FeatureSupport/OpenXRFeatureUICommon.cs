using System;

using UnityEditor;

using UnityEngine;

namespace UnityEditor.XR.OpenXR.Features
{
    static class CommonContent
    {
        public static readonly GUIContent k_Download = new GUIContent("Download");
        public static readonly GUIContent k_WarningIcon = EditorGUIUtility.IconContent("Warning@2x");
        public static readonly GUIContent k_ErrorIcon = EditorGUIUtility.IconContent("Error@2x");
        public static readonly GUIContent k_HelpIcon = EditorGUIUtility.IconContent("_Help@2x");

        public static readonly GUIContent k_Validation = new GUIContent("Your project has some settings that are incompatible with OpenXR. Click to open the project validator.");
        public static readonly GUIContent k_ValidationErrorIcon = new GUIContent("", CommonContent.k_ErrorIcon.image, k_Validation.text);
        public static readonly GUIContent k_ValidationWarningIcon = new GUIContent("", CommonContent.k_WarningIcon.image, k_Validation.text);
    }

}
