using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features;
using Assert = UnityEngine.Assertions.Assert;
using static UnityEditor.XR.OpenXR.Features.OpenXRFeatureSetManager;
using static UnityEditor.XR.OpenXR.Tests.OpenXREditorTestHelpers;
using UnityEngine.XR.OpenXR.Tests;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class FeatureSetTests : OpenXRLoaderSetup
    {
        const string k_KnownFeatureSetName = "Known Test";
        const string k_TestFeatureSetName = "Test Feature Set";
        const string k_TestFeatureSetNameHandAndEye = "Test Feature Set Hand and Eye Tracking";
        const string k_TestFeatureSetNameHand = "Test Feature Set Hand Tracking";
        const string k_TestFeatureSetDescription = "Test feature set";
        const string k_TestFeatureSetId = "com.unity.xr.test.featureset";
        const string k_TestFeatureSetIdTwo = "com.unity.xr.test.featureset2";
        const string k_TestFeatureSetIdThree = "com.unity.xr.test.featureset3";
        const string k_TestFeatureSetIdFour = "com.unity.xr.test.featureset4";

        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId
                },
            UiName = k_TestFeatureSetName,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetId,
            SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.Standalone },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId
                }
        )]
        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                },
            UiName = k_TestFeatureSetNameHandAndEye,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetIdTwo,
            SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.WSA },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                }
        )]
        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                },
            UiName = k_TestFeatureSetNameHand,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetIdThree,
            SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.WSA },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                }
        )]
        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                },
            UiName = k_TestFeatureSetName,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetId,
            SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.Android },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                }
        )]
        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                HTCViveControllerProfile.featureId,
                OculusTouchControllerProfile.featureId,
                },
            UiName = k_TestFeatureSetName,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetIdFour,
            SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.Standalone },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                },
            DefaultFeatureIds = new string[]{
                HTCViveControllerProfile.featureId,
            }
        )]
        sealed class TestFeatureSet { }


        public override void BeforeTest()
        {
            base.BeforeTest();
            OpenXRFeature.canSetFeatureDisabled = null;

            InitializeFeatureSets(true);
        }

        /// <summary>
        /// Initialize the feature sets by disabling all features sets and all features
        /// </summary>
        /// <param name="addTestFeatures">True to include test features</param>
        private void InitializeFeatureSets(bool addTestFeatures)
        {
            // Initialize first with test feature sets so we can make sure all feature sets are disabled
            OpenXRFeatureSetManager.InitializeFeatureSets(true);

            foreach (var buildTargetGroup in GetBuildTargetGroups())
            {
                // Disable all feature sets for this build target
                foreach (var featureSetInfo in FeatureSetInfosForBuildTarget(buildTargetGroup))
                {
                    featureSetInfo.isEnabled = false;
                    featureSetInfo.wasEnabled = false;
                    OpenXREditorSettings.Instance.SetFeatureSetSelected(buildTargetGroup, featureSetInfo.featureSetId, false);
                }

                // Disable all features for this build target
                var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(buildTargetGroup);
                foreach (var ext in extInfo.Features)
                {
                    ext.Feature.enabled = false;
                }
            }

            // If requested with no feature sets then reinitialize
            if(!addTestFeatures)
                OpenXRFeatureSetManager.InitializeFeatureSets(false);

            foreach(var buildTargetGroup in GetBuildTargetGroups())
            {
                // No feature sets should be enabled for any build target
                Assert.IsFalse(FeatureSetInfosForBuildTarget(buildTargetGroup).Any(f => f.isEnabled));

                // No features should be enabled
                AssertAllFeatures(buildTargetGroup, FeatureDisabled);
            }
        }

        public override void AfterTest()
        {
            base.AfterTest();
            OpenXRFeature.canSetFeatureDisabled = OpenXRFeatureSetManager.CanFeatureBeDisabled;
        }


        [Test]
        public void NoFeatureSetsReturnsEmptyList()
        {
            var featureSets = FeatureSetsForBuildTarget(BuildTargetGroup.iOS);
            Assert.AreEqual(0, featureSets.Count);
        }

        [Test]
        public void FoundExpectedFeatureSets()
        {
            InitializeFeatureSets(false);

            string[] expectedFeatureSets = new string[] {
                KnownFeatureSetsContent.s_MicrosoftFeatureSetId
            };

            var featureSets = FeatureSetsForBuildTarget(BuildTargetGroup.Standalone);
            Assert.IsNotNull(featureSets);
            Assert.AreEqual(expectedFeatureSets.Length, featureSets.Count);

            foreach (var featureSet in featureSets)
            {
                if (Array.IndexOf(expectedFeatureSets, featureSet.featureSetId) == -1)
                    Assert.IsTrue(false, $"Found unexpected feature set id {featureSet.featureSetId}!");
            }
        }

        [Test]
        public void UnknownFeatureSetRerturnNull()
        {
            // For this test we do not want the test features enabled so rerun the initilization with
            InitializeFeatureSets(false);
            var foundFeatureSet = GetFeatureSetWithId(BuildTargetGroup.iOS, k_TestFeatureSetId);
            Assert.IsNull(foundFeatureSet);
            foundFeatureSet = GetFeatureSetWithId(BuildTargetGroup.Standalone, "BAD FEATURE SET ID");
            Assert.IsNull(foundFeatureSet);
        }

        [Test]
        public void OverrideKnownTestFeatureSet()
        {
            var foundFeatureSet = GetFeatureSetWithId(BuildTargetGroup.Standalone, k_TestFeatureSetId);
            Assert.IsNotNull(foundFeatureSet);
            Assert.AreEqual(0, String.Compare(foundFeatureSet.name, k_TestFeatureSetName, true));
        }

        [Test]
        public void NonoverrideKnownTestFeatureSet()
        {
            var foundFeatureSet = GetFeatureSetWithId(BuildTargetGroup.WSA, k_TestFeatureSetId);
            Assert.IsNotNull(foundFeatureSet);
            Assert.AreEqual(0, String.Compare(foundFeatureSet.name, k_KnownFeatureSetName, true));
        }

        [Test]
        public void EnableFeatureSetEnablesFeatures()
        {
            EnableFeatureSet(BuildTargetGroup.Standalone, k_TestFeatureSetId, enabled:true);
            AssertOnlyFeatures(BuildTargetGroup.Standalone, new string[] { MicrosoftHandInteraction.featureId }, FeatureEnabled);
        }

        [Test]
        public void DisableFeatureSetDisabledFeatures()
        {
            // Enable the feature set and make sure only its features are enabled
            EnableFeatureSet(BuildTargetGroup.Standalone, k_TestFeatureSetId, true);
            AssertOnlyFeatures(BuildTargetGroup.Standalone, new string[] { MicrosoftHandInteraction.featureId }, FeatureEnabled);

            // Disable the feature set an make sure its features are disabled
            EnableFeatureSet(BuildTargetGroup.Standalone, k_TestFeatureSetId, false);
            AssertAllFeatures(BuildTargetGroup.Standalone, FeatureDisabled);
        }

        [Test]
        public void DisableSharedFeaturesLeaveSharedFeaturesEnabled()
        {
            // Ensable all WSA feature sets and make sure only the WSA feature set features are enabled
            EnableFeatureSets(BuildTargetGroup.WSA, enabled: true);

            AssertOnlyFeatures(BuildTargetGroup.WSA, new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
            }, FeatureEnabled);

            // Disable the feature seth with both features set as required
            EnableFeatureSet(BuildTargetGroup.WSA, k_TestFeatureSetIdTwo, enabled: false);
            AssertOnlyFeatures(BuildTargetGroup.WSA, new string[] {
                MicrosoftHandInteraction.featureId,
            }, FeatureEnabled);

            // Disable all WSA feature sets and make sure all features are disabled
            EnableFeatureSets(BuildTargetGroup.WSA, enabled: false);
            AssertAllFeatures(BuildTargetGroup.WSA, FeatureDisabled);
        }

        [Test]
        public void DisableSharedFeaturesLeaveOthersFeaturesEnabled()
        {
            string[] allFeatureIds = new string[]{
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                MicrosoftMotionControllerProfile.featureId,
            };

            string[] otherFeatureIds = new string[] {
                MicrosoftMotionControllerProfile.featureId,
            };

            EnableFeatureInfos(BuildTargetGroup.WSA, otherFeatureIds, true);

            // Enable the second feature set and ensure that only features in the `all` list are enabled
            var featureSetToEnable = GetFeatureSetInfoWithId(BuildTargetGroup.WSA, k_TestFeatureSetIdTwo);
            EnableFeatureSet(BuildTargetGroup.WSA, featureSetToEnable.featureSetId, true);
            AssertOnlyFeatures(BuildTargetGroup.WSA, allFeatureIds, FeatureEnabled);

            // Disable the second feature set and ensure only features in the `others` list are enabled
            var featureSetToDisable = GetFeatureSetInfoWithId(BuildTargetGroup.WSA, k_TestFeatureSetIdTwo);
            Assert.IsNotNull(featureSetToDisable);
            EnableFeatureSet(BuildTargetGroup.WSA, featureSetToDisable.featureSetId, enabled: false);
            AssertOnlyFeatures(BuildTargetGroup.WSA, otherFeatureIds, FeatureEnabled);
        }

        [Test]
        public void EnablingFeatureSetEnabledDefaultFeatures()
        {
            var foundFeatureSet = GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);
            EnableFeatureSet(BuildTargetGroup.Standalone, foundFeatureSet.featureSetId, true);

            // Ensure that only the non-optional features are enabled
            AssertOnlyFeatures(BuildTargetGroup.Standalone, foundFeatureSet.featureIds, (f) => f.Feature.enabled == !FeatureIsOptional(foundFeatureSet, f));
        }

        [Test]
        public void EnablingFeatureSetLeavesOptionFeaturesEnabled()
        {
            // Enable the feature set
            var foundFeatureSet = GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);
            EnableFeatureSet(BuildTargetGroup.Standalone, foundFeatureSet.featureSetId, true);

            // Ensure the Optional features are all disabled
            AssertAllFeatures(BuildTargetGroup.Standalone, (f) => !FeatureIsOptional(foundFeatureSet, f) || !f.Feature.enabled);

            // Disable the feature set and ensure the optional features are disabled
            EnableFeatureSet(BuildTargetGroup.Standalone, foundFeatureSet.featureSetId, false);
            AssertAllFeatures(BuildTargetGroup.Standalone, (f) => !FeatureIsOptional(foundFeatureSet, f) || !f.Feature.enabled);

            // Enable the optional features and the feature set and ensure the optional features are still enabled
            EnableFeatureInfos(BuildTargetGroup.Standalone, true, (f) => FeatureIsOptional(foundFeatureSet, f));
            EnableFeatureSet(BuildTargetGroup.Standalone, foundFeatureSet.featureSetId, enabled: true);
            AssertAllFeatures(BuildTargetGroup.Standalone, (f) => !FeatureIsOptional(foundFeatureSet, f) || f.Feature.enabled);

            // Enable the feature set again and make sure the optional features are still enabled
            EnableFeatureSet(BuildTargetGroup.Standalone, foundFeatureSet.featureSetId, enabled: true);
            AssertAllFeatures(BuildTargetGroup.Standalone, (f) => !FeatureIsOptional(foundFeatureSet, f) || f.Feature.enabled);
        }

        [Test]
        public void DisablingFeatureSetLeavesDefaultFeaturesEnabled()
        {
            var foundFeatureSet = GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);
            EnableFeatureSet(BuildTargetGroup.Standalone, foundFeatureSet.featureSetId, true);

            // Ensure that the only enabled features are the non optional features
            AssertAllFeatures(BuildTargetGroup.Standalone, foundFeatureSet.featureIds, (f) => f.Feature.enabled == !FeatureIsOptional(foundFeatureSet, f));

            // Disabling the feature set should disable the required components but not the default ones
            EnableFeatureSet(BuildTargetGroup.Standalone, foundFeatureSet.featureSetId, enabled: false);
            AssertAllFeatures(BuildTargetGroup.Standalone, foundFeatureSet.requiredFeatureIds, FeatureDisabled);
            AssertAllFeatures(BuildTargetGroup.Standalone, foundFeatureSet.defaultFeatureIds, FeatureEnabled);
        }

        [Test]
        public void DisablingFeatureSetLeavesDisabledDefaultFeaturesDisabled()
        {
            var buildTargetGroup = BuildTargetGroup.Standalone;
            var foundFeatureSet = GetFeatureSetInfoWithId(buildTargetGroup, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);

            EnableFeatureSet(buildTargetGroup, foundFeatureSet.featureSetId, enabled: true);

            // Ensure that only the non optional features are enabled
            AssertOnlyFeatures(buildTargetGroup, foundFeatureSet.featureIds, (f) => f.Feature.enabled == !FeatureIsOptional(foundFeatureSet, f));

            // Disable all features in the default feature list
            EnableFeatureInfos(buildTargetGroup, foundFeatureSet.defaultFeatureIds, enable: false);
            AssertAllFeatures(buildTargetGroup, foundFeatureSet.defaultFeatureIds, FeatureDisabled);

            // Ensure that all features in the required list are enabled and that all features in the default features list are disabled
            AssertAllFeatures(buildTargetGroup, foundFeatureSet.requiredFeatureIds, FeatureEnabled);
        }

        [Test]
        public void CanNotChangeEnabledStateOfRequiredFeature()
        {
            OpenXRFeatureSetManager.activeBuildTarget = BuildTargetGroup.Standalone;

            var foundFeatureSet = GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);

            var featureInfos = GetFeatureInfos(BuildTargetGroup.Standalone, foundFeatureSet.requiredFeatureIds);
            foreach (var featureInfo in featureInfos)
            {
                AssertFeatureEnabled(featureInfo, false);
                featureInfo.Feature.enabled = true;
                AssertFeatureEnabled(featureInfo, true);
                featureInfo.Feature.enabled = false;
                AssertFeatureEnabled(featureInfo, false);
            }

            EnableFeatureSet(BuildTargetGroup.Standalone, foundFeatureSet.featureSetId, enabled: true);

            OpenXRFeature.canSetFeatureDisabled = OpenXRFeatureSetManager.CanFeatureBeDisabled;

            foreach (var featureInfo in featureInfos)
            {
                AssertFeatureEnabled(featureInfo, true);
                featureInfo.Feature.enabled = false;
                AssertFeatureEnabled(featureInfo, true);
            }

            EnableFeatureSet(BuildTargetGroup.Standalone, foundFeatureSet.featureSetId, enabled: false);

            foreach (var featureInfo in featureInfos)
            {
                AssertFeatureEnabled(featureInfo, false);
                featureInfo.Feature.enabled = true;
                AssertFeatureEnabled(featureInfo, true);
                featureInfo.Feature.enabled = false;
                AssertFeatureEnabled(featureInfo, false);
            }

            OpenXRFeatureSetManager.activeBuildTarget = BuildTargetGroup.Unknown;
        }
    }
}