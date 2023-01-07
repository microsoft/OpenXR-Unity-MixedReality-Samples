using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.Build.Reporting;
using UnityEditor.VersionControl;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.Mock;
using Assert = UnityEngine.Assertions.Assert;
using UnityEngine.XR.OpenXR.Tests;
using static UnityEditor.XR.OpenXR.Tests.OpenXREditorTestHelpers;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class FeatureModifyingTests : OpenXRLoaderSetup
    {
        // Override AfterTest to prevent OpenXRSettings.Instance.features from getting reset.
        // This test suite destroys and restores OpenXRSettings.Instance.features manually.
        public override void AfterTest()
        {
        }

        [Test]
        public void DuplicateSettingAssetTest()
        {
            // Local OpenXR filepath that contains the test OpenXR Package Settings.asset
            string openXRFolder = Path.GetFullPath("Packages/com.unity.xr.openxr");

            string settingsFilePath = OpenXRPackageSettings.OpenXRPackageSettingsAssetPath();
            string metaFilePath = settingsFilePath + ".meta";

            string testAssetName = "OpenXR Package Settings With Duplicates.asset";
            string testAssetPath = Path.Combine(openXRFolder, "Tests", "Editor", testAssetName);
            string testMetaAssetPath = testAssetPath + ".meta";

            // Copy in the test files (the files with duplicate settings)
            File.Delete(settingsFilePath);
            File.Delete(metaFilePath);
            File.Copy(testAssetPath, settingsFilePath);
            File.Copy(testMetaAssetPath, metaFilePath);

            // Verify that we detect duplicates in the test file.
            Assert.IsFalse(OpenXRProjectValidation.AssetHasNoDuplicates(), "The duplicate settings on the bad asset should be detected.");

            // Regenerate the asset (as if the user clicks on the Fix button in the validation window)
            OpenXRProjectValidation.RegenerateXRPackageSettingsAsset();

            // Verify that there are no duplicates in the settings file now.
            Assert.IsTrue(OpenXRProjectValidation.AssetHasNoDuplicates(), "After regenerating the asset, the duplicate settings should be removed.");
        }
    }
}