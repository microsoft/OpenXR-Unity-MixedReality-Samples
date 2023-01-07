using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

#if PROJECT_VALIDATION_AVAILABLE
using System;
using Unity.XR.CoreUtils.Editor;
#endif

namespace UnityEditor.XR.OpenXR
{
#if PROJECT_VALIDATION_AVAILABLE
    internal class OpenXRProjectValidationRulesSetup
    {
        static BuildTargetGroup[] s_BuildTargetGroups =
            ((BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup))).Distinct().ToArray();

        static BuildTargetGroup s_SelectedBuildTargetGroup = BuildTargetGroup.Unknown;

        internal const string OpenXRProjectValidationSettingsPath = "Project/XR Plug-in Management/Project Validation";

        [InitializeOnLoadMethod]
        static void OpenXRProjectValidationCheck()
        {
            UnityEditor.PackageManager.Events.registeredPackages += (packageRegistrationEventArgs) =>
            {
                // In the Player Settings UI we have to delay the call one frame to let OpenXRSettings constructor to get initialized
                EditorApplication.delayCall += () => {
                    if (HasXRPackageVersionChanged(packageRegistrationEventArgs))
                    {
                        ShowWindowIfIssuesExist();
                    }
                };
            };

            AddOpenXRValidationRules();
        }

        private static bool HasXRPackageVersionChanged(PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            bool packageChanged = packageRegistrationEventArgs.changedTo.Any(p => p.name.Equals(OpenXRManagementSettings.PackageId));
            OpenXRSettings.Instance.versionChanged = packageChanged;
            return packageRegistrationEventArgs.added.Any(p => p.name.Equals(OpenXRManagementSettings.PackageId)) || packageChanged;
        }

        private static void ShowWindowIfIssuesExist()
        {
            List<OpenXRFeature.ValidationRule> failures = new List<OpenXRFeature.ValidationRule>();
            BuildTargetGroup activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            OpenXRProjectValidation.GetCurrentValidationIssues(failures, activeBuildTargetGroup);

            if (failures.Count > 0)
            {
                ShowWindow();
            }
        }

        static void AddOpenXRValidationRules()
        {
            foreach (var buildTargetGroup in s_BuildTargetGroups)
            {
                var issues = new List<OpenXRFeature.ValidationRule>();
                OpenXRProjectValidation.GetAllValidationIssues(issues, buildTargetGroup);

                var coreIssues = new List<BuildValidationRule>();
                foreach (var issue in issues)
                {

                    var rule = new BuildValidationRule
                    {
                        // This will hide the rules given a condition so that when you click "Show all" it doesn't show up as passed
                        IsRuleEnabled = () =>
                        {
                            // If OpenXR isn't enabled, no need to show the rule
                            if (!BuildHelperUtils.HasLoader(buildTargetGroup, typeof(OpenXRLoaderBase)))
                                return false;

                            // If the feature isn't enabled, don't show this rule
                            if (issue.feature != null && !issue.feature.enabled)
                                return false;

                            return true;
                        },
                        CheckPredicate = issue.checkPredicate,
                        Error = issue.error,
                        FixIt = issue.fixIt,
                        FixItAutomatic = issue.fixItAutomatic,
                        FixItMessage = issue.fixItMessage,
                        HelpLink = issue.helpLink,
                        HelpText = issue.helpText,
                        Message = issue.message,
                        Category = issue.feature != null ? issue.feature.nameUi : "OpenXR",
                        SceneOnlyValidation = false
                    };

                    coreIssues.Add(rule);
                }

                BuildValidator.AddRules(buildTargetGroup, coreIssues);
            }
        }

        [MenuItem("Window/XR/OpenXR/Project Validation")]
        private static void MenuItem()
        {
            ShowWindow();
        }

        internal static void SetSelectedBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            if (s_SelectedBuildTargetGroup == buildTargetGroup)
                return;

            s_SelectedBuildTargetGroup = buildTargetGroup;
        }

        internal static void ShowWindow(BuildTargetGroup buildTargetGroup = BuildTargetGroup.Unknown)
        {
            // Delay opening the window since sometimes other settings in the player settings provider redirect to the
            // project validation window causing serialized objects to be nullified
            EditorApplication.delayCall += () =>
            {
                SetSelectedBuildTargetGroup(buildTargetGroup);
                SettingsService.OpenProjectSettings(OpenXRProjectValidationSettingsPath);
            };
        }

        internal static void CloseWindow(){}
    }
