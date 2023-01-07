using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

using UnityEditorInternal;

namespace UnityEditor.XR.OpenXR.Features
{
    enum IssueType
    {
        None,
        Warning,
        Error
    }


    internal class ChildListItem
    {
        public GUIContent uiName;
        public GUIContent documentationIcon;
        public GUIContent categoryName;
        public GUIContent version;
        public GUIContent partner;
        public string partnerName;
        public string documentationLink;
        public bool settingsExpanded;
        public OpenXRFeature feature;
        public bool shouldDisplaySettings;
        public UnityEditor.Editor settingsEditor;
        public string featureId;
        public IssueType issueType;
    }


    internal class OpenXRFeatureEditor
    {
        /// <summary>
        /// Path of the OpenXR settings in the Settings window. Uses "/" as separator. The last token becomes the settings label if none is provided.
        /// </summary>
        public const string k_FeatureSettingsPathUI =
#if XR_MGMT_3_2_0_OR_NEWER
            "Project/XR Plug-in Management/OpenXR/Features";
#else
            "Project/XR Plugin Management/OpenXR/Features";
#endif

        static class Styles
        {
            public static float k_IconWidth = 16f;
            public static float k_DefaultSelectionWidth = 200f;
            public static float k_DefualtLineMultiplier = 2f;

            public static GUIStyle s_SelectionStyle = "TV Selection";
            public static GUIStyle s_SelectionBackground = "ScrollViewAlt";
            public static GUIStyle s_FeatureSetTitleLable;
            public static GUIStyle s_ListLabel;
            public static GUIStyle s_ListSelectedLabel;
            public static GUIStyle s_ListLabelToggle;
            public static GUIStyle s_Feature;
            public static GUIStyle s_FeatureSettings;
        }

        static class Content
        {
            public static readonly GUIContent k_HelpIcon = EditorGUIUtility.IconContent("_Help");
            public static readonly GUIContent k_SettingsIcon = EditorGUIUtility.IconContent("Settings");

            public static readonly GUIContent k_Settings = new GUIContent("", k_SettingsIcon.image, "Open settings editor for this feature.");
            public static readonly GUIContent k_InteractionProfilesTitle = new GUIContent("Interaction Profiles");
        }

        List<OpenXRFeatureSetManager.FeatureSetInfo> selectionListItems = new List<OpenXRFeatureSetManager.FeatureSetInfo>();
        OpenXRFeatureSetManager.FeatureSetInfo selectedItem = null;
        private List<IssueType> issuesPerFeatureSet = new List<IssueType>();

        List<ChildListItem> filteredListItems = new List<ChildListItem>();
        List<ChildListItem> allListItems = new List<ChildListItem>();

        Dictionary<string, ChildListItem> interactionItems = new Dictionary<string, ChildListItem>();
        List<string> selectedFeatureIds = new List<string>();
        ReorderableList interactionFeaturesList = null;
        HashSet<string> requiredFeatures = new HashSet<string>();

        FeatureHelpersInternal.AllFeatureInfo allFeatureInfos = null;
        BuildTargetGroup activeBuildTarget = BuildTargetGroup.Unknown;

        List<OpenXRFeature.ValidationRule> _issues = new List<OpenXRFeature.ValidationRule>();

        Dictionary<BuildTargetGroup, int> lastSelectedItemIndex = new Dictionary<BuildTargetGroup, int>();

        OpenXRFeatureSettingsEditor featureSetSettingsEditor = null;

        bool mustInitializeFeatures = false;

        static readonly string s_AllFeatures = "All Features";

        public OpenXRFeatureEditor()
        {
            SetupInteractionListUI();
        }

        (Rect, Rect) TakeFromFrontOfRect(Rect rect, float width)
        {
            var newRect = new Rect(rect);
            newRect.x = rect.x + 5;
            newRect.width = width;
            rect.x = (newRect.x + newRect.width) + 1;
            rect.width -= width + 1;
            return (newRect, rect);
        }

