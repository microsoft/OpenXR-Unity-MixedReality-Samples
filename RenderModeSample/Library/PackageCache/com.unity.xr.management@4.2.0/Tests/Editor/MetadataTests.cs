using NUnit.Framework;

using System;
using System.Collections;
using System.Linq;

using UnityEngine.TestTools;
using UnityEditor.XR.Management.Metadata;

using Unity.XR.Management.TestPackage;
using Unity.XR.Management.TestPackage.Editor;

namespace UnityEditor.XR.Management.Tests
{
    class MetadataTests
    {
        [SetUp]
        public void Setup()
        {
            AssetDatabase.DeleteAsset("Assets/XR");
            XRPackageInitializationBootstrap.BeginPackageInitialization();

            TestPackage pkg = new TestPackage();
            XRPackageMetadataStore.AddPluginPackage(pkg);
            XRPackageInitializationBootstrap.InitPackage(pkg);
        }

        [TearDown]
        public void Teardown()
        {
            AssetDatabase.DeleteAsset("Assets/XR");
        }

        [UnityTest]
        public IEnumerator CreateSettingsAndLoaders()
        {
            int frameCount = 0;
            string[] assets = new string[0]{};


            while (frameCount < 60)
            {
                yield return null;
                frameCount++;
            }

            yield return null;

            string[] assetTypes = new string[]{"TestLoaderOne", "TestLoaderTwo", "TestLoaderThree","TestSettings"};
            foreach (var assetType in assetTypes)
            {
                assets = AssetDatabase.FindAssets($"t:{assetType}");
                Assert.IsTrue(assets.Length == 1);
            }
        }

        [Test, Sequential]
        public void FilteringMetadataByBuildTargetGroup(
            [Values(BuildTargetGroup.Standalone, BuildTargetGroup.Android, BuildTargetGroup.Unknown, BuildTargetGroup.WebGL)]BuildTargetGroup buildTargetGroup,
            [Values("Test Loader One", "Test Loader Two", "Test Loader Three", "")]string expectedType)
        {
            var loaders = XRPackageMetadataStore.GetLoadersForBuildTarget(buildTargetGroup);
            Assert.IsTrue((loaders.Count > 0 && !String.IsNullOrEmpty(expectedType)) ||
                            (loaders.Count <= 0 && String.IsNullOrEmpty(expectedType)));

            if (!String.IsNullOrEmpty(expectedType))
            {
                var loaderNames = from lm in loaders where String.Compare(lm.loaderName, expectedType, false) == 0 select lm.loaderName;
                Assert.IsTrue(loaderNames.Any());
                Assert.IsTrue(loaderNames.Count() == 1);
            }
        }

        [UnityTest]
        public IEnumerator AccessSettings()
        {
            int frameCount = 0;
            string[] assets = new string[0] { };

            TestPackage pkg = new TestPackage();
            XRPackageMetadataStore.AddPluginPackage(pkg);
            XRPackageInitializationBootstrap.InitPackage(pkg);

            while (frameCount < 60)
            {
                yield return null;
                frameCount++;
            }

            yield return null;

            var metadata = XRPackageMetadataStore.GetMetadataForPackage("com.unity.xr.testpackage");
            Assert.IsNotNull(metadata);

            assets = AssetDatabase.FindAssets($"t:{metadata.settingsType}");
            Assert.IsTrue(assets.Length == 1);
            var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);

