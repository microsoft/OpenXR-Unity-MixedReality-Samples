using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    [TestFixture]
    class BoundsUtilsTests
    {
        const float k_Delta = 1e-6f;

        GameObject m_GameObject, m_Parent, m_Other;
        List<GameObject> m_ToCleanupAfterEach = new List<GameObject>();

        [SetUp]
        public void BeforeEach()
        {
            m_GameObject = new GameObject("object utils test");
            m_Parent = new GameObject("parent");
            m_Parent.transform.position += new Vector3(2, 4, 8);
            m_Other = new GameObject("other");
            m_Other.transform.position += new Vector3(-5, 1, 2);

            m_ToCleanupAfterEach.AddRange(new[] { m_GameObject, m_Parent, m_Other });
        }

        [Test]
        public void GetBounds_WithoutExtents()
        {
            var localBounds = new Bounds(m_Other.transform.position, new Vector3(0, 0, 0));
            var bounds = BoundsUtils.GetBounds(m_Other.transform);

            Assert.AreEqual(localBounds, bounds);
        }

        [Test]
        public void GetBounds_Array()
        {
            var boundsA = new GameObject();
            boundsA.transform.position += new Vector3(-5, -2, 8);
            var boundsB = new GameObject();
            boundsB.transform.position += new Vector3(2, 6, 4);
            var transforms = new[] { boundsA.transform, boundsB.transform };

            // if you want to work with more than one object in a test, add them to cleanup list manually
            m_ToCleanupAfterEach.AddRange(new[] { boundsA, boundsB });

            var bounds = BoundsUtils.GetBounds(transforms);
            var expected = new Bounds(new Vector3(-1.5f, 2f, 6f), new Vector3(7f, 8f, 4f));

            Assert.That(bounds, Is.EqualTo(expected).Within(k_Delta));
        }

        [TearDown]
        public void CleanupAfterEach()
        {
            foreach (var o in m_ToCleanupAfterEach)
            {
                UnityObjectUtils.Destroy(o);
            }
        }
    }
}
