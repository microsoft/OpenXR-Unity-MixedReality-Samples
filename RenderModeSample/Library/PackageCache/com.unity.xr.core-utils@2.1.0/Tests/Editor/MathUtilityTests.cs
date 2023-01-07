using NUnit.Framework;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class MathUtilityTests
    {
        // each test case makes the range between the numbers much smaller,
        // so that we can be confident that we get the same answers even in very precise cases
        [TestCase(0.00001f)]
        [TestCase(0.0000000001f)]
        [TestCase(0.00000000000000000001f)]
        [TestCase(0.000000000000000000000000000001f)]
        [TestCase(0.0000000000000000000000000000000000001f)]
        public void ApproximatelyGivesSameResultsAsMathf(float r)
        {
            const int callCount = 100;
            var testCases = new Vector2[callCount];
            Random.InitState(0);
            for (var i = 0; i < callCount; i++)
            {
                testCases[i] = new Vector2(Random.Range(-r, r), Random.Range(-r, r));
            }

            foreach (var pair in testCases)
            {
                var ourResult = MathUtility.Approximately(pair.x, pair.y);
                var mathfResult = Mathf.Approximately(pair.x, pair.y);
                Assert.AreEqual(mathfResult, ourResult, $"comparing {pair.x} to {pair.y} - Mathf said {mathfResult} , we said {ourResult}");
            }
        }
    }
}
