// #define COVERAGE_ANALYTICS_LOGGING

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.TestTools.CodeCoverage.Utils;
using UnityEngine;
using UnityEngine.Analytics;

namespace UnityEditor.TestTools.CodeCoverage.Analytics
{
    [Serializable]
    internal class Timer
    {
        public void Start()
        {
            startTime = DateTime.Now;
        }

        public long elapsedTimeMs => (long)(DateTime.Now - startTime).TotalMilliseconds;

        [SerializeField]
        private DateTime startTime = DateTime.Now;
    }

    [Serializable]
    internal class CoverageAnalytics : ScriptableSingleton<CoverageAnalytics>
    {
        [SerializeField]
        private bool s_Registered;
        [SerializeField]
        private List<int> s_ResultsIdsList;

        public CoverageAnalyticsEvent CurrentCoverageEvent;
        public Timer CoverageTimer;

        protected CoverageAnalytics() : base()
        {
            ResetEvents();
        }

        public void ResetEvents()
        {
            CurrentCoverageEvent = new CoverageAnalyticsEvent();
            CoverageTimer = new Timer();
            s_ResultsIdsList = new List<int>();

            CurrentCoverageEvent.actionID = ActionID.Other;
            CurrentCoverageEvent.coverageModeId = CoverageModeID.None;
            CurrentCoverageEvent.numOfIncludedPaths = 0;
            CurrentCoverageEvent.numOfExcludedPaths = 0;
        }

        public void StartTimer()
        {
            CoverageTimer.Start();
        }

        // See ResultsLogger.cs for details
        public void AddResult(ResultID resultId)
        {
            s_ResultsIdsList.Add((int)resultId);
        }

        public void SendCoverageEvent(bool success)
        {
            CurrentCoverageEvent.success = success;
            CurrentCoverageEvent.duration = CoverageTimer.elapsedTimeMs;
            CurrentCoverageEvent.resultIds = s_ResultsIdsList.ToArray();

            bool runFromCommandLine = CommandLineManager.instance.runFromCommandLine;
            bool batchmode = CommandLineManager.instance.batchmode;

            CurrentCoverageEvent.runFromCommandLine = runFromCommandLine;
            CurrentCoverageEvent.batchmode = batchmode;

            CurrentCoverageEvent.autogenerate = batchmode ?
                CommandLineManager.instance.generateBadgeReport || CommandLineManager.instance.generateHTMLReport :
                CommandLineManager.instance.generateBadgeReport || CommandLineManager.instance.generateHTMLReport || CoveragePreferences.instance.GetBool("AutoGenerateReport", true);

            CurrentCoverageEvent.createBadges = batchmode ?
                CommandLineManager.instance.generateBadgeReport :
                CommandLineManager.instance.generateBadgeReport || CoveragePreferences.instance.GetBool("GenerateBadge", true);

            CurrentCoverageEvent.generateHistory = batchmode ?
                CommandLineManager.instance.generateHTMLReportHistory :
                CommandLineManager.instance.generateHTMLReportHistory || CoveragePreferences.instance.GetBool("IncludeHistoryInReport", true);

            CurrentCoverageEvent.generateHTMLReport = batchmode ?
                CommandLineManager.instance.generateHTMLReport :
                CommandLineManager.instance.generateHTMLReport || CoveragePreferences.instance.GetBool("GenerateHTMLReport", true);

            CurrentCoverageEvent.generateMetrics = batchmode ?
                CommandLineManager.instance.generateAdditionalMetrics :
                CommandLineManager.instance.generateAdditionalMetrics || CoveragePreferences.instance.GetBool("GenerateAdditionalMetrics", false);

            if (batchmode)
                CurrentCoverageEvent.useDefaultAssemblyFilters = !CommandLineManager.instance.assemblyFiltersSpecified;
            else
                CurrentCoverageEvent.useDefaultAssemblyFilters = CommandLineManager.instance.assemblyFiltersSpecified ? false :
                    string.Equals(CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString()), AssemblyFiltering.GetUserOnlyAssembliesString(), StringComparison.InvariantCultureIgnoreCase);

