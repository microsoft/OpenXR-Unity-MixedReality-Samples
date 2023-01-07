using NUnit.Framework;
using System.Collections.Generic;

namespace UnityEngine.XR.ARSubsystems.Tests
{
    public class XRCameraSubsystemImpl : XRCameraSubsystem
    {
        public class ProviderImpl : Provider
        {
            public override void Start() { }
            public override void Stop() { }
            public override void Destroy() { }
        }
    }

    [TestFixture]
    public class XRCameraSubsystemTestFixture
    {
        [OneTimeSetUp]
        public void RegisterTestDescriptor()
        {
            XRCameraSubsystem.Register(new XRCameraSubsystemCinfo
            {
                id = "Test-Camera",
                providerType = typeof(XRCameraSubsystemImpl.ProviderImpl),
                subsystemTypeOverride = typeof(XRCameraSubsystemImpl)
            });
        }

        static List<XRCameraSubsystemDescriptor> s_Descs = new List<XRCameraSubsystemDescriptor>();
        static XRCameraSubsystem CreateTestCameraSubsystem()
        {
            SubsystemManager.GetSubsystemDescriptors(s_Descs);
            return s_Descs[0].Create();
        }

        [Test]
        public void RunningStateTests()
        {
            XRCameraSubsystem subsystem = CreateTestCameraSubsystem();

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