#else // !PROJECT_VALIDATION_AVAILABLE just display the original editor window
    [InitializeOnLoad]
    internal class OpenXRProjectValidationRulesSetup : EditorWindow
    {
        static OpenXRProjectValidationRulesSetup()
        {
            UnityEditor.PackageManager.Events.registeredPackages += (packageRegistrationEventArgs) =>
            {
                if (HasXRPackageVersionChanged(packageRegistrationEventArgs))
                {
                    ShowWindowIfIssuesExist();
                }
            };
        }

        private static bool HasXRPackageVersionChanged(PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            bool packageChanged = packageRegistrationEventArgs.changedTo.Any(p => p.name.Equals(OpenXRManagementSettings.PackageId));
            OpenXRSettings.Instance.versionChanged = packageChanged;
            return packageRegistrationEventArgs.added.Any(p => p.name.Equals(OpenXRManagementSettings.PackageId)) || packageChanged;
        }

        private static void ShowWindowIfIssuesExist()
        {
            List<OpenXRFeature.ValidationRule> failures = new List<OpenXRFeature.ValidationRule>();
            BuildTargetGroup activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            OpenXRProjectValidation.GetCurrentValidationIssues(failures, activeBuildTargetGroup);

            if (failures.Count > 0)
            {
                ShowWindow();
            }
        }

        private Vector2 _scrollViewPos = Vector2.zero;

        private readonly List<OpenXRFeature.ValidationRule> _failures = new List<OpenXRFeature.ValidationRule>();

        // Fix all state
        private List<OpenXRFeature.ValidationRule> _fixAllStack = new List<OpenXRFeature.ValidationRule>();

        /// <summary>
        /// Last time the issues in the window were upated
        /// </summary>
        private double _lastUpdate = 0.0f;

        /// <summary>
        /// Interval that that issues should be updated
        /// </summary>
        private const double k_UpdateInterval = 1.0f;

        /// <summary>
        /// Interval that that issues should be updated when the window does not have focus
        /// </summary>
        private const double k_BackgroundUpdateInterval = 3.0f;

        private static class Content
        {
            public static readonly GUIContent k_Title = new GUIContent(" OpenXR Project Validation", CommonContent.k_ErrorIcon.image);
            public static readonly GUIContent k_FixButton = new GUIContent("Fix", "");
            public static readonly GUIContent k_EditButton = new GUIContent("Edit", "");
            public static readonly GUIContent k_PlayMode = new GUIContent("  Exit play mode before fixing project validation issues.", EditorGUIUtility.IconContent("console.infoicon").image);
            public static readonly GUIContent k_HelpButton = new GUIContent(CommonContent.k_HelpIcon.image);
            public static readonly Vector2 k_IconSize = new Vector2(16.0f, 16.0f);
        }

        private static class Styles
        {
            public static GUIStyle s_SelectionStyle = "TV Selection";
            public static GUIStyle s_IssuesBackground = "ScrollViewAlt";
            public static GUIStyle s_ListLabel;
            public static GUIStyle s_IssuesTitleLabel;
            public static GUIStyle s_Wrap;
            public static GUIStyle s_Icon;
            public static GUIStyle s_InfoBanner;
            public static GUIStyle s_FixAll;
        }

        private static bool s_Dirty = true;
        private static BuildTargetGroup s_SelectedBuildTargetGroup = BuildTargetGroup.Unknown;

        [MenuItem("Window/XR/OpenXR/Project Validation")]
        private static void MenuItem()
        {
            ShowWindow();
        }

        internal static void SetSelectedBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            if (s_SelectedBuildTargetGroup == buildTargetGroup)
                return;

            s_Dirty = true;
            s_SelectedBuildTargetGroup = buildTargetGroup;
        }

        internal static void ShowWindow(BuildTargetGroup buildTargetGroup = BuildTargetGroup.Unknown)
        {
            SetSelectedBuildTargetGroup(buildTargetGroup);

            var window = (OpenXRProjectValidationRulesSetup) GetWindow(typeof(OpenXRProjectValidationRulesSetup));
            window.titleContent = Content.k_Title;
            window.minSize = new Vector2(500.0f, 300.0f);
            window.UpdateIssues();
            window.Show();
        }

        internal static void CloseWindow()
        {
            var window = (OpenXRProjectValidationRulesSetup) GetWindow(typeof(OpenXRProjectValidationRulesSetup));
            window.Close();
        }

        private static void InitStyles()
        {
            if (Styles.s_ListLabel != null)
                return;

            Styles.s_ListLabel = new GUIStyle(Styles.s_SelectionStyle)
            {
                border = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(5, 5, 5, 5),
                margin = new RectOffset(5, 5, 5, 5)
            };

            Styles.s_IssuesTitleLabel = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(10, 10, 0, 0)
            };

            Styles.s_Wrap = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 5, 1, 1)
            };

            Styles.s_Icon = new GUIStyle(EditorStyles.label)
            {
                margin = new RectOffset(5, 5, 0, 0),
                fixedWidth = Content.k_IconSize.x * 2
            };

            Styles.s_InfoBanner = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(10,10,15,5)
            };

            Styles.s_FixAll = new GUIStyle(EditorStyles.miniButton)
            {
                stretchWidth = false,
                fixedWidth = 80,
                margin = new RectOffset(0,10,2,2)
            };
        }

        protected void OnFocus() => UpdateIssues(true);

        protected void Update() => UpdateIssues();

        private void DrawIssuesList()
        {
            var hasFix = _failures.Any(f => f.fixIt != null);
            var hasAutoFix = hasFix && _failures.Any(f => f.fixIt != null && f.fixItAutomatic);

            // Header
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                EditorGUILayout.LabelField($"Issues ({_failures.Count})", Styles.s_IssuesTitleLabel);
            }

            // FixAll button
            if (hasAutoFix)
            {
                using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || _fixAllStack.Count > 0))
                {
                    if (GUILayout.Button("Fix All", Styles.s_FixAll))
                        _fixAllStack = _failures.Where(i => i.fixIt != null && i.fixItAutomatic).ToList();
                }
            }
            EditorGUILayout.EndHorizontal();

            _scrollViewPos = EditorGUILayout.BeginScrollView(_scrollViewPos, Styles.s_IssuesBackground, GUILayout.ExpandHeight(true));

            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                foreach (var result in _failures)
                {
                    EditorGUILayout.BeginHorizontal(Styles.s_ListLabel);

                    GUILayout.Label(result.error ? CommonContent.k_ErrorIcon : CommonContent.k_WarningIcon, Styles.s_Icon, GUILayout.Width(Content.k_IconSize.x));

                    string message = result.message;
                    if (result.feature != null)
                        message = $"[{result.feature.nameUi}] {result.message}";
                    GUILayout.Label(message, Styles.s_Wrap);
                    GUILayout.FlexibleSpace();

                    if (!string.IsNullOrEmpty(result.helpText) || !string.IsNullOrEmpty(result.helpLink))
                    {
                        Content.k_HelpButton.tooltip = result.helpText;
                        if (GUILayout.Button(Content.k_HelpButton, Styles.s_Icon, GUILayout.Width(Content.k_IconSize.x * 1.5f)))
                        {
                            if (!string.IsNullOrEmpty(result.helpLink))
                                UnityEngine.Application.OpenURL(result.helpLink);
                        }
                    }
                    else
                        GUILayout.Label("", GUILayout.Width(Content.k_IconSize.x * 1.5f));


                    if (result.fixIt != null)
                    {
                        using (new EditorGUI.DisabledScope(_fixAllStack.Count != 0))
                        {
                            var button = result.fixItAutomatic ? Content.k_FixButton : Content.k_EditButton;
                            button.tooltip = result.fixItMessage;
                            if (GUILayout.Button(button, GUILayout.Width(80.0f)))
                            {
                                if (result.fixItAutomatic)
                                    _fixAllStack.Add(result);
                                else
                                    result.fixIt();
                            }
                        }
                    }
                    else if (hasFix)
                    {
                        GUILayout.Label("", GUILayout.Width(80.0f));
                    }


                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void UpdateIssues(bool force = false)
        {
            var interval = EditorWindow.focusedWindow == this ? k_UpdateInterval : k_BackgroundUpdateInterval;
            if (!s_Dirty && !force && EditorApplication.timeSinceStartup - _lastUpdate < interval)
                return;

            s_Dirty = false;

            if (_fixAllStack.Count > 0)
            {
                _fixAllStack[0].fixIt?.Invoke();
                _fixAllStack.RemoveAt(0);
            }

            var activeBuildTargetGroup = s_SelectedBuildTargetGroup;
            if (activeBuildTargetGroup == BuildTargetGroup.Unknown)
                activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            var failureCount = _failures.Count;

            OpenXRProjectValidation.GetCurrentValidationIssues(_failures, activeBuildTargetGroup);

            // Repaint the window if the failure count has changed
            if(_failures.Count > 0 || failureCount > 0)
                Repaint();

            _lastUpdate = EditorApplication.timeSinceStartup;
        }

        public void OnGUI()
        {
            InitStyles();

            EditorGUIUtility.SetIconSize(Content.k_IconSize);
            EditorGUILayout.BeginVertical();

            if (EditorApplication.isPlaying && _failures.Count > 0)
            {
                GUILayout.Label(Content.k_PlayMode, Styles.s_InfoBanner);
            }

            EditorGUILayout.Space();

            DrawIssuesList();

            EditorGUILayout.EndVertical();
        }
    }
#endif //PROJECT_VALIDATION_AVAILABLE
    internal class OpenXRProjectValidationBuildStep : IPreprocessBuildWithReport
    {
        [OnOpenAsset(0)]
        static bool ConsoleErrorDoubleClicked(int instanceId, int line)
        {
            var objName = EditorUtility.InstanceIDToObject(instanceId).name;
            if (objName == "OpenXRProjectValidation")
            {
                OpenXRProjectValidationRulesSetup.ShowWindow();
                return true;
            }

            return false;
        }

        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!BuildHelperUtils.HasLoader(report.summary.platformGroup, typeof(OpenXRLoaderBase)))
                return;

            if (OpenXRProjectValidation.LogBuildValidationIssues(report.summary.platformGroup))
                throw new BuildFailedException("OpenXR Build Failed.");
        }
    }
}