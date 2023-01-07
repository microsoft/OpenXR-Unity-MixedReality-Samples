using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.Management.Tests;
using Object = UnityEngine.Object;

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX

namespace UnityEditor.XR.Management.Tests.BuildTests
{
#if UNITY_EDITOR_WIN
    [TestFixture(GraphicsDeviceType.Direct3D11, false, new [] { GraphicsDeviceType.Direct3D11})]
    [TestFixture(GraphicsDeviceType.Direct3D11, false, new [] { GraphicsDeviceType.Direct3D12, GraphicsDeviceType.Direct3D11})]
    [TestFixture(GraphicsDeviceType.Direct3D11, true, new [] { GraphicsDeviceType.Direct3D12, GraphicsDeviceType.Vulkan})]
    [TestFixture(GraphicsDeviceType.Direct3D11, false, new [] { GraphicsDeviceType.Null, GraphicsDeviceType.Vulkan})]
    [TestFixture(GraphicsDeviceType.Direct3D11, false, new [] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.Null})]
#elif UNITY_EDITOR_OSX
    [TestFixture(GraphicsDeviceType.Metal, false, new [] { GraphicsDeviceType.Metal})]
    [TestFixture(GraphicsDeviceType.Metal, false, new [] { GraphicsDeviceType.Direct3D12, GraphicsDeviceType.Metal})]
    [TestFixture(GraphicsDeviceType.Metal, true, new [] { GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.Vulkan})]
    [TestFixture(GraphicsDeviceType.Metal, false, new [] { GraphicsDeviceType.Null, GraphicsDeviceType.Vulkan})]
    [TestFixture(GraphicsDeviceType.Metal, false, new [] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.Null})]
#endif
    class GraphicsAPICompatibilityTests
    {
        XRManagerSettings m_Manager;
        List<XRLoader> m_Loaders = new List<XRLoader>();

        private GraphicsDeviceType m_PlayerSettingsDeviceType;
        private GraphicsDeviceType[]  m_LoadersSupporteDeviceTypes;
        bool m_BuildFails;

        public GraphicsAPICompatibilityTests(GraphicsDeviceType playerSettingsDeviceType, bool fails, GraphicsDeviceType[] loaders)
        {
            m_BuildFails = fails;
            m_PlayerSettingsDeviceType = playerSettingsDeviceType;
            m_LoadersSupporteDeviceTypes = loaders;
        }

        [SetUp]
        public void SetupPlayerSettings()
        {
#if UNITY_EDITOR_WIN
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { m_PlayerSettingsDeviceType });
#elif UNITY_EDITOR_OSX
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneOSX, new[] { m_PlayerSettingsDeviceType });
#endif
            m_Manager = ScriptableObject.CreateInstance<XRManagerSettings>();
            m_Manager.automaticLoading = false;

            m_Loaders = new List<XRLoader>();

            for (int i = 0; i < m_LoadersSupporteDeviceTypes.Length; i++)
            {
                DummyLoader dl = ScriptableObject.CreateInstance(typeof(DummyLoader)) as DummyLoader;
                dl.id = i;
                dl.supportedDeviceType = m_LoadersSupporteDeviceTypes[i];
                m_Loaders.Add(dl);
            }
            m_Manager.TrySetLoaders(m_Loaders);
        }

        [TearDown]
        public void TeadDown()
        {
            Object.DestroyImmediate(m_Manager);
            m_Manager = null;

        }

        [Test]
        public void CheckGraphicsAPICompatibilityOnBuild()
        {
            try
            {
                XRGeneralBuildProcessor.VerifyGraphicsAPICompatibility(m_Manager, m_PlayerSettingsDeviceType);
            }
            catch (BuildFailedException)
            {
                Assert.True(m_BuildFails);
                return;
            }

            Assert.False(m_BuildFails);
        }
    }

    [TestFixture(BuildTargetGroup.Standalone)]
    [TestFixture(BuildTargetGroup.Android)]
    [TestFixture(BuildTargetGroup.iOS)]
#if (!UNITY_2021_2_OR_NEWER)
    [TestFixture(BuildTargetGroup.Lumin)]
#endif    
    [TestFixture(BuildTargetGroup.PS4)]
    class XRGeneralSettingsBuildTests
    {
        const string k_TemporaryTestPath = "Assets/Hidden_XRManagement_Test_Assets";
        const string k_AssetName = "TestGeneralAsset.asset";

        BuildTargetGroup m_BuildTargetGroup;

        XRGeneralSettingsPerBuildTarget m_OldBuildTargetSettings;

        public XRGeneralSettingsBuildTests(BuildTargetGroup group)
        {
            m_BuildTargetGroup = group;
        }

        void CleanupOldSettings() => BuildHelpers.CleanOldSettings<XRGeneralSettings>();

        [SetUp]
        public void SetupPlayerSettings()
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out m_OldBuildTargetSettings);
            EditorBuildSettings.RemoveConfigObject(XRGeneralSettings.k_SettingsKey);

            var emptyBuildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            generalSettings.AssignedSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            emptyBuildTargetSettings.SetSettingsForBuildTarget(m_BuildTargetGroup, generalSettings);
            emptyBuildTargetSettings.SettingsForBuildTarget(m_BuildTargetGroup).AssignedSettings.TrySetLoaders(new List<XRLoader>());

            Directory.CreateDirectory(k_TemporaryTestPath);
            AssetDatabase.CreateAsset(emptyBuildTargetSettings, Path.Combine(k_TemporaryTestPath, k_AssetName));

            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, emptyBuildTargetSettings, true);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_OldBuildTargetSettings != null)
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, m_OldBuildTargetSettings, true);

            // AssetDatabase.DeleteAsset(k_TemporaryTestPath);
            AssetDatabase.DeleteAsset(Path.Combine(k_TemporaryTestPath, k_AssetName));
            if (Directory.Exists(Path.Combine("./", k_TemporaryTestPath)))
            {
                Directory.Delete(Path.Combine("./", k_TemporaryTestPath));
                File.Delete($"./{k_TemporaryTestPath}.meta");
                AssetDatabase.Refresh();
            }
        }

        [Test]
        public void CheckEmptyXRGeneralAssetWillNotGetIncludedInAssets()
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget buildTargetSettings);
            Assert.False(buildTargetSettings == null);

            var settings = buildTargetSettings.SettingsForBuildTarget(m_BuildTargetGroup);
            Assert.False(settings == null);

            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            var settingsIncludedInPreloadedAssets = preloadedAssets.Contains(settings);

            // Use the logic in XRGeneralBuildProcessor.OnPreprocessBuild() to determine if the XR General Settings will
            // be include or not.
            if (!settingsIncludedInPreloadedAssets && settings.AssignedSettings.activeLoaders.Count > 0)
            {
                var assets = preloadedAssets.ToList();
                assets.Add(settings);
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
            else
            {
                CleanupOldSettings();
            }

            Assert.False(PlayerSettings.GetPreloadedAssets().Contains(settings));
        }
    }
}
#endif //UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
