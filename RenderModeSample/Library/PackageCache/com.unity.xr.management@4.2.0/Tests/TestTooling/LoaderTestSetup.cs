using System.IO;

using NUnit.Framework;

using UnityEditor;

using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR;
using UnityEngine.XR.Management;


namespace Unity.XR.TestTooling
{
    public abstract class LoaderTestSetup<L, S> : ManagementTestSetup, IPrebuildSetup, IPostBuildCleanup 
        where L : XRLoader 
        where S : ScriptableObject
    {
        protected abstract string settingsKey { get; }

        protected L loader = null;
        protected S settings = null;
        private bool isRunning = false;

        public override void SetupTest()
        {
            base.SetupTest();

            Assert.IsNotNull(xrGeneralSettings);

            // Deleted by ManagementSetup - deletes whole Temp folder

            // Setup Loader
            loader = ScriptableObject.CreateInstance<L>();
            var path = GetAssetPathForComponents(s_TempSettingsPath);
            AssetDatabase.CreateAsset(loader, Path.Combine(path, $"Test_{typeof(L).Name}.asset"));
            xrGeneralSettings.Manager.loaders.Add(loader);

            // Setup Settings
            settings = ScriptableObject.CreateInstance<S>();
            AssetDatabase.CreateAsset(settings, Path.Combine(path, $"Test_{typeof(S).Name}.asset"));
            EditorBuildSettings.AddConfigObject(settingsKey, settings, true);

            AssetDatabase.SaveAssets();
        }

        public override void TearDownTest()
        {
            if (isRunning)
                StopAndShutdown();
            xrGeneralSettings.Manager.loaders.Remove(loader);
            loader = null;

            base.TearDownTest();
        }

        protected void InitializeAndStart()
        {
            if (loader != null)
            {
                if (loader.Initialize())
                    isRunning = loader.Start();
            }
        }

        protected void StopAndShutdown()
        {
            if (loader != null)
            {
                loader.Stop();
                loader.Deinitialize();
                isRunning = false;
            }
        }

        protected void RestartProvider()
        {
            StopAndShutdown();
            InitializeAndStart();
        }

        // IPrebuildSetup - Build time setup
        void IPrebuildSetup.Setup()
        {
            if (XRGeneralSettings.Instance != null)
                XRGeneralSettings.Instance.InitManagerOnStart = false;
        }

        // IPostBuildCleanup - Build time cleanup
        void IPostBuildCleanup.Cleanup()
        {
            if (XRGeneralSettings.Instance != null)
                XRGeneralSettings.Instance.InitManagerOnStart = true;
        }
    }
}
