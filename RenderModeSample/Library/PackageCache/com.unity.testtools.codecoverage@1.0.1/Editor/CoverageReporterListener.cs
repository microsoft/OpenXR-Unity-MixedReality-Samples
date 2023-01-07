using UnityEditor.TestTools.CodeCoverage.Analytics;
using UnityEditor.TestTools.CodeCoverage.Utils;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

#if NO_COV_EDITORPREF
using System.Linq;
using UnityEditor.PackageManager;
#endif

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CoverageReporterListener : ScriptableObject, ICallbacks
    {
        private CoverageReporterManager m_CoverageReporterManager;
        private bool m_IsConnectedToPlayer;

        public CoverageReporterManager CoverageReporterManager
        {
            set
            {
                m_CoverageReporterManager = value;
            }
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            if (!Coverage.enabled)
                return;

            m_IsConnectedToPlayer = CoverageUtils.IsConnectedToPlayer;

            if (m_IsConnectedToPlayer)
            {
                ResultsLogger.Log(ResultID.Warning_StandaloneUnsupported);
                return;
            }

            if (CoverageRunData.instance.isRunning || EditorApplication.isCompiling)
                return;

            CoverageRunData.instance.Start();
            m_CoverageReporterManager.CreateCoverageReporter();
            ICoverageReporter coverageReporter = m_CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnRunStarted(testsToRun);
        }

        public void RunFinished(ITestResultAdaptor testResults)
        {
            if (!Coverage.enabled)
                return;

            if (CoverageRunData.instance.isRecording || m_IsConnectedToPlayer)
                return;

            CoverageRunData.instance.Stop();

            if (!CoverageRunData.instance.DidTestsRun())
                return;

            ICoverageReporter coverageReporter = m_CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnRunFinished(testResults);

            m_CoverageReporterManager.GenerateReport();
        }

        public void TestStarted(ITestAdaptor test)
        {
            if (!Coverage.enabled)
                return;

            if (CoverageRunData.instance.HasLastIgnoredSuiteID() || m_IsConnectedToPlayer)
                return;

            if (test.RunState == RunState.Ignored)
            {
                if (test.IsSuite)
                    CoverageRunData.instance.SetLastIgnoredSuiteID(test.Id);

                return;
            }

            if (test.IsSuite)
                return;

            CoverageRunData.instance.IncrementTestRunCount();
            ICoverageReporter coverageReporter = m_CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnTestStarted(test);
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (!Coverage.enabled)
                return;

            if (m_IsConnectedToPlayer)
                return;

            if (result.Test.RunState == RunState.Ignored)
            {
                if (result.Test.IsSuite && string.Equals(CoverageRunData.instance.GetLastIgnoredSuiteID(), result.Test.Id))
                    CoverageRunData.instance.SetLastIgnoredSuiteID(string.Empty);

                return;
            }

            if (CoverageRunData.instance.HasLastIgnoredSuiteID())
                return;

            if (result.Test.IsSuite)
                return;

            ICoverageReporter coverageReporter = m_CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnTestFinished(result);
        }
    }

    [InitializeOnLoad]
    internal class CoverageReporterStarter
    {
        public static CoverageReporterManager CoverageReporterManager;

        static CoverageReporterStarter()
        {
#if NO_COV_EDITORPREF
            if (!CommandLineManager.instance.runFromCommandLine)
            {
                bool localCoverageEnabled = CoveragePreferences.instance.GetBool("EnableCodeCoverage", false);
                if (localCoverageEnabled != Coverage.enabled)
                    Coverage.enabled = localCoverageEnabled;

                PackageManager.Events.registeringPackages += OnRegisteringPackages;
            } 
#endif
            if (!Coverage.enabled)
                return;

#if CONDITIONAL_IGNORE_SUPPORTED
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping("IgnoreForCoverage", true);
#endif
            CoverageReporterListener listener = ScriptableObject.CreateInstance<CoverageReporterListener>();

#if TEST_FRAMEWORK_1_2_OR_NEWER
            TestRunnerApi.RegisterTestCallback(listener);
#else
            TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(listener);
#endif

            CoverageSettings coverageSettings = new CoverageSettings()
            {
                resultsPathFromCommandLine = CommandLineManager.instance.coverageResultsPath,
                historyPathFromCommandLine = CommandLineManager.instance.coverageHistoryPath
            };

            CoverageReporterManager = new CoverageReporterManager(coverageSettings);

            listener.CoverageReporterManager = CoverageReporterManager;

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            // Generate a report if running from the command line,
            // generateHTMLReport or generateBadgeReport is passed to -coverageOptions
            // and -runTests has not been passed to the command line
            if (CommandLineManager.instance.runFromCommandLine &&
                (CommandLineManager.instance.generateHTMLReport || CommandLineManager.instance.generateBadgeReport) &&
                !CommandLineManager.instance.runTests)
            {
                if (CoverageRunData.instance.reportWasGenerated)
                    return;

                // Start the timer for analytics for Report only
                CoverageAnalytics.instance.StartTimer();
                CoverageAnalytics.instance.CurrentCoverageEvent.actionID = ActionID.ReportOnly;

                coverageSettings.rootFolderPath = CoverageUtils.GetRootFolderPath(coverageSettings);
                coverageSettings.historyFolderPath = CoverageUtils.GetHistoryFolderPath(coverageSettings);

                CoverageReporterManager.ReportGenerator.Generate(coverageSettings);
            }
        }

#if NO_COV_EDITORPREF
        static void OnRegisteringPackages(PackageRegistrationEventArgs args)
        {
            if (args.removed.Any(info => info.name == "com.unity.testtools.codecoverage"))
            {
                Coverage.enabled = false;
            }
        }
#endif

        static void OnBeforeAssemblyReload()
        {
            if (!CoverageRunData.instance.isRunning)
                return;

            if (!CoverageRunData.instance.DidTestsRun())
                return;

            if (CoverageRunData.instance.isRecording && CoverageRunData.instance.isRecordingPaused)
                return;

            ICoverageReporter coverageReporter = CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnBeforeAssemblyReload();
        }

        static void OnAfterAssemblyReload()
        {
            if (!CoverageRunData.instance.isRunning)
                return;

            CoverageReporterManager.CreateCoverageReporter();
        }
    }
}
