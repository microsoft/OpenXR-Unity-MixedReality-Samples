using NUnit.Framework;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class PoseExtensionsTests
    {
        readonly Pose m_IdentityPose = new Pose(new Vector3(), Quaternion.identity);
        readonly Pose m_NonIdentityRotationPose = new Pose(new Vector3(), Quaternion.Inverse(Quaternion.Euler(10f, 20f, 30f)));
        readonly Pose m_PositionOnlyOffsetPose = new Pose(new Vector3(2f, 2f, 2f), Quaternion.identity);
        readonly Pose m_DefaultPose = new Pose(new Vector3(1f, 1f, 1f), Quaternion.identity);

        [Test]
        public void IdentityPoseOffsetDoesNothing()
        {
            Assert.AreEqual(m_DefaultPose, m_IdentityPose.ApplyOffsetTo(m_DefaultPose));
            Assert.AreEqual(m_DefaultPose, m_DefaultPose.ApplyOffsetTo(m_IdentityPose));
        }

        [Test]
        public void ApplyOffsetToPosition()
        {
            var offset = m_PositionOnlyOffsetPose.ApplyOffsetTo(m_DefaultPose);
            Assert.AreEqual(m_DefaultPose.position + m_PositionOnlyOffsetPose.position, offset.position);
            Assert.AreEqual(Quaternion.identity, offset.rotation);
        }

        [Test]
        public void ApplyOffsetToRotation()
        {
            var offset = m_NonIdentityRotationPose.ApplyOffsetTo(m_DefaultPose);
            Assert.AreEqual(m_NonIdentityRotationPose.rotation, offset.rotation);
        }
    }
}
