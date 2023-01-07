using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor
{
    class ProjectValidationDrawer
    {
        class Styles
        {
            internal static readonly Vector2 IconSize = new Vector2(16.0f, 16.0f);
            internal static readonly float DisabledRulePadding = IconSize.x + 5;

            internal static readonly float MessagePadding = Styles.IconSize.x + styles.IconStyle.padding.horizontal;

            internal const float NoButtonPadding = 5;
            internal const float ErrorIconPadding = 3;

            internal const float Space = 15.0f;
            internal const float FixButtonWidth = 80.0f;
            internal const float ShowAllChecksWidth = 96f;
            internal const float IgnoreBuildErrorsWidth = 140f;

            internal const string DisabledRuleTooltip = "This rule is disabled and won't be checked until certain conditions are met.";

            internal readonly GUIStyle Wrap;
            internal readonly GUIContent FixButton;
            internal readonly GUIContent EditButton;
            internal readonly GUIContent HelpButton;
            internal readonly GUIContent PlayMode;

            internal readonly GUIContent IgnoreBuildErrorsContent;

            internal readonly GUIContent WarningIcon;
            internal readonly GUIContent ErrorIcon;
            internal readonly GUIContent TestPassedIcon;

            internal GUIStyle IssuesBackground;
            internal GUIStyle ListLabel;
            internal GUIStyle IssuesTitleLabel;
            internal GUIStyle FixAllStyle;
            internal GUIStyle IconStyle;

            internal Styles()
            {
                FixButton = new GUIContent("Fix");
                EditButton = new GUIContent("Edit");
                HelpButton = new GUIContent(EditorGUIUtility.IconContent("_Help@2x").image);
                PlayMode = new GUIContent("Exit play mode before fixing project validation issues.", EditorGUIUtility.IconContent("console.infoicon").image);

                IgnoreBuildErrorsContent = new GUIContent("Ignore build errors",
                    "Errors from Build Validator Rules will not cause the build to fail.");

                WarningIcon = EditorGUIUtility.IconContent("Warning@2x");
                ErrorIcon = EditorGUIUtility.IconContent("Error@2x");
                TestPassedIcon = EditorGUIUtility.IconContent("TestPassed");

                IssuesBackground = "ScrollViewAlt";

                ListLabel = new GUIStyle("TV Selection")
                {
                    border = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(5, 5, 0, 3),
                    margin = new RectOffset(5, 5, 5, 5)
                };

                IssuesTitleLabel = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(10, 10, 0, 0)
                };

                Wrap = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(0, 5, 1, 1)
                };

                IconStyle = new GUIStyle(EditorStyles.label)
                {
                    margin = new RectOffset(5, 5, 4, 0),
                    fixedWidth = IconSize.x * 2
                };

                FixAllStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    stretchWidth = false,
                    fixedWidth = 80,
                    margin = new RectOffset(0, 10, 2, 2)
                };
            }
        }

        const string k_PrefPrefix = "XRProjectValidation";
        const string k_BuildValidationShowAllPref = k_PrefPrefix + ".BuildValidationShowAll";
        const string k_BuildValidationIgnoreBuildErrorsPref = k_PrefPrefix + ".BuildValidationIgnoreBuildErrors";

        /// <summary>
        /// Interval between issue updates
        /// </summary>
        const double k_UpdateInterval = 1.0d;

        /// <summary>
        /// Interval between issue updates when the window does not have focus
        /// </summary>
        const double k_BackgroundUpdateInterval = 3.0d;

        static Styles s_Styles;
        // Delay creation of Styles till first access
        static Styles styles => s_Styles ?? (s_Styles = new Styles());

        static bool s_BuildValidationShowAll;
        static bool BuildValidationShowAll
        {
            get { return s_BuildValidationShowAll; }
            set
            {
                s_BuildValidationShowAll = value;
                EditorPrefs.SetBool(k_BuildValidationShowAllPref, value);
            }
        }

        static bool s_BuildValidationIgnoreBuildErrors;
        static bool BuildValidationIgnoreBuildErrors
        {
            get { return s_BuildValidationIgnoreBuildErrors; }
            set
            {
                if (s_BuildValidationIgnoreBuildErrors != value)
                {
                    EditorPrefs.SetBool(k_BuildValidationIgnoreBuildErrorsPref, value);
                    s_BuildValidationIgnoreBuildErrors = value;
                }
            }
        }

        /// <summary>
        /// Last time the issues in the window were updated
        /// </summary>
        double m_LastUpdate;

        Vector2 m_ScrollViewPos = Vector2.zero;
        bool m_CheckedInPlayMode;

        List<BuildValidationRule> m_BuildRules = new List<BuildValidationRule>();

        // Fix all state
        Queue<BuildValidationRule> m_FixAllQueue = new Queue<BuildValidationRule>();

        HashSet<BuildValidationRule> m_RuleFailures = new HashSet<BuildValidationRule>();

        BuildTargetGroup m_SelectedBuildTargetGroup;

        bool CheckInPlayMode
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (!m_CheckedInPlayMode)
                    {
                        m_CheckedInPlayMode = true;
                        return true;
                    }

                    return false;
                }

                m_CheckedInPlayMode = false;
                return false;
            }
        }

        internal ProjectValidationDrawer(BuildTargetGroup targetGroup)
        {
            s_BuildValidationShowAll = EditorPrefs.GetBool(k_BuildValidationShowAllPref);
            s_BuildValidationIgnoreBuildErrors = EditorPrefs.GetBool(k_BuildValidationIgnoreBuildErrorsPref);

            m_SelectedBuildTargetGroup = targetGroup;

            BuildValidator.GetCurrentValidationIssues(m_RuleFailures, m_SelectedBuildTargetGroup);
        }

        internal void OnGUI()
        {
            EditorGUIUtility.SetIconSize(Styles.IconSize);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                m_SelectedBuildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();
                if (m_SelectedBuildTargetGroup == BuildTargetGroup.Unknown)
                {
                    m_SelectedBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                }

                if (!BuildValidator.PlatformRules.TryGetValue(m_SelectedBuildTargetGroup, out m_BuildRules))
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField($"'{m_SelectedBuildTargetGroup}' does not have any associated build rules.",
                        styles.IssuesTitleLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndBuildTargetSelectionGrouping();
                    return;
                }

                if (change.changed)
                {
                    BuildValidator.GetCurrentValidationIssues(m_RuleFailures, m_SelectedBuildTargetGroup);
                }
            }

            EditorGUILayout.BeginVertical();

            if (EditorApplication.isPlaying && m_RuleFailures.Count > 0)
            {
                GUILayout.Space(Styles.Space);
                GUILayout.Label(styles.PlayMode);
            }

            EditorGUILayout.Space();

            DrawIssuesList();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndBuildTargetSelectionGrouping();
        }

        void DrawIssuesList()
        {
            var hasFix = m_RuleFailures.Any(f => f.FixIt != null);
            var hasAutoFix = hasFix && m_RuleFailures.Any(f => f.FixIt != null && f.FixItAutomatic);

            using (new EditorGUI.DisabledGroupScope(EditorApplication.isPlaying))
            {
                // Header
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Issues ({m_RuleFailures.Count}) of Checks ({m_BuildRules.Count(rule => rule.IsRuleEnabled())})",
                        styles.IssuesTitleLabel);

                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var showAllChecks = EditorGUILayout.ToggleLeft("Show all",
                            BuildValidationShowAll, GUILayout.Width(Styles.ShowAllChecksWidth));

                        if (change.changed)
                            BuildValidationShowAll = showAllChecks;
                    }

                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var ignoreBuildErrorsCheck = EditorGUILayout.ToggleLeft(styles.IgnoreBuildErrorsContent,
                            BuildValidationIgnoreBuildErrors, GUILayout.Width(Styles.IgnoreBuildErrorsWidth));

                        if (change.changed)
                            BuildValidationIgnoreBuildErrors = ignoreBuildErrorsCheck;
                    }

                    // FixAll button
                    if (hasAutoFix)
                    {
                        using (new EditorGUI.DisabledScope(m_FixAllQueue.Count > 0))
                        {
                            if (GUILayout.Button("Fix All", styles.FixAllStyle, GUILayout.Width(Styles.FixButtonWidth)))
                            {
                                foreach (var ruleFailure in m_RuleFailures)
                                {
                                    if (ruleFailure.FixIt != null && ruleFailure.FixItAutomatic)
                                        m_FixAllQueue.Enqueue(ruleFailure);
                                }
                            }
                        }
                    }
                }

                m_ScrollViewPos = EditorGUILayout.BeginScrollView(m_ScrollViewPos, styles.IssuesBackground,
                    GUILayout.ExpandHeight(true));
                
                m_BuildRules = SortRulesByEnabledCondition(m_BuildRules);
                foreach (var result in m_BuildRules)
                {
                    var rulePassed = !m_RuleFailures.Contains(result);
                    if (BuildValidationShowAll || !rulePassed)
                        DrawIssue(result, rulePassed, hasFix);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        void DrawIssue(BuildValidationRule result, bool rulePassed, bool hasFix)
        {
            bool isRuleEnabled = result.IsRuleEnabled();
            using (new EditorGUI.DisabledScope(!isRuleEnabled))
            {
                EditorGUILayout.BeginHorizontal(styles.ListLabel);
                if (isRuleEnabled)
                {
                    if(!rulePassed && result.Error)
                        GUILayout.Space(Styles.ErrorIconPadding);

                    GUILayout.Label(rulePassed ? styles.TestPassedIcon
                        : result.Error ? styles.ErrorIcon
                        : styles.WarningIcon, styles.IconStyle,
                        GUILayout.Width(Styles.IconSize.x));
                }
                else
                {
                    GUILayout.Space(Styles.DisabledRulePadding);
                }
                
                var message = string.IsNullOrEmpty(result.Category) ? result.Message : $"[{result.Category}] {result.Message}";
                
                GUILayout.Label(new GUIContent(message, isRuleEnabled ? string.Empty : Styles.DisabledRuleTooltip), styles.Wrap);
                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(result.HelpText) || !string.IsNullOrEmpty(result.HelpLink))
                {
                    styles.HelpButton.tooltip = result.HelpText;
                    if (GUILayout.Button(styles.HelpButton, styles.IconStyle, GUILayout.Width(Styles.IconSize.x
                            + styles.IconStyle.padding.horizontal +
                            (result.FixIt != null ? 0 : Styles.NoButtonPadding))))
                    {
                        if (!string.IsNullOrEmpty(result.HelpLink))
                            Application.OpenURL(result.HelpLink);
                    }
                }
                else
                {
                    GUILayout.Space(Styles.MessagePadding);
                }

                using (new EditorGUI.DisabledScope(!m_RuleFailures.Contains(result)))
                {
                    if (result.FixIt != null)
                    {
                        using (new EditorGUI.DisabledScope(m_FixAllQueue.Count != 0))
                        {
                            var button = result.FixItAutomatic ? styles.FixButton : styles.EditButton;
                            button.tooltip = result.FixItMessage;
                            if (GUILayout.Button(button, GUILayout.Width(Styles.FixButtonWidth)))
                            {
                                if (result.FixItAutomatic)
                                    m_FixAllQueue.Enqueue(result);
                                else
                                    result.FixIt();
                            }
                        }
                    }
                    else if (hasFix)
                    {
                        GUILayout.Space(Styles.FixButtonWidth);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        internal bool UpdateIssues(bool focused, bool force)
        {
            if (CheckInPlayMode)
                force = true;
            else if (Application.isPlaying)
                return false;

            var interval = focused ? k_UpdateInterval : k_BackgroundUpdateInterval;
            if (!force && EditorApplication.timeSinceStartup - m_LastUpdate < interval)
                return false;

            if (m_FixAllQueue.Count > 0)
            {
                // Fixit actions can popup dialogs that may cause the action to be called
                // again from `UpdateIssues` if it is not removed before invoking.
                var fixIt = m_FixAllQueue.Dequeue().FixIt;
                fixIt?.Invoke();
            }

            var activeBuildTargetGroup = m_SelectedBuildTargetGroup;
            if (activeBuildTargetGroup == BuildTargetGroup.Unknown)
                activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            if (!BuildValidator.HasRulesForPlatform(activeBuildTargetGroup))
                return false;

            var failureCount = m_BuildRules.Count;

            BuildValidator.GetCurrentValidationIssues(m_RuleFailures, activeBuildTargetGroup);

            // Repaint the window if the failure count has changed
            var needsRepaint = m_BuildRules.Count > 0 || failureCount > 0;

            m_LastUpdate = EditorApplication.timeSinceStartup;
            return needsRepaint;
        }
        
        List<BuildValidationRule> SortRulesByEnabledCondition(List<BuildValidationRule> rulesToSort)
        {
            var sortedRules = new List<BuildValidationRule>();
            foreach(var rule in rulesToSort)
            {
                if(rule.IsRuleEnabled.Invoke())
                    sortedRules.Insert(0, rule);
                else
                    sortedRules.Add(rule);
            }

            return sortedRules;
        }
    }
}
