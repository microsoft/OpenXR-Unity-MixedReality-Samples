using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils.Tests
{
    [AddComponentMenu("")]
    class ConvexHullPerformanceTest : PerformanceTest
    {
        const int k_ExampleCount = 3;
        const int k_PointsPerExample = 128;

        List<List<Vector3>> m_Cases = new List<List<Vector3>>();

        List<Vector3> m_Hull = new List<Vector3>(64);

        protected override void SetupData()
        {
            m_MethodLabel = "GeometryUtils.ConvexHull2D()";
            m_CallCount *= k_ExampleCount;

            Random.InitState(100);
            for (var i = 0; i < k_ExampleCount; i++)
            {
                var list = new List<Vector3>();
                var range = Mathf.Sqrt((i + 1) * 5) * 0.5f;
                for (var j = 0; j < k_PointsPerExample; j++)
                {
                    var x = Random.Range(-range, range);
                    var z = Random.Range(-range, range);
                    list.Add(new Vector3(x, 0f, z));
                }

                m_Cases.Add(list);
            }
        }

        protected override void RunTestFrame()
        {
            foreach (var c in m_Cases)
            {
                m_Timer.Restart();
                GeometryUtils.ConvexHull2D(c, m_Hull);
                m_Timer.Stop();
                m_ElapsedTicks += m_Timer.ElapsedTicks;
            }
        }
    }
}
