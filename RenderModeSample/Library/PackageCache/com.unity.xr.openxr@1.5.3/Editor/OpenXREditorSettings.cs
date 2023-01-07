using System;
using System.IO;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace UnityEditor.XR.OpenXR
{
    internal class OpenXREditorSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        public static OpenXREditorSettings Instance => OpenXREditorSettings.GetInstance();

        static OpenXREditorSettings s_Instance = null;
        static object s_Lock = new object();

        static string GetAssetPathForComponents(string[] pathComponents)
        {
            if (pathComponents.Length <= 0)
                return null;

            string path = "Assets";
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

        static OpenXREditorSettings CreateScriptableObjectInstance(string path)
        {
            ScriptableObject obj = ScriptableObject.CreateInstance(typeof(OpenXREditorSettings)) as ScriptableObject;
            if (obj != null)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    string fileName = String.Format("OpenXR Editor Settings.asset");
                    string targetPath = Path.Combine(path, fileName);
                    AssetDatabase.CreateAsset(obj, targetPath);
                    AssetDatabase.SaveAssets();
                    return obj as OpenXREditorSettings;
                }
            }

            Debug.LogError("Error attempting to create instance of OpenXR Editor Settings.");
            return null;
        }

        static OpenXREditorSettings GetInstance()
        {
            if (s_Instance == null)
            {
                lock(s_Lock)
                {
                    if (s_Instance == null)
                    {
                        string path = GetAssetPathForComponents(new string[] { "XR", "Settings" });
                        var assetGuids = AssetDatabase.FindAssets($"t:{typeof(OpenXREditorSettings).Name}");
                        foreach (var assetGuid in assetGuids)
                        {
                            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                            var asset = AssetDatabase.LoadAssetAtPath<OpenXREditorSettings>(assetPath);
                            if (asset != null)
                            {
                                s_Instance = asset;
                            }
                        }

                        if (s_Instance == null)
                            s_Instance = CreateScriptableObjectInstance(path);
                    }
                }
            }

            return s_Instance;
        }


        [Serializable]
        struct BuildTargetFeatureSets
        {
            public List<string> featureSets;
        }

        [SerializeField]
        List<BuildTargetGroup> Keys = new List<BuildTargetGroup>();

        [SerializeField]
        List<BuildTargetFeatureSets> Values = new List<BuildTargetFeatureSets>();
        Dictionary<BuildTargetGroup, BuildTargetFeatureSets> selectedFeatureSets = new Dictionary<BuildTargetGroup, BuildTargetFeatureSets>();


        public void OnBeforeSerialize()
        {
            Keys.Clear();
            Values.Clear();

            foreach (var kv in selectedFeatureSets)
            {
                Keys.Add(kv.Key);
                Values.Add(kv.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            selectedFeatureSets = new Dictionary<BuildTargetGroup, BuildTargetFeatureSets>();
            for (int i = 0; i < Math.Min(Keys.Count, Values.Count); i++)
            {
                selectedFeatureSets.Add(Keys[i], Values[i]);
            }
        }

        internal bool IsFeatureSetSelected(BuildTargetGroup buildTargetGroup, string featureSetId)
        {
            bool ret = false;

            if (selectedFeatureSets.ContainsKey(buildTargetGroup))
            {
                ret = selectedFeatureSets[buildTargetGroup].featureSets.Contains(featureSetId);
            }

            return ret;
        }

        /// <summary>
        /// Set the selected state of the given feature set
        /// </summary>
        /// <returns>True if the state was changed, false if not</returns>
        internal bool SetFeatureSetSelected(BuildTargetGroup buildTargetGroup, string featureSetId, bool selected)
        {
            var dirty = false;
            if (!selectedFeatureSets.ContainsKey(buildTargetGroup))
            {
                selectedFeatureSets.Add(buildTargetGroup, new BuildTargetFeatureSets() { featureSets = new List<string>() });
                dirty = true;
            }

            var featureSets = selectedFeatureSets[buildTargetGroup].featureSets;

            if (selected && !featureSets.Contains(featureSetId))
            {
                featureSets.Add(featureSetId);
                dirty = true;
            }
            else if (!selected && featureSets.Contains(featureSetId))
            {
                featureSets.Remove(featureSetId);
                dirty = true;
            }

            if(dirty)
                EditorUtility.SetDirty(this);

            return dirty;
        }
    }
}