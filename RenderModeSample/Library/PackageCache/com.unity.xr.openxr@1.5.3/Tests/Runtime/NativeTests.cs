using NUnit.Framework;
using UnityEngine.XR.OpenXR.Input;
using System.Text;
using UnityEngine.TestTools;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class NativeTests : OpenXRLoaderSetup
    {
        public override void BeforeTest()
        {
            OpenXRLoaderBase.Internal_UnloadOpenXRLibrary();
            base.BeforeTest();
        }

        [Test]
        public void OpenXRLoader_LoadOpenXRLibrary_NullLoaderPath()
        {
            Assert.IsFalse(OpenXRLoaderBase.Internal_LoadOpenXRLibrary(null));
        }

        [Test]
        public void OpenXRLoader_LoadOpenXRLibrary_InvalidLoaderPath()
        {
            Assert.IsFalse(OpenXRLoaderBase.Internal_LoadOpenXRLibrary(OpenXRLoaderBase.StringToWCHAR_T("abababab")));
        }

        [Test]
        public void OpenXRLoader_InitializeSession_BeforeLoadingLibrary()
        {
            Assert.IsFalse(OpenXRLoaderBase.Internal_InitializeSession());
        }

        [Test]
        public void OpenXRLoader_CreateSessionIfNeeded_BeforeLoadingLibrary()
        {
            Assert.IsFalse(OpenXRLoaderBase.Internal_CreateSessionIfNeeded());
        }

        [Test]
        public void OpenXRLoader_RequestEnableExtensionString_BeforeLoadingLibrary()
        {
            Assert.IsFalse(OpenXRLoaderBase.Internal_RequestEnableExtensionString(null));
        }

        [Test]
        public void OpenXRLoader_RequestEnableExtensionString_Null()
        {
            Assert.IsFalse(OpenXRLoaderBase.Internal_RequestEnableExtensionString("some_extension"));
        }

        [Test]
        public void OpenXRInput_TryGetInputSourceName_BeforeInitializing()
        {
            Assert.IsFalse(OpenXRInput.Internal_TryGetInputSourceName(0, 0, 0, 0, out var name));
        }

        [Test]
        public void OpenXRInput_SuggestBindings_BeforeInitializing()
        {
            Assert.IsFalse(OpenXRInput.Internal_SuggestBindings("", null, 0));
        }

        [Test]
        public void OpenXRInput_AttachActionSets_BeforeInitializing()
        {
            Assert.IsFalse(OpenXRInput.Internal_AttachActionSets());
        }
    }
}
