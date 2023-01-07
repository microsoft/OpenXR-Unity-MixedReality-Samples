using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.XR.CoreUtils.EditorTests
{
    [TestFixture]
    class UnityObjectUtilsTests
    {
        [UnityTest]
        public IEnumerator Destroy_OneArg_DestroysImmediately_InEditMode()
        {
            Assert.IsFalse(Application.isPlaying);
            var go = new GameObject();
            UnityObjectUtils.Destroy(go);
            yield return null; // skip frame to allow destruction to run
            Assert.IsTrue(go == null);
        }

        [Test]
        public void RemoveDestroyedObjectsTest()
        {
            var go = new GameObject();
            var list = new List<GameObject>{ go };
            UnityObjectUtils.Destroy(go);
            UnityObjectUtils.RemoveDestroyedObjects(list);
            Assert.Zero(list.Count);
        }

        [Test]
        public void RemoveDestroyedKeysTest()
        {
            var go = new GameObject();
            var dictionary = new Dictionary<GameObject, object> { { go, null } };
            UnityObjectUtils.Destroy(go);
            UnityObjectUtils.RemoveDestroyedKeys(dictionary);
            Assert.Zero(dictionary.Count);
        }
    }
}
