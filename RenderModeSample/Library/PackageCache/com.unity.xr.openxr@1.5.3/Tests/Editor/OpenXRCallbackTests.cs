using NUnit.Framework;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR.Tests;
using Assert = UnityEngine.Assertions.Assert;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class OpenXRCallbackTests : OpenXRLoaderSetup
    {
        [Test]
        public void InstanceCreated()
        {
            bool instanceCreated = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnInstanceCreate))
                {
                    instanceCreated = true;
                    Assert.AreEqual(10, (ulong)param);
                }
                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            Assert.IsTrue(instanceCreated);
        }

        [Test]
        public void SessionCreated()
        {
            bool sessionCreated = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionCreate))
                {
                    sessionCreated = true;
                    Assert.AreEqual(3, (ulong)param);
                }
                return true;
            };

            base.InitializeAndStart();

            Assert.IsTrue(sessionCreated);
        }
    }
}