using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_2021_2_OR_NEWER
using PrefabStageUtility = UnityEditor.SceneManagement.PrefabStageUtility;
#else
using PrefabStageUtility = UnityEditor.Experimental.SceneManagement.PrefabStageUtility;
#endif

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Class that contains <see cref="BuildValidationRule"/>
    /// rules for testing against package correctness
    /// </summary>
    [InitializeOnLoad]
    public static class BuildValidator
    {
        static Dictionary<BuildTargetGroup, List<BuildValidationRule>> s_PlatformRules =
            new Dictionary<BuildTargetGroup, List<BuildValidationRule>>();

        internal static Dictionary<BuildTargetGroup, List<BuildValidationRule>> PlatformRules => s_PlatformRules;

        static BuildValidator()
        {
            // Used implicitly. Called when Unity launches the Editor / Player or recompiles scripts.
        }

        /// <summary>
        /// Adds a set of <see cref="BuildValidationRule"/> for a given platform
        /// </summary>
        /// <param name="group">The platform this rule will be added to.</param>
        /// <param name="rules">The rules to add to the platform.</param>
        public static void AddRules(BuildTargetGroup group, IEnumerable<BuildValidationRule> rules)
        {
            if (s_PlatformRules.TryGetValue(group, out var groupRules))
                groupRules.AddRange(rules);
            else
            {
                groupRules = new List<BuildValidationRule>(rules);
                s_PlatformRules.Add(group, groupRules);
            }
        }

        internal static void GetCurrentValidationIssues(HashSet<BuildValidationRule> failures,
            BuildTargetGroup buildTargetGroup)
        {
            failures.Clear();
            if (!s_PlatformRules.TryGetValue(buildTargetGroup, out var rules))
                return;

            var inPrefabStage = PrefabStageUtility.GetCurrentPrefabStage() != null;
            foreach (var validation in rules)
            {
                // If current scene is prefab isolation do not run scene validation
                if (inPrefabStage && validation.SceneOnlyValidation)
                    continue;

                if (validation.CheckPredicate == null)
                    failures.Add(validation);
                else if (validation.IsRuleEnabled.Invoke() && !validation.CheckPredicate.Invoke())
                    failures.Add(validation);
            }
        }

        /// <summary>
        /// Checks if a given set of types are present in the current open scene
        /// </summary>
        /// <param name="subscribers">The set of types to check on scenes.</param>
        /// <returns>Returns True if any of the types have been found in the current open scene.</returns>
        public static bool HasTypesInSceneSetup(IEnumerable<Type> subscribers)
        {
            if (Application.isPlaying)
                return false;
            
            foreach (var sceneSetup in EditorSceneManager.GetSceneManagerSetup())
            {
                if (!sceneSetup.isLoaded)
                    continue;

                var scene = SceneManager.GetSceneByPath(sceneSetup.path);

                foreach (var go in scene.GetRootGameObjects())
                {
                    if (subscribers.Any(subscriber => go.GetComponentInChildren(subscriber, true)))
                        return true;
                }
            }

            return false;
        }

        internal static bool HasRulesForPlatform(BuildTargetGroup buildTarget)
        {
            return s_PlatformRules.TryGetValue(buildTarget, out _);
        }
    }
}