            if (batchmode)
                CurrentCoverageEvent.useDefaultPathFilters = !CommandLineManager.instance.pathFiltersSpecified;
            else
                CurrentCoverageEvent.useDefaultPathFilters = CommandLineManager.instance.pathFiltersSpecified ? false :
                    string.Equals(CoveragePreferences.instance.GetString("PathsToInclude", string.Empty), string.Empty) && string.Equals(CoveragePreferences.instance.GetString("PathsToExclude", string.Empty), string.Empty);

            if (batchmode)
                CurrentCoverageEvent.useDefaultResultsLoc = CommandLineManager.instance.coverageResultsPath.Length == 0;
            else
                CurrentCoverageEvent.useDefaultResultsLoc = CommandLineManager.instance.coverageResultsPath.Length > 0  ? false :
                    string.Equals(CoveragePreferences.instance.GetStringForPaths("Path", string.Empty), CoverageUtils.GetProjectPath(), StringComparison.InvariantCultureIgnoreCase);

            if (batchmode)
                CurrentCoverageEvent.useDefaultHistoryLoc = CommandLineManager.instance.coverageHistoryPath.Length == 0;
            else
                CurrentCoverageEvent.useDefaultHistoryLoc = CommandLineManager.instance.coverageHistoryPath.Length > 0 ? false :
                    string.Equals(CoveragePreferences.instance.GetStringForPaths("HistoryPath", string.Empty), CoverageUtils.GetProjectPath(), StringComparison.InvariantCultureIgnoreCase);

#if UNITY_2020_1_OR_NEWER
            CurrentCoverageEvent.inDebugMode = Compilation.CompilationPipeline.codeOptimization == Compilation.CodeOptimization.Debug;
#else
            CurrentCoverageEvent.inDebugMode = true;
#endif
            if (!runFromCommandLine || (runFromCommandLine && !batchmode && !CommandLineManager.instance.assemblyFiltersSpecified))
            {
                if (CurrentCoverageEvent.actionID == ActionID.ReportOnly)
                {
                    string includeAssemblies = CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString());
                    string[] includeAssembliesArray = includeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    CurrentCoverageEvent.includedAssemblies = includeAssembliesArray;
                }
            }

            Send(EventName.codeCoverage, CurrentCoverageEvent);

            ResetEvents();
        }

        public bool RegisterEvents()
        {
            if (!EditorAnalytics.enabled)
            {
                ResultsLogger.LogSessionItem("Editor analytics are disabled", LogVerbosityLevel.Info);
                return false;
            }

            if (s_Registered)
            {
                return true;
            }

            var allNames = Enum.GetNames(typeof(EventName));
            if (allNames.Any(eventName => !RegisterEvent(eventName)))
            {
                return false;
            }

            s_Registered = true;
            return true;
        }

        private bool RegisterEvent(string eventName)
        {
            const string vendorKey = "unity.testtools.codecoverage";
            var result = EditorAnalytics.RegisterEventWithLimit(eventName, 100, 1000, vendorKey);
            switch (result)
            {
                case AnalyticsResult.Ok:
                    {
#if COVERAGE_ANALYTICS_LOGGING
                        ResultsLogger.LogSessionItem($"Registered analytics event: {eventName}", LogVerbosityLevel.Info);
#endif
                        return true;
                    }
                case AnalyticsResult.TooManyRequests:
                    // this is fine - event registration survives domain reload (native)
                    return true;
                default:
                    {
                        ResultsLogger.LogSessionItem($"Failed to register analytics event {eventName}. Result: {result}", LogVerbosityLevel.Error);
                        return false;
                    }
            }
        }

        private void Send(EventName eventName, object eventData)
        {
            if (!RegisterEvents())
            {
#if COVERAGE_ANALYTICS_LOGGING
                Console.WriteLine($"[{CoverageSettings.PackageName}] Analytics disabled: event='{eventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(eventData, true)}");
#endif
                return;
            }
            try
            {
                var result = EditorAnalytics.SendEventWithLimit(eventName.ToString(), eventData);
                if (result == AnalyticsResult.Ok)
                {
#if COVERAGE_ANALYTICS_LOGGING
                    ResultsLogger.LogSessionItem($"Event={eventName}, time={DateTime.Now:HH:mm:ss}, payload={EditorJsonUtility.ToJson(eventData, true)}", LogVerbosityLevel.Info);
#endif
                }
                else
                {
                    ResultsLogger.LogSessionItem($"Failed to send analytics event {eventName}. Result: {result}", LogVerbosityLevel.Error);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
