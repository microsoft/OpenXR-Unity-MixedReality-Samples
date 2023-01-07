#if UNITY_EDITOR && ENABLE_TEST_SUPPORT
#define TEST_SUPPORT
#endif

#if TEST_SUPPORT

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine.TestTools;

[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Tests.Editor")]

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class OpenXRLoaderTests : OpenXRLoaderSetup
    {
        public override void BeforeTest()
        {
            RemoveLoaderAndSettings();
            DestroyLoaderAndSettings();
            SetupLoaderAndSettings();
            base.BeforeTest();
        }

        public override void AfterTest()
        {
            base.AfterTest();
            RemoveLoaderAndSettings();
            DestroyLoaderAndSettings();
            SetupLoaderAndSettings();
        }

#pragma warning disable CS0649
        public struct ExpectedLogMessage
        {
            public LogType logType;
            public string matchingRegex;
        }
#pragma warning restore CS0649

        public struct StateTransition
        {
            public OpenXRLoader.LoaderState targetState;
            public bool expectedInitializeReturn;
            public bool expectedStartReturn;
            public bool expectedStopReturn;
            public bool expectedDeinitializeReturn;
            public ExpectedLogMessage[] expectedLogMessages;
        }

        public static List<StateTransition> stateTransitions = new List<StateTransition>()
        {
            new StateTransition()
            {
                targetState = OpenXRLoader.LoaderState.InitializeAttempted,
                expectedInitializeReturn = false,
                expectedStartReturn = false,
                expectedStopReturn = false,
                expectedDeinitializeReturn = true,
                expectedLogMessages = new ExpectedLogMessage[] { },
            },
            new StateTransition()
            {
                targetState = OpenXRLoader.LoaderState.Initialized,
                expectedInitializeReturn = true,
                expectedStartReturn = true,
                expectedStopReturn = true,
                expectedDeinitializeReturn = true,
                expectedLogMessages = new ExpectedLogMessage[] { },
            },
            new StateTransition()
            {
                targetState = OpenXRLoader.LoaderState.StartAttempted,
                expectedInitializeReturn = true,
                expectedStartReturn = false,
                expectedStopReturn = true,
                expectedDeinitializeReturn = true,
                expectedLogMessages = new ExpectedLogMessage[] { },
            },
            new StateTransition()
            {
                targetState = OpenXRLoader.LoaderState.Started,
                expectedInitializeReturn = true,
                expectedStartReturn = true,
                expectedStopReturn = true,
                expectedDeinitializeReturn = true,
                expectedLogMessages = new ExpectedLogMessage[] { },
            },
            new StateTransition()
            {
                targetState = OpenXRLoader.LoaderState.StopAttempted,
                expectedInitializeReturn = true,
                expectedStartReturn = true,
                expectedStopReturn = false,
                expectedDeinitializeReturn = false,
                expectedLogMessages = new ExpectedLogMessage[] { },
            },
            new StateTransition()
            {
                targetState = OpenXRLoader.LoaderState.Stopped,
                expectedInitializeReturn = true,
                expectedStartReturn = true,
                expectedStopReturn = true,
                expectedDeinitializeReturn = true,
                expectedLogMessages = new ExpectedLogMessage[] { },
            },
            new StateTransition()
            {
                targetState = OpenXRLoader.LoaderState.DeinitializeAttempted,
                expectedInitializeReturn = true,
                expectedStartReturn = true,
                expectedStopReturn = true,
                expectedDeinitializeReturn = false,
                expectedLogMessages = new ExpectedLogMessage[] { },
            },
        };

        [UnityTest]
        [Category("Loader Tests")]
        public IEnumerator StateTransitionValidation([ValueSource("stateTransitions")] StateTransition stateTransition)
        {
            Assert.IsNotNull(Loader);
            if (Loader != null)
                Loader.targetLoaderState = stateTransition.targetState;

            bool ret = Loader.Initialize();
            yield return null;
            Assert.AreEqual(stateTransition.expectedInitializeReturn, ret);

            ret = Loader.Start();
            yield return null;
            Assert.AreEqual(stateTransition.expectedStartReturn, ret);

            ret = Loader.Stop();
            yield return null;
            Assert.AreEqual(stateTransition.expectedStopReturn, ret);

            ret = Loader.Deinitialize();
            yield return null;
            Assert.AreEqual(stateTransition.expectedDeinitializeReturn, ret);

            foreach (var expectedLog in stateTransition.expectedLogMessages)
            {
                LogAssert.Expect(expectedLog.logType, expectedLog.matchingRegex);
            }

            Loader.targetLoaderState = OpenXRLoaderBase.LoaderState.Uninitialized;
        }

        [UnityTest]
        [Category("Loader Tests")]
        public IEnumerator CanStopAndStartMultipleTimes()
        {
            Assert.IsNotNull(Loader);

            bool ret = Loader.Initialize();
            Assert.IsTrue(ret);
            yield return null;
            Assert.AreEqual(OpenXRLoader.LoaderState.Initialized, Loader.currentLoaderState);


            for (int i = 0; i < 5; i++)
            {
                ret = Loader.Start();
                Assert.IsTrue(ret);
                yield return null;
                Assert.AreEqual(OpenXRLoader.LoaderState.Started, Loader.currentLoaderState);

                ret = Loader.Stop();
                Assert.IsTrue(ret);
                yield return null;
                Assert.AreEqual(OpenXRLoader.LoaderState.Stopped, Loader.currentLoaderState);
            }

            ret = Loader.Deinitialize();
            Assert.IsTrue(ret);
            yield return null;
            Assert.AreEqual(OpenXRLoader.LoaderState.Uninitialized, Loader.currentLoaderState);

        }


    }
}
#endif // TEST_SUPPORT
