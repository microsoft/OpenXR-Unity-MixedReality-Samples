using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.XR.Management;
using UnityEngine.TestTools;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class NoRuntimeTests : OpenXRLoaderSetup
    {
        private XRManagerSettings manager => XRGeneralSettings.Instance?.Manager ?? null;

        private XRLoader activeLoader => manager?.activeLoader ?? null;

        public override void BeforeTest()
        {
            base.BeforeTest();
            Environment.SetEnvironmentVariable("XR_RUNTIME_JSON", "asdf.json");
            EnableMockRuntime(false);
            Loader.DisableValidationChecksOnEnteringPlaymode = true;
        }

        public override void AfterTest()
        {
            if (Loader != null)
                Loader.DisableValidationChecksOnEnteringPlaymode = false;
            Environment.SetEnvironmentVariable("XR_RUNTIME_JSON", "");
            base.AfterTest();
        }

        [UnityTest]
        [Category("Loader Tests")]
        [UnityPlatform(include = new[] {RuntimePlatform.WindowsEditor})] // we can't run these tests on player because only the mock loader is included - this needs the khronos loader
        public IEnumerator NoInitNoCrash()
        {
            base.InitializeAndStart();

            yield return null;

            Assert.IsNull(activeLoader);
        }

        [UnityTest]
        [Category("Loader Tests")]
        [UnityPlatform(include = new[] {RuntimePlatform.WindowsEditor})]
        public IEnumerator LoadRuntimeAfterNoRuntime()
        {
            base.InitializeAndStart();

            yield return null;

            Assert.IsNull(activeLoader);

            #if !OPENXR_USE_KHRONOS_LOADER
            Environment.SetEnvironmentVariable("XR_RUNTIME_JSON", "");
            EnableMockRuntime();

            base.InitializeAndStart();

            yield return null;

            Assert.IsNotNull(activeLoader);

            Assert.AreEqual(OpenXRRuntime.name, "Unity Mock Runtime");
            #endif
        }
    }
}