        void SetupInteractionListUI()
        {
            if (interactionFeaturesList != null)
                return;

            interactionFeaturesList = new ReorderableList(selectedFeatureIds, typeof(ChildListItem), false, true, true, true);

            interactionFeaturesList.drawHeaderCallback = (rect) =>
            {
                var labelSize = EditorStyles.label.CalcSize(Content.k_InteractionProfilesTitle);
                var labelRect = new Rect(rect);
                labelRect.width = labelSize.x;

                EditorGUI.LabelField(labelRect, Content.k_InteractionProfilesTitle, EditorStyles.label);
            };
            interactionFeaturesList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                Rect fieldRect;
                string featureId = selectedFeatureIds[index];
                var item = interactionItems[featureId];
                var labelSize = EditorStyles.label.CalcSize(item.uiName);
                (fieldRect, rect) = TakeFromFrontOfRect(rect, labelSize.x);
                EditorGUI.BeginDisabledGroup(requiredFeatures.Contains(item.featureId));
                EditorGUI.LabelField(fieldRect, item.uiName, EditorStyles.label);
                EditorGUI.EndDisabledGroup();

                if (!String.IsNullOrEmpty(item.documentationLink))
                {
                    var size = EditorStyles.label.CalcSize(item.documentationIcon);
                    (fieldRect, rect) = TakeFromFrontOfRect(rect, size.x);
                    if (GUI.Button(fieldRect, item.documentationIcon, EditorStyles.label))
                    {
                        UnityEngine.Application.OpenURL(item.documentationLink);
                    }
                }

                if (item.issueType != IssueType.None)
                {
                    GUIContent icon = (item.issueType == IssueType.Error) ? CommonContent.k_ValidationErrorIcon : CommonContent.k_ValidationWarningIcon;
                    var size = EditorStyles.label.CalcSize(icon);
                    (fieldRect, rect) = TakeFromFrontOfRect(rect, size.x);
                    if (GUI.Button(fieldRect, icon, EditorStyles.label))
                    {
                        OpenXRProjectValidationRulesSetup.ShowWindow(activeBuildTarget);
                    }
                }
            };
            interactionFeaturesList.onAddDropdownCallback = (rect, list) =>
            {
                GenericMenu menu = new GenericMenu();
                foreach (var kvp in interactionItems)
                {
                    if (selectedFeatureIds.IndexOf(kvp.Key) == -1)
                    {
                        menu.AddItem(kvp.Value.uiName, false, (object obj) =>
                        {
                            string featureId = obj as string;
                            if (!String.IsNullOrEmpty(featureId))
                            {
                                selectedFeatureIds.Add(featureId);
                                var interactionItem = interactionItems[featureId];
                                interactionItem.feature.enabled = true;
                            }
                        }, kvp.Key);
                    }
                }
                menu.DropDown(rect);
            };
            interactionFeaturesList.onCanRemoveCallback = (list) =>
            {
                var featureId = selectedFeatureIds[list.index];
                return !requiredFeatures.Contains(featureId);
            };
            interactionFeaturesList.onRemoveCallback = (list) =>
            {
                var featureId = selectedFeatureIds[list.index];
                if (requiredFeatures.Contains(featureId))
                    return;

                var interactionItem = interactionItems[featureId];
                interactionItem.feature.enabled = false;
                selectedFeatureIds.RemoveAt(list.index);
            };
        }


