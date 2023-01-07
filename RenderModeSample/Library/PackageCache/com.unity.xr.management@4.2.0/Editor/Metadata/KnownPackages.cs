using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace UnityEditor.XR.Management.Metadata
{
    internal class KnownPackages
    {
        internal static string k_KnownPackageMockHMDLoader = "Unity.XR.MockHMD.MockHMDLoader";

        class KnownLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName { get; set; }
            public string loaderType { get; set; }
            public List<BuildTargetGroup> supportedBuildTargets { get; set; }
        }

        class KnownPackageMetadata : IXRPackageMetadata
        {
            public string packageName { get; set; }
            public string packageId { get; set; }
            public string settingsType { get; set; }
            public List<IXRLoaderMetadata> loaderMetadata { get; set; }
        }

        class KnownPackage : IXRPackage
        {
            public IXRPackageMetadata metadata { get; set; }
            public bool PopulateNewSettingsInstance(ScriptableObject obj) { return true; }
        }

        private static Lazy<List<IXRPackage>> s_KnownPackages = new Lazy<List<IXRPackage>>(InitKnownPackages);

        internal static List<IXRPackage> Packages => s_KnownPackages.Value;

        static List<IXRPackage> InitKnownPackages()
        {
            List<IXRPackage> packages = new List<IXRPackage>();

#if UNITY_2020_3_OR_NEWER
            packages.Add(new KnownPackage() {
                metadata = new KnownPackageMetadata(){
                    packageName = "Open XR Plugin",
                    packageId = "com.unity.xr.openxr",
                    settingsType = "UnityEditor.XR.OpenXR.OpenXRPackageSettings",
                    loaderMetadata = new List<IXRLoaderMetadata>() {
                    new KnownLoaderMetadata() {
                            loaderName = "Open XR",
                            loaderType = "UnityEngine.XR.OpenXR.OpenXRLoader",
                            supportedBuildTargets = new List<BuildTargetGroup>() {
                                BuildTargetGroup.Standalone,
                                BuildTargetGroup.WSA
                            }
                        },
                    }
                }
            });
#endif

#if !UNITY_2021_2_OR_NEWER
            packages.Add(new KnownPackage() {
                metadata = new KnownPackageMetadata(){
                    packageName = "Windows XR Plugin",
                    packageId = "com.unity.xr.windowsmr",
                    settingsType = "UnityEditor.XR.WindowsMR.WindowsMRPackageSettings",
                    loaderMetadata = new List<IXRLoaderMetadata>() {
                    new KnownLoaderMetadata() {
                            loaderName = "Windows Mixed Reality",
                            loaderType = "UnityEngine.XR.WindowsMR.WindowsMRLoader",
                            supportedBuildTargets = new List<BuildTargetGroup>() {
                                BuildTargetGroup.Standalone,
                                BuildTargetGroup.WSA
                            }
                        },
                    }
                }
            });
#endif

            packages.Add(new KnownPackage() {
                metadata = new KnownPackageMetadata(){
                    packageName = "Oculus XR Plugin",
                    packageId = "com.unity.xr.oculus",
                    settingsType = "Unity.XR.Oculus.OculusSettings",
                    loaderMetadata = new List<IXRLoaderMetadata>() {
                    new KnownLoaderMetadata() {
                            loaderName = "Oculus",
                            loaderType = "Unity.XR.Oculus.OculusLoader",
                            supportedBuildTargets = new List<BuildTargetGroup>() {
                                BuildTargetGroup.Standalone,
                                BuildTargetGroup.Android,
                            }
                        },
                    }
                }
            });

// This section will get replaced with the RELISH package in the near future.
#if !UNITY_2021_2_OR_NEWER
            packages.Add(new KnownPackage() {
                metadata = new KnownPackageMetadata(){
                    packageName = "Magic Leap XR Plugin",
                    packageId = "com.unity.xr.magicleap",
                    settingsType = "UnityEngine.XR.MagicLeap.MagicLeapSettings",
                    loaderMetadata = new List<IXRLoaderMetadata>() {
                    new KnownLoaderMetadata() {
                            loaderName = "Magic Leap Zero Iteration",
                            loaderType = "UnityEngine.XR.MagicLeap.MagicLeapLoader",
                            supportedBuildTargets = new List<BuildTargetGroup>() {
                                BuildTargetGroup.Standalone,
                            }
                        },
                    new KnownLoaderMetadata() {
                            loaderName = "Magic Leap",
                            loaderType = "UnityEngine.XR.MagicLeap.MagicLeapLoader",
                            supportedBuildTargets = new List<BuildTargetGroup>() {
                                BuildTargetGroup.Lumin
                            }
                        },
                    }
                }
            });
#endif // !UNITY_2021_2_OR_NEWER

            packages.Add(new KnownPackage() {
                metadata = new KnownPackageMetadata(){
                    packageName = "ARCore XR Plugin",
                    packageId = "com.unity.xr.arcore",
                    settingsType = "UnityEditor.XR.ARCore.ARCoreSettings",
                    loaderMetadata = new List<IXRLoaderMetadata>() {
                    new KnownLoaderMetadata() {
                            loaderName = "ARCore",
                            loaderType = "UnityEngine.XR.ARCore.ARCoreLoader",
                            supportedBuildTargets = new List<BuildTargetGroup>() {
                                BuildTargetGroup.Android,
                            }
                        },
                    }
                }
            });


            packages.Add(new KnownPackage() {
                metadata = new KnownPackageMetadata(){
                    packageName = "ARKit XR Plugin",
                    packageId = "com.unity.xr.arkit",
                    settingsType = "UnityEditor.XR.ARKit.ARKitSettings",
                    loaderMetadata = new List<IXRLoaderMetadata>() {
                    new KnownLoaderMetadata() {
                            loaderName = "ARKit",
                            loaderType = "UnityEngine.XR.ARKit.ARKitLoader",
                            supportedBuildTargets = new List<BuildTargetGroup>() {
                                BuildTargetGroup.iOS,
                            }
                        },
                    }
                }
            });


            packages.Add(new KnownPackage() {
                metadata = new KnownPackageMetadata() {
                    packageName = "MockHMD XR Plugin",
                    packageId = "com.unity.xr.mock-hmd",
                    settingsType = "Unity.XR.MockHMD.MockHMDBuildSettings",
                    loaderMetadata = new List<IXRLoaderMetadata>() {
                    new KnownLoaderMetadata() {
                            loaderName = "Unity Mock HMD",
                            loaderType = k_KnownPackageMockHMDLoader,
                            supportedBuildTargets = new List<BuildTargetGroup>() {
                                BuildTargetGroup.Standalone,
                                BuildTargetGroup.Android
                            }
                        },
                    }
                }
            });
            return packages;
        }
    }
}
