using NUnit.Framework;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.XR.CoreUtils.EditorTests
{
    class ComponentUtilsTests
    {
        [Test]
        public void GetComponent()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<BoxCollider>();
            gameObject.AddComponent<MeshRenderer>();
            gameObject.AddComponent<BoxCollider>();
            Assert.AreEqual(gameObject.GetComponent<BoxCollider>(), ComponentUtils<BoxCollider>.GetComponent(gameObject));
        }

        [Test]
        public void GetComponentInChildren()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<MeshFilter>();
            var child = new GameObject();
            child.transform.SetParent(gameObject.transform);
            child.AddComponent<BoxCollider>();
            child.AddComponent<MeshRenderer>();
            child.AddComponent<BoxCollider>();
            Assert.AreEqual(gameObject.GetComponentInChildren<BoxCollider>(), ComponentUtils<BoxCollider>.GetComponentInChildren(gameObject));
        }

        [Test]
        public void GetOrAddIf_GetsExistingComponent()
        {
            var gameObject = new GameObject();
            var collider = gameObject.AddComponent<BoxCollider>();
            Assert.AreEqual(collider, ComponentUtils.GetOrAddIf<BoxCollider>(gameObject, false));
        }

        [Test]
        public void GetOrAddIf_DoesNotAddExtraComponent()
        {
            var gameObject = new GameObject();
            var collider = gameObject.AddComponent<BoxCollider>();
            Assert.AreEqual(collider, ComponentUtils.GetOrAddIf<BoxCollider>(gameObject, true));
        }

        [Test]
        public void GetOrAddIf_DoesNotAddIfFalse()
        {
            var gameObject = new GameObject();
            Assert.AreEqual(null, ComponentUtils.GetOrAddIf<BoxCollider>(gameObject, false));
        }

        [Test]
        public void GetOrAddIf_DoesAddIfTrue()
        {
            var gameObject = new GameObject();
            var collider = ComponentUtils.GetOrAddIf<BoxCollider>(gameObject, true);
            Assert.IsNotNull(collider);
            Assert.AreEqual(gameObject.GetComponent<BoxCollider>(), collider);
        }
    }
}
