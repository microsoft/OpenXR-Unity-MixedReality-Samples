using System;

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR
{
#if UNITY_EDITOR
    public interface IPackageSettings
    {
        OpenXRSettings GetSettingsForBuildTargetGroup(UnityEditor.BuildTargetGroup buildTargetGroup);
        string GetActiveLoaderLibraryPath();

        /// <summary>
        /// Returns all features of a given type from all existing build target groups.
        /// </summary>
        /// <typeparam name="T">Feature type</typeparam>
        /// <returns>All known features of the given type within the package settings</returns>
        public IEnumerable<(BuildTargetGroup buildTargetGroup, T feature)> GetFeatures<T>() where T : OpenXRFeature;

        internal void RefreshFeatureSets();

        internal string PackageSettingsAssetPath();
    }
#endif


    /// <summary>
    /// Build time settings for OpenXR. These are serialized and available at runtime.
    /// </summary>
    [Serializable]
    public partial class OpenXRSettings : ScriptableObject
    {
#if UNITY_EDITOR
        internal bool versionChanged = false;
#else
        private static OpenXRSettings s_RuntimeInstance = null;

        private void Awake()
        {
            s_RuntimeInstance = this;
        }
#endif
        internal void ApplySettings()
        {
            ApplyRenderSettings();
        }

        private static OpenXRSettings GetInstance(bool useActiveBuildTarget)
        {
            OpenXRSettings settings = null;
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            settings = GetSettingsForBuildTargetGroup(useActiveBuildTarget ?
                BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget) :
                BuildTargetGroup.Standalone);
#else
            settings = s_RuntimeInstance;
            if (settings == null)
                settings = ScriptableObject.CreateInstance<OpenXRSettings>();
#endif

            return settings;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Returns the Settings object for the given BuildTargetGroup
        /// </summary>
        /// <param name="buildTargetGroup">BuildTargetGroup to request settings for</param>
        /// <returns>OpenXRSettings object for the given build target group</returns>
        public static OpenXRSettings GetSettingsForBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            var packageSettings = GetPackageSettings();
            if(null == packageSettings)
                return null;

            return packageSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
        }

        internal static IPackageSettings GetPackageSettings()
        {
            if (EditorBuildSettings.TryGetConfigObject<UnityEngine.Object>(Constants.k_SettingsKey, out var obj) && (obj is IPackageSettings packageSettings))
                return packageSettings;

            return null;
        }
#endif

        /// <summary>
        /// Accessor to OpenXR build time settings.
        ///
        /// In the Unity Editor, this returns the settings for the active build target group.
        /// </summary>
        public static OpenXRSettings ActiveBuildTargetInstance => GetInstance(true);

        /// <summary>
        /// Accessor to OpenXR build time settings.
        ///
        /// In the Unity Editor, this returns the settings for the Standalone build target group.
        /// </summary>
        public static OpenXRSettings Instance => GetInstance(false);
    }
}
