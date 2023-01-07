using System;
using NUnit.Framework;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class OpenXREditorTests
    {
        [Test]
        public void DocumentationVersion ()
        {
            var version = PackageManager.PackageInfo.FindForAssembly(typeof(OpenXREditorTests).Assembly)?.version;
            var majorminor = "@" + OpenXRFeatureAttribute.k_PackageVersionRegex.Match(version).Groups[1].Value + "/";
            UnityEngine.Debug.Log(typeof(KHRSimpleControllerProfile).GetCustomAttribute<OpenXRFeatureAttribute>().InternalDocumentationLink);
            Assert.IsTrue(typeof(KHRSimpleControllerProfile).GetCustomAttribute<OpenXRFeatureAttribute>().InternalDocumentationLink.Contains(majorminor));
        }

        [Test]
        public void PluginVersion()
        {
            var version = PackageManager.PackageInfo.FindForAssembly(typeof(OpenXREditorTests).Assembly)?.version;
            System.Text.RegularExpressions.Regex ReleaseVersion = new System.Text.RegularExpressions.Regex(@"^(\d+\.\d+\.\d+)$");
            //check for release build provider version number matches package version
            if (ReleaseVersion.IsMatch(version))
            {
                var tag = OpenXRRuntime.pluginVersion;
                Assert.AreEqual(0, String.Compare(version, tag), "Tag in github must match the package version number.");
                return;
            }
            //check for non-release build package version number supposed to be x.x.x-pre.x(pre-release) or x.x.x-exp.x(experimental)
            System.Text.RegularExpressions.Regex PreviewVersion = new System.Text.RegularExpressions.Regex(@"^(\d+\.\d+\.\d+\-\w+\.\d+)$");
            Assert.IsTrue(PreviewVersion.IsMatch(version), "Wrong package version format! Non-release branch should follow x.x.x-pre.x(pre-release) or x.x.x-exp.x(experimental)");
        }
    }
}

