namespace Unity.XR.CoreUtils.Tests
{
    abstract class PerformanceTest : PerformanceTestBase
    {
        public void Awake()
        {
            SetupData();

            RunTestFrame(); // make sure we JIT the code ahead of time
            m_Timer.Reset();
            m_ElapsedTicks = 0;

            m_TestClassLabel = GetType().Name;
        }

        protected override string GetReport()
        {
            var count = (float) (m_CallCount * m_FrameCounter);
            m_Report = $"{m_TestClassLabel} - {m_CallCount * m_FrameCount} calls\n\n";
            m_Report += $"using {m_MethodLabel}\naverage {m_ElapsedTicks / count} ticks / call\n";
            return m_Report;
        }
    }
}
