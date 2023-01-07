using NUnit.Framework;
using System;
using UnityEngine.XR.ARSubsystems;

namespace UnityEditor.XR.ARSubsystems.Tests
{
    [TestFixture]
    class ARSubsystemEditorTests
    {
        [MenuItem("AR Foundation/Image Libraries/Clear data stores")]
        static void ClearReferenceImageLibraryDataStores()
        {
            foreach (var library in XRReferenceImageLibrary.All())
            {
                library.ClearDataStore();
            }
        }

        [Test]
        public void CanRoundtripGuid()
        {
            var guid = Guid.NewGuid();
            guid.Decompose(out var low, out var high);
            var recomposedGuid = GuidUtil.Compose(low, high);
            Assert.AreEqual(guid, recomposedGuid);
        }
    }
}
