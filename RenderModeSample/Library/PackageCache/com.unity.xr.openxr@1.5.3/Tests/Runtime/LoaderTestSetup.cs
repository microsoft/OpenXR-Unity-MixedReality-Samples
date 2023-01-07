using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.TestTools;
using UnityEngine.XR.Management;


namespace UnityEngine.XR.TestTooling
{
    internal abstract class LoaderTestSetup<L, S> : ManagementTestSetup, IPrebuildSetup, IPostBuildCleanup
        where L : XRLoader
        where S : ScriptableObject
    {
        protected abstract string settingsKey { get; }

        protected L loader = null;
        protected S settings = null;

        public bool IsRunning<T>()
            where T : class, ISubsystem
        {
            return XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<T>()?.running ?? false;
        }


#if UNITY_EDITOR
        T GetOrCreateAsset<T>(string path) where T : UnityEngine.ScriptableObject
        {
            T asset = default(T);

            if (!File.Exists(path))
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
            }
            else
            {
                asset = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return asset as T;
        }
#endif

        protected void DestroyLoaderAndSettings()
        {
#if UNITY_EDITOR

            var path = GetAssetPathForComponents(s_TempSettingsPath);
            var settingsPath = Path.Combine(path, $"Test_{typeof(S).Name}.asset");
            AssetDatabase.DeleteAsset(settingsPath);

            var loaderPath = Path.Combine(path, $"Test_{typeof(L).Name}.asset");
            AssetDatabase.DeleteAsset(loaderPath);
#endif
        }

        protected void SetupLoaderAndSettings()
        {
#if UNITY_EDITOR
            // Setup Loader
            var path = GetAssetPathForComponents(s_TempSettingsPath);
            var loaderPath = Path.Combine(path, $"Test_{typeof(L).Name}.asset");
            loader = GetOrCreateAsset<L>(loaderPath);

            if (xrGeneralSettings == null)
            {
                xrGeneralSettings = XRGeneralSettings.Instance;
                testManager = xrGeneralSettings?.Manager ?? null;
            }

#pragma warning disable CS0618
            xrGeneralSettings?.Manager.loaders.Clear();
            xrGeneralSettings?.Manager.loaders.Add(loader);
#pragma warning restore CS0618

            // Setup Settings
            var settingsPath = Path.Combine(path, $"Test_{typeof(S).Name}.asset");
            settings = GetOrCreateAsset<S>(settingsPath);

            EditorBuildSettings.AddConfigObject(settingsKey, settings, true);

#endif
        }

        public override void SetupTest()
        {
#if UNITY_EDITOR
            base.SetupTest();
            SetupLoaderAndSettings();
#endif
        }

        protected void RemoveLoaderAndSettings()
        {
#if UNITY_EDITOR
            StopAndShutdown();

            if (loader != null)
            {
#pragma warning disable CS0618
                xrGeneralSettings.Manager.loaders.Remove(loader);
#pragma warning restore CS0618
            }

            EditorBuildSettings.RemoveConfigObject(settingsKey);

            loader = null;
#endif
        }

        public override void TearDownTest()
        {
#if UNITY_EDITOR
            RemoveLoaderAndSettings();
            base.TearDownTest();
#endif
        }

        protected void Initialize()
        {
            var manager = XRGeneralSettings.Instance?.Manager;
            manager?.InitializeLoaderSync();
        }

        protected void Start()
        {
            var manager = XRGeneralSettings.Instance?.Manager;
            if((manager?.activeLoader ?? null) != null)
            {
                manager.StartSubsystems();
            }
        }

        protected void InitializeAndStart()
        {
            Initialize();
            Start();
        }

        protected void Stop()
        {
            var manager = XRGeneralSettings.Instance?.Manager;
            if (manager != null && manager.activeLoader != null)
                manager?.StopSubsystems();
            else if (loader != null)
                loader.Stop();
        }

        protected void Shutdown()
        {
            var manager = XRGeneralSettings.Instance?.Manager;
            if (manager != null && manager.activeLoader != null)
                manager?.DeinitializeLoader();
            else if (loader != null)
                loader.Deinitialize();
        }

        protected void StopAndShutdown()
        {
            Stop();
            Shutdown();
        }

        protected void RestartProvider()
        {
            StopAndShutdown();
            InitializeAndStart();
        }

        // IPrebuildSetup - Build time setup
        public virtual void Setup()
        {
#if UNITY_EDITOR
            if (XRGeneralSettings.Instance != null)
                XRGeneralSettings.Instance.InitManagerOnStart = false;
#endif
        }

        // IPostBuildCleanup - Build time cleanup
        public virtual void Cleanup()
        {
#if UNITY_EDITOR
            if (XRGeneralSettings.Instance != null)
                XRGeneralSettings.Instance.InitManagerOnStart = true;
#endif
        }
    }
}
