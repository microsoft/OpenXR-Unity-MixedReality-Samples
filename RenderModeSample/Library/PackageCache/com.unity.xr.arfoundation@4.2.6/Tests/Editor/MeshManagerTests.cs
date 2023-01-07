using NUnit.Framework;
using UnityEngine.XR.ARFoundation;

namespace UnityEditor.XR.ARFoundation
{
    [TestFixture]
    class MeshManagerTests
    {
        [Test]
        public void CanBeUsedInPrefab()
        {
            var arSessionGameObject =
                ObjectFactory.CreateGameObject("AR Session Origin", typeof(ARSessionOrigin));
            var arMeshManagerGameObject = ObjectFactory.CreateGameObject("Meshing");
            arMeshManagerGameObject.transform.SetParent(arSessionGameObject.transform);
            arMeshManagerGameObject.AddComponent<ARMeshManager>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(
                arSessionGameObject,
                "Assets/MeshingPrefab.prefab");

            Assert.IsTrue(prefab != null);
            var meshManager = prefab.GetComponentInChildren<ARMeshManager>(true);
            Assert.IsTrue(meshManager != null);
            Assert.IsTrue(meshManager.IsValid());

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(prefab));
        }
    }
}