        void UpdateValidationIssues(BuildTargetGroup buildTargetGroup)
        {
            _issues.Clear();
            OpenXRProjectValidation.GetCurrentValidationIssues(_issues, buildTargetGroup);

            foreach (var item in allListItems)
            {
                item.issueType = GetValidationIssueType(item.feature);
            }

            foreach (var item in interactionItems.Values)
            {
                item.issueType = GetValidationIssueType(item.feature);
            }

            issuesPerFeatureSet.Clear();
            foreach (var featureSet in selectionListItems)
            {
                var featureSetIssue = IssueType.None;
                foreach (var item in allListItems)
                {
                    if (featureSet.featureIds == null)
                        break;
                    if (Array.IndexOf(featureSet.featureIds, item.featureId) == -1)
                        continue;

                    if (item.issueType == IssueType.Error)
                    {
                        featureSetIssue = IssueType.Error;
                        break;
                    }

                    if (item.issueType == IssueType.Warning)
                    {
                        featureSetIssue = IssueType.Warning;
                    }
                }
                issuesPerFeatureSet.Add(featureSetIssue);
            }
        }

        IssueType GetValidationIssueType(OpenXRFeature feature)
        {
            IssueType ret = IssueType.None;

            foreach (var issue in _issues)
            {
                if (feature == issue.feature)
                {
                    if (issue.error)
                    {
                        ret = IssueType.Error;
                        break;
                    }

                    ret = IssueType.Warning;
                }
            }

            return ret;
        }

        void DrawInteractionList()
        {
            EditorGUILayout.Space();

            if (interactionItems.Count == 0)
                return;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            interactionFeaturesList.DoLayoutList();
            EditorGUILayout.EndVertical();
        }

        void OnSelectItem(OpenXRFeatureSetManager.FeatureSetInfo selectedItem)
        {
            this.selectedItem = selectedItem;

            int selectedItemIndex = selectionListItems.IndexOf(selectedItem);
            if (lastSelectedItemIndex.ContainsKey(activeBuildTarget))
                lastSelectedItemIndex[activeBuildTarget] = selectedItemIndex;
            else
                lastSelectedItemIndex.Add(activeBuildTarget, selectedItemIndex);

            if (this.selectedItem != null)
            {
                if (String.IsNullOrEmpty(selectedItem.featureSetId))
                {
                    filteredListItems = allListItems.
                                            OrderBy((item) => item.uiName.text).
                                            ToList();
                }
                else
                {
                    filteredListItems = allListItems.
                                        Where((item) => Array.IndexOf(selectedItem.featureIds, item.featureId) > -1 ).
                                        OrderBy((item) => item.uiName.text).
                                        ToList();
                }
            }
        }


