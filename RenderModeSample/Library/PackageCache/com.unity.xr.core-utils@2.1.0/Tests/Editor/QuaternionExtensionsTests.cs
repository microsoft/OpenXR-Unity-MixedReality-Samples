using NUnit.Framework;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class QuaternionExtensionsTests
    {
        [Test]
        public void ConstrainYawRotation()
        {
            var rotation = new Quaternion(4, 3, 2, 1);
            var newRotation = rotation.ConstrainYaw();
            Assert.AreEqual(new Quaternion(0, rotation.y, 0, rotation.w), newRotation);
        }

        [Test]
        public void ConstrainYawRotationNormalized()
        {
            var rotation = Quaternion.Euler(4, 3, 2);
            var newRotation = rotation.ConstrainYawNormalized();
            Assert.IsTrue(Quaternion.Euler(0, rotation.eulerAngles.y, 0) == newRotation);
        }

        [Test]
        public void ConstrainYawPitchRotationNormalized()
        {
            var rotation = Quaternion.Euler(15, 30, 60);
            var newRotation = rotation.ConstrainYawPitchNormalized();
            Assert.IsTrue(Quaternion.Euler(15, 30, 0) == newRotation);
        }
    }
}
