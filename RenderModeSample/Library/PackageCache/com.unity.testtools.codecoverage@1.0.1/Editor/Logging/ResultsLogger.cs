using System;
using System.Collections.Generic;
using UnityEditor.TestTools.CodeCoverage.Analytics;
using UnityEngine;

namespace UnityEditor.TestTools.CodeCoverage.Utils
{
    struct ResultData
    {
        // The type of the result (log, warning, error, assert)
        public LogType type;
        // Result message
        public string message;
        
        public ResultData(LogType type, string message)
        {
            this.type = type;
            this.message = message;
        }
    }

    internal enum ResultID
    {
        Log_ResultsSaved = 0,
        Log_ReportSaved = 1,
        Error_FailedReport = 2,
        Error_FailedReportNoTests = 3,
        Error_FailedReportNoAssemblies = 4,
        Assert_NullAssemblyTypes = 5,
        Warning_DebugCodeOptimization = 6,
        Warning_AssemblyFiltersNotPrefixed = 7,
        Warning_PathFiltersNotPrefixed = 8,
        Warning_MultipleResultsPaths = 9,
        Warning_MultipleHistoryPaths = 10,
        Warning_NoCoverageResultsSaved = 11,
        Warning_FailedToDeleteDir = 12,
        Warning_FailedToDeleteFile = 13,
        Warning_FailedReportNullCoverageSettings = 14,
        Warning_BurstCompilationEnabled = 15,
        Warning_ExcludeAttributeAssembly = 16,
        Warning_ExcludeAttributeClass = 17,
        Warning_ExcludeAttributeMethod = 18,
        Warning_StandaloneUnsupported = 19,
    }

    internal static class ResultsLogger
    {
        internal static LogVerbosityLevel VerbosityLevel = LogVerbosityLevel.Info;

        static Dictionary<ResultID, ResultData> s_Results = new Dictionary<ResultID, ResultData>()
        {
            { ResultID.Log_ResultsSaved, new ResultData(LogType.Log, "Code Coverage Results were saved in {0}") },
            { ResultID.Log_ReportSaved, new ResultData(LogType.Log, "Code Coverage Report was generated in {0}") },
            { ResultID.Error_FailedReport, new ResultData(LogType.Error, "Failed to generate Code Coverage Report.") },
            { ResultID.Error_FailedReportNoTests, new ResultData(LogType.Error, "Failed to generate Code Coverage Report. Make sure you have run one or more tests before generating a report.") },
            { ResultID.Error_FailedReportNoAssemblies, new ResultData(LogType.Error, "Failed to generate Code Coverage Report. Make sure you have included at least one assembly before generating a report.") },
            { ResultID.Assert_NullAssemblyTypes, new ResultData(LogType.Assert, "assemblyTypes cannot be null") },
            { ResultID.Warning_DebugCodeOptimization, new ResultData(LogType.Warning, "Code Coverage requires Code Optimization to be set to debug mode in order to obtain accurate coverage information. Switch to debug mode in the Editor (bottom right corner, select the Bug icon > Switch to debug mode), using the CompilationPipeline api by setting 'CompilationPipeline.codeOptimization = CodeOptimization.Debug' or by passing '-debugCodeOptimization' to the command line in batchmode.") },
            { ResultID.Warning_AssemblyFiltersNotPrefixed, new ResultData(LogType.Warning, "-coverageOptions assemblyFilters argument {0} would not be applied as it is not prefixed with +/-.") },
            { ResultID.Warning_PathFiltersNotPrefixed, new ResultData(LogType.Warning, "-coverageOptions pathFilters argument {0} would not be applied as it is not prefixed with +/-.") },
            { ResultID.Warning_MultipleResultsPaths, new ResultData(LogType.Warning, "'-coverageResultsPath' has already been specified on the command-line. Keeping the original setting: '{0}'.") },
            { ResultID.Warning_MultipleHistoryPaths, new ResultData(LogType.Warning, "'-coverageHistoryPath' has already been specified on the command-line. Keeping the original setting: '{0}'.") },
            { ResultID.Warning_NoCoverageResultsSaved, new ResultData(LogType.Warning, "No coverage results were saved.") },
            { ResultID.Warning_FailedToDeleteDir, new ResultData(LogType.Warning, "Failed to delete directory: {0}") },
            { ResultID.Warning_FailedToDeleteFile, new ResultData(LogType.Warning, "Failed to delete file: {0}") },
            { ResultID.Warning_FailedReportNullCoverageSettings, new ResultData(LogType.Warning, "Failed to generate Code Coverage Report. CoverageSettings was not set.") },
            { ResultID.Warning_BurstCompilationEnabled, new ResultData(LogType.Warning, "Code Coverage requires Burst Compilation to be disabled in order to obtain accurate coverage information. To disable Burst Compilation uncheck Jobs > Burst > Enable Compilation or pass '-burst-disable-compilation' to the command line in batchmode.") },
            { ResultID.Warning_ExcludeAttributeAssembly, new ResultData(LogType.Warning, "Not able to detect custom attribute ExcludeFromCoverage in Assembly: {0}") },
            { ResultID.Warning_ExcludeAttributeClass, new ResultData(LogType.Warning, "Not able to detect custom attribute ExcludeFromCoverage in Class: {0}, Assembly: {1}") },
            { ResultID.Warning_ExcludeAttributeMethod, new ResultData(LogType.Warning, "Not able to detect custom attribute ExcludeFromCoverage in Method: {0}, Class: {1}, Assembly: {2}") },
            { ResultID.Warning_StandaloneUnsupported, new ResultData(LogType.Warning, "Code Coverage is not supported in standalone currently. Code Coverage Results and Report will not be generated.") },
        };

