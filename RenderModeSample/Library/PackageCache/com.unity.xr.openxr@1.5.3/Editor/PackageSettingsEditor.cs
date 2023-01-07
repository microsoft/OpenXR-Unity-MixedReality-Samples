using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR
{
    [CustomEditor(typeof(OpenXRPackageSettings))]
    internal class PackageSettingsEditor : UnityEditor.Editor
    {
        OpenXRFeatureEditor m_FeatureEditor = null;
        Vector2 scrollPos = Vector2.zero;

#if XR_MGMT_4_1_0_OR_OLDER
        static PackageSettingsEditor s_LastPackageSettingsEditor = null;
#endif

        static class Content
        {
            public const float k_Space = 15.0f;

            public static readonly GUIContent k_renderModeLabel = new GUIContent("Render Mode");

            public static readonly GUIContent[] k_renderModeOptions = new GUIContent[2]
            {
                new GUIContent("Multi-pass"),
                new GUIContent("Single Pass Instanced"),
            };

            public static readonly GUIContent[] k_androidRenderModeOptions = new GUIContent[2]
            {
                new GUIContent("Multi-pass"),
                new GUIContent("Single Pass Instanced \\ Multi-view"),
            };
        }

        private void Awake()
        {
            m_FeatureEditor = OpenXRFeatureEditor.CreateFeatureEditor();

#if XR_MGMT_4_1_0_OR_OLDER
            // Due to a bug in XRManagement that was fixed in builds newer than 4.1.0 the OnDestroy method
            // is not called when the old package settings editor is abandoned.  This causes the OnUpdateFeatureSetState
            // event handlers to pile up unecessarily.  To fix this we maintain a static reference to the last editor
            // that Awake was called on and unregister the event handler.
            if (s_LastPackageSettingsEditor != null)
            {
                OpenXRFeatureSetManager.onFeatureSetStateChanged -= s_LastPackageSettingsEditor.OnFeatureSetStateChanged;
            }

            s_LastPackageSettingsEditor = this;
#endif

            OpenXRFeatureSetManager.onFeatureSetStateChanged += OnFeatureSetStateChanged;
        }

        private void OnDestroy()
        {
            OpenXRFeatureSetManager.onFeatureSetStateChanged -= OnFeatureSetStateChanged;
        }

        public void OnEnable()
        {
            OpenXRRuntimeSelector.RefreshRuntimeDetectorList();
        }

        public override void OnInspectorGUI()
        {
            var buildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();
            OpenXRProjectValidationRulesSetup.SetSelectedBuildTargetGroup(buildTargetGroup);

            OpenXRPackageSettings settings = serializedObject.targetObject as OpenXRPackageSettings;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            EditorGUILayout.BeginVertical();

            var openXrSettings = settings.GetSettingsForBuildTargetGroup(buildTargetGroup);
            var serializedOpenXrSettings = new SerializedObject(openXrSettings);

            EditorGUIUtility.labelWidth = 200;

            int newRenderMode;
            GUILayout.BeginHorizontal();
            if (buildTargetGroup == BuildTargetGroup.Android)
            {
                newRenderMode = EditorGUILayout.Popup(Content.k_renderModeLabel, (int)openXrSettings.renderMode, Content.k_androidRenderModeOptions);
            }
            else
            {
                newRenderMode = EditorGUILayout.Popup(Content.k_renderModeLabel, (int)openXrSettings.renderMode, Content.k_renderModeOptions);
            }

            if (newRenderMode != (int)openXrSettings.renderMode)
            {
                openXrSettings.renderMode = (OpenXRSettings.RenderMode)newRenderMode;
            }

            GUILayout.EndHorizontal();

            DrawPropertiesExcluding(serializedOpenXrSettings, "m_Script", "m_renderMode");

            EditorGUIUtility.labelWidth = 0;

            if (serializedOpenXrSettings.hasModifiedProperties)
                serializedOpenXrSettings.ApplyModifiedProperties();

            if (buildTargetGroup == BuildTargetGroup.Standalone)
            {
                EditorGUILayout.Space();
                OpenXRRuntimeSelector.DrawSelector();
            }

            EditorGUILayout.EndVertical();


            if (m_FeatureEditor != null)
            {
                EditorGUILayout.Space();
                m_FeatureEditor.OnGUI(buildTargetGroup);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndBuildTargetSelectionGrouping();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Helper method to force the project settings window to repaint.
        /// </summary>
        private static void RepaintProjectSettingsWindow()
        {
            var type = Type.GetType("UnityEditor.ProjectSettingsWindow,UnityEditor");
            if (null == type)
                return;

            var window = Resources.FindObjectsOfTypeAll(type).FirstOrDefault() as EditorWindow;
            if (window != null)
                window.Repaint();
        }

        private void OnFeatureSetStateChanged(BuildTargetGroup buildTargetGroup)
        {
            if (null == m_FeatureEditor)
                return;

            m_FeatureEditor.OnFeatureSetStateChanged(buildTargetGroup);

            RepaintProjectSettingsWindow();
        }
    }
}