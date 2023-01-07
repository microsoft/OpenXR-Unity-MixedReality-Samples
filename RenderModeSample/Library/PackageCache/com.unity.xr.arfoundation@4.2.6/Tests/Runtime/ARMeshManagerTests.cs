using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.ARFoundation
{
    [TestFixture]
    class ARMeshManagerTests
    {
        ARSessionOrigin m_SessionOrigin;
        ARMeshManager m_MeshManager;
        Transform m_Content;

        [Test]
        public void CanGetARSessionOriginAfterMakeContentAppearAt()
        {
            m_SessionOrigin.MakeContentAppearAt(m_Content, new Vector3(1, 2, 3));
            Assert.NotNull(m_MeshManager.GetSessionOrigin(), $"The {nameof(ARSessionOrigin)} was null after a call to {nameof(ARSessionOrigin.MakeContentAppearAt)}");
        }

        [OneTimeSetUp]
        public void Setup()
        {
            var sessionOriginGO = new GameObject("Session Origin");
            m_SessionOrigin = sessionOriginGO.AddComponent<ARSessionOrigin>();

            var meshManagerGO = new GameObject("Meshing");
            meshManagerGO.transform.parent = sessionOriginGO.transform;
            m_MeshManager = meshManagerGO.AddComponent<ARMeshManager>();

            var contentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            contentGO.name = "Content";
            m_Content = contentGO.transform;
        }
    }
}