            var instance = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TestSettings));
            Assert.IsNotNull(instance);
        }

        [Test, Sequential]
        public void FilteringAllMetadataByBuildTargetGroup(
            [Values(BuildTargetGroup.Standalone, BuildTargetGroup.Android, BuildTargetGroup.Unknown, BuildTargetGroup.WebGL)]BuildTargetGroup buildTargetGroup,
            [Values("Test Loader One", "Test Loader Two", "Test Loader Three", "")]string expectedType)
        {
            var packages = XRPackageMetadataStore.GetAllPackageMetadataForBuildTarget(buildTargetGroup);
            Assert.IsTrue((packages.Count > 0 && !String.IsNullOrEmpty(expectedType)) ||
                            (packages.Count <= 0 && String.IsNullOrEmpty(expectedType)));

            if (!String.IsNullOrEmpty(expectedType))
            {
                var loaderNames = from lm in (from p in packages select p.metadata.loaderMetadata)
                                    from l in lm
                                    where String.Compare(l.loaderName, expectedType, false) == 0
                                    select l.loaderName;
                Assert.IsTrue(loaderNames.Any());
                Assert.IsTrue(loaderNames.Count() == 1);
            }
        }

        [Test, Sequential]
        public void FilteringAllMetadataByBuildTargetGroupReturnsSameSet(
            [Values(BuildTargetGroup.Standalone, BuildTargetGroup.Android, BuildTargetGroup.Unknown, BuildTargetGroup.WebGL)]BuildTargetGroup buildTargetGroup,
            [Values("Test Loader One", "Test Loader Two", "Test Loader Three", "")]string expectedType)
        {
            var loaders = XRPackageMetadataStore.GetLoadersForBuildTarget(buildTargetGroup);
            Assert.IsTrue((loaders.Count > 0 && !String.IsNullOrEmpty(expectedType)) ||
                            (loaders.Count <= 0 && String.IsNullOrEmpty(expectedType)));

            var packages = XRPackageMetadataStore.GetAllPackageMetadataForBuildTarget(buildTargetGroup);
            Assert.IsTrue((packages.Count > 0 && !String.IsNullOrEmpty(expectedType)) ||
                            (packages.Count <= 0 && String.IsNullOrEmpty(expectedType)));

            if (!String.IsNullOrEmpty(expectedType))
            {
                var loaderNames = from lm in loaders where String.Compare(lm.loaderName, expectedType, false) == 0 select lm.loaderName;
                Assert.IsTrue(loaderNames.Any());
                Assert.IsTrue(loaderNames.Count() == 1);

                var packageLoaderNames = from lm in (from p in packages select p.metadata.loaderMetadata)
                                    from l in lm
                                    where String.Compare(l.loaderName, expectedType, false) == 0
                                    select l.loaderName;
                Assert.IsTrue(packageLoaderNames.Any());
                Assert.IsTrue(packageLoaderNames.Count() == 1);

                Assert.AreEqual(loaderNames.First(), packageLoaderNames.First());
            }
        }

        [Test]
        public void CanGetAllPackageMetadata()
        {
            var packages = XRPackageMetadataStore.GetAllPackageMetadata();

            foreach (var package in packages)
            {

                if (String.CompareOrdinal("Test Package", package.metadata.packageName) != 0)
                    continue;

                Assert.AreEqual("Test Package", package.metadata.packageName);
                Assert.AreEqual("com.unity.xr.testpackage", package.metadata.packageId);
                Assert.AreEqual(typeof(TestSettings).FullName, package.metadata.settingsType);

                Assert.AreEqual(3, package.metadata.loaderMetadata.Count);
                foreach (var lm in package.metadata.loaderMetadata)
                {
                    switch (lm.loaderName)
                    {
                        case "Test Loader One":
                            Assert.AreEqual(typeof(TestLoaderOne).FullName, lm.loaderType);
                            Assert.IsTrue(lm.supportedBuildTargets.Count == 2);
                            Assert.IsTrue(lm.supportedBuildTargets.Contains(BuildTargetGroup.Standalone));
                            Assert.IsTrue(lm.supportedBuildTargets.Contains(BuildTargetGroup.WSA));
                        break;

                        case "Test Loader Two":
                            Assert.AreEqual(typeof(TestLoaderTwo).FullName, lm.loaderType);
                            Assert.IsTrue(lm.supportedBuildTargets.Count == 3);
                            Assert.IsTrue(lm.supportedBuildTargets.Contains(BuildTargetGroup.Android));
                            Assert.IsTrue(lm.supportedBuildTargets.Contains(BuildTargetGroup.iOS));
                            Assert.IsTrue(lm.supportedBuildTargets.Contains(BuildTargetGroup.Lumin));
                        break;

                        case "Test Loader Three":
                            Assert.AreEqual(typeof(TestLoaderThree).FullName, lm.loaderType);
                            Assert.IsTrue(lm.supportedBuildTargets.Count == 1);
                            Assert.IsTrue(lm.supportedBuildTargets.Contains(BuildTargetGroup.Unknown));
                        break;

                        default:
                            throw new Exception($"Found unknown test loader {lm.loaderName}");
                    }
                }
            }
        }

    }
}
