using UnityEngine;

namespace Unity.XR.CoreUtils.Tests
{
    [AddComponentMenu("")]
    class CustomApproximatePerformanceTest : PerformanceComparisonTest
    {
        Vector2[] m_Cases;

        protected override void SetupData()
        {
            m_MethodLabel = "MathUtility.Approximately()";
            m_MethodBLabel = "Mathf.Approximately()";
            Random.InitState(0);
            m_Cases = TestData.RandomVector2Array(m_CallCount);
        }

        protected override void RunTestFrame()
        {
            foreach (var c in m_Cases)
            {
                m_Timer.Restart();
                MathUtility.Approximately(c.x, c.y);
                m_Timer.Stop();
                m_ElapsedTicks += m_Timer.ElapsedTicks;

                m_TimerB.Restart();
                Mathf.Approximately(c.x, c.y);
                m_TimerB.Stop();
                m_ElapsedTicksB += m_TimerB.ElapsedTicks;
            }
        }
    }
}
