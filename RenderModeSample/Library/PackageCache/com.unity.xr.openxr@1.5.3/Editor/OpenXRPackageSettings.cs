using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.XR.OpenXR.Features;
using UnityEditor.Build;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR
{
    [XRConfigurationData("OpenXR", Constants.k_SettingsKey)]
    internal class OpenXRPackageSettings : ScriptableObject, ISerializationCallbackReceiver, IPackageSettings
    {
        [SerializeField]
        List<BuildTargetGroup> Keys = new List<BuildTargetGroup>();
        [SerializeField]
        List<OpenXRSettings> Values = new List<OpenXRSettings>();

        Dictionary<BuildTargetGroup, OpenXRSettings> Settings = new Dictionary<BuildTargetGroup, OpenXRSettings>();


        public static OpenXRPackageSettings Instance
        {
            get
            {
                OpenXRPackageSettings ret = null;
                EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out ret);
                if (ret == null)
                {
                    string path = OpenXRPackageSettingsAssetPath();
                    if (!path.Equals(String.Empty))
                    {
                        ret = AssetDatabase.LoadAssetAtPath(path, typeof(OpenXRPackageSettings)) as OpenXRPackageSettings;
                        if (ret != null)
                        {
                            EditorBuildSettings.AddConfigObject(Constants.k_SettingsKey, ret, true);
                        }

                        // Can't modify EditorBuildSettings once a build has already started - it won't get picked up
                        // Not sure how to gracefully fix this, for now fail the build so there are now surprises.
                        if (BuildPipeline.isBuildingPlayer)
                            throw new BuildFailedException(
                                "OpenXR Settings found in project but not yet loaded.  Please build again.");
                    }
                }
                return ret;
            }
        }

        public static OpenXRPackageSettings GetOrCreateInstance()
        {
            if (Instance != null)
                return Instance;

            OpenXRPackageSettings settings = ScriptableObject.CreateInstance<OpenXRPackageSettings>();
            if (settings != null)
            {
                string newAssetName = String.Format(s_PackageSettingsAssetName);
                string assetPath = GetAssetPathForComponents(s_PackageSettingsDefaultSettingsPath);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    assetPath = Path.Combine(assetPath, newAssetName);
                    AssetDatabase.CreateAsset(settings, assetPath);
                    EditorBuildSettings.AddConfigObject(Constants.k_SettingsKey, settings, true);
                }
            }
            return settings;
        }

        internal static readonly string s_PackageSettingsAssetName = "OpenXR Package Settings.asset";

        internal static readonly string[] s_PackageSettingsDefaultSettingsPath = {"XR","Settings"};

        string IPackageSettings.PackageSettingsAssetPath()
        {
            return OpenXRPackageSettingsAssetPath();
        }

        internal static string OpenXRPackageSettingsAssetPath()
        {
            return Path.Combine(GetAssetPathForComponents(s_PackageSettingsDefaultSettingsPath), s_PackageSettingsAssetName);
        }

        internal static string GetAssetPathForComponents(string[] pathComponents, string root = "Assets")
        {
            if (pathComponents.Length <= 0)
                return null;

            string path = root;
            foreach( var pc in pathComponents)
            {
                string subFolder = Path.Combine(path, pc);
                bool shouldCreate = true;
                foreach (var f in AssetDatabase.GetSubFolders(path))
                {
                    if (String.Compare(Path.GetFullPath(f), Path.GetFullPath(subFolder), true) == 0)
                    {
                        shouldCreate = false;
                        break;
                    }
                }

                if (shouldCreate)
                    AssetDatabase.CreateFolder(path, pc);
                path = subFolder;
            }

            return path;
        }

        public string GetActiveLoaderLibraryPath()
        {
            return OpenXRChooseRuntimeLibraries.GetLoaderLibraryPath();
        }

        void IPackageSettings.RefreshFeatureSets()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets();
        }

        private bool IsValidBuildTargetGroup(BuildTargetGroup buildTargetGroup) =>
            buildTargetGroup == BuildTargetGroup.Standalone ||
            Enum.GetValues(typeof(BuildTarget)).Cast<BuildTarget>().Any(bt =>
            {
                var group = BuildPipeline.GetBuildTargetGroup(bt);
                return group == buildTargetGroup && BuildPipeline.IsBuildTargetSupported(group, bt);
            });

        public OpenXRSettings GetSettingsForBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            OpenXRSettings ret = null;
            Settings.TryGetValue(buildTargetGroup, out ret);
            if (ret == null)
            {
                if (!IsValidBuildTargetGroup(buildTargetGroup))
                    return null;

                ret = ScriptableObject.CreateInstance<OpenXRSettings>();
                if (Settings.ContainsKey(buildTargetGroup))
                {
                    Settings[buildTargetGroup] = ret;
                }
                else
                {
                    Settings.Add(buildTargetGroup, ret);
                }

                ret.name = buildTargetGroup.ToString();

                AssetDatabase.AddObjectToAsset(ret, this);
            }

            return ret;
        }

        /// <summary>Pre-serialization action</summary>
        public void OnBeforeSerialize()
        {
            Keys.Clear();
            Values.Clear();

            foreach (var kv in Settings)
            {
                Keys.Add(kv.Key);
                Values.Add(kv.Value);
            }
        }

        /// <summary>Post-deserialization action</summary>
        public void OnAfterDeserialize()
        {
            Settings = new Dictionary<BuildTargetGroup, OpenXRSettings>();
            for (int i = 0; i < Math.Min(Keys.Count, Values.Count); i++)
            {
                Settings.Add(Keys[i], Values[i]);
            }
        }

        /// <summary>
        /// Return all features of the given type from all available build target groups.
        /// </summary>
        /// <typeparam name="T">Feature type to retrieve</typeparam>
        /// <returns>All features and their build target group that match the given feature type.</returns>
        public IEnumerable<(BuildTargetGroup buildTargetGroup,T feature)> GetFeatures<T>() where T : OpenXRFeature
        {
            foreach (var kv in Settings)
            {
                if (kv.Value.features == null)
                    continue;

                foreach (var feature in kv.Value.features)
                {
                    if(feature is T featureT)
                        yield return (kv.Key, featureT);
                }
            }
        }
    }
}