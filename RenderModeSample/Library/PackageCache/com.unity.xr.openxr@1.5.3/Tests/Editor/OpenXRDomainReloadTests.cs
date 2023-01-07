using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Tests;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.TestTools;
using UnityEngine.Scripting;
using Assert=UnityEngine.Assertions.Assert;

namespace UnityEditor.XR.OpenXR.Tests
{
#if false  // Tests are temporarily disabled due to an issue with [BeforeTest] being called twice during a domain reload

    /// <summary>
    /// Tests domain reloads within OpenXR.  Note that his class was named with two z's in front
    /// because the domain reloads during this test were causing other tests to fail that ran after it.
    /// </summary>
    internal class zzOpenXRDomainReloadTests : OpenXRLoaderSetup
    {
        [UnityTest]
        public IEnumerator AfterInitialize()
        {
            MockRuntimeFeature.Instance.TestCallback = (methodName, param) => true;

            Assert.IsNull(OpenXRLoaderBase.Instance);

            base.InitializeAndStart();

            // Perform a domain reload and wait for it to complete
            EditorUtility.RequestScriptReload();
            yield return new WaitForDomainReload();

            Assert.IsNotNull(OpenXRLoaderBase.Instance);

            base.StopAndShutdown();

            Assert.IsNull(OpenXRLoaderBase.Instance);

            yield return null;
        }

        [UnityTest]
        public IEnumerator BeforeInitialize()
        {
            MockRuntimeFeature.Instance.TestCallback = (methodName, param) => true;

            // Perform a domain reload and wait for it to complete
            EditorUtility.RequestScriptReload();
            yield return new WaitForDomainReload();

            base.InitializeAndStart();
            base.StopAndShutdown();

            yield return null;
        }
    }
#endif
}
