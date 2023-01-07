using System.Runtime.CompilerServices;

using UnityEngine;

using UnityEditor.XR.Management.Metadata;

[assembly:InternalsVisibleTo("Unity.XR.Management.EditorTests")]
[assembly:InternalsVisibleTo("Unity.XR.Management.Tests.Standalone")]
namespace Unity.XR.Management.TestPackage.Editor
{
    internal class TestPackage : IXRPackage
    {
        public TestPackage() {}

        public IXRPackageMetadata metadata 
        { 
            get
            {
                return TestMetadata.CreateAndGetMetadata();
            }
        }
        
        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            TestSettings packageSettings = obj as TestSettings;
            if (packageSettings != null)
            {
                return true;
            }
            return false;
        }

    }
}
