using NUnit.Framework;

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management.Metadata;

using Unity.XR.Management.TestPackage;
using Unity.XR.Management.TestPackage.Editor;

namespace UnityEditor.XR.Management.Tests
{
    class PackageManagementTests
    {
        internal static readonly string[] s_TempSettingsPath = { "Temp", "Test" };

        XRGeneralSettingsPerBuildTarget m_TestSettingsPerBuildTarget = null;
        XRGeneralSettings m_TestSettings = null;
        XRManagerSettings m_Settings = null;

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset("Assets/XR");

            AssetDatabase.CreateFolder("Assets", "XR");

            m_Settings = ScriptableObject.CreateInstance<XRManagerSettings>() as XRManagerSettings;
            m_Settings.name = "Actual testable settings.";

            m_TestSettings = ScriptableObject.CreateInstance<XRGeneralSettings>() as XRGeneralSettings;
            m_TestSettings.Manager = m_Settings;
            m_TestSettings.name = "Standalone Settings Container.";

            m_TestSettingsPerBuildTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>() as XRGeneralSettingsPerBuildTarget;
            m_TestSettingsPerBuildTarget.SetSettingsForBuildTarget(BuildTargetGroup.Standalone, m_TestSettings);

            var testPath = XRGeneralSettingsTests.GetAssetPathForComponents(s_TempSettingsPath);
            if (!string.IsNullOrEmpty(testPath))
            {
                AssetDatabase.CreateAsset(m_TestSettingsPerBuildTarget, Path.Combine(testPath, "Test_XRGeneralSettings.asset"));

                AssetDatabase.AddObjectToAsset(m_TestSettings, AssetDatabase.GetAssetOrScenePath(m_TestSettingsPerBuildTarget));

                AssetDatabase.CreateFolder(testPath, "Settings");
                testPath = Path.Combine(testPath, "Settings");
                AssetDatabase.CreateAsset(m_Settings, Path.Combine(testPath, "Test_XRSettingsManager.asset"));

                m_TestSettings.AssignedSettings = m_Settings;
                AssetDatabase.SaveAssets();
            }

            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, m_TestSettingsPerBuildTarget, true);

            XRPackageInitializationBootstrap.BeginPackageInitialization();

            TestPackage pkg = new TestPackage();
            XRPackageMetadataStore.AddPluginPackage(pkg);
            XRPackageInitializationBootstrap.InitPackage(pkg);

            TestLoaderBase.WasAssigned = false;
            TestLoaderBase.WasUnassigned = false;
        }

        [TearDown]
        public void Teardown()
        {
            AssetDatabase.DeleteAsset("Assets/Temp");
            AssetDatabase.DeleteAsset("Assets/XR");
        }

        private string LoaderTypeNameForBuildTarget(BuildTargetGroup buildTargetGroup)
        {
            var loaders = XRPackageMetadataStore.GetLoadersForBuildTarget(buildTargetGroup);
            var filteredLoaders = from l in loaders where String.Compare(l.loaderType, typeof(TestLoaderOne).FullName) == 0 select l;

            if (filteredLoaders.Any())
            {
                var loaderInfo = filteredLoaders.First();
                return loaderInfo.loaderType;
            }

            return "";
        }

        private bool AssignLoaderToSettings(XRManagerSettings settings, string loaderTypeName, BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone)
        {
            if (String.IsNullOrEmpty(loaderTypeName))
                return false;

            return XRPackageMetadataStore.AssignLoader(m_Settings, loaderTypeName, buildTargetGroup);
        }

        private bool SettingsHasLoaderOfType(XRManagerSettings settings, string loaderTypeName)
        {
            bool wasFound = false;
            foreach (var l in m_Settings.activeLoaders)
            {
                if (String.Compare(l.GetType().FullName, loaderTypeName) == 0)
                    wasFound = true;
            }
            return wasFound;
        }


        [UnityTest]
        public IEnumerator TestLoaderAssignment()
        {
            Assert.IsNotNull(m_Settings);

            string loaderTypeName = LoaderTypeNameForBuildTarget(BuildTargetGroup.Standalone);
            Assert.IsFalse(String.IsNullOrEmpty(loaderTypeName));

            bool wasFound = false;
            foreach (var l in m_Settings.activeLoaders)
            {
                if (String.Compare(l.GetType().FullName, loaderTypeName) == 0)
                    wasFound = true;
            }
            Assert.IsFalse(wasFound);

            Assert.IsTrue(XRPackageMetadataStore.AssignLoader(m_Settings, loaderTypeName, BuildTargetGroup.Standalone));

            yield return null;

            Assert.IsTrue(SettingsHasLoaderOfType(m_Settings, loaderTypeName));
            Assert.IsTrue(TestLoaderBase.WasAssigned);

        }

        [Test]
        public void TestLoaderAssignmentSerializes()
        {
            Assert.IsNotNull(m_Settings);
            string loaderTypeName = LoaderTypeNameForBuildTarget(BuildTargetGroup.Standalone);
            Assert.IsFalse(String.IsNullOrEmpty(loaderTypeName));
            AssignLoaderToSettings(m_Settings, loaderTypeName);
            Assert.IsTrue(SettingsHasLoaderOfType(m_Settings, loaderTypeName));

            m_Settings = null;
            var settings = EditorUtilities.GetInstanceOfTypeFromAssetDatabase<XRManagerSettings>();
            m_Settings =  settings as XRManagerSettings;
            Assert.IsNotNull(m_Settings);

            Assert.IsTrue(SettingsHasLoaderOfType(m_Settings, loaderTypeName));
            Assert.IsTrue(TestLoaderBase.WasAssigned);
        }


        [Test]
        public void TestLoaderRemoval()
        {
            Assert.IsNotNull(m_Settings);
            string loaderTypeName = LoaderTypeNameForBuildTarget(BuildTargetGroup.Standalone);
            Assert.IsFalse(String.IsNullOrEmpty(loaderTypeName));
            AssignLoaderToSettings(m_Settings, loaderTypeName);
            Assert.IsTrue(SettingsHasLoaderOfType(m_Settings, loaderTypeName));

            Assert.IsTrue(XRPackageMetadataStore.RemoveLoader(m_Settings, loaderTypeName, BuildTargetGroup.Standalone));

            m_Settings = null;
            var settings = EditorUtilities.GetInstanceOfTypeFromAssetDatabase<XRManagerSettings>();
            m_Settings = settings as XRManagerSettings;
            Assert.IsNotNull(m_Settings);
            Assert.IsFalse(SettingsHasLoaderOfType(m_Settings, loaderTypeName));

            Assert.IsTrue(TestLoaderBase.WasUnassigned);
        }

        [UnityTest]
        public IEnumerator TestInvalidPackageErrorsOut()
        {
#if !UNITY_2020_2_OR_NEWER
            XRPackageMetadataStore.InstallPackageAndAssignLoaderForBuildTarget("com.unity.invalid.package.id", String.Empty, BuildTargetGroup.Standalone);

            LogAssert.Expect(LogType.Error, new Regex(@"Management error"));

            while (XRPackageMetadataStore.isDoingQueueProcessing)
            {
                yield return null;
            }
#else
            yield return null;
#endif //UNITY_2020_2_OR_NEWER

        }

        [UnityTest]
        public IEnumerator TestNoPackageIdErrorsOut()
        {
#if !UNITY_2020_2_OR_NEWER
            XRPackageMetadataStore.InstallPackageAndAssignLoaderForBuildTarget("", String.Empty, BuildTargetGroup.Standalone);

            LogAssert.Expect(LogType.Error, new Regex(@"no package id"));

            while (XRPackageMetadataStore.isDoingQueueProcessing)
            {
                yield return null;
            }
#else
            yield return null;
#endif //UNITY_2020_2_OR_NEWER
        }
    }
}
