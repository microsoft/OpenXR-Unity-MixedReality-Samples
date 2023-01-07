using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.Mock;
using Assert = UnityEngine.Assertions.Assert;
using UnityEngine.XR.OpenXR.Tests;
using static UnityEditor.XR.OpenXR.Tests.OpenXREditorTestHelpers;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class FeatureTests : OpenXRLoaderSetup
    {
        [Test]
        public void EnableFeatures()
        {
            var featureInfos = GetFeatureInfos(BuildTargetGroup.Standalone);
            featureInfos.SingleOrDefault(ext => ext.Attribute.UiName == "Mock Runtime").Feature.enabled = true;
            Assert.IsTrue(MockRuntime.Instance.enabled);

            featureInfos.SingleOrDefault(ext => ext.Attribute.UiName == "Mock Runtime").Feature.enabled = false;
            Assert.IsFalse(MockRuntime.Instance.enabled);
        }

        [Test]
        public void CheckDefaultValues()
        {
            var featureInfos = GetFeatureInfos(BuildTargetGroup.Standalone);
            var mockExtInfo = featureInfos.SingleOrDefault(ext => ext.Attribute.UiName == "Mock Runtime");

            Assert.AreEqual(mockExtInfo.Attribute.UiName, mockExtInfo.Feature.nameUi);
            Assert.AreEqual(mockExtInfo.Attribute.Version, mockExtInfo.Feature.version);
            Assert.AreEqual(mockExtInfo.Attribute.OpenxrExtensionStrings, mockExtInfo.Feature.openxrExtensionStrings);
        }

        [Test]
        public void ValidationError()
        {
            bool errorFixed = false;

            // Set up a validation check ...
            MockRuntime.Instance.TestCallback = (s, o) =>
            {
                if (s == "GetValidationChecks")
                {
                    var validationChecks = o as List<OpenXRFeature.ValidationRule>;
                    validationChecks?.Add(new OpenXRFeature.ValidationRule
                    {
                        message = "Mock Validation Fail",
                        checkPredicate = () => errorFixed,
                        fixIt = () => errorFixed = true,
                        error = true
                    });
                }

                return true;
            };

            // Try to build the player ...
            var report = zBuildHookTests.BuildMockPlayer();

            // It will fail because of the above validation issue ...
            Assert.AreEqual(BuildResult.Failed, report.summary.result);

            // There's one validation issue ...
            var validationIssues = new List<OpenXRFeature.ValidationRule>();
            OpenXRProjectValidation.GetCurrentValidationIssues(validationIssues, BuildTargetGroup.Standalone);
            Assert.AreEqual(1, validationIssues.Count);

            // Fix it ...
            Assert.IsFalse(errorFixed);
            validationIssues[0].fixIt.Invoke();
            Assert.IsTrue(errorFixed);

            // Now there's zero validation issues ...
            OpenXRProjectValidation.GetCurrentValidationIssues(validationIssues, BuildTargetGroup.Standalone);
            Assert.AreEqual(0, validationIssues.Count);

            // Close the validation window ...
            OpenXRProjectValidationRulesSetup.CloseWindow();

        }

        [Test]
        public void GetFeatureByFeatureId()
        {
            var feature = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget(MockRuntime.featureId);
            Assert.IsNotNull(feature);
        }

        [Test]
        public void GetFeatureByUnknownFeatureIdReturnsNull()
        {
            var feature = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget("some.unknown.feature.id");
            Assert.IsNull(feature);

            feature = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget("");
            Assert.IsNull(feature);

            feature = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget(null);
            Assert.IsNull(feature);
        }

        [Test]
        public void GetFeaturesWithIdsReturnsFeatures()
        {
            var featureIds = new string[] { MockRuntime.featureId, EyeGazeInteraction.featureId };
            var features = FeatureHelpers.GetFeaturesWithIdsForActiveBuildTarget(featureIds);
            Assert.IsNotNull(features);
            Assert.IsTrue(features.Length == 2);

            var expectedTypes = new Type[]{ typeof(MockRuntime), typeof(EyeGazeInteraction)};
            foreach (var feature in features)
            {
                Assert.IsTrue(Array.IndexOf(expectedTypes, feature.GetType()) > -1);
            }
        }
    }
}