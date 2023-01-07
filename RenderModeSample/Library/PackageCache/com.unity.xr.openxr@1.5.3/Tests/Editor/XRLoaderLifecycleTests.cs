using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR.Tests;
using Assert=UnityEngine.Assertions.Assert;

[assembly:UnityPlatform(RuntimePlatform.WindowsEditor)]

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class XRLoaderLifecycleTests : OpenXRLoaderSetup
    {
        [Test]
        public void FullLifecycleOrder()
        {
            bool subsystemCreate = false;
            bool subsystemStart = false;
            bool hookInstanceProc = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                switch (methodName)
                {
                    case nameof(OpenXRFeature.HookGetInstanceProcAddr):
                        Assert.IsFalse(subsystemCreate);
                        Assert.IsFalse(subsystemStart);
                        hookInstanceProc = true;
                        break;

                    case nameof(OpenXRFeature.OnSubsystemCreate):
                        Assert.IsTrue(hookInstanceProc);
                        Assert.IsFalse(subsystemStart);
                        subsystemCreate = true;
                        break;

                    case nameof(OpenXRFeature.OnSubsystemStart):
                        Assert.IsTrue(hookInstanceProc);
                        Assert.IsTrue(subsystemCreate);
                        subsystemStart = true;
                        break;
                }

                return true;
            };

            base.InitializeAndStart();

            ProcessOpenXRMessageLoop();

            Assert.IsTrue(hookInstanceProc);
            Assert.IsTrue(subsystemCreate);
            Assert.IsTrue(subsystemStart);

            bool subsystemStop = false;
            bool subsystemDestroy = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                switch (methodName)
                {
                    case nameof(OpenXRFeature.OnSubsystemStop):
                        Assert.IsFalse(subsystemDestroy);
                        subsystemStop = true;
                        break;

                    case nameof(OpenXRFeature.OnSubsystemDestroy):
                        Assert.IsTrue(subsystemStop);
                        subsystemDestroy = true;
                        break;
                }
                return true;
            };

            base.StopAndShutdown();

            Assert.IsTrue(subsystemStop);
            Assert.IsTrue(subsystemDestroy);
        }

        [Test]
        public void InstanceCreate() => TestInstanceCreate(true, false);

        [Test]
        public void InstanceCreateFail() => TestInstanceCreate(false, false);

        [Test]
        public void InstanceCreateRequired() => TestInstanceCreate(true, true);

        [Test]
        public void InstanceCreateFailRequired() => TestInstanceCreate(false, true);

        public void TestInstanceCreate(bool result, bool required)
        {
            Loader.DisableValidationChecksOnEnteringPlaymode = true;
            bool instanceCreated = false;
            bool hookInstanceProcAddr = false;
            bool other = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                switch (methodName)
                {
                    case nameof(OpenXRFeature.OnInstanceCreate):
                        instanceCreated = true;
                        return result;

                    case nameof(OpenXRFeature.HookGetInstanceProcAddr):
                        hookInstanceProcAddr = true;
                        break;

                    default:
                        other = true;
                        break;
                }

                return true;
            };

            MockRuntime.Instance.required = required;
            base.InitializeAndStart();

            Assert.IsTrue(instanceCreated);
            Assert.IsTrue(hookInstanceProcAddr);

            if (required && !result)
                Assert.IsNull(OpenXRLoaderBase.Instance);
            else
                Assert.IsNotNull(OpenXRLoaderBase.Instance);

            // A feature that fails that is not required should be disabled
            if (!result && !required)
                Assert.IsFalse(MockRuntime.Instance.enabled);

            base.StopAndShutdown();

            if (result)
                Assert.IsTrue(other);
            else
                // A feature that fails initialize should have no further callbacks
                Assert.IsFalse(other);
        }

        [Test]
        public void DeinitWithoutInit()
        {
            bool callback = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                callback = true;
                return true;
            };

            base.StopAndShutdown();

            Assert.IsFalse(callback);
        }
    }
}
