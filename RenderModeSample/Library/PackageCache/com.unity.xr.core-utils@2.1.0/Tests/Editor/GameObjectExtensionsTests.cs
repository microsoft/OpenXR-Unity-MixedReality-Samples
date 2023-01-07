using NUnit.Framework;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class GameObjectExtensionsTests
    {
        GameObject m_RootObject;
        readonly GameObject[] m_RootChildren = new GameObject[3];
        readonly GameObject[] m_NestedChildren = new GameObject[3];

        [OneTimeSetUp]
        public void Setup()
        {
            m_RootObject = new GameObject("root");
            for (var i = 0; i < m_RootChildren.Length; i++)
            {
                var child = new GameObject("root child " + i);
                child.transform.SetParent(m_RootObject.transform);
                m_RootChildren[i] = child;
            }

            var nestedParent = m_RootChildren[0];
            for (var i = 0; i < m_NestedChildren.Length; i++)
            {
                var child = new GameObject("nested child " + i);
                child.transform.SetParent(nestedParent.transform);
                m_NestedChildren[i] = child;
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            m_RootObject.layer = 0;
            m_RootObject.hideFlags = HideFlags.None;
            foreach (var child in m_RootChildren)
            {
                child.layer = 0;
                child.hideFlags = HideFlags.None;
            }
            foreach (var child in m_NestedChildren)
            {
                child.layer = 0;
                child.hideFlags = HideFlags.None;
            }
        }

        void AssertHideFlagsEqual(HideFlags flags)
        {
            Assert.AreEqual(flags, m_RootObject.hideFlags);
            foreach (var child in m_RootChildren)
            {
                Assert.AreEqual(flags, child.hideFlags);
            }
            foreach (var child in m_NestedChildren)
            {
                Assert.AreEqual(flags, child.hideFlags);
            }
        }

        void AssertLayerEqual(int layer)
        {
            Assert.AreEqual(layer, m_RootObject.layer);
            foreach (var child in m_RootChildren)
            {
                Assert.AreEqual(layer, child.layer);
            }
            foreach (var child in m_NestedChildren)
            {
                Assert.AreEqual(layer, child.layer);
            }
        }

        [Test]
        public void SetHideFlagsRecursively()
        {
            m_RootObject.SetHideFlagsRecursively(HideFlags.HideAndDontSave);
            AssertHideFlagsEqual(HideFlags.HideAndDontSave);
            m_RootObject.SetHideFlagsRecursively(HideFlags.DontUnloadUnusedAsset);
            AssertHideFlagsEqual(HideFlags.DontUnloadUnusedAsset);
        }

        [Test]
        public void SetLayerRecursively()
        {
            m_RootObject.SetLayerRecursively(10);
            AssertLayerEqual(10);
            m_RootObject.SetLayerRecursively(5);
            AssertLayerEqual(5);
        }

        [Test]
        public void SetLayerAndHideFlagsRecursively()
        {
            const HideFlags hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInBuild;
            m_RootObject.SetLayerAndHideFlagsRecursively(10, hideFlags);
            AssertLayerEqual(10);
            AssertHideFlagsEqual(hideFlags);
        }

        [Test]
        public void SetLayerAndAddToHideFlagsRecursively()
        {
            m_RootObject.SetHideFlagsRecursively(HideFlags.NotEditable);
            m_RootObject.SetLayerAndAddToHideFlagsRecursively(10, HideFlags.HideAndDontSave);
            AssertLayerEqual(10);
            AssertHideFlagsEqual(HideFlags.NotEditable | HideFlags.HideAndDontSave);
        }
    }
}
