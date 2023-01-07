using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.OpenXR;

using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR.Features
{
    /// <summary>
    /// API for finding and managing feature sets for OpenXR.
    /// </summary>
    public static class OpenXRFeatureSetManager
    {
        [InitializeOnLoadMethod]
        static void InitializeOnLoad ()
        {
            void OnFirstUpdate()
            {
                EditorApplication.update -= OnFirstUpdate;
                InitializeFeatureSets();
            }

            OpenXRFeature.canSetFeatureDisabled = CanFeatureBeDisabled;
            EditorApplication.update += OnFirstUpdate;
        }

        /// <summary>
        /// Description of a known (either built-in or found) feature set.
        /// </summary>
        public class FeatureSet
        {
            /// <summary>
            /// Toggles the enabled state for this feature. Impacts the effect of <see cref="OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets"/>.
            /// If you change this value, you must call <see cref="OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets"/> to reflect that change on the actual feature sets.
            /// </summary>
            public bool isEnabled;

            /// <summary>
            /// The name that displays in the UI.
            /// </summary>
            public string name;

            /// <summary>
            /// Description of this feature set.
            /// </summary>
            public string description;

            /// <summary>
            /// The feature set id as defined in <see cref="OpenXRFeatureSetAttribute.FeatureSetId"/>.
            /// </summary>
            public string featureSetId;

            /// <summary>
            /// The text to be shown with the <see cref="downloadLink" />.
            /// </summary>
            public string downloadText;

            /// <summary>
            /// The URI string used to link to external documentation.
            /// </summary>
            public string downloadLink;

            /// <summary>
            /// The set of features that this feature set menages.
            /// </summary>
            public string[] featureIds;

            /// <summary>
            /// State that tracks whether this feature set is built in or was detected after the user installed it.
            /// </summary>
            public bool isInstalled;

            /// <summary>
            /// The set of required features that this feature set manages.
            /// </summary>
            public string[] requiredFeatureIds;

            /// <summary>
            /// The set of default features that this feature set manages.
            /// </summary>
            public string[] defaultFeatureIds;
        }

        internal class FeatureSetInfo : FeatureSet
        {
            public GUIContent uiName;

            public GUIContent uiLongName;

            public GUIContent uiDescription;

            public GUIContent helpIcon;

            /// <summary>
            /// Stores the previous known value of isEnabled as of the last call to `SetFeaturesFromEnabledFeatureSets`
            /// </summary>
            public bool wasEnabled;
        }

        static Dictionary<BuildTargetGroup, List<FeatureSetInfo>> s_AllFeatureSets = null;

        struct FeatureSetState
        {
            public HashSet<string> featureSetFeatureIds;
            public HashSet<string> requiredToEnabledFeatureIds;
            public HashSet<string> requiredToDisabledFeatureIds;
            public HashSet<string> defaultToEnabledFeatureIds;
        }

        static Dictionary<BuildTargetGroup, FeatureSetState> s_FeatureSetState = new Dictionary<BuildTargetGroup, FeatureSetState>();

        /// <summary>
        /// Event called when the feature set state has been changed.
        /// </summary>
        internal static event Action<BuildTargetGroup> onFeatureSetStateChanged;

        /// <summary>
        /// The current active build target. Used to handle callbacks from <see cref="OpenXRFeature.enabled" /> into
        /// <see cref="CanFeatureBeDisabled" /> to determine if a feature can currently be disabled.
        /// </summary>
        public static BuildTargetGroup activeBuildTarget = BuildTargetGroup.Unknown;

        static void FillKnownFeatureSets(bool addTestFeatureSet = false)
        {
            BuildTargetGroup[] buildTargetGroups = new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android };

            if (addTestFeatureSet)
            {
                foreach (var buildTargetGroup in buildTargetGroups)
                {
                    List<FeatureSetInfo> knownFeatureSets = new List<FeatureSetInfo>();
                    if (addTestFeatureSet)
                    {
                        knownFeatureSets.Add(new FeatureSetInfo(){
                            isEnabled = false,
                            name = "Known Test",
                            featureSetId = "com.unity.xr.test.featureset",
                            description = "Known Test feature group.",
                            downloadText = "Click here to go to the Unity main website.",
                            downloadLink = Constants.k_DocumentationURL,
                            uiName = new GUIContent("Known Test"),
                            uiDescription = new GUIContent("Known Test feature group."),
                            helpIcon = new GUIContent("", CommonContent.k_HelpIcon.image, "Click here to go to the Unity main website."),
                        });
                    }
                    s_AllFeatureSets.Add(buildTargetGroup, knownFeatureSets);
                }
            }

            foreach (var kvp in KnownFeatureSets.k_KnownFeatureSets)
            {
                List<FeatureSetInfo> knownFeatureSets;
                if (!s_AllFeatureSets.TryGetValue(kvp.Key, out knownFeatureSets))
                {
                    knownFeatureSets= new List<FeatureSetInfo>();
                    foreach (var featureSet in kvp.Value)
                    {
                        knownFeatureSets.Add(new FeatureSetInfo(){
                            isEnabled = false,
                            name = featureSet.name,
                            featureSetId = featureSet.featureSetId,
                            description = featureSet.description,
                            downloadText = featureSet.downloadText,
                            downloadLink = featureSet.downloadLink,
                            uiName = new GUIContent(featureSet.name),
                            uiLongName = new GUIContent($"{featureSet.name} feature group"),
                            uiDescription = new GUIContent(featureSet.description),
                            helpIcon = new GUIContent("", CommonContent.k_HelpIcon.image, featureSet.downloadText),
                        });
                    }
                    s_AllFeatureSets.Add(kvp.Key, knownFeatureSets);
                }
            }
        }

        /// <summary>
        /// Initializes all currently known feature sets. This will do two initialization passes:
        ///
        /// 1) Starts with all built in/known feature sets.
        /// 2) Queries the system for anything with an <see cref="OpenXRFeatureSetAttribute"/>
        /// defined on it and uses that to add/update the store of known feature sets.
        /// </summary>
        public static void InitializeFeatureSets()
        {
            InitializeFeatureSets(false);
        }

        internal static void InitializeFeatureSets(bool addTestFeatureSet)
        {
            if (s_AllFeatureSets == null)
                s_AllFeatureSets = new Dictionary<BuildTargetGroup, List<FeatureSetInfo>>();

            s_AllFeatureSets.Clear();

            FillKnownFeatureSets(addTestFeatureSet);

            var types = TypeCache.GetTypesWithAttribute<OpenXRFeatureSetAttribute>();
            foreach (var t in types)
            {
                var attrs = Attribute.GetCustomAttributes(t);
                foreach (var attr in attrs)
                {
                    var featureSetAttr = attr as OpenXRFeatureSetAttribute;
                    if (featureSetAttr == null)
                        continue;

                    if (!addTestFeatureSet && featureSetAttr.FeatureSetId.Contains("com.unity.xr.test.featureset"))
                        continue;

                    foreach (var buildTargetGroup in featureSetAttr.SupportedBuildTargets)
                    {
                        var key = buildTargetGroup;
                        if (!s_AllFeatureSets.ContainsKey(key))
                        {
                            s_AllFeatureSets.Add(key, new List<FeatureSetInfo>());
                        }

                        var isEnabled = OpenXREditorSettings.Instance.IsFeatureSetSelected(buildTargetGroup, featureSetAttr.FeatureSetId);
                        var newFeatureSet = new FeatureSetInfo(){
                            isEnabled = isEnabled,
                            wasEnabled = isEnabled,
                            name = featureSetAttr.UiName,
                            description = featureSetAttr.Description,
                            featureSetId = featureSetAttr.FeatureSetId,
                            downloadText = "",
                            downloadLink = "",
                            featureIds = featureSetAttr.FeatureIds,
                            requiredFeatureIds = featureSetAttr.RequiredFeatureIds,
                            defaultFeatureIds = featureSetAttr.DefaultFeatureIds,
                            isInstalled = true,
                            uiName = new GUIContent(featureSetAttr.UiName),
                            uiLongName = new GUIContent($"{featureSetAttr.UiName} feature group"),
                            uiDescription = new GUIContent(featureSetAttr.Description),
                            helpIcon = String.IsNullOrEmpty(featureSetAttr.Description) ? null : new GUIContent("", CommonContent.k_HelpIcon.image, featureSetAttr.Description),
                        };

                        bool foundFeatureSet = false;
                        var featureSets = s_AllFeatureSets[key];
                        for (int i = 0; i < featureSets.Count; i++)
                        {
                            if (String.Compare(featureSets[i].featureSetId, newFeatureSet.featureSetId, true) == 0)
                            {
                                foundFeatureSet = true;
                                featureSets[i] = newFeatureSet;
                                break;
                            }
                        }

                        if (!foundFeatureSet)
                            featureSets.Add(newFeatureSet);
                    }
                }
            }


            var buildTargetGroups = Enum.GetValues(typeof(BuildTargetGroup));
            foreach (BuildTargetGroup buildTargetGroup in buildTargetGroups)
            {
                FeatureSetState fsi;
                if (!s_FeatureSetState.TryGetValue(buildTargetGroup, out fsi))
                {
                    fsi = new FeatureSetState();
                    fsi.featureSetFeatureIds = new HashSet<string>();
                    fsi.requiredToEnabledFeatureIds = new HashSet<string>();
                    fsi.requiredToDisabledFeatureIds = new HashSet<string>();
                    fsi.defaultToEnabledFeatureIds = new HashSet<string>();

                    s_FeatureSetState.Add(buildTargetGroup, fsi);
                }

                SetFeaturesFromEnabledFeatureSets(buildTargetGroup);
            }
        }

        /// <summary>
        /// Returns the list of all <see cref="FeatureSet"/> for the given build target group.
        /// </summary>
        /// <param name="buildTargetGroup">The build target group to find the feature sets for.</param>
        /// <returns>List of <see cref="FeatureSet"/> or null if there is nothing that matches the given input.</returns>
        public static List<FeatureSet> FeatureSetsForBuildTarget(BuildTargetGroup buildTargetGroup)
        {
            return OpenXRFeatureSetManager.FeatureSetInfosForBuildTarget(buildTargetGroup).Select((fi) => fi as FeatureSet).ToList();
        }

        internal static List<FeatureSetInfo> FeatureSetInfosForBuildTarget(BuildTargetGroup buildTargetGroup)
        {
            List<FeatureSetInfo> ret = new List<FeatureSetInfo>();
            HashSet<FeatureSetInfo> featureSetsForBuildTargetGroup = new HashSet<FeatureSetInfo>();

            if (s_AllFeatureSets == null)
                InitializeFeatureSets();

            if (s_AllFeatureSets == null)
                return ret;

            foreach (var key in s_AllFeatureSets.Keys)
            {
                if (key == buildTargetGroup)
                {
                    featureSetsForBuildTargetGroup.UnionWith(s_AllFeatureSets[key]);
                }
            }

            ret.AddRange(featureSetsForBuildTargetGroup);
            return ret;
        }

        /// <summary>
        /// Returns a specific <see cref="FeatureSet"/> instance that matches the input.
        /// </summary>
        /// <param name="buildTargetGroup">The build target group this feature set supports.</param>
        /// <param name="featureSetId">The feature set id for the specific feature set being requested.</param>
        /// <returns>The matching <see cref="FeatureSet"/> or null.</returns>
        public static FeatureSet GetFeatureSetWithId(BuildTargetGroup buildTargetGroup, string featureSetId)
        {
            return GetFeatureSetInfoWithId(buildTargetGroup, featureSetId) as FeatureSet;
        }

        internal static FeatureSetInfo GetFeatureSetInfoWithId(BuildTargetGroup buildTargetGroup, string featureSetId)
        {
            var featureSets = FeatureSetInfosForBuildTarget(buildTargetGroup);
            if (featureSets != null)
            {
                foreach (var featureSet in featureSets)
                {
                    if (String.Compare(featureSet.featureSetId, featureSetId, true) == 0)
                        return featureSet;
                }
            }
            return null;
        }

        /// <summary>
        /// Given the current enabled state of the feature sets that match for a build target group, enable and disable the features associated with
        /// each feature set. Features that overlap sets of varying enabled states will maintain their enabled setting.
        /// </summary>
        /// <param name="buildTargetGroup">The build target group to process features sets for.</param>
        public static void SetFeaturesFromEnabledFeatureSets(BuildTargetGroup buildTargetGroup)
        {
            var featureSets = FeatureSetInfosForBuildTarget(buildTargetGroup);
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(buildTargetGroup);

            var fsi = s_FeatureSetState[buildTargetGroup];

            fsi.featureSetFeatureIds.Clear();
            foreach (var featureSet in featureSets)
            {
                if (featureSet.featureIds != null)
                    fsi.featureSetFeatureIds.UnionWith(featureSet.featureIds);
            }

            fsi.featureSetFeatureIds.Clear();
            fsi.requiredToEnabledFeatureIds.Clear();
            fsi.requiredToDisabledFeatureIds.Clear();
            fsi.defaultToEnabledFeatureIds.Clear();

            // Update the selected feature set states first
            foreach (var featureSet in featureSets)
            {
                if (featureSet.featureIds == null)
                    continue;

                OpenXREditorSettings.Instance.SetFeatureSetSelected(buildTargetGroup, featureSet.featureSetId, featureSet.isEnabled);
            }

            foreach (var featureSet in featureSets)
            {
                if (featureSet.featureIds == null)
                    continue;

                if (featureSet.isEnabled && featureSet.requiredFeatureIds != null)
                {
                    fsi.requiredToEnabledFeatureIds.UnionWith(featureSet.requiredFeatureIds);
                }

                if (featureSet.isEnabled != featureSet.wasEnabled)
                {
                    if (featureSet.isEnabled && featureSet.defaultFeatureIds != null)
                    {
                        fsi.defaultToEnabledFeatureIds.UnionWith(featureSet.defaultFeatureIds);
                    }
                    else if (!featureSet.isEnabled && featureSet.requiredFeatureIds != null)
                    {
                        fsi.requiredToDisabledFeatureIds.UnionWith(featureSet.requiredFeatureIds);
                    }

                    featureSet.wasEnabled = featureSet.isEnabled;
                }
            }

            foreach (var ext in extInfo.Features)
            {
                if (ext.Feature.enabled && fsi.requiredToDisabledFeatureIds.Contains(ext.Attribute.FeatureId))
                    ext.Feature.enabled = false;

                if (!ext.Feature.enabled && fsi.requiredToEnabledFeatureIds.Contains(ext.Attribute.FeatureId))
                    ext.Feature.enabled = true;

                if (!ext.Feature.enabled && fsi.defaultToEnabledFeatureIds.Contains(ext.Attribute.FeatureId))
                    ext.Feature.enabled = true;
            }

            s_FeatureSetState[buildTargetGroup] = fsi;

            onFeatureSetStateChanged?.Invoke(buildTargetGroup);
        }

        /// <summary>
        /// Tell the user if the feature with passed in feature id can be disabled.
        ///
        /// Uses the currently set <see cref="activeBuildTarget" /> to determine
        /// the BuildTargetGroup to use for checking against. If this value is not set, or
        /// set for a different build target than expected then return value may be incorrect.
        /// </summary>
        /// <param name="featureId">The feature id of the feature to check.</param>
        /// <returns>True if currently required by some feature set, false otherwise.</returns>
        internal static bool CanFeatureBeDisabled(string featureId)
        {
            return CanFeatureBeDisabled(featureId, activeBuildTarget);
        }

        /// <summary>
        /// Tell the user if the feature with passed in feature id can be disabled based on the build target group.
        /// </summary>
        /// <param name="featureId">The feature id of the feature to check.</param>
        /// <param name="buildTargetGroup">The build target group whose feature sets you are checking against.</param>
        /// <returns>True if currently required by some feature set, false otherwise.</returns>
        public static bool CanFeatureBeDisabled(string featureId, BuildTargetGroup buildTargetGroup)
        {
            if (!s_FeatureSetState.ContainsKey(buildTargetGroup))
                return true;

            var fsi = s_FeatureSetState[buildTargetGroup];
            return !fsi.requiredToEnabledFeatureIds.Contains(featureId);
        }

        internal static bool IsKnownFeatureSet(BuildTargetGroup buildTargetGroup, string featureSetId)
        {
            if (!KnownFeatureSets.k_KnownFeatureSets.ContainsKey(buildTargetGroup))
                return false;

            var featureSets = KnownFeatureSets.k_KnownFeatureSets[buildTargetGroup].
                Where((fs) => fs.featureSetId == featureSetId);

            return featureSets.Any();
        }
    }
}
