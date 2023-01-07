using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Tests;
using Assert = UnityEngine.Assertions.Assert;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class zBuildHookTests : OpenXRLoaderSetup
    {
        internal static BuildReport BuildMockPlayer()
        {
            BuildPlayerOptions opts = new BuildPlayerOptions();
#if UNITY_EDITOR_WIN
            opts.target = BuildTarget.StandaloneWindows64;
#elif UNITY_EDITOR_OSX
            opts.target = BuildTarget.StandaloneOSX;
#endif
            if (File.Exists("Assets/main.unity"))
                opts.scenes = new string[] {"Assets/main.unity"};
            opts.targetGroup = BuildTargetGroup.Standalone;
            opts.locationPathName = "mocktest/mocktest.exe";

            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, opts.target);
            var report = BuildPipeline.BuildPlayer(opts);
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            return report;
        }

        [Test]
        public void PrePostCallbacksAreReceived()
        {
            bool preprocessCalled = false;
            bool postprocessCalled = false;

            BuildCallbacks.TestCallback = (methodName, param) =>
            {
                if (methodName == "OnPreprocessBuildExt")
                {
                    preprocessCalled = true;
                }

                if (methodName == "OnPostprocessBuildExt")
                {
                    postprocessCalled = true;
                }

                return true;
            };

            var result = BuildMockPlayer();

            if (Environment.GetEnvironmentVariable("UNITY_OPENXR_YAMATO") == "1")
                Assert.IsTrue(result.summary.result == BuildResult.Succeeded);
            else if (result.summary.result != BuildResult.Succeeded)
                return;

            Assert.IsTrue(preprocessCalled);
            Assert.IsTrue(postprocessCalled);
        }

        [Test]
        public void NoBuildCallbacksFeatureDisabled()
        {
            bool preprocessCalled = false;
            bool postprocessCalled = false;

            BuildCallbacks.TestCallback = (methodName, param) =>
            {
                if (methodName == "OnPreprocessBuildExt")
                {
                    preprocessCalled = true;
                }

                if (methodName == "OnPostprocessBuildExt")
                {
                    postprocessCalled = true;
                }

                return true;
            };

            // Disable mock runtime, no callbacks should occur during build
            EnableFeature<MockRuntime>(false);
            BuildMockPlayer();
            Assert.IsFalse(preprocessCalled);
            Assert.IsFalse(postprocessCalled);
        }

        [Test]
        public void NoBuildCallbacksOpenXRDisabled()
        {
            bool preprocessCalled = false;
            bool postprocessCalled = false;

            BuildCallbacks.TestCallback = (methodName, param) =>
            {
                if (methodName == "OnPreprocessBuildExt")
                {
                    preprocessCalled = true;
                }

                if (methodName == "OnPostprocessBuildExt")
                {
                    postprocessCalled = true;
                }

                return true;
            };

            // Remove OpenXR Loader, no callbacks should occur during build
            var loaders = XRGeneralSettings.Instance.Manager.activeLoaders;
            XRGeneralSettings.Instance.Manager.TrySetLoaders(new List<XRLoader>());
            BuildMockPlayer();
            XRGeneralSettings.Instance.Manager.TrySetLoaders(new List<XRLoader>(loaders));
            Assert.IsFalse(preprocessCalled);
            Assert.IsFalse(postprocessCalled);
        }

        private bool HasOpenXRLibraries(BuildReport report)
        {
            var path = Path.GetDirectoryName(report.summary.outputPath);
            var dir = new DirectoryInfo(path);
            var dlls = dir.EnumerateFiles("*.dll", SearchOption.AllDirectories).Select(s => s.Name).ToList();
            return dlls.Contains("openxr_loader.dll") || dlls.Contains("UnityOpenXR.dll");
        }

        [Test]
        public void VerifyBuildOutputLibraries()
        {
            var resultWithOpenXR = BuildMockPlayer();
            // Disable this test if we're not running our openxr yamato infrastructure
            if (resultWithOpenXR.summary.result != BuildResult.Succeeded && Environment.GetEnvironmentVariable("UNITY_OPENXR_YAMATO") != "1")
                return;
            Assert.IsTrue(HasOpenXRLibraries(resultWithOpenXR));

            // Remove OpenXR Loader
            XRGeneralSettings.Instance.Manager.TrySetLoaders(new List<XRLoader>());
            var resultWithoutOpenXR = BuildMockPlayer();
            Assert.IsFalse(HasOpenXRLibraries(resultWithoutOpenXR));
        }

        internal class BuildCallbacks : OpenXRFeatureBuildHooks
        {
            [NonSerialized] internal static Func<string, object, bool> TestCallback = (methodName, param) => true;

            public override int callbackOrder => 1;
            public override Type featureType => typeof(MockRuntime);

            protected override void OnPreprocessBuildExt(BuildReport report)
            {
                TestCallback(MethodBase.GetCurrentMethod().Name, report);
            }

            protected override void OnPostGenerateGradleAndroidProjectExt(string path)
            {
                TestCallback(MethodBase.GetCurrentMethod().Name, path);
            }

            protected override void OnPostprocessBuildExt(BuildReport report)
            {
                TestCallback(MethodBase.GetCurrentMethod().Name, report);
            }
        }
    }
}