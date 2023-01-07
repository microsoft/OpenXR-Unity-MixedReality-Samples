using NUnit.Framework;
using Unity.Collections;
using System.Collections.Generic;

namespace UnityEngine.XR.ARSubsystems.Tests
{
    public class XRDepthSubsystemImpl : XRDepthSubsystem
    {
        public class TestProvider : Provider
        {
            public TestProvider() { }
            public override TrackableChanges<XRPointCloud> GetChanges(XRPointCloud defaultPointCloud, Allocator allocator) => default;
            public override XRPointCloudData GetPointCloudData(TrackableId trackableId, Allocator allocator) => default;
        }
    }

    [TestFixture]
    public class XRDepthSubsystemTestFixture
    {
        [OneTimeSetUp]
        public void RegisterTestDescriptor()
        {
            XRDepthSubsystemDescriptor.RegisterDescriptor(new XRDepthSubsystemDescriptor.Cinfo
            {
                id = "Test-Depth",
                providerType = typeof(XRDepthSubsystemImpl.TestProvider),
                subsystemTypeOverride = typeof(XRDepthSubsystemImpl)
            });
        }

        static List<XRDepthSubsystemDescriptor> s_Descs = new List<XRDepthSubsystemDescriptor>();
        static XRDepthSubsystem CreateTestDepthSubsystem()
        {
            SubsystemManager.GetSubsystemDescriptors(s_Descs);
            return s_Descs[0].Create();
        }

        [Test]
        public void RunningStateTests()
        {
            XRDepthSubsystem subsystem = CreateTestDepthSubsystem();

            // Initial state is not running
            Assert.That(subsystem.running == false);

            // After start subsystem is running
            subsystem.Start();
            Assert.That(subsystem.running == true);

            // After start subsystem is running
            subsystem.Stop();
            Assert.That(subsystem.running == false);
        }
    }
}