using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor.XR.Management.Metadata;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Management;

namespace UnityEditor.XR.Management
{
    class XRSettingsManager : SettingsProvider
    {
        internal static class Styles
        {
            public static readonly GUIStyle k_UrlLabelPersonal = new GUIStyle(EditorStyles.label)
            {
                name = "url-label",
                richText = true,
                normal = new GUIStyleState { textColor = new Color(8 / 255f, 8 / 255f, 252 / 255f) },
            };

            public static readonly GUIStyle k_UrlLabelProfessional = new GUIStyle(EditorStyles.label)
            {
                name = "url-label",
                richText = true,
                normal = new GUIStyleState { textColor = new Color(79 / 255f, 128 / 255f, 248 / 255f) },
            };

            public static readonly GUIStyle k_LabelWordWrap = new GUIStyle(EditorStyles.label) { wordWrap = true };
        }

        struct Content
        {
            public static readonly GUIContent k_InitializeOnStart = new GUIContent("Initialize XR on Startup");
            public static readonly GUIContent k_XRConfigurationText = new GUIContent("Information about configuration, tracking and migration can be found below.");
            public static readonly GUIContent k_XRConfigurationDocUriText = new GUIContent("View Documentation");
            public static readonly Uri k_XRConfigurationUri = new Uri(" https://docs.unity3d.com/Manual/configuring-project-for-xr.html");
        }

        internal static GUIStyle GetStyle(string styleName)
        {
            GUIStyle s = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (s == null)
            {
                Debug.LogError("Missing built-in guistyle " + styleName);
                s = GUI.skin.box;
            }
            return s;
        }

        static string s_SettingsRootTitle = $"Project/{XRConstants.k_XRPluginManagement}";
        static XRSettingsManager s_SettingsManager = null;

        internal static XRSettingsManager Instance => s_SettingsManager;

        private bool resetUi = false;
        internal bool ResetUi
        {
            get
            {
                return resetUi;
            }
            set
            {
                resetUi = value;
                if (resetUi)
                    Repaint();
            }
        }

        SerializedObject m_SettingsWrapper;

        private Dictionary<BuildTargetGroup, XRManagerSettingsEditor> CachedSettingsEditor = new Dictionary<BuildTargetGroup, XRManagerSettingsEditor>();


        private BuildTargetGroup m_LastBuildTargetGroup = BuildTargetGroup.Unknown;

        static XRGeneralSettingsPerBuildTarget currentSettings
        {
            get
            {
                XRGeneralSettingsPerBuildTarget generalSettings = null;
                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out generalSettings);
                if (generalSettings == null)
                {
                    lock(s_SettingsManager)
                    {
                        EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out generalSettings);
                        if (generalSettings == null)
                        {
                            string searchText = "t:XRGeneralSettings";
                            string[] assets = AssetDatabase.FindAssets(searchText);
                            if (assets.Length > 0)
                            {
                                string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                                generalSettings = AssetDatabase.LoadAssetAtPath(path, typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
                            }
                        }

                        if (generalSettings == null)
                        {
                            generalSettings = ScriptableObject.CreateInstance(typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
                            string assetPath = EditorUtilities.GetAssetPathForComponents(EditorUtilities.s_DefaultGeneralSettingsPath);
                            if (!string.IsNullOrEmpty(assetPath))
                            {
                                assetPath = Path.Combine(assetPath, "XRGeneralSettings.asset");
                                AssetDatabase.CreateAsset(generalSettings, assetPath);
                            }
                        }

                        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, generalSettings, true);

                    }
                }
                return generalSettings;
            }
        }

        [UnityEngine.Internal.ExcludeFromDocs]
        XRSettingsManager(string path, SettingsScope scopes = SettingsScope.Project) : base(path, scopes)
        {
        }

        [SettingsProvider]
        [UnityEngine.Internal.ExcludeFromDocs]
        static SettingsProvider Create()
        {
            if (s_SettingsManager == null)
            {
                s_SettingsManager = new XRSettingsManager(s_SettingsRootTitle);
            }

            return s_SettingsManager;
        }

        [SettingsProviderGroup]
        [UnityEngine.Internal.ExcludeFromDocs]
        static SettingsProvider[] CreateAllChildSettingsProviders()
        {
            List<SettingsProvider> ret = new List<SettingsProvider>();
            if (s_SettingsManager != null)
            {
                var ats = TypeLoaderExtensions.GetAllTypesWithAttribute<XRConfigurationDataAttribute>();
                foreach (var at in ats)
                {
                    if (at.FullName.Contains("Unity.XR.Management.TestPackage"))
                        continue;

                    XRConfigurationDataAttribute xrbda = at.GetCustomAttributes(typeof(XRConfigurationDataAttribute), true)[0] as XRConfigurationDataAttribute;
                    string settingsPath = String.Format("{1}/{0}", xrbda.displayName, s_SettingsRootTitle);
                    var resProv = new XRConfigurationProvider(settingsPath, xrbda.buildSettingsKey, at);
                    ret.Add(resProv);
                }
            }

            return ret.ToArray();
        }

        void InitEditorData(ScriptableObject settings)
        {
            if (settings != null)
            {
                m_SettingsWrapper = new SerializedObject(settings);
            }
        }


        /// <summary>See <see href="https://docs.unity3d.com/ScriptReference/SettingsProvider.html">SettingsProvider documentation</see>.</summary>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            InitEditorData(currentSettings);
        }

        /// <summary>See <see href="https://docs.unity3d.com/ScriptReference/SettingsProvider.html">SettingsProvider documentation</see>.</summary>
        public override void OnDeactivate()
        {
            m_SettingsWrapper = null;
            CachedSettingsEditor.Clear();
        }

        private void DisplayLoaderSelectionUI()
        {
            BuildTargetGroup buildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();

            try
            {
                bool buildTargetChanged = m_LastBuildTargetGroup != buildTargetGroup;
                if (buildTargetChanged)
                    m_LastBuildTargetGroup = buildTargetGroup;

                if (!currentSettings.HasManagerSettingsForBuildTarget(buildTargetGroup))
                {
                    currentSettings.CreateDefaultManagerSettingsForBuildTarget(buildTargetGroup);
                }
                XRGeneralSettings settings = currentSettings.SettingsForBuildTarget(buildTargetGroup);

                var serializedSettingsObject = new SerializedObject(settings);
                serializedSettingsObject.Update();

                SerializedProperty initOnStart = serializedSettingsObject.FindProperty("m_InitManagerOnStart");
                EditorGUILayout.PropertyField(initOnStart, Content.k_InitializeOnStart);
                EditorGUILayout.Space();


                SerializedProperty loaderProp = serializedSettingsObject.FindProperty("m_LoaderManagerInstance");
                var obj = loaderProp.objectReferenceValue;

                if (obj != null)
                {
                    loaderProp.objectReferenceValue = obj;

                    if (!CachedSettingsEditor.ContainsKey(buildTargetGroup))
                    {
                        CachedSettingsEditor.Add(buildTargetGroup, null);
                    }

                    if (CachedSettingsEditor[buildTargetGroup] == null)
                    {
                        CachedSettingsEditor[buildTargetGroup] = Editor.CreateEditor(obj) as XRManagerSettingsEditor;

                        if (CachedSettingsEditor[buildTargetGroup] == null)
                        {
                            Debug.LogError("Failed to create a view for XR Manager Settings Instance");
                        }
                    }

                    if (CachedSettingsEditor[buildTargetGroup] != null)
                    {
                        if (ResetUi)
                        {
                            ResetUi = false;
                            CachedSettingsEditor[buildTargetGroup].Reload();
                        }

                        CachedSettingsEditor[buildTargetGroup].BuildTarget = buildTargetGroup;
                        CachedSettingsEditor[buildTargetGroup].OnInspectorGUI();
                    }
                }
                else if (obj == null)
                {
                    settings.AssignedSettings = null;
                    loaderProp.objectReferenceValue = null;
                }

                serializedSettingsObject.ApplyModifiedProperties();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error trying to display plug-in assingment UI : {ex.Message}");
            }

            EditorGUILayout.EndBuildTargetSelectionGrouping();
        }

        private void DisplayLink(GUIContent text, Uri link, int leftMargin)
        {
            var labelStyle = EditorGUIUtility.isProSkin ? Styles.k_UrlLabelProfessional : Styles.k_UrlLabelPersonal;
            var size = labelStyle.CalcSize(text);
            var uriRect = GUILayoutUtility.GetRect(text, labelStyle);
            uriRect.x += leftMargin;
            uriRect.width = size.x;
            if (GUI.Button(uriRect, text, labelStyle))
            {
                System.Diagnostics.Process.Start(link.AbsoluteUri);
            }
            EditorGUIUtility.AddCursorRect(uriRect, MouseCursor.Link);
            EditorGUI.DrawRect(new Rect(uriRect.x, uriRect.y + uriRect.height - 1, uriRect.width, 1), labelStyle.normal.textColor);
        }

        private void DisplayXRTrackingDocumentationLink()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField(Content.k_XRConfigurationText, Styles.k_LabelWordWrap);
                DisplayLink(Content.k_XRConfigurationDocUriText, Content.k_XRConfigurationUri, 2);
            }
            GUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DisplayLoadOrderUi()
        {
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(XRPackageMetadataStore.isDoingQueueProcessing || EditorApplication.isPlaying || EditorApplication.isPaused);
            if (m_SettingsWrapper != null && m_SettingsWrapper.targetObject != null)
            {
                m_SettingsWrapper.Update();

                EditorGUILayout.Space();

                DisplayLoaderSelectionUI();

                m_SettingsWrapper.ApplyModifiedProperties();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

        }

        /// <summary>See <see href="https://docs.unity3d.com/ScriptReference/SettingsProvider.html">SettingsProvider documentation</see>.</summary>
        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();

            DisplayLoadOrderUi();
            DisplayXRTrackingDocumentationLink();

            base.OnGUI(searchContext);
        }

    }
}
