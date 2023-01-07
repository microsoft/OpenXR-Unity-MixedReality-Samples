using NUnit.Framework;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.Management.Tests;

using Unity.XR.TestTooling;

namespace UnityEditor.XR.Management.Tests
{

    class XRGeneralSettingsTests : ManagementTestSetup
    {
        protected override bool TestManagerUpgradePath => true;
        BuildTargetGroup previousBuildTargetSelection { get; set;  }

        [SetUp]
        public override void SetupTest()
        {

            base.SetupTest();

            previousBuildTargetSelection = EditorUserBuildSettings.selectedBuildTargetGroup;
            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
        }

        [TearDown]
        public override void TearDownTest()
        {
            EditorUserBuildSettings.selectedBuildTargetGroup = previousBuildTargetSelection;
            base.TearDownTest();
        }


        [Test]
        public void UpdateGeneralSettings_ToPerBuildTargetSettings()
        {
            bool success = XRGeneralSettingsUpgrade.UpgradeSettingsToPerBuildTarget(testPathToSettings);
            Assert.IsTrue(success);

            XRGeneralSettingsPerBuildTarget pbtgs = null;

            pbtgs = AssetDatabase.LoadAssetAtPath(testPathToSettings, typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
            Assert.IsNotNull(pbtgs);

            var settings = pbtgs.SettingsForBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup);
            Assert.IsNotNull(settings);
            Assert.IsNotNull(settings.Manager);
            Assert.AreEqual(testManager, settings.Manager);
        }

        [Test]
        public void CanCreateNewSettingsForMissingBuildTargetSettings()
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget buildTargetSettings);
            Assert.False(buildTargetSettings == null);

            Assert.IsFalse(buildTargetSettings.HasSettingsForBuildTarget(BuildTargetGroup.PS5));
            if (!buildTargetSettings.HasSettingsForBuildTarget(BuildTargetGroup.PS5))
                buildTargetSettings.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.PS5);
            var settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.PS5);
            Assert.False(settings == null);


        }

        [Test]
        public void CanCreateNewManagerSettingsForMissingBuildTargetSettings()
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget buildTargetSettings);
            Assert.False(buildTargetSettings == null);

            Assert.IsFalse(buildTargetSettings.HasManagerSettingsForBuildTarget(BuildTargetGroup.PS5));
            if (!buildTargetSettings.HasManagerSettingsForBuildTarget(BuildTargetGroup.PS5))
                buildTargetSettings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.PS5);
            var settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.PS5);
            Assert.IsNotNull(settings);
            Assert.IsNotNull(settings.Manager);
        }
    }

    [TestFixture(0)] // Test case where no loaders exist in the list
    [TestFixture(1)]
    [TestFixture(2)]
    [TestFixture(3)]
    [TestFixture(4)]
    class EditorActiveLoadersManipulationTests : XRSettingsManagerTestBase
    {
        public EditorActiveLoadersManipulationTests(int loaderCount)
            : base(loaderCount)
        {
        }

        [SetUp]
        public void SetupEditorActiveLoadersManipulationTest()
        {
            SetupBase();
        }

        [TearDown]
        public void TeardownEditorActiveLoadersManipulationTest()
        {
            TeardownBase();
        }

        [Test]
        public void CheckIfSetLegalLoaderListWorks()
        {
            Assert.IsNotNull(manager);

            var originalLoaders = new List<XRLoader>(manager.activeLoaders);

            // An empty loader list is valid
            Assert.True(manager.TrySetLoaders(new List<XRLoader>()));

            Assert.AreEqual(manager.activeLoaders.Count, 0);

            // All loaders should be registered
            Assert.True(manager.TrySetLoaders(originalLoaders));

            Assert.AreEqual(manager.activeLoaders.Count, originalLoaders.Count);
        }

        [Test]
        public void CheckThatEmptyingListRemovesAllRegisteredLoaders()
        {
            Assert.IsNotNull(manager);

            var originalLoaders = new List<XRLoader>(manager.activeLoaders);

            foreach (var loader in originalLoaders)
            {
                manager.TryRemoveLoader(loader);
                Assert.False(manager.registeredLoaders.Contains(loader));
            }
            Assert.IsEmpty(manager.registeredLoaders);

            foreach (var loader in originalLoaders)
            {
                manager.TryAddLoader(loader);
                Assert.True(manager.registeredLoaders.Contains(loader));
            }

            if (loaderCount > 0)
                Assert.IsNotEmpty(manager.registeredLoaders);

            Assert.True(manager.TrySetLoaders(new List<XRLoader>()));
            Assert.IsEmpty(manager.registeredLoaders);

            Assert.True(manager.TrySetLoaders(originalLoaders));
            foreach (var loader in originalLoaders)
            {
                Assert.True(manager.registeredLoaders.Contains(loader));
            }
        }

        [Test]
        public void CheckIfLegalAddSucceeds()
        {
            Assert.IsNotNull(manager);

            var originalLoaders = manager.activeLoaders;

            Assert.True(manager.TrySetLoaders(new List<XRLoader>()));

            for (var i = 0; i < originalLoaders.Count; ++i)
            {
                Assert.True(manager.TryAddLoader(originalLoaders[originalLoaders.Count - 1 - i]));
            }
        }

        [Test]
        public void CheckIfIllegalSetLoaderListFails()
        {
            Assert.IsNotNull(manager);

            var dl = ScriptableObject.CreateInstance(typeof(DummyLoader)) as DummyLoader;
            dl.id = -1;
            dl.shouldFail = true;

            var invalidList = new List<XRLoader>(manager.activeLoaders) { dl, dl };

            Assert.False(manager.TrySetLoaders(invalidList));
        }

        [Test]
        public void CheckIfRemoveAndReAddAtSameIndexWorks()
        {
            Assert.IsNotNull(manager);

            var originalList = manager.activeLoaders;

            for (var i = 0; i < originalList.Count; ++i)
            {
                var loader = originalList[i];
                Assert.True(manager.TryRemoveLoader(loader));
                Assert.True(manager.TryAddLoader(loader, i));

                Assert.AreEqual(originalList[i], manager.activeLoaders[i]);
            }
        }

        [Test]
        public void CheckIfAttemptToAddDuplicateLoadersFails()
        {
            Assert.IsNotNull(manager);

            var originalLoaders = manager.activeLoaders;
            foreach (var loader in originalLoaders)
            {
                Assert.False(manager.TryAddLoader(loader));
            }
        }

        [Test]
        public void CheckIfAttemptsToSetLoaderListThatContainDuplicatesFails()
        {
            Assert.IsNotNull(manager);

            if (loaderCount > 0)
            {
                var originalLoaders = manager.activeLoaders;
                var loadersWithDuplicates = new List<XRLoader>(manager.activeLoaders);

                loadersWithDuplicates.AddRange(originalLoaders);

                Assert.False(manager.TrySetLoaders(loadersWithDuplicates));
            }
        }
    }
}
