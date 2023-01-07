using System.Diagnostics;

namespace Unity.XR.CoreUtils.Tests
{
    abstract class PerformanceComparisonTest : PerformanceTestBase
    {
        protected readonly Stopwatch m_TimerB = new Stopwatch();
        protected long m_ElapsedTicksB;

        protected string m_MethodBLabel;

        public void Awake()
        {
            SetupData();

            RunTestFrame(); // make sure we JIT the code ahead of time
            m_Timer.Reset();
            m_TimerB.Reset();
            m_ElapsedTicks = 0;
            m_ElapsedTicksB = 0;

            m_TestClassLabel = GetType().Name;
        }

        protected override string GetReport()
        {
            var count = (float) (m_CallCount * m_FrameCounter);
            m_Report = $"{m_TestClassLabel} - {m_CallCount * m_FrameCount} calls\n\n";

            var ratio = m_ElapsedTicks / (float) m_ElapsedTicksB;
            var ratioMsg = ratio.ToString("F5");
            m_Report += $"{ratioMsg} : 1 ratio for execution time\n\n";
            m_Report += $"using {m_MethodLabel}\naverage {m_ElapsedTicks / count} ticks / call\n\n";
            m_Report += $"using {m_MethodBLabel}\naverage {m_ElapsedTicksB / count} ticks / call";
            return m_Report;
        }
    }
}
