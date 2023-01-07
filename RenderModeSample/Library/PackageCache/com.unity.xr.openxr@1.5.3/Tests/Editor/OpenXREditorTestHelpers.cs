using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.XR.OpenXR.Features;
using static UnityEditor.XR.OpenXR.Features.OpenXRFeatureSetManager;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal static class OpenXREditorTestHelpers
    {
        public delegate bool FeatureInfoPredicate(FeatureHelpersInternal.FeatureInfo featureInfo);

        public static bool FeatureEnabled(FeatureHelpersInternal.FeatureInfo featureInfo) => featureInfo.Feature.enabled;
        public static bool FeatureDisabled(FeatureHelpersInternal.FeatureInfo featureInfo) => !featureInfo.Feature.enabled;

        private static Dictionary<BuildTargetGroup, FeatureHelpersInternal.FeatureInfo[]> s_FeatureInfos;

        private static BuildTargetGroup[] s_BuildTargetGroups =
            ((BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup))).Distinct().ToArray();

        /// <summary>
        /// Return the distinct list of build target groups to test
        /// </summary>
        public static BuildTargetGroup[] GetBuildTargetGroups() => s_BuildTargetGroups;

        /// <summary>
        /// Clear the FeatureInfo cache
        /// </summary>
        public static void ClearFeatureInfos ()
        {
            s_FeatureInfos = null;
        }

        /// <summary>
        /// Helper function to retrieve the cached FeatureInfos for a given build target group
        /// </summary>
        /// <param name="buildTargetGroup">Build target group</param>
        /// <returns>Array of FeatureInfos for the build target group</returns>
        public static FeatureHelpersInternal.FeatureInfo[] GetFeatureInfos(BuildTargetGroup buildTargetGroup)
        {
            if (null == s_FeatureInfos)
                s_FeatureInfos = new Dictionary<BuildTargetGroup, FeatureHelpersInternal.FeatureInfo[]>();

            if (!s_FeatureInfos.TryGetValue(buildTargetGroup, out var featureInfos))
            {
                featureInfos = FeatureHelpersInternal.GetAllFeatureInfo(buildTargetGroup).Features.ToArray();
                s_FeatureInfos[buildTargetGroup] = featureInfos;
            }

            return featureInfos;
        }

        /// <summary>
        /// Helper function to retrieve a subset of FeatureInfos in <paramref name="buildTargetGroup"/>.
        /// </summary>
        /// <param name="buildTargetGroup">Build target group</param>
        /// <param name="featureIds">Specific feature identifiers to retrieve</param>
        /// <returns>Array of FeatureInfos matching request</returns>
        public static FeatureHelpersInternal.FeatureInfo[] GetFeatureInfos(BuildTargetGroup buildTargetGroup, string[] featureIds)
        {
            return GetFeatureInfos(buildTargetGroup).Where(f => featureIds.Contains(f.Attribute.FeatureId)).ToArray();
        }


        /// <summary>
        /// Enable or Disable a feature set
        /// </summary>
        /// <param name="buildTargetGroup">Build target group containing feature set</param>
        /// <param name="featureSetId">Identifier of feature</param>
        /// <param name="enabled">True to enable, false to disable</param>
        /// <param name="changed">True if the FeatureSet enabled state should be considered changed</param>
        /// <param name="commit">True if the FeatureSet enabled state should be automatically commmitted with `SetFeaturesFromEnabledFeatureSets`</param>
        public static void EnableFeatureSet(BuildTargetGroup buildTargetGroup, string featureSetId, bool enabled = true, bool changed = true, bool commit = true)
        {
            var featureSetInfo = GetFeatureSetInfoWithId(buildTargetGroup, featureSetId);
            Assert.IsNotNull(featureSetInfo, $"FeatureSetInfo '{featureSetId}' not found ");

            featureSetInfo.isEnabled = enabled;
            featureSetInfo.wasEnabled = changed ? !enabled : enabled;

            if (commit)
                SetFeaturesFromEnabledFeatureSets(buildTargetGroup);
        }

        /// <summary>
        /// Enable/Disable all feature sets for <paramref name="buildTargetGroup"/>
        /// </summary>
        /// <param name="buildTargetGroup">Build Target Group</param>
        /// <param name="enabled">Enabled state</param>
        /// <param name="changed">True if the feature set should be marked as changed, false if not</param>
        public static void EnableFeatureSets(BuildTargetGroup buildTargetGroup, bool enabled, bool changed = true, bool commit = true)
        {
            foreach (var featureSetInfo in FeatureSetInfosForBuildTarget(buildTargetGroup))
            {
                featureSetInfo.isEnabled = enabled;
                featureSetInfo.wasEnabled = changed ? !enabled : enabled;
            }

            if (commit)
                SetFeaturesFromEnabledFeatureSets(buildTargetGroup);
        }

        public static void EnableFeatureInfos(BuildTargetGroup buildTargetGroup, string[] featureIds, bool enable, FeatureInfoPredicate predicate = null)
        {
            foreach (var featureInfo in GetFeatureInfos(buildTargetGroup, featureIds))
            {
                if (null == predicate || predicate(featureInfo))
                    featureInfo.Feature.enabled = enable;
            }
        }

        public static void EnableFeatureInfos(BuildTargetGroup buildTargetGroup, bool enable, FeatureInfoPredicate predicate = null)
        {
            foreach (var featureInfo in GetFeatureInfos(buildTargetGroup))
            {
                if (null == predicate || predicate(featureInfo))
                    featureInfo.Feature.enabled = enable;
            }
        }

        /// <summary>
        /// Builds a comma separated list of features that pass the <paramref name="check"/> within <paramref name="featureInfos"/>
        /// </summary>
        /// <param name="featureInfos">Feature infos</param>
        /// <param name="check">Delegate used to check status</param>
        /// <param name="exclude">Optional list of feature identifiers exclude</param>
        /// <returns>Comma separated list of features</returns>
        public static string FeaturesToString(FeatureHelpersInternal.FeatureInfo[] featureInfos, FeatureInfoPredicate check, string[] exclude = null)
        {
            return string.Join(",", featureInfos
                .Where(f => (exclude == null || !exclude.Contains(f.Attribute.FeatureId)) && check(f))
                .Select(f => f.Attribute.FeatureId));
        }

        /// <summary>
        /// Builds a comma separated list of features from <paramref name="featureIds"/> in <paramref name="buildTargetGroup"/> that pass the <paramref name="check"/>
        /// </summary>
        /// <param name="buildTargetGroup">Build Target Group</param>
        /// <param name="featureIds">Feature ids to include</param>
        /// <param name="check">Delegate used to check status</param>
        /// <returns>Comma separated list of features</returns>
        public static string FeaturesToString(BuildTargetGroup buildTargetGroup, string[] featureIds, FeatureInfoPredicate check)
        {
            return FeaturesToString(GetFeatureInfos(buildTargetGroup).Where(f => featureIds.Contains(f.Attribute.FeatureId)).ToArray(), check);
        }

        /// <summary>
        /// Builds a comma separated list of features identifier that pass the <paramref name="check"/> within <paramref name="buildTargetGroup"/>
        /// </summary>
        /// <param name="buildTargetGroup">Build Target Group</param>
        /// <param name="check">Delegate used to check status</param>
        /// <param name="exclude">Optional list of feature identifiers to exclude from the list</param>
        /// <returns>Comma separated list of features</returns>
        public static string FeaturesToString(BuildTargetGroup buildTargetGroup, FeatureInfoPredicate check, string[] exclude = null)
        {
            return FeaturesToString(GetFeatureInfos(buildTargetGroup), check, exclude: exclude);
        }

        /// <summary>
        /// Checks if all features for the <paramref name="buildTargetGroup"/> pass the <paramref name="check"/>.
        /// </summary>
        /// <param name="buildTargetGroup">Build Target Group</param>
        /// <param name="check">Delegate used to check status</param>
        /// <returns>True if the check passes, false if not</returns>
        public static bool CheckAllFeatures(BuildTargetGroup buildTargetGroup, FeatureInfoPredicate check)
        {
            return GetFeatureInfos(buildTargetGroup).All(f => check(f));
        }

        /// <summary>
        /// Check that all features in <paramref name="featureIds"/> in <paramref name="buildTargetGroup"/> match the pass the <paramref name="check"/>.
        /// </summary>
        /// <param name="buildTargetGroup">Build target group</param>
        /// <param name="featureIds">Features to check</param>
        /// <param name="check">Delegate used to check status</param>
        /// <returns>True if the check passes, false if not</returns>
        public static bool CheckAllFeatures(BuildTargetGroup buildTargetGroup, string[] featureIds, FeatureInfoPredicate check)
        {
            return GetFeatureInfos(buildTargetGroup).All(f => !featureIds.Contains(f.Attribute.FeatureId) || check(f));
        }

        /// <summary>
        /// Check that only features in <paramref name="featureIds"/> for <paramref name="buildTargetGroup"/> pass the <paramref name="check"/>.
        /// </summary>
        /// <param name="buildTargetGroup">Build target group</param>
        /// <param name="featureIds">Features to check</param>
        /// <param name="check">Delegate used to check a FeatureInfo</param>
        /// <returns>True if the check passes, false if not</returns>
        public static bool CheckOnlyFeatures(BuildTargetGroup buildTargetGroup, string[] featureIds, FeatureInfoPredicate check)
        {
            return GetFeatureInfos(buildTargetGroup).All(f => featureIds.Contains(f.Attribute.FeatureId) == check(f));
        }

        private static string AssertFeaturesMessage(string features) =>
            $"The following features failed the check: {features}";

        public static void AssertOnlyFeatures(BuildTargetGroup buildTargetGroup, string[] featureIds, FeatureInfoPredicate check)
        {
            Assert.IsTrue(
                CheckOnlyFeatures(buildTargetGroup, featureIds, check),
                AssertFeaturesMessage(FeaturesToString(buildTargetGroup, featureIds, (f) => !check(f))));
        }

        public static void AssertAllFeatures(BuildTargetGroup buildTargetGroup, string[] featureIds, FeatureInfoPredicate check)
        {
            Assert.IsTrue(
                CheckAllFeatures(buildTargetGroup, featureIds, check),
                AssertFeaturesMessage(FeaturesToString(buildTargetGroup, featureIds, (f) => !check(f))));
        }

        public static void AssertAllFeatures(BuildTargetGroup buildTargetGroup, FeatureInfoPredicate check)
        {
            Assert.IsTrue(
                CheckAllFeatures(buildTargetGroup, check),
                AssertFeaturesMessage(FeaturesToString(buildTargetGroup, (f) => !check(f))));
        }

        public static void AssertFeatureEnabled(FeatureHelpersInternal.FeatureInfo featureInfo, bool enabled = true)
        {
            Assert.IsTrue(featureInfo.Feature.enabled == enabled, $"{featureInfo.Attribute.FeatureId} should be {(enabled ? "enabled" : "disabled")}");
        }

        public static bool FeatureIsOptional(FeatureSetInfo featureSet, FeatureHelpersInternal.FeatureInfo feature)
        {
            return Array.IndexOf(featureSet.featureIds, feature.Attribute.FeatureId) > -1 &&
                (featureSet.requiredFeatureIds == null || Array.IndexOf(featureSet.requiredFeatureIds, feature.Attribute.FeatureId) == -1) &&
                (featureSet.defaultFeatureIds == null || Array.IndexOf(featureSet.defaultFeatureIds, feature.Attribute.FeatureId) == -1);
        }
    }
}