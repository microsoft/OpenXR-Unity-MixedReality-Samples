using NUnit.Framework;
using Unity.Collections;

namespace UnityEngine.XR.ARFoundation
{
    [TestFixture]
    public class PoseExtensionsTestFixture
    {
        [Test]
        public void InverseTransformPositionTest()
        {
            Pose pose = default(Pose);
            Vector3 vec = new Vector3(1,2,3);

            Assert.That(vec == pose.InverseTransformPosition(vec), "Inverse transform position not the same as input for a default pose.");
        }

        [Test]
        public void ARFacesUndisposableDoNotThrowOnElementAccess()
        {
            using var array = new NativeArray<int>(42, Allocator.Temp);
            var undisposable = ARFace.GetUndisposable(array);
            var sum = 0;
            for (var i = 0; i < undisposable.Length; i++)
            {
                sum += undisposable[i];
            }
            Assert.AreEqual(42, undisposable.Length);
        }
    }
}
