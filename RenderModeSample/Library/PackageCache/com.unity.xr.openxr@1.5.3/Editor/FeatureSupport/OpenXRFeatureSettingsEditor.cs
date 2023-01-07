using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

using UnityEditor;

namespace UnityEditor.XR.OpenXR.Features
{
    internal class OpenXRFeatureSettingsEditor : EditorWindow
    {
        struct Styles
        {
            public static GUIStyle s_WrapTextLabel;
        }

        struct Content
        {
            public static GUIContent k_NoSelectionTitleContent = new GUIContent("OpenXR Feature Settings");
            public static GUIContent k_NoSelectionHelpMsg = new GUIContent("There is no current feature selected for the this build target. Go to Player Settings->XR Plug-in Management->OpenXR to select a settings for a feature to edit.");
        }


        static void InitStyles()
        {
            if (Styles.s_WrapTextLabel == null)
            {
                Styles.s_WrapTextLabel = new GUIStyle(EditorStyles.label);
                Styles.s_WrapTextLabel.wordWrap = true;
            }
        }

        BuildTargetGroup activeBuildTarget;
        string activeFeatureId = String.Empty;

        Editor activeItemEditor = null;
        OpenXRFeature featureToEdit = null;
        OpenXRFeatureAttribute featureAttr = null;
        GUIContent uiName;
        GUIContent categoryName;
        GUIContent partner;
        GUIContent version;
        string documentationLink;
        bool itemDidChange = false;

        internal BuildTargetGroup ActiveBuildTarget
        {
            get => activeBuildTarget;
            set
            {
                if (value != activeBuildTarget)
                {
                    activeBuildTarget = value;
                    itemDidChange = true;
                    Repaint();
                }
            }
        }

        internal string ActiveItem
        {
            get => activeFeatureId;
            set
            {
                if (String.Compare(value, activeFeatureId, true) != 0)
                {
                    activeFeatureId = value;
                    itemDidChange = true;
                    Repaint();
                }
            }
        }

        public void InitActiveItemInfo()
        {
            if (!itemDidChange)
                return;

            itemDidChange = false;

            featureToEdit = null;
            titleContent = Content.k_NoSelectionTitleContent;
            activeItemEditor = null;
            categoryName = null;
            partner = null;
            version = null;
            documentationLink = null;

            featureToEdit = FeatureHelpers.GetFeatureWithIdForBuildTarget(activeBuildTarget, activeFeatureId);
            if (featureToEdit != null)
            {
                foreach (Attribute attr in Attribute.GetCustomAttributes(featureToEdit.GetType()))
                {
                    if (attr is OpenXRFeatureAttribute)
                    {
                        featureAttr = (OpenXRFeatureAttribute) attr;
                        break;
                    }
                }

                if (featureAttr != null)
                {
                    activeItemEditor = UnityEditor.Editor.CreateEditor(featureToEdit);
                    uiName = new GUIContent(featureAttr.UiName);
                    categoryName = new GUIContent($"Category: {featureAttr.Category.ToString()}");
                    partner = new GUIContent($"Author: {featureAttr.Company}");
                    version = new GUIContent($"Version: {featureAttr.Version}");
                    documentationLink = featureAttr.InternalDocumentationLink;
                    titleContent = uiName;
                }
            }

        }

        public void OnGUI()
        {
            InitStyles();
            InitActiveItemInfo();

            if (featureToEdit == null)
            {
                titleContent = Content.k_NoSelectionTitleContent;
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(Content.k_NoSelectionHelpMsg, Styles.s_WrapTextLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Feature Information", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.indentLevel += 1;
            EditorGUILayout.LabelField(categoryName);
            EditorGUILayout.LabelField(partner);
            EditorGUILayout.LabelField(version);

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();

            if (activeItemEditor != null)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Feature Settings", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel += 1;
                activeItemEditor.OnInspectorGUI();
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            if (!String.IsNullOrEmpty(documentationLink))
            {
                if (GUILayout.Button("Documentation", EditorStyles.linkLabel))
                {
                    UnityEngine.Application.OpenURL(documentationLink);
                }
            }
        }
    }
}