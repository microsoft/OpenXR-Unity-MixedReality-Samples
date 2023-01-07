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
    class PackageInitSettingsTests
    {
        string oldAssetName;
        string oldFolderName;
        string oldSettingsPath;
        string oldPackageInitPath;

        string tempFolderName;

        [SetUp]
        public void SetUp()
        {
            oldAssetName = XRPackageInitializationSettings.s_ProjectSettingsAssetName;
            oldFolderName = XRPackageInitializationSettings.s_ProjectSettingsFolder;
            oldSettingsPath = XRPackageInitializationSettings.s_ProjectSettingsPath;
            oldPackageInitPath = XRPackageInitializationSettings.s_PackageInitPath;

            tempFolderName = $"Temp{DateTime.Now.Ticks}";

            XRPackageInitializationSettings.s_ProjectSettingsAssetName = "test.asset";
            XRPackageInitializationSettings.s_ProjectSettingsFolder = $"{tempFolderName}/ProjectSettings";
            XRPackageInitializationSettings.s_ProjectSettingsPath = "";
            XRPackageInitializationSettings.s_PackageInitPath = "";

        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset($"Assets/{tempFolderName}");
            XRPackageInitializationSettings.s_ProjectSettingsAssetName = oldAssetName;
            XRPackageInitializationSettings.s_ProjectSettingsFolder = oldFolderName;
            XRPackageInitializationSettings.s_ProjectSettingsPath = oldSettingsPath;
            XRPackageInitializationSettings.s_PackageInitPath = oldPackageInitPath;
        }

        [Test]
        public void TestSettingsSave()
        {
            Assert.IsTrue(String.IsNullOrEmpty(XRPackageInitializationSettings.s_ProjectSettingsPath));
            Assert.IsTrue(String.IsNullOrEmpty(XRPackageInitializationSettings.s_PackageInitPath));

            Assert.IsFalse(Directory.Exists(XRPackageInitializationSettings.s_ProjectSettingsPath));
            Assert.IsFalse(File.Exists(XRPackageInitializationSettings.s_PackageInitPath));

            XRPackageInitializationSettings.Instance.AddSettings("TestTestTest");
            Assert.IsTrue(XRPackageInitializationSettings.Instance.HasSettings("TestTestTest"));

            XRPackageInitializationSettings.Instance.SaveSettings();

            Assert.IsTrue(Directory.Exists(XRPackageInitializationSettings.s_ProjectSettingsPath));
            Assert.IsTrue(File.Exists(XRPackageInitializationSettings.s_PackageInitPath));

            XRPackageInitializationSettings.Instance.AddSettings("AgainTestTestTest");

            XRPackageInitializationSettings.Instance.LoadSettings();

            Assert.IsFalse(XRPackageInitializationSettings.Instance.HasSettings("AgainTestTestTest"));
            Assert.IsTrue(XRPackageInitializationSettings.Instance.HasSettings("TestTestTest"));
        }
    }

}
