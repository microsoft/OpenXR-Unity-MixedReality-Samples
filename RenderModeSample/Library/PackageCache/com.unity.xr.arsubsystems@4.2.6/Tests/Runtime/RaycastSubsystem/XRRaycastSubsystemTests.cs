using NUnit.Framework;
using Unity.Collections;
using System.Collections.Generic;

namespace UnityEngine.XR.ARSubsystems.Tests
{
    public class XRRaycastSubsystemImpl : XRRaycastSubsystem
    {
        public class MockProvider : Provider
        {
            public override TrackableChanges<XRRaycast> GetChanges(XRRaycast defaultRaycast, Allocator allocator)
            {
                throw new System.NotImplementedException();
            }
        }
    }

    [TestFixture]
    public class XRRaycastSubsystemTestFixture
    {
        [OneTimeSetUp]
        public void RegisterTestDescriptor()
        {
            XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
            {
                id = "Test-Raycast",
                providerType = typeof(XRRaycastSubsystemImpl.MockProvider),
                subsystemTypeOverride = typeof(XRRaycastSubsystemImpl)
            });
        }

        static List<XRRaycastSubsystemDescriptor> s_Descs = new List<XRRaycastSubsystemDescriptor>();
        static XRRaycastSubsystem CreateTestRaycastSubsystem()
        {
            SubsystemManager.GetSubsystemDescriptors(s_Descs);
            return s_Descs[0].Create();
        }

        [Test]
        public void RunningStateTests()
        {
            XRRaycastSubsystem subsystem = CreateTestRaycastSubsystem();

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
