#if XR_MGMT_3_2_0_OR_NEWER
using System.Collections.Generic;
using UnityEngine.XR.OpenXR;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;

namespace UnityEditor.XR.OpenXR
{
    public class OpenXRManagementSettings : IXRPackage
    {
        private class MyLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName { get; set; }
            public string loaderType { get; set; }
            public List<BuildTargetGroup> supportedBuildTargets { get; set; }
        }

        private class MyPackageMetadata : IXRPackageMetadata
        {
            public string packageName { get; set; }
            public string packageId { get; set; }
            public string settingsType { get; set; }
            public List<IXRLoaderMetadata> loaderMetadata { get; set; }
        }

        private static IXRPackageMetadata s_Metadata = new MyPackageMetadata()
        {
            packageName = "OpenXR XR Plugin",
            packageId = "com.unity.xr.openxr",
            settingsType = "UnityEditor.XR.OpenXR.OpenXRPackageSettings",
            loaderMetadata = new List<IXRLoaderMetadata>()
            {
                new MyLoaderMetadata()
                {
                    loaderName = "OpenXR Loader",
                    loaderType = "UnityEngine.XR.OpenXR.OpenXRLoader",
                    supportedBuildTargets = new List<BuildTargetGroup>()
                    {
                        BuildTargetGroup.Standalone,
                        BuildTargetGroup.Android,
                        BuildTargetGroup.WSA
                    },
                },
            }
        };


        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            try
            {
                EditorBuildSettings.AddConfigObject(Constants.k_SettingsKey, obj, true);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.Log($"Erorr adding new OpenXR Settings object to build settings.\n{ex.Message}");
            }

            return false;
        }

        public IXRPackageMetadata metadata => s_Metadata;

        internal static string PackageId => s_Metadata.packageId;
    }
}
#endif