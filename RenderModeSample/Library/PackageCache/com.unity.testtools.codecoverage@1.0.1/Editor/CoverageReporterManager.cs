using UnityEngine;
using UnityEditor.TestTools.CodeCoverage.OpenCover;
using UnityEditor.TestTools.CodeCoverage.Analytics;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CoverageReporterManager
    {
        private CoverageSettings m_CoverageSettings = null;
        private ICoverageReporter m_CoverageReporter = null;
        CoverageReportGenerator m_ReportGenerator = null;

        public CoverageReporterManager(CoverageSettings coverageSettings)
        {
            m_CoverageSettings = coverageSettings;
        }

        public ICoverageReporter CoverageReporter
        {
            get
            {
                if (m_CoverageReporter == null)
                {
                    CreateCoverageReporter();
                }
                return m_CoverageReporter;
            }
        }

        public void CreateCoverageReporter()
        {
            m_CoverageReporter = null;

            // Use OpenCover format as currently this is the only one supported
            CoverageFormat coverageFormat = CoverageFormat.OpenCover;

            switch (coverageFormat)
            {
                case CoverageFormat.OpenCover:
                    m_CoverageSettings.resultsFileExtension = "xml";
                    m_CoverageSettings.resultsFolderSuffix = "-opencov";
                    m_CoverageSettings.resultsFileName = CoverageRunData.instance.isRecording ? "RecordingCoverageResults" : "TestCoverageResults";

                    m_CoverageReporter = new OpenCoverReporter();
                    break;
            }

            if (m_CoverageReporter != null)
            {
                m_CoverageReporter.OnInitialise(m_CoverageSettings);
            }
        }

        public void GenerateReport()
        {
            bool autoGenerateReport, generateHTMLReport, generateBadge;

            if (CommandLineManager.instance.batchmode)
            {
                generateHTMLReport = CommandLineManager.instance.generateHTMLReport;
                generateBadge = CommandLineManager.instance.generateBadgeReport;
                autoGenerateReport = generateHTMLReport || generateBadge;
            }
            else
            {
                generateHTMLReport = CoveragePreferences.instance.GetBool("GenerateHTMLReport", true);
                generateBadge = CoveragePreferences.instance.GetBool("GenerateBadge", true);
                autoGenerateReport = CoveragePreferences.instance.GetBool("AutoGenerateReport", true) && (generateHTMLReport || generateBadge);
                bool commandLineAutoGenerateReport = CommandLineManager.instance.runFromCommandLine && (CommandLineManager.instance.generateHTMLReport || CommandLineManager.instance.generateBadgeReport);
                autoGenerateReport = autoGenerateReport || commandLineAutoGenerateReport;
            }

            if (!autoGenerateReport)
            {
                // Clear ProgressBar left from saving results to file,
                // otherwise continue on the same ProgressBar
                EditorUtility.ClearProgressBar();

                // Send Analytics event (Data Only)
                CoverageAnalytics.instance.SendCoverageEvent(true);

                return;
            }

            if (m_CoverageSettings != null)
            {
                CoverageAnalytics.instance.CurrentCoverageEvent.actionID = ActionID.DataReport;
                ReportGenerator.Generate(m_CoverageSettings);
            }
        }

        public CoverageReportGenerator ReportGenerator
        {
            get 
            {
                if (m_ReportGenerator == null)
                    m_ReportGenerator = new CoverageReportGenerator();

                return m_ReportGenerator;
            }         
        }
    }
}
