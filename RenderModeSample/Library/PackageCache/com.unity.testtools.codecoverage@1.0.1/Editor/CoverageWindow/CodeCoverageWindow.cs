using System;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.TestTools.CodeCoverage.Utils;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.CodeCoverage.Analytics;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.TestTools.CodeCoverage
{
    [ExcludeFromCoverage]
    internal class CodeCoverageWindow : EditorWindow
    {
        private bool m_EnableCodeCoverage;
        private bool m_HasLatestScriptingRuntime;
        private string m_CodeCoveragePath;
        private string m_CodeCoverageHistoryPath;
        private CoverageFormat m_CodeCoverageFormat;
        private bool m_IncludeHistoryInReport;
        private string m_AssembliesToInclude;
        private int m_AssembliesToIncludeLength;
        private string m_PathsToInclude;
        private string m_PathsToExclude;
        private PathsToAddHandler m_AddPathsToIncludeHandler;
        private PathsToAddHandler m_AddPathsToExcludeHandler;
        private PathsToRemoveDropDownMenu m_RemovePathsToIncludeDropDownMenu;
        private PathsToRemoveDropDownMenu m_RemovePathsToExcludeDropDownMenu;

        private CoverageReportGenerator m_ReportGenerator;
        private bool m_GenerateHTMLReport;
        private bool m_GenerateBadge;
        private bool m_GenerateAdditionalMetrics;
        private bool m_AutoGenerateReport;

        private CoverageSettings m_CoverageSettings;

        private bool m_DoRepaint;
        private bool m_IncludeWarnings;
        private static readonly Vector2 s_WindowMinSizeNormal = new Vector2(445, 385);
        private float m_WarningsAddedAccumulativeHeight = 0;
        private float m_WarningsAddedAccumulativeHeightLast = 0;
        private bool m_LoseFocus = false; 

        private bool m_GenerateReport = false;
        private bool m_StopRecording = false;

        private ReorderableList m_PathsToIncludeReorderableList;
        private ReorderableList m_PathsToExcludeReorderableList;
        private List<string> m_PathsToIncludeList;
        private List<string> m_PathsToExcludeList;

        private Vector2 m_WindowScrollPosition = new Vector2(0, 0);

        private readonly string kLatestScriptingRuntimeMessage = L10n.Tr("Code Coverage requires the latest Scripting Runtime Version (.NET 4.x). You can set this in the Player Settings.");
        private readonly string kCodeCoverageDisabledNoRestartMessage = L10n.Tr("Code Coverage should be enabled in order to generate Coverage data and reports.\nNote that Code Coverage can affect the Editor performance.");
        private readonly string kEnablingCodeCoverageMessage = L10n.Tr("Enabling Code Coverage will not take effect until Unity is restarted.");
        private readonly string kDisablingCodeCoverageMessage = L10n.Tr("Disabling Code Coverage will not take effect until Unity is restarted.");
        private readonly string kCodeOptimizationMessage = L10n.Tr("Code Coverage requires Code Optimization to be set to debug mode in order to obtain accurate coverage information.");
        private readonly string kBurstCompilationOnMessage = L10n.Tr("Code Coverage requires Burst Compilation to be disabled in order to obtain accurate coverage information.");
        private readonly string kSelectCoverageDirectoryMessage = L10n.Tr("Select the Code Coverage directory");
        private readonly string kSelectCoverageHistoryDirectoryMessage = L10n.Tr("Select the Coverage Report history directory");
        private readonly string kClearDataMessage = L10n.Tr("Are you sure you would like to clear the Coverage data from previous test runs or from previous Coverage Recording sessions? Note that you cannot undo this action.");
        private readonly string kClearHistoryMessage = L10n.Tr("Are you sure you would like to clear the coverage report history? Note that you cannot undo this action.");
        private readonly string kNoAssembliesSelectedMessage = L10n.Tr("Make sure you have included at least one assembly.");
        private readonly string kSettingOverriddenMessage = L10n.Tr("{0} is overridden by the {1} command line argument.");
        private readonly string kSettingsOverriddenMessage = L10n.Tr("{0} are overridden by the {1} command line argument.");

        private void Update()
        {
            if (m_GenerateReport)
            {
                // Start the timer for analytics on 'Generate from Last' (report only)
                CoverageAnalytics.instance.StartTimer();
                CoverageAnalytics.instance.CurrentCoverageEvent.actionID = ActionID.ReportOnly;
                CoverageAnalytics.instance.CurrentCoverageEvent.generateFromLast = true;

                m_ReportGenerator.Generate(m_CoverageSettings);
                m_GenerateReport = false;
            }

            if (m_StopRecording)
            {
                CodeCoverage.StopRecordingInternal();
                m_StopRecording = false;
            }
        }

        public void LoseFocus()
        {
            m_LoseFocus = true;
        }

        public string AssembliesToInclude
        {
            set
            {
                m_AssembliesToInclude = value.TrimStart(',').TrimEnd(',');
                m_AssembliesToIncludeLength = m_AssembliesToInclude.Length;
                CoveragePreferences.instance.SetString("IncludeAssemblies", m_AssembliesToInclude);
            }
        }

        public string PathsToInclude
        {
            set
            {
                m_PathsToInclude = value.TrimStart(',').TrimEnd(',');
                m_PathsToInclude = CoverageUtils.NormaliseFolderSeparators(m_PathsToInclude, false);
                CoveragePreferences.instance.SetStringForPaths("PathsToInclude", m_PathsToInclude);

                m_PathsToIncludeList = m_PathsToInclude.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                m_PathsToIncludeReorderableList.list = m_PathsToIncludeList;

                CoverageAnalytics.instance.CurrentCoverageEvent.updateIncludedPaths = true;
            }
        }

        public string PathsToExclude
        {
            set
            {
                m_PathsToExclude = value.TrimStart(',').TrimEnd(',');
                m_PathsToExclude = CoverageUtils.NormaliseFolderSeparators(m_PathsToExclude, false);
                CoveragePreferences.instance.SetStringForPaths("PathsToExclude", m_PathsToExclude);

                m_PathsToExcludeList = new List<string>(m_PathsToExclude.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                m_PathsToExcludeReorderableList.list = m_PathsToExcludeList;

                CoverageAnalytics.instance.CurrentCoverageEvent.updateExcludedPaths = true;
            }
        }

        class Styles
        {
            static bool s_Initialized;

            public static readonly GUIContent SwitchToDebugCodeOptimizationButton = EditorGUIUtility.TrTextContent("Switch to debug mode");
            public static readonly GUIContent SwitchBurstCompilationOffButton = EditorGUIUtility.TrTextContent("Disable Burst Compilation");
            public static readonly GUIContent CodeCoverageResultsLocationLabel = EditorGUIUtility.TrTextContent("Results Location", "Click the Browse button to specify the folder where the coverage results and report will be saved to. The default location is the Project's folder.");
            public static readonly GUIContent CodeCoverageHistoryLocationLabel = EditorGUIUtility.TrTextContent("History Location", "Click the Browse button to specify the folder where the coverage report history will be saved to. The default location is the Project's folder.");
            public static readonly GUIContent CoverageSettingsLabel = EditorGUIUtility.TrTextContent("Settings");
            public static readonly GUIContent EnableCodeCoverageLabel = EditorGUIUtility.TrTextContent("Enable Code Coverage", "Check this to enable Code Coverage. This is required in order to generate Coverage data and reports. Note that Code Coverage can affect the Editor performance.");
            public static readonly GUIContent CodeCoverageFormat = EditorGUIUtility.TrTextContent("Coverage Format", "The Code Coverage format used when saving the results.");
            public static readonly GUIContent GenerateAdditionalMetricsLabel = EditorGUIUtility.TrTextContent("Generate Additional Metrics", "Check this to generate and include additional metrics in the HTML report. These currently include Cyclomatic Complexity and Crap Score calculations for each method.");
            public static readonly GUIContent CoverageHistoryLabel = EditorGUIUtility.TrTextContent("Generate History", "Check this to generate and include the coverage history in the HTML report.");
            public static readonly GUIContent AssembliesToIncludeLabel = EditorGUIUtility.TrTextContent("Included Assemblies", "Specify the assemblies that will be included in the coverage results.\n\nClick the dropdown to view and select or deselect the assemblies.");
            public static GUIContent AssembliesToIncludeDropdownLabel = EditorGUIUtility.TrTextContent("<this will contain a list of the assemblies>", "<this will contain a list of the assemblies>");
            public static readonly GUIContent AssembliesToIncludeEmptyDropdownLabel = EditorGUIUtility.TrTextContent(" Select", "Click this to view and select or deselect the assemblies.");
            public static readonly GUIContent PathsToIncludeLabel = EditorGUIUtility.TrTextContent("Included Paths", "Specify the paths that will be included in the coverage results.  All folders and files are included if the list is empty. Globbing can be used to filter the paths.\n\nClick Add Folder to add folders or Add File to add files.");
            public static readonly GUIContent PathsToExcludeLabel = EditorGUIUtility.TrTextContent("Excluded Paths", "Specify the paths that will be excluded from the coverage results. Globbing can be used to filter the paths.\n\nClick Add Folder to add folders or Add File to add files.");
            public static readonly GUIContent PathsToIncludeAddFolderLabel = EditorGUIUtility.TrTextContent("Add Folder", "Click this to add folders that will be included in the coverage results.");
            public static readonly GUIContent PathsToIncludeAddFileLabel = EditorGUIUtility.TrTextContent("Add File", "Click this to add files that will be included in the coverage results.");
            public static readonly GUIContent PathsToIncludeRemoveLabel = EditorGUIUtility.TrTextContent("Remove", "Click this to remove entries in the list.");
            public static readonly GUIContent PathsToExcludeAddFolderLabel = EditorGUIUtility.TrTextContent("Add Folder", "Click this to add folders that will be excluded from the coverage results.");
            public static readonly GUIContent PathsToExcludeAddFileLabel = EditorGUIUtility.TrTextContent("Add File", "Click this to add files that will be excluded from the coverage results.");
            public static readonly GUIContent PathsToExcludeRemoveLabel = EditorGUIUtility.TrTextContent("Remove", "Click this to remove entries in the list.");
            public static readonly GUIContent BrowseButtonLabel = EditorGUIUtility.TrTextContent("Browse", "Click this to specify the folder where the coverage results and report will be saved to.");
            public static readonly GUIContent GenerateHTMLReportLabel = EditorGUIUtility.TrTextContent("Generate HTML Report", "Check this to generate an HTML version of the report.");
            public static readonly GUIContent GenerateBadgeReportLabel = EditorGUIUtility.TrTextContent("Generate Summary Badges", "Check this to generate coverage summary badges in SVG and PNG format.");
            public static readonly GUIContent AutoGenerateReportLabel = EditorGUIUtility.TrTextContent("Auto Generate Report", "Check this to generate the report automatically after the Test Runner has finished running the tests or the Coverage Recording session has completed.");
            public static readonly GUIContent GenerateReportButtonLabel = EditorGUIUtility.TrTextContent("Generate from Last", "Generates a coverage report from the last set of tests that were run in the Test Runner or from the last Coverage Recording session.");
            public static readonly GUIContent ClearCoverageButtonLabel = EditorGUIUtility.TrTextContent("Clear Data", "Clears the Coverage data from previous test runs or from previous Coverage Recording sessions, for the current project.");
            public static readonly GUIContent ClearHistoryButtonLabel = EditorGUIUtility.TrTextContent("Clear History", "Clears the coverage report history.");
            public static readonly GUIContent StartRecordingButtonLabel = EditorGUIUtility.TrTextContentWithIcon(" Start Recording", "Record coverage data.", "Record Off");
            public static readonly GUIContent StopRecordingButtonLabel = EditorGUIUtility.TrTextContentWithIcon(" Stop Recording", "Stop recording coverage data.", "Record On");

            public static readonly GUIStyle largeButton = "LargeButton";

            public static GUIStyle settings;

            public static void Init()
            {
                if (s_Initialized)
                    return;

                s_Initialized = true;

                settings = new GUIStyle()
                {
                    margin = new RectOffset(8, 4, 18, 4)
                };
            }
        }

        [MenuItem("Window/Analysis/Code Coverage")]
        public static void ShowWindow()
        {
#if TEST_FRAMEWORK_1_1_18_OR_NEWER
            TestRunnerWindow.ShowWindow();
#else
            TestRunnerWindow.ShowPlaymodeTestsRunnerWindowCodeBased();
#endif
            CodeCoverageWindow window = GetWindow<CodeCoverageWindow>(L10n.Tr("Code Coverage"), typeof(TestRunnerWindow));
            window.minSize = s_WindowMinSizeNormal;
            window.Show();
        }

        private void InitCodeCoverageWindow()
        {
            m_CoverageSettings = new CoverageSettings()
            {
                resultsPathFromCommandLine = CommandLineManager.instance.coverageResultsPath,
                historyPathFromCommandLine = CommandLineManager.instance.coverageHistoryPath
            };

            m_CodeCoveragePath = CoveragePreferences.instance.GetStringForPaths("Path", string.Empty);
            m_CodeCoverageHistoryPath = CoveragePreferences.instance.GetStringForPaths("HistoryPath", string.Empty);
            m_CodeCoverageFormat = (CoverageFormat)CoveragePreferences.instance.GetInt("Format", 0);
            m_GenerateAdditionalMetrics = CoveragePreferences.instance.GetBool("GenerateAdditionalMetrics", false);
            m_IncludeHistoryInReport = CoveragePreferences.instance.GetBool("IncludeHistoryInReport", true);
            m_AssembliesToInclude = CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString());
            m_AssembliesToIncludeLength = m_AssembliesToInclude.Length;
            m_PathsToInclude = CoveragePreferences.instance.GetStringForPaths("PathsToInclude", string.Empty);
            m_PathsToExclude = CoveragePreferences.instance.GetStringForPaths("PathsToExclude", string.Empty);
            m_ReportGenerator = new CoverageReportGenerator();
            m_GenerateHTMLReport = CoveragePreferences.instance.GetBool("GenerateHTMLReport", true);
            m_GenerateBadge = CoveragePreferences.instance.GetBool("GenerateBadge", true);
            m_AutoGenerateReport = CoveragePreferences.instance.GetBool("AutoGenerateReport", true);

            m_AddPathsToIncludeHandler = new PathsToAddHandler(this, PathFilterType.Include);
            m_AddPathsToExcludeHandler = new PathsToAddHandler(this, PathFilterType.Exclude);
            m_RemovePathsToIncludeDropDownMenu = new PathsToRemoveDropDownMenu(this, PathFilterType.Include);
            m_RemovePathsToExcludeDropDownMenu = new PathsToRemoveDropDownMenu(this, PathFilterType.Exclude);
            m_PathsToIncludeList = new List<string>(m_PathsToInclude.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            m_PathsToIncludeReorderableList = new ReorderableList(m_PathsToIncludeList, typeof(String), true, false, false, false);
            m_PathsToIncludeReorderableList.headerHeight = 0;
            m_PathsToIncludeReorderableList.footerHeight = 0;
            m_PathsToExcludeList = new List<string>(m_PathsToExclude.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            m_PathsToExcludeReorderableList = new ReorderableList(m_PathsToExcludeList, typeof(String), true, false, false, false);
            m_PathsToExcludeReorderableList.headerHeight = 0;
            m_PathsToExcludeReorderableList.footerHeight = 0;

            UpdateCoverageSettings();
            RefreshCodeCoverageWindow();

            m_GenerateReport = false;
            m_StopRecording = false;
        }

        private void RefreshCodeCoverageWindow()
        {
            UpdateWindowSize();
            Repaint();
        }

        private void UpdateWindowSize()
        {
            minSize = MaxWindowSize;
        }

        private Vector2 MaxWindowSize
        {
            get
            {
                if (m_IncludeWarnings)
                {
                    Vector2 windowMinSizeWithWarnings = s_WindowMinSizeNormal;
                    windowMinSizeWithWarnings.y += m_WarningsAddedAccumulativeHeight;
                    return windowMinSizeWithWarnings;
                }
                else
                {
                    return s_WindowMinSizeNormal;
                }
            }
        }

        private void UpdateCoverageSettings()
        {
            if (m_CoverageSettings != null)
            {
                m_CoverageSettings.rootFolderPath = CoverageUtils.GetRootFolderPath(m_CoverageSettings);
                m_CoverageSettings.historyFolderPath = CoverageUtils.GetHistoryFolderPath(m_CoverageSettings);

                if (m_CodeCoverageFormat == CoverageFormat.OpenCover)
                {
                    m_CoverageSettings.resultsFolderSuffix = "-opencov";
                    string folderName = CoverageUtils.GetProjectFolderName();
                    string resultsRootDirectoryName = string.Concat(folderName, m_CoverageSettings.resultsFolderSuffix);
                    m_CoverageSettings.resultsRootFolderPath = CoverageUtils.NormaliseFolderSeparators(CoverageUtils.JoinPaths(m_CoverageSettings.rootFolderPath, resultsRootDirectoryName));
                }
            }
        }

        private void OnEnable()
        {
            InitCodeCoverageWindow();
        }

        private void OnFocus()
        {
            RefreshCodeCoverageWindow();
        }

        public void OnGUI()
        {
            Styles.Init();

            EditorGUIUtility.labelWidth = 190f;

            GUILayout.BeginVertical();

            // Window scrollbar
            float maxHeight = Mathf.Max(position.height, MaxWindowSize.y);
            m_WindowScrollPosition = GUILayout.BeginScrollView(m_WindowScrollPosition, GUILayout.Height(maxHeight));

            GUILayout.BeginVertical(Styles.settings);

            ResetIncludeWarnings();

            CheckScriptingRuntimeVersion();
            CheckCoverageEnabled();
            CheckCodeOptimization();
            CheckBurstCompilation();

            using (new EditorGUI.DisabledScope(!m_HasLatestScriptingRuntime))
            {
                DrawCodeCoverageLocation();
                DrawCodeCoverageHistoryLocation();

                // Draw Settings
                GUILayout.Space(10);
                EditorGUILayout.LabelField(Styles.CoverageSettingsLabel, EditorStyles.boldLabel);
                using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning))
                {
                    DrawCoverageSettings();
                }

                DrawFooterButtons();
            }

            GUILayout.EndVertical();

            HandleWarningsRepaint();

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            HandleFocusRepaint();
        }

        void HandleFocusRepaint()
        {
            Rect r = EditorGUILayout.GetControlRect();
            // Lose focus if mouse is down outside of UI elements
            if (Event.current.type == EventType.MouseDown && !r.Contains(Event.current.mousePosition))
            {
                m_LoseFocus = true;
            }

            if (m_LoseFocus)
            {
                GUI.FocusControl("");
                m_PathsToIncludeReorderableList.ReleaseKeyboardFocus();
                m_PathsToExcludeReorderableList.ReleaseKeyboardFocus();
                m_LoseFocus = false;
                Repaint();
            }
        }

        void ResetIncludeWarnings()
        {
            m_WarningsAddedAccumulativeHeightLast = m_WarningsAddedAccumulativeHeight;
            m_WarningsAddedAccumulativeHeight = 0;
            m_IncludeWarnings = false;
        }

        void HandleWarningsRepaint()
        {
            if (m_WarningsAddedAccumulativeHeight != m_WarningsAddedAccumulativeHeightLast)
            {
                m_DoRepaint = true;
            }

            if (m_WarningsAddedAccumulativeHeight > 0)
            {
                m_IncludeWarnings = true;
            }

            if (m_DoRepaint)
            {
                RefreshCodeCoverageWindow();
                m_DoRepaint = false;
            }
        }

        void CheckScriptingRuntimeVersion()
        {
#if UNITY_2019_3_OR_NEWER
            m_HasLatestScriptingRuntime = true;
#else
            m_HasLatestScriptingRuntime = PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Latest;
#endif

            if (!m_HasLatestScriptingRuntime)
            {
                EditorGUILayout.HelpBox(kLatestScriptingRuntimeMessage, MessageType.Warning);
                GUILayout.Space(5);

                m_WarningsAddedAccumulativeHeight += 45;
            }
        }

        void CheckCoverageEnabled()
        {
#if UNITY_2020_2_OR_NEWER
            m_EnableCodeCoverage = Coverage.enabled;
#else
            m_EnableCodeCoverage = EditorPrefs.GetBool("CodeCoverageEnabled", false);
#endif
        }

        void CheckCodeOptimization()
        {
#if UNITY_2020_1_OR_NEWER
            if (Compilation.CompilationPipeline.codeOptimization == Compilation.CodeOptimization.Release)
            {
                EditorGUILayout.HelpBox(kCodeOptimizationMessage, MessageType.Warning);

                using (new EditorGUI.DisabledScope(EditorApplication.isCompiling))
                {
                    if (GUILayout.Button(Styles.SwitchToDebugCodeOptimizationButton))
                    {
                        CoverageAnalytics.instance.CurrentCoverageEvent.switchToDebugMode = true;
                        Compilation.CompilationPipeline.codeOptimization = Compilation.CodeOptimization.Debug;
                        EditorPrefs.SetBool("ScriptDebugInfoEnabled", true);
                        HandleWarningsRepaint();
                    }
                }

                GUILayout.Space(5);
                m_WarningsAddedAccumulativeHeight += 65;
            }
#endif
        }

        void CheckBurstCompilation()
        {
#if BURST_INSTALLED
            if (EditorPrefs.GetBool("BurstCompilation", true))
            {
                EditorGUILayout.HelpBox(kBurstCompilationOnMessage, MessageType.Warning);

                if (GUILayout.Button(Styles.SwitchBurstCompilationOffButton))
                {
                    CoverageAnalytics.instance.CurrentCoverageEvent.switchBurstOff = true;
                    EditorApplication.ExecuteMenuItem("Jobs/Burst/Enable Compilation");
                    HandleWarningsRepaint();
                }

                GUILayout.Space(5);
                m_WarningsAddedAccumulativeHeight += 65;
            }
#endif
        }

        void CheckIfIncludedAssembliesIsEmpty()
        {
            if (m_AssembliesToIncludeLength == 0)
            {
                EditorGUILayout.HelpBox(kNoAssembliesSelectedMessage, MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawCodeCoverageLocation()
        {
            GUILayout.BeginHorizontal();

            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine && CommandLineManager.instance.coverageResultsPath.Length > 0;

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning || settingPassedInCmdLine))
            {
                Rect textFieldPosition = EditorGUILayout.GetControlRect();
                textFieldPosition = EditorGUI.PrefixLabel(textFieldPosition, Styles.CodeCoverageResultsLocationLabel);
                EditorGUI.SelectableLabel(textFieldPosition, settingPassedInCmdLine ? CommandLineManager.instance.coverageResultsPath : m_CodeCoveragePath, EditorStyles.textField);

                bool autoDetect = !CoverageUtils.EnsureFolderExists(m_CodeCoveragePath);

                if (autoDetect)
                {
                    SetDefaultCoverageLocation();
                }

                Vector2 buttonSize = EditorStyles.miniButton.CalcSize(Styles.BrowseButtonLabel);
                if (EditorGUILayout.DropdownButton(Styles.BrowseButtonLabel, FocusType.Keyboard, EditorStyles.miniButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    string candidate = CoverageUtils.BrowseForDir(m_CodeCoveragePath, kSelectCoverageDirectoryMessage);
                    if (CoverageUtils.IsValidFolder(candidate))
                    {
                        m_CodeCoveragePath = candidate;
                        CoveragePreferences.instance.SetStringForPaths("Path", m_CodeCoveragePath);

                        UpdateCoverageSettings();
                        m_LoseFocus = true;
                    }
#if UNITY_EDITOR_OSX
                    //After returning from a native dialog on OSX GUILayout gets into a corrupt state, stop rendering UI for this frame.
                    GUIUtility.ExitGUI();
#endif
                }
            }

            GUILayout.EndHorizontal();

            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingOverriddenMessage, "Results Location", "-coverageResultsPath"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawCodeCoverageHistoryLocation()
        {
            GUILayout.BeginHorizontal();

            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine && CommandLineManager.instance.coverageHistoryPath.Length > 0;

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning || settingPassedInCmdLine))
            {
                Rect textFieldPosition = EditorGUILayout.GetControlRect();
                textFieldPosition = EditorGUI.PrefixLabel(textFieldPosition, Styles.CodeCoverageHistoryLocationLabel);
                EditorGUI.SelectableLabel(textFieldPosition, settingPassedInCmdLine ? CommandLineManager.instance.coverageHistoryPath : m_CodeCoverageHistoryPath, EditorStyles.textField);

                bool autoDetect = !CoverageUtils.EnsureFolderExists(m_CodeCoverageHistoryPath);

                if (autoDetect)
                {
                    SetDefaultCoverageHistoryLocation();
                }

                Vector2 buttonSize = EditorStyles.miniButton.CalcSize(Styles.BrowseButtonLabel);
                if (EditorGUILayout.DropdownButton(Styles.BrowseButtonLabel, FocusType.Keyboard, EditorStyles.miniButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    string candidate = CoverageUtils.BrowseForDir(m_CodeCoverageHistoryPath, kSelectCoverageHistoryDirectoryMessage);
                    if (CoverageUtils.IsValidFolder(candidate))
                    {
                        m_CodeCoverageHistoryPath = candidate;
                        CoveragePreferences.instance.SetStringForPaths("HistoryPath", m_CodeCoverageHistoryPath);

                        UpdateCoverageSettings();
                        m_LoseFocus = true;
                    }
#if UNITY_EDITOR_OSX
                    //After returning from a native dialog on OSX GUILayout gets into a corrupt state, stop rendering UI for this frame.
                    GUIUtility.ExitGUI();
#endif
                }
            }

            GUILayout.EndHorizontal();

            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingOverriddenMessage, "History Location", "-coverageHistoryPath"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawIncludedAssemblies()
        {
            GUILayout.BeginHorizontal();

            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine && CommandLineManager.instance.assemblyFiltersSpecified;

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning || settingPassedInCmdLine))
            {
                EditorGUILayout.PrefixLabel(Styles.AssembliesToIncludeLabel);

                Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(10));

                Styles.AssembliesToIncludeDropdownLabel.text = string.Concat(" ", m_AssembliesToInclude);
                Styles.AssembliesToIncludeDropdownLabel.tooltip = m_AssembliesToInclude.Replace(",", "\n");

                if (EditorGUI.DropdownButton(buttonRect, m_AssembliesToInclude.Length > 0 ? Styles.AssembliesToIncludeDropdownLabel : Styles.AssembliesToIncludeEmptyDropdownLabel, FocusType.Keyboard, EditorStyles.miniPullDown))
                {
                    CoverageAnalytics.instance.CurrentCoverageEvent.enterAssembliesDialog = true;
                    GUI.FocusControl("");
                    PopupWindow.Show(buttonRect, new IncludedAssembliesPopupWindow(this, m_AssembliesToInclude) { Width = buttonRect.width });
                }
            }

            GUILayout.EndHorizontal();

            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingOverriddenMessage, "Included Assemblies", "-coverageOptions: assemblyFilters"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawPathFiltering()
        {
            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine && CommandLineManager.instance.pathFiltersSpecified;

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning || settingPassedInCmdLine))
            {
                DrawIncludedPaths();
                DrawExcludedPaths();
            }

            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingsOverriddenMessage, "Included/Excluded Paths", "-coverageOptions: pathFilters"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawIncludedPaths()
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(Styles.PathsToIncludeLabel);

            GUILayout.BeginVertical();

            GUILayout.Space(4);
            EditorGUI.BeginChangeCheck();

            m_PathsToIncludeReorderableList.drawElementCallback = DrawPathsToIncludeListItem;
            m_PathsToIncludeReorderableList.onChangedCallback = (rl) => { PathsToInclude = string.Join(",", rl.list as List<string>); };
            m_PathsToIncludeReorderableList.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                OnPathsListChange(m_PathsToIncludeReorderableList, PathFilterType.Include);
            }

            GUILayout.Space(2);
            GUILayout.EndVertical();

            GUILayout.Space(2);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Vector2 buttonSize = EditorStyles.miniButton.CalcSize(Styles.PathsToIncludeAddFolderLabel);
            Rect btnPosition = GUILayoutUtility.GetRect(Styles.PathsToIncludeAddFolderLabel, EditorStyles.miniButton, GUILayout.MaxWidth(buttonSize.x));

            if (EditorGUI.DropdownButton(btnPosition, Styles.PathsToIncludeAddFolderLabel, FocusType.Keyboard, EditorStyles.miniButton))
            {
                CoverageAnalytics.instance.CurrentCoverageEvent.selectAddFolder_IncludedPaths = true;
                m_AddPathsToIncludeHandler.BrowseForDir(m_PathsToInclude);
            }

            GUILayout.Space(3);

            buttonSize = EditorStyles.miniButton.CalcSize(Styles.PathsToIncludeAddFileLabel);
            btnPosition = GUILayoutUtility.GetRect(Styles.PathsToIncludeAddFileLabel, EditorStyles.miniButton, GUILayout.MaxWidth(buttonSize.x));

            if (EditorGUI.DropdownButton(btnPosition, Styles.PathsToIncludeAddFileLabel, FocusType.Keyboard, EditorStyles.miniButton))
            {
                CoverageAnalytics.instance.CurrentCoverageEvent.selectAddFile_IncludedPaths = true;
                m_AddPathsToIncludeHandler.BrowseForFile(m_PathsToInclude);
            }

            buttonSize = EditorStyles.miniPullDown.CalcSize(Styles.PathsToIncludeRemoveLabel);
            btnPosition = GUILayoutUtility.GetRect(Styles.PathsToIncludeRemoveLabel, EditorStyles.miniPullDown, GUILayout.MaxWidth(buttonSize.x));

            using (new EditorGUI.DisabledScope(m_PathsToIncludeList.Count == 0))
            {
                if (EditorGUI.DropdownButton(btnPosition, Styles.PathsToIncludeRemoveLabel, FocusType.Keyboard, EditorStyles.miniPullDown))
                {
                    RemovePathsToIncludeListItem(btnPosition);
                }
            }

            GUILayout.EndHorizontal();
        }

        void DrawExcludedPaths()
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(Styles.PathsToExcludeLabel);

            GUILayout.BeginVertical();

            GUILayout.Space(4);
            EditorGUI.BeginChangeCheck();

            m_PathsToExcludeReorderableList.drawElementCallback = DrawPathsToExcludeListItem;
            m_PathsToExcludeReorderableList.onChangedCallback = (rl) => { PathsToExclude = string.Join(",", rl.list as List<string>); };
            m_PathsToExcludeReorderableList.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                OnPathsListChange(m_PathsToExcludeReorderableList, PathFilterType.Exclude);
            }

            GUILayout.Space(2);
            GUILayout.EndVertical();

            GUILayout.Space(2);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Vector2 buttonSize = EditorStyles.miniButton.CalcSize(Styles.PathsToExcludeAddFolderLabel);
            Rect btnPosition = GUILayoutUtility.GetRect(Styles.PathsToExcludeAddFolderLabel, EditorStyles.miniButton, GUILayout.MaxWidth(buttonSize.x));

            if (EditorGUI.DropdownButton(btnPosition, Styles.PathsToExcludeAddFolderLabel, FocusType.Keyboard, EditorStyles.miniButton))
            {
                CoverageAnalytics.instance.CurrentCoverageEvent.selectAddFolder_ExcludedPaths = true;
                m_AddPathsToExcludeHandler.BrowseForDir(m_PathsToExclude);
            }

            GUILayout.Space(3);

            buttonSize = EditorStyles.miniButton.CalcSize(Styles.PathsToExcludeAddFileLabel);
            btnPosition = GUILayoutUtility.GetRect(Styles.PathsToExcludeAddFileLabel, EditorStyles.miniButton, GUILayout.MaxWidth(buttonSize.x));

            if (EditorGUI.DropdownButton(btnPosition, Styles.PathsToExcludeAddFileLabel, FocusType.Keyboard, EditorStyles.miniButton))
            {
                CoverageAnalytics.instance.CurrentCoverageEvent.selectAddFile_ExcludedPaths = true;
                m_AddPathsToExcludeHandler.BrowseForFile(m_PathsToExclude);
            }

            buttonSize = EditorStyles.miniPullDown.CalcSize(Styles.PathsToExcludeRemoveLabel);
            btnPosition = GUILayoutUtility.GetRect(Styles.PathsToExcludeRemoveLabel, EditorStyles.miniPullDown, GUILayout.MaxWidth(buttonSize.x));

            using (new EditorGUI.DisabledScope(m_PathsToExcludeList.Count == 0))
            {
                if (EditorGUI.DropdownButton(btnPosition, Styles.PathsToExcludeRemoveLabel, FocusType.Keyboard, EditorStyles.miniPullDown))
                {
                    RemovePathsToExcludeListItem(btnPosition);
                }
            }

            GUILayout.EndHorizontal();
        }

        void DrawPathsToIncludeListItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= 0 && index < m_PathsToIncludeList.Count)
            {
                string pathToInclude = m_PathsToIncludeList[index].Replace(",", "");
                m_PathsToIncludeReorderableList.list[index] = EditorGUI.TextField(rect, pathToInclude);
            }
        }

        void RemovePathsToIncludeListItem(Rect rect)
        {
            m_RemovePathsToIncludeDropDownMenu.Show(rect, m_PathsToIncludeReorderableList, m_PathsToIncludeList);
        }

        void DrawPathsToExcludeListItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= 0 && index < m_PathsToExcludeList.Count)
            {
                string pathToExclude = m_PathsToExcludeList[index];
                m_PathsToExcludeReorderableList.list[index] = EditorGUI.TextField(rect, pathToExclude);
            }
        }

        void OnPathsListChange(ReorderableList rl, PathFilterType pathFilterType)
        {
            var pathsList = rl.list as List<string>;
            int listSize = pathsList.Count;

            for (int i = 0; i < listSize; ++i)
            {
                var itemStr = pathsList[i];
                itemStr = itemStr.Replace(",", "");

                if (string.IsNullOrWhiteSpace(itemStr))
                {
                    itemStr = "-";
                }

                pathsList[i] = itemStr;
            }

            if (pathFilterType == PathFilterType.Include)
                PathsToInclude = string.Join(",", pathsList);
            else if (pathFilterType == PathFilterType.Exclude)
                PathsToExclude = string.Join(",", pathsList);
        }

        void RemovePathsToExcludeListItem(Rect rect)
        {
            m_RemovePathsToExcludeDropDownMenu.Show(rect, m_PathsToExcludeReorderableList, m_PathsToExcludeList);
        }

        void CheckIfCodeCoverageIsDisabled()
        {
            if (!m_EnableCodeCoverage)
            {
                EditorGUILayout.HelpBox(kCodeCoverageDisabledNoRestartMessage, MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
#if !UNITY_2020_2_OR_NEWER
            if (m_EnableCodeCoverage != Coverage.enabled)
            {
                if (m_EnableCodeCoverage)
                    EditorGUILayout.HelpBox(kEnablingCodeCoverageMessage, MessageType.Warning);
                else
                    EditorGUILayout.HelpBox(kDisablingCodeCoverageMessage, MessageType.Warning);

                m_WarningsAddedAccumulativeHeight += 40;
            }
#endif
        }

        void DrawEnableCoverageCheckbox()
        {
            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine;

            using (new EditorGUI.DisabledScope(EditorApplication.isCompiling || settingPassedInCmdLine))
            {
                EditorGUI.BeginChangeCheck();
                m_EnableCodeCoverage = EditorGUILayout.Toggle(Styles.EnableCodeCoverageLabel, m_EnableCodeCoverage, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    CoveragePreferences.instance.SetBool("EnableCodeCoverage", m_EnableCodeCoverage);
#if UNITY_2020_2_OR_NEWER
                    Coverage.enabled = m_EnableCodeCoverage;
#else
                    EditorPrefs.SetBool("CodeCoverageEnabled", m_EnableCodeCoverage);
                    EditorPrefs.SetBool("CodeCoverageEnabledMessageShown", false);
                    SettingsService.NotifySettingsProviderChanged();
#endif
                }
            }
            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingOverriddenMessage, "Enable Code Coverage", "-enableCodeCoverage"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
            else
            {
                CheckIfCodeCoverageIsDisabled();
            }
        }

        void DrawGenerateHTMLReportCheckbox()
        {
            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine && CommandLineManager.instance.generateHTMLReport;

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning || settingPassedInCmdLine))
            {
                EditorGUI.BeginChangeCheck();
                m_GenerateHTMLReport = EditorGUILayout.Toggle(Styles.GenerateHTMLReportLabel, m_GenerateHTMLReport, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    CoveragePreferences.instance.SetBool("GenerateHTMLReport", m_GenerateHTMLReport);
                }
            }

            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingOverriddenMessage, "Generate HTML Report", "-coverageOptions: generateHtmlReport"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawGenerateBadgeReportCheckbox()
        {
            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine && CommandLineManager.instance.generateBadgeReport;

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning || settingPassedInCmdLine))
            {
                EditorGUI.BeginChangeCheck();
                m_GenerateBadge = EditorGUILayout.Toggle(Styles.GenerateBadgeReportLabel, m_GenerateBadge, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    CoveragePreferences.instance.SetBool("GenerateBadge", m_GenerateBadge);
                }
            }

            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingOverriddenMessage, "Generate Summary Badges", "-coverageOptions: generateBadgeReport"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawGenerateHistoryCheckbox()
        {
            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine && CommandLineManager.instance.generateHTMLReportHistory;

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning || settingPassedInCmdLine))
            {
                EditorGUI.BeginChangeCheck();
                using (new EditorGUI.DisabledScope(!m_GenerateHTMLReport && !m_GenerateBadge))
                {
                    m_IncludeHistoryInReport = EditorGUILayout.Toggle(Styles.CoverageHistoryLabel, m_IncludeHistoryInReport, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        CoveragePreferences.instance.SetBool("IncludeHistoryInReport", m_IncludeHistoryInReport);
                    }
                }
            }

            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingOverriddenMessage, "Generate History", "-coverageOptions: generateHtmlReportHistory"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawGenerateAdditionalMetricsCheckbox()
        {
            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine && CommandLineManager.instance.generateAdditionalMetrics;

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning || settingPassedInCmdLine))
            {
                EditorGUI.BeginChangeCheck();
                m_GenerateAdditionalMetrics = EditorGUILayout.Toggle(Styles.GenerateAdditionalMetricsLabel, m_GenerateAdditionalMetrics, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    CoveragePreferences.instance.SetBool("GenerateAdditionalMetrics", m_GenerateAdditionalMetrics);
                }
            }

            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingOverriddenMessage, "Generate Additional Metrics", "-coverageOptions: generateAdditionalMetrics"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawAutoGenerateReportCheckbox()
        {
            bool settingPassedInCmdLine = CommandLineManager.instance.runFromCommandLine && (CommandLineManager.instance.generateHTMLReport || CommandLineManager.instance.generateBadgeReport);

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning || settingPassedInCmdLine))
            {
                EditorGUI.BeginChangeCheck();
                m_AutoGenerateReport = EditorGUILayout.Toggle(Styles.AutoGenerateReportLabel, m_AutoGenerateReport, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    CoveragePreferences.instance.SetBool("AutoGenerateReport", m_AutoGenerateReport);
                }
            }

            if (settingPassedInCmdLine)
            {
                EditorGUILayout.HelpBox(string.Format(kSettingOverriddenMessage, "Auto Generate Report", CommandLineManager.instance.generateHTMLReport ? "-coverageOptions: generateHtmlReport" : "-coverageOptions: generateBadgeReport"), MessageType.Warning);
                m_WarningsAddedAccumulativeHeight += 40;
            }
        }

        void DrawCoverageSettings()
        {
            DrawEnableCoverageCheckbox();
            DrawIncludedAssemblies();
            CheckIfIncludedAssembliesIsEmpty();
            DrawPathFiltering();
            DrawGenerateHTMLReportCheckbox();
            DrawGenerateBadgeReportCheckbox();
            DrawGenerateHistoryCheckbox();
            DrawGenerateAdditionalMetricsCheckbox();
            DrawAutoGenerateReportCheckbox();
        }

        void DrawFooterButtons()
        {
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();

            Vector2 buttonSize = EditorStyles.miniButton.CalcSize(Styles.ClearCoverageButtonLabel);
            using (new EditorGUI.DisabledScope(!DoesResultsRootFolderExist() || CoverageRunData.instance.isRunning))
            {
                if (EditorGUILayout.DropdownButton(Styles.ClearCoverageButtonLabel, FocusType.Keyboard, Styles.largeButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    ClearResultsRootFolderIfExists();
                }
            }

            buttonSize = EditorStyles.miniButton.CalcSize(Styles.ClearHistoryButtonLabel);

            using (new EditorGUI.DisabledScope(!DoesReportHistoryExist() || CoverageRunData.instance.isRunning))
            {
                if (EditorGUILayout.DropdownButton(Styles.ClearHistoryButtonLabel, FocusType.Keyboard, Styles.largeButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    ClearReportHistoryFolderIfExists();
                }
            }

            GUILayout.FlexibleSpace();

            buttonSize = EditorStyles.miniButton.CalcSize(Styles.GenerateReportButtonLabel);
            using (new EditorGUI.DisabledScope((!m_GenerateHTMLReport && !m_GenerateBadge) || !DoesResultsRootFolderExist() || CoverageRunData.instance.isRunning || m_GenerateReport || m_AssembliesToIncludeLength == 0 || !Coverage.enabled || m_EnableCodeCoverage != Coverage.enabled || EditorApplication.isCompiling))
            {
                if (EditorGUILayout.DropdownButton(Styles.GenerateReportButtonLabel, FocusType.Keyboard, Styles.largeButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    m_GenerateReport = true;
                }
            }

            // Coverage Recording
            bool isRunning = CoverageRunData.instance.isRunning;
            bool isRecording = CoverageRunData.instance.isRecording;

            using (new EditorGUI.DisabledScope((isRunning && !isRecording) || m_StopRecording || m_AssembliesToIncludeLength == 0 || !Coverage.enabled || m_EnableCodeCoverage != Coverage.enabled || EditorApplication.isCompiling))
            {
                buttonSize = EditorStyles.miniButton.CalcSize(Styles.StartRecordingButtonLabel);
                if (EditorGUILayout.DropdownButton(isRecording ? Styles.StopRecordingButtonLabel : Styles.StartRecordingButtonLabel, FocusType.Keyboard, Styles.largeButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    if (isRecording)
                    {
                        m_StopRecording = true;
                    }
                    else
                    {
                        CodeCoverage.StartRecordingInternal();
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private bool DoesResultsRootFolderExist()
        {
            if (m_CoverageSettings == null)
                return false;

            string resultsRootFolderPath = m_CoverageSettings.resultsRootFolderPath;
            return CoverageUtils.DoesFolderExistAndNotEmpty(resultsRootFolderPath);
        }

        private void ClearResultsRootFolderIfExists()
        {
            CoverageAnalytics.instance.CurrentCoverageEvent.clearData = true;

            if (!EditorUtility.DisplayDialog(L10n.Tr("Clear Data"), kClearDataMessage, L10n.Tr("Clear"), L10n.Tr("Cancel")))
                return;

            if (m_CoverageSettings == null)
                return;

            string resultsRootFolderPath = m_CoverageSettings.resultsRootFolderPath;

            CoverageUtils.ClearFolderIfExists(resultsRootFolderPath, "*.xml");
        }

        private bool DoesReportHistoryExist()
        {
            if (m_CoverageSettings == null)
                return false;

            string historyFolderPath = m_CoverageSettings.historyFolderPath;

            return CoverageUtils.DoesFolderExistAndNotEmpty(historyFolderPath) && 
                   CoverageUtils.GetNumberOfFilesInFolder(historyFolderPath, "????-??-??_??-??-??_CoverageHistory.xml", SearchOption.TopDirectoryOnly) > 0;
        }

        private void ClearReportHistoryFolderIfExists()
        {
            CoverageAnalytics.instance.CurrentCoverageEvent.clearHistory = true;

            if (!EditorUtility.DisplayDialog(L10n.Tr("Clear History"), kClearHistoryMessage, L10n.Tr("Clear"), L10n.Tr("Cancel")))
                return;

            if (m_CoverageSettings == null)
                return;

            string historyFolderPath = m_CoverageSettings.historyFolderPath;

            CoverageUtils.ClearFolderIfExists(historyFolderPath, "????-??-??_??-??-??_CoverageHistory.xml");
        }

        void SetDefaultCoverageLocation()
        {
            string projectPath = CoverageUtils.GetProjectPath();
            if (CoverageUtils.IsValidFolder(projectPath))
            {
                m_CodeCoveragePath = projectPath;
                CoveragePreferences.instance.SetStringForPaths("Path", m_CodeCoveragePath);
                UpdateCoverageSettings();
            }
        }

        void SetDefaultCoverageHistoryLocation()
        {
            string projectPath = CoverageUtils.GetProjectPath();
            if (CoverageUtils.IsValidFolder(projectPath))
            {
                m_CodeCoverageHistoryPath = projectPath;
                CoveragePreferences.instance.SetStringForPaths("HistoryPath", m_CodeCoverageHistoryPath);
                UpdateCoverageSettings();
            }
        }
    }
}
