using NUnit.Framework;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class MonoBehaviourExtensionsTests
    {
        class DummyBehaviour : MonoBehaviour {}

        GameObject m_GameObject;
        DummyBehaviour m_Behaviour;

        [OneTimeSetUp]
        public void Setup()
        {
            m_GameObject = new GameObject("MonoBehaviour extensions test");
            m_Behaviour = m_GameObject.AddComponent<DummyBehaviour>();
        }

        [Test]
        public void StartRunInEditMode()
        {
            Assert.False(m_Behaviour.runInEditMode);
            m_Behaviour.StartRunInEditMode();
            Assert.True(m_Behaviour.runInEditMode);
        }

        [Test]
        public void StopRunInEditModeWhenEnabled()
        {
            m_Behaviour.runInEditMode = true;
            m_Behaviour.enabled = true;
            m_Behaviour.StopRunInEditMode();
            Assert.False(m_Behaviour.runInEditMode);
            Assert.True(m_Behaviour.enabled);
        }

        [Test]
        public void StopRunInEditModeWhenDisabled()
        {
            m_Behaviour.runInEditMode = true;
            m_Behaviour.enabled = false;
            m_Behaviour.StopRunInEditMode();
            Assert.False(m_Behaviour.runInEditMode);
            Assert.False(m_Behaviour.enabled);
        }
    }
}