        void DrawSelectionList()
        {
            var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            var lineHeight = EditorGUIUtility.singleLineHeight * Styles.k_DefualtLineMultiplier;

            EditorGUILayout.BeginVertical(GUILayout.Width(Styles.k_DefaultSelectionWidth), GUILayout.ExpandWidth(true));
            {
                EditorGUILayout.LabelField("OpenXR Feature Groups", Styles.s_FeatureSetTitleLable);

                EditorGUILayout.BeginVertical(Styles.s_SelectionBackground, GUILayout.ExpandHeight(true));
                {
                    int index = 0;
                    foreach (var item in selectionListItems)
                    {
                        var typeOfIssues = issuesPerFeatureSet[index++];
                        var selected = (item == this.selectedItem);
                        var style = selected ? Styles.s_ListSelectedLabel : Styles.s_ListLabel;
                        bool disabled = item.uiName.text != "All" && item.featureIds == null;
                        EditorGUILayout.BeginHorizontal(style, GUILayout.ExpandWidth(true));
                        {
                            EditorGUI.BeginDisabledGroup(disabled);

                            if (String.Compare(item.uiName.text, s_AllFeatures, true) != 0)
                            {
                                var currentToggleState = item.isEnabled;
                                var newToggleState = EditorGUILayout.ToggleLeft("", currentToggleState, GUILayout.ExpandWidth(false), GUILayout.Width(Styles.k_IconWidth), GUILayout.Height(lineHeight));
                                if (newToggleState != currentToggleState)
                                {
                                    item.isEnabled = newToggleState;
                                    OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(activeBuildTarget);
                                }
                            }

                            if (GUILayout.Button(item.uiName, Styles.s_ListLabel, GUILayout.ExpandWidth(true), GUILayout.Height(lineHeight)))
                            {
                                OnSelectItem(item);
                            }

                            EditorGUI.EndDisabledGroup();

                            if (disabled && item.helpIcon != null)
                            {
                                if (GUILayout.Button(item.helpIcon, EditorStyles.label, GUILayout.Width(Styles.k_IconWidth), GUILayout.Height(lineHeight)))
                                {
                                    UnityEngine.Application.OpenURL(item.downloadLink);
                                }
                            }
                            if (typeOfIssues != IssueType.None)
                            {
                                GUIContent icon = (typeOfIssues == IssueType.Error) ? CommonContent.k_ValidationErrorIcon : CommonContent.k_ValidationWarningIcon;
                                if (GUILayout.Button(icon, EditorStyles.label, GUILayout.Width(Styles.k_IconWidth), GUILayout.Height(lineHeight)))
                                {
                                    OpenXRProjectValidationRulesSetup.ShowWindow(activeBuildTarget);
                                }
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }
        }

        void DrawFeatureList()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("", Styles.s_FeatureSetTitleLable);

                foreach (var filteredListItem in filteredListItems)
                {

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    {
                        EditorGUILayout.BeginVertical(Styles.s_Feature, GUILayout.ExpandWidth(false));
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                            {
                                var typeOfIssue = filteredListItem.issueType;
                                var featureNameSize = EditorStyles.toggle.CalcSize(filteredListItem.uiName);
                                var oldEnabledState = filteredListItem.feature.enabled;
                                EditorGUI.BeginDisabledGroup(requiredFeatures.Contains(filteredListItem.featureId));
                                filteredListItem.feature.enabled = EditorGUILayout.ToggleLeft(filteredListItem.uiName, filteredListItem.feature.enabled, GUILayout.ExpandWidth(false), GUILayout.Width(featureNameSize.x));
                                EditorGUI.EndDisabledGroup();

                                if (!String.IsNullOrEmpty(filteredListItem.documentationLink))
                                {
                                    if (GUILayout.Button(filteredListItem.documentationIcon, EditorStyles.label, GUILayout.Width(Styles.k_IconWidth)))
                                    {
                                        UnityEngine.Application.OpenURL(filteredListItem.documentationLink);
                                    }
                                }

                                if (typeOfIssue != IssueType.None)
                                {
                                    GUIContent icon = (typeOfIssue == IssueType.Error) ? CommonContent.k_ValidationErrorIcon : CommonContent.k_ValidationWarningIcon;
                                    if (GUILayout.Button(icon, EditorStyles.label, GUILayout.Width(Styles.k_IconWidth)))
                                    {
                                        OpenXRProjectValidationRulesSetup.ShowWindow(activeBuildTarget);
                                    }
                                }


                                if (filteredListItem.shouldDisplaySettings)
                                {
                                    if (GUILayout.Button(Content.k_Settings, Styles.s_FeatureSettings, GUILayout.ExpandWidth(true)))
                                    {
                                        if (featureSetSettingsEditor == null)
                                        {
                                            if (EditorWindow.HasOpenInstances<OpenXRFeatureSettingsEditor>())
                                            {
                                                featureSetSettingsEditor = EditorWindow.GetWindow<OpenXRFeatureSettingsEditor>();
                                            }
                                            else
                                            {
                                                featureSetSettingsEditor = ScriptableObject.CreateInstance<OpenXRFeatureSettingsEditor>() as OpenXRFeatureSettingsEditor;
                                            }
                                        }


                                        if (featureSetSettingsEditor != null)
                                        {
                                            featureSetSettingsEditor.ActiveItem = filteredListItem.featureId;
                                            featureSetSettingsEditor.ActiveBuildTarget = activeBuildTarget;
                                            featureSetSettingsEditor.ShowUtility();
                                            featureSetSettingsEditor.Focus();
                                        }
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.Space();

                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                }
                EditorGUILayout.EndVertical();
            }
        }

        void DrawFeatureSetUI()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            DrawSelectionList();
            DrawFeatureList();

            EditorGUILayout.EndHorizontal();
        }

        public void OnGUI(BuildTargetGroup buildTargetGroup)
        {
            InitStyles();
            Vector2 iconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(Styles.k_IconWidth, Styles.k_IconWidth));

            if (buildTargetGroup != activeBuildTarget || mustInitializeFeatures)
            {
                InitializeFeatures(buildTargetGroup);
                OpenXRFeatureSetManager.activeBuildTarget = buildTargetGroup;
                OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(buildTargetGroup);

                // This must be done after SetFeaturesFromEnabledFeatureSets to ensure we dont get an infinite update loop
                mustInitializeFeatures = false;

                if (EditorWindow.HasOpenInstances<OpenXRFeatureSettingsEditor>())
                {
                    featureSetSettingsEditor = EditorWindow.GetWindow<OpenXRFeatureSettingsEditor>();
                }

                if (featureSetSettingsEditor != null)
                {
                    featureSetSettingsEditor.ActiveBuildTarget = activeBuildTarget;
                }
            }

            if (allFeatureInfos != null)
            {
                UpdateValidationIssues(buildTargetGroup);
                DrawInteractionList();
                EditorGUILayout.Space();
                DrawFeatureSetUI();
            }

            EditorGUIUtility.SetIconSize(iconSize);
        }

        bool HasSettingsToDisplay(OpenXRFeature feature)
        {
            FieldInfo[] fieldInfo = feature.GetType().GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            foreach (var field in fieldInfo)
            {
                var nonSerializedAttrs = field.GetCustomAttributes(typeof(NonSerializedAttribute));
                if (nonSerializedAttrs.Count() == 0)
                    return true;
            }

            fieldInfo = feature.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            foreach (var field in fieldInfo)
            {
                var serializedAttrs = field.GetCustomAttributes(typeof(SerializeField));
                if (serializedAttrs.Count() > 0)
                    return true;
            }

            return false;
        }

        void InitializeFeatures(BuildTargetGroup group)
        {
            selectionListItems.Clear();
            filteredListItems.Clear();
            allListItems.Clear();
            interactionItems.Clear();
            selectedFeatureIds.Clear();
            requiredFeatures.Clear();

            allFeatureInfos = FeatureHelpersInternal.GetAllFeatureInfo(group);

            activeBuildTarget = group;

            var featureSets = OpenXRFeatureSetManager.FeatureSetInfosForBuildTarget(group).
                                OrderBy((fs) => fs.uiName.text);
            foreach(var featureSet in featureSets)
            {
                bool isKnownUninstalledFeatureSet = featureSet.featureIds == null && OpenXRFeatureSetManager.IsKnownFeatureSet(activeBuildTarget, featureSet.featureSetId);

                var featureSetFeatures = allFeatureInfos.Features.
                    Where((f) =>
                        featureSet.featureIds != null && Array.IndexOf(featureSet.featureIds, f.Attribute.FeatureId) > -1);

                if (isKnownUninstalledFeatureSet || featureSetFeatures.Any())
                {
                    selectionListItems.Add(featureSet);
                    if (featureSet.isEnabled && featureSet.requiredFeatureIds != null)
                        requiredFeatures.UnionWith(featureSet.requiredFeatureIds);
                }
            }

            foreach(var _ext in allFeatureInfos.Features)
            {
                if (_ext.Attribute.Hidden)
                    continue;

                var listItem = new ChildListItem()
                {
                    uiName = new GUIContent(_ext.Attribute.UiName),
                    documentationIcon = new GUIContent("", Content.k_HelpIcon.image, "Click for documentation"),
                    categoryName = new GUIContent($"Category: {_ext.Category.ToString()}"),
                    partner = new GUIContent($"Author: {_ext.Attribute.Company}"),
                    version = new GUIContent($"Version: {_ext.Attribute.Version}"),
                    partnerName = _ext.Attribute.Company,
                    documentationLink = _ext.Attribute.InternalDocumentationLink,
                    shouldDisplaySettings = HasSettingsToDisplay(_ext.Feature),
                    feature = _ext.Feature,
                    featureId = _ext.Attribute.FeatureId
                };

                if (_ext.Attribute.Category == UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction)
                {
                    interactionItems.Add(listItem.featureId, listItem);
                    if (listItem.feature.enabled)
                    {
                        selectedFeatureIds.Add(listItem.featureId);
                    }
                }
                else
                {
                    allListItems.Add(listItem);
                }
            }

            selectionListItems.Add(new OpenXRFeatureSetManager.FeatureSetInfo() {
                        uiName = new GUIContent(s_AllFeatures),
                        featureSetId = string.Empty,
                        featureIds = allFeatureInfos.Features.Select((e) => e.Attribute.FeatureId).ToArray(),
                    });

            var initialSelectedItem = selectionListItems[selectionListItems.Count - 1];
            if (lastSelectedItemIndex.ContainsKey(activeBuildTarget))
            {
                initialSelectedItem = selectionListItems[lastSelectedItemIndex[activeBuildTarget]];
            }
            OnSelectItem(initialSelectedItem);
        }

        void InitStyles()
        {
            if (Styles.s_ListLabel == null)
            {
                Styles.s_FeatureSetTitleLable = new GUIStyle(EditorStyles.label);
                Styles.s_FeatureSetTitleLable.fontSize = 14;
                Styles.s_FeatureSetTitleLable.fontStyle = FontStyle.Bold;

                Styles.s_ListLabel = new GUIStyle(EditorStyles.label);
                Styles.s_ListLabel.border = new RectOffset(0,0,0,0);
                Styles.s_ListLabel.padding = new RectOffset(5, 0, 0, 0);
                Styles.s_ListLabel.margin = new RectOffset(2, 2, 2, 2);

                Styles.s_ListSelectedLabel = new GUIStyle(Styles.s_SelectionStyle);
                Styles.s_ListSelectedLabel.border = Styles.s_ListLabel.border;
                Styles.s_ListSelectedLabel.padding = Styles.s_ListLabel.padding;
                Styles.s_ListSelectedLabel.margin = Styles.s_ListLabel.margin;

                Styles.s_ListLabelToggle = new GUIStyle(EditorStyles.toggle);
                Styles.s_ListLabelToggle.border = Styles.s_ListLabel.border;
                Styles.s_ListLabelToggle.padding = Styles.s_ListLabel.padding;
                Styles.s_ListLabelToggle.margin = Styles.s_ListLabel.margin;

                Styles.s_FeatureSettings = new GUIStyle(Styles.s_SelectionStyle);
                Styles.s_FeatureSettings.alignment = TextAnchor.MiddleRight;
                Styles.s_FeatureSettings.border = new RectOffset(2, 2, 0, 0);
                Styles.s_FeatureSettings.padding = new RectOffset(0, 2, 5, 0);

                Styles.s_Feature = new GUIStyle(Styles.s_SelectionStyle);
                Styles.s_Feature.border = new RectOffset(0, 0, 0, 0);
                Styles.s_Feature.padding = new RectOffset(5, 0, 0, 0);
                Styles.s_Feature.margin = new RectOffset(2, 2, 2, 2);
            }
        }

        public static OpenXRFeatureEditor CreateFeatureEditor()
        {
            if (OpenXRSettings.Instance == null)
                return null;
            if (TypeCache.GetTypesWithAttribute<OpenXRFeatureAttribute>().Count > 0)
                return new OpenXRFeatureEditor();
            return null;
        }

        internal void OnFeatureSetStateChanged(BuildTargetGroup buildTargetGroup)
        {
            if (activeBuildTarget != buildTargetGroup)
                return;

            mustInitializeFeatures = true;
        }
    }
}
