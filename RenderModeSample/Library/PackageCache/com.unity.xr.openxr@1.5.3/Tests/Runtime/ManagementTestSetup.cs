using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
#endif
using UnityEngine.XR.Management;

namespace UnityEngine.XR.TestTooling
{
    // Mostly borrowed from XRManagement - this should probably live in that package.
    internal abstract class ManagementTestSetup
    {
        protected static readonly string[] s_TestGeneralSettings = { "Temp", "Test" };
        protected static readonly string[] s_TempSettingsPath = {"Temp", "Test", "Settings" };

        /// <summary>
        /// When true, AssetDatabase.AddObjectToAsset will not be called to add XRManagerSettings to XRGeneralSettings.
        /// </summary>
        protected virtual bool TestManagerUpgradePath => false;

        protected string testPathToGeneralSettings;
        protected string testPathToSettings;

#pragma warning disable 0414 // CS0414: The field 'ManagementTestSetup.currentSettings' is assigned but its value is never used
        private UnityEngine.Object currentSettings = null;
#pragma warning restore 0414

        protected XRManagerSettings testManager = null;
        protected XRGeneralSettings xrGeneralSettings = null;
#if UNITY_EDITOR
        protected XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
#endif

        public virtual void SetupTest()
        {
#if UNITY_EDITOR
            var windowTypes = TypeCache.GetTypesDerivedFrom(typeof(EditorWindow));
            foreach (var wt in windowTypes)
            {
                if (wt.Name.Contains("ProjectSettingsWindow"))
                {
                    var projectSettingsWindow = EditorWindow.GetWindow(wt);
                    if (projectSettingsWindow != null)
                    {
                        projectSettingsWindow.Close();
                    }
                }
            }

            testPathToGeneralSettings = GetAssetPathForComponents(s_TestGeneralSettings);
            testPathToGeneralSettings = Path.Combine(testPathToGeneralSettings, "Test_XRGeneralSettingsPerBuildTarget.asset");
            if (File.Exists(testPathToGeneralSettings))
            {
                AssetDatabase.DeleteAsset(testPathToGeneralSettings);
            }

            buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(buildTargetSettings, testPathToGeneralSettings);

            testPathToSettings = GetAssetPathForComponents(s_TempSettingsPath);
            testPathToSettings = Path.Combine(testPathToSettings, "Test_XRGeneralSettings.asset");

            if (File.Exists(testPathToSettings))
            {
                AssetDatabase.DeleteAsset(testPathToSettings);
            }

            testManager = ScriptableObject.CreateInstance<XRManagerSettings>();

            xrGeneralSettings = ScriptableObject.CreateInstance<XRGeneralSettings>() as XRGeneralSettings;
            XRGeneralSettings.Instance = xrGeneralSettings;

            xrGeneralSettings.Manager = testManager;
            buildTargetSettings.SetSettingsForBuildTarget(BuildTargetGroup.Standalone, xrGeneralSettings);
            buildTargetSettings.SetSettingsForBuildTarget(BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget), xrGeneralSettings);

            AssetDatabase.CreateAsset(xrGeneralSettings, testPathToSettings);
            AssetDatabase.AddObjectToAsset(testManager, xrGeneralSettings);

            AssetDatabase.SaveAssets();

            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);

#endif
        }

        public virtual void TearDownTest()
        {
#if UNITY_EDITOR
            EditorBuildSettings.RemoveConfigObject(XRGeneralSettings.k_SettingsKey);
            buildTargetSettings = null;
            testManager = null;
            xrGeneralSettings = null;
            AssetDatabase.DeleteAsset(Path.Combine("Assets","Temp"));
#endif
        }

#if UNITY_EDITOR
        public static string GetAssetPathForComponents(string[] pathComponents, string root = "Assets")
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
#endif
    }
}
