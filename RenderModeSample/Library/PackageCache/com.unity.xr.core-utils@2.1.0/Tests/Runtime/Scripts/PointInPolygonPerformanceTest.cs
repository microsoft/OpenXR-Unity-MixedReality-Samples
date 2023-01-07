using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils.Tests
{
    [AddComponentMenu("")]
    class PointInPolygonPerformanceTest : PerformanceTest
    {
        static readonly List<Vector3> k_TestHexagon = new List<Vector3>
        {
            new Vector3(4f, 0f, 4f), new Vector3(3f, 0f, 4f), new Vector3(2f, 0f, 5f),
            new Vector3(3f, 0f, 6f), new Vector3(4f, 0f, 6f), new Vector3(5f, 0f, 5f)
        };

        Vector3[] m_TestPoints;

        protected override void SetupData()
        {
            Random.InitState(2000);
            m_TestPoints = TestData.RandomXZVector3Array(m_CallCount);
            m_MethodLabel = "GeometryUtils.PointInPolygon(p, vertices)";
        }

        protected override void RunTestFrame()
        {
            foreach (var p in m_TestPoints)
            {
                m_Timer.Restart();
                GeometryUtils.PointInPolygon(p, k_TestHexagon);
                m_Timer.Stop();
                m_ElapsedTicks += m_Timer.ElapsedTicks;
            }
        }
    }
}
