using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;

[assembly:UnityPlatform(RuntimePlatform.WindowsEditor)]

namespace UnityEditor.XR.OpenXR.Tests
{
    public class aAssetBundleTests
    {
        [Test]
        public void BuildAssetBundle()
        {
#if UNITY_EDITOR_WIN
            var target = BuildTarget.StandaloneWindows64;
#elif UNITY_EDITOR_OSX
            var target = BuildTarget.StandaloneOSX;
#else
            var target = BuildTarget.NoTarget;
#endif

            // Test is only valid if we have a valid build target and that build target is available
            if (target == BuildTarget.NoTarget || !BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                return;

            // Create an asset and add it to an asset bundle
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            EditorSceneManager.SaveScene(scene, "Assets/abtest.unity");
            AssetDatabase.Refresh();
            var importer = AssetImporter.GetAtPath("Assets/abtest.unity");
            importer.assetBundleName = "mocktest";

            if (!Directory.Exists("mocktest/ab"))
                Directory.CreateDirectory("mocktest/ab");

            // Build the asset bundle
            BuildPipeline.BuildAssetBundles("mocktest/ab", BuildAssetBundleOptions.ForceRebuildAssetBundle, target);

            // Cleanup
            AssetDatabase.DeleteAsset("Assets/abtest.unity");
        }
    }
}