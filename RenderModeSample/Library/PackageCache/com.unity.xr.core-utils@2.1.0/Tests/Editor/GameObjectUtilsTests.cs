using NUnit.Framework;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.XR.CoreUtils.EditorTests
{
    class GameObjectUtilsTests
    {
        const int k_ChildCount = 2;
        static HideFlags[] s_HideFlagsValues = EnumValues<HideFlags>.Values;

        GameObject m_GameObject;
        GameObject m_GameObjectCopy;

        [TearDown]
        public void AfterEach()
        {
            UnityObject.DestroyImmediate(m_GameObject);
            UnityObject.DestroyImmediate(m_GameObjectCopy);
        }

        [TestCaseSource(typeof(GameObjectUtilsTests), nameof(s_HideFlagsValues))]
        public void CloneWithHideFlags_NoChildren(HideFlags hideFlags)
        {
            m_GameObject = new GameObject { hideFlags = hideFlags };
            m_GameObjectCopy = GameObjectUtils.CloneWithHideFlags(m_GameObject);
            Assert.AreEqual(m_GameObject.hideFlags, m_GameObjectCopy.hideFlags);
        }

        [TestCaseSource(typeof(GameObjectUtilsTests), nameof(s_HideFlagsValues))]
        public void CloneWithHideFlags_Children_SameFlags(HideFlags hideFlags)
        {
            m_GameObject = new GameObject { hideFlags = hideFlags };
            for (var i = 0; i < k_ChildCount; ++i)
            {
                var child = new GameObject { hideFlags = hideFlags };
                child.transform.parent = m_GameObject.transform;
                for (var j = 0; j < k_ChildCount; ++j)
                {
                    var childChild = new GameObject { hideFlags = hideFlags };
                    childChild.transform.parent = child.transform;
                }
            }

            m_GameObjectCopy = GameObjectUtils.CloneWithHideFlags(m_GameObject);
            CompareHideFlagsRecursively(m_GameObject, m_GameObjectCopy);
        }

        [Test]
        public void CloneWithHideFlags_Children_DifferentFlags()
        {
            var hideFlagsCount = s_HideFlagsValues.Length;
            m_GameObject = new GameObject { hideFlags = s_HideFlagsValues[0] };
            var originals = new GameObject[hideFlagsCount];
            originals[0] = m_GameObject;
            for (var i = 1; i < hideFlagsCount; ++i)
            {
                var child = new GameObject { hideFlags = s_HideFlagsValues[i] };
                child.transform.parent = originals[(i - 1) / k_ChildCount].transform;
                originals[i] = child;
            }

            m_GameObjectCopy= GameObjectUtils.CloneWithHideFlags(m_GameObject);
            CompareHideFlagsRecursively(m_GameObject, m_GameObjectCopy);
        }

        static void CompareHideFlagsRecursively(GameObject obj1, GameObject obj2)
        {
            Assert.AreEqual(obj1.hideFlags, obj2.hideFlags);
            var obj1Transform = obj1.transform;
            var obj2Transform = obj2.transform;
            for (var i = 0; i < obj1Transform.childCount; ++i)
            {
                CompareHideFlagsRecursively(obj1Transform.GetChild(i).gameObject, obj2Transform.GetChild(i).gameObject);
            }
        }
    }
}