        public static bool Log(ResultID resultId, params string[] extraParams)
        {
            if (!s_Results.ContainsKey(resultId))
            {
                Debug.LogWarning($"[{CoverageSettings.PackageName}] ResultsLogger could not find result with id: {resultId}");
                return false;
            }

            ResultData result = s_Results[resultId];

            string message = string.Concat(
                $"[{CoverageSettings.PackageName}] ",
                extraParams != null ? string.Format(result.message, extraParams) : result.message);

            switch (result.type)
            {
                case LogType.Log:
                    if(VerbosityLevel <= LogVerbosityLevel.Info)
                        Debug.Log(message);

                    CoverageAnalytics.instance.AddResult(resultId);
                    break;
                case LogType.Warning:
                    if (VerbosityLevel <= LogVerbosityLevel.Warning)
                        Debug.LogWarning(message);

                    CoverageAnalytics.instance.AddResult(resultId);
                    break;
                case LogType.Error:
                    if (VerbosityLevel <= LogVerbosityLevel.Error)
                        Debug.LogError(message);

                    CoverageAnalytics.instance.AddResult(resultId);
                    CoverageAnalytics.instance.SendCoverageEvent(false);
                    break;
                case LogType.Assert:
                    if (extraParams[0] != null && string.Equals(extraParams[0], "0"))
                    {
                        CoverageAnalytics.instance.AddResult(resultId);
                        CoverageAnalytics.instance.SendCoverageEvent(false);
                        Debug.Assert(false, message);
                    }                      
                    break;
                default:
                    if (VerbosityLevel <= LogVerbosityLevel.Info)
                        Debug.Log(message);

                    CoverageAnalytics.instance.AddResult(resultId);
                    break;
            }

            return true;
        }

        public static bool LogSessionItem(string message, LogVerbosityLevel logLevel = LogVerbosityLevel.Info)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            if (logLevel >= VerbosityLevel)
            {
                message = string.Concat($"[{CoverageSettings.PackageName}] {message}");

                Console.WriteLine(message);
                return true;
            }
  
            return false;
        }
    }
}
