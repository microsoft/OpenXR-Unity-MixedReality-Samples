using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEditor;
using UnityEditor.XR.Management.Metadata;

[assembly:InternalsVisibleTo("UnityEditor.XR.Management.Tests")]
namespace Unity.XR.Management.TestPackage.Editor
{
    class TestLoaderMetadata : IXRLoaderMetadata 
    {
        public string loaderName { get; set; }
        public string loaderType { get; set; }
        public List<BuildTargetGroup> supportedBuildTargets { get; set; }
    }

    class TestPackageMetadata : IXRPackageMetadata
    {
        public string packageName { get; set; }
        public string packageId { get; set; }
        public string settingsType { get; set; }
        public List<IXRLoaderMetadata> loaderMetadata { get; set; } 
    }

    static class TestMetadata
    {
        static TestPackageMetadata s_Metadata = null;

        internal static TestPackageMetadata CreateAndGetMetadata()
        {
            if (s_Metadata == null)
            {
                s_Metadata = new TestPackageMetadata();
                s_Metadata.packageName = "Test Package";
                s_Metadata.packageId = "com.unity.xr.testpackage";
                s_Metadata.settingsType = typeof(TestSettings).FullName;

                s_Metadata.loaderMetadata = new List<IXRLoaderMetadata>() {
                    new TestLoaderMetadata() {
                        loaderName = "Test Loader One",
                        loaderType = typeof(TestLoaderOne).FullName,
                        supportedBuildTargets = new List<BuildTargetGroup>() {
                            BuildTargetGroup.Standalone,
                            BuildTargetGroup.WSA
                        }
                    },
                    new TestLoaderMetadata() {
                        loaderName = "Test Loader Two",
                        loaderType = typeof(TestLoaderTwo).FullName,
                        supportedBuildTargets = new List<BuildTargetGroup>() {
                            BuildTargetGroup.Android,
                            BuildTargetGroup.iOS,
                            BuildTargetGroup.Lumin
                        }
                    },
                    new TestLoaderMetadata() {
                        loaderName = "Test Loader Three",
                        loaderType = typeof(TestLoaderThree).FullName,
                        supportedBuildTargets = new List<BuildTargetGroup>() {
                            BuildTargetGroup.Unknown
                        }
                    },
                };
            }

            return s_Metadata;
        }
    }
}
