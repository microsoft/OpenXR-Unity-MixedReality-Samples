namespace UnityEditor.TestTools.CodeCoverage
{
    interface ICoverageReporterFilter
    {
        void SetupFiltering();
        bool ShouldProcessAssembly(string assemblyName);
        bool ShouldProcessFile(string filename);
        bool ShouldGenerateAdditionalMetrics();
    }
}
