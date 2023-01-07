using System;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using Unity.XR.TestTooling;
using UnityEngine.XR.Management.Tests.Standalone;
using UnityEngine.XR.Management.Tests.Standalone.Providing;

namespace UnityEngine.XR.Management.Tests
{
    class RuntimeLoaderTests : LoaderTestSetup<StandaloneLoader, RuntimeTestSettings>
    {
        protected override string settingsKey => "RuntimeTestSettings";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            StandaloneSubsystemParams parms = new StandaloneSubsystemParams("Standalone Subsystem", typeof(StandaloneSubsystem));
            StandaloneSubsystemDescriptor.Create(parms);
        }

        [SetUp]
        public override void SetupTest()
        {
            base.SetupTest();
        }

        [TearDown]
        public override void TearDownTest()
        {
            base.TearDownTest();
        }

        [Test]
        public void DoesItWork()
        {
            Assert.NotNull(loader);
            Assert.NotNull(settings);
        }

        [Test]
        public void StartsAndStops()
        {
            Assert.IsNull(loader.standaloneSubsystem);
            Assert.IsFalse(loader.started);
            Assert.IsFalse(loader.stopped);
            Assert.IsFalse(loader.deInitialized);

            InitializeAndStart();

            Assert.IsNotNull(loader.standaloneSubsystem);
            Assert.IsTrue(loader.started);
            Assert.IsFalse(loader.stopped);
            Assert.IsFalse(loader.deInitialized);

            StopAndShutdown();

            Assert.IsTrue(loader.started);
            Assert.IsTrue(loader.stopped);
            Assert.IsTrue(loader.deInitialized);
        }


        [Test]
        public void DeinitClearSubsystems()
        {
            Assert.IsNull(loader.standaloneSubsystem);

            InitializeAndStart();

            Assert.IsNotNull(loader.GetLoadedSubsystem<StandaloneSubsystem>());

            loader.Stop();
            loader.Deinitialize();

            Assert.IsNull(loader.GetLoadedSubsystem<StandaloneSubsystem>());

        }

    }
}
