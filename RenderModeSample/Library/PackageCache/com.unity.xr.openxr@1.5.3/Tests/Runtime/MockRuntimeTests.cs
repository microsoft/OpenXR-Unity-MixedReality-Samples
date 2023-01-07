using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Tests
{
    /// <summary>
    /// Defines tests that validate the MockRuntime itself.
    /// </summary>
    internal class MockRuntimeTests : OpenXRLoaderSetup
    {
        [UnityTest]
        public IEnumerator TransitionToState ()
        {
            InitializeAndStart();
            yield return new WaitForXrFrame();

            Assert.AreEqual(MockRuntime.sessionState, XrSessionState.Focused, "MockRuntime must be in focused state for this test to work correctly");
            Assert.IsTrue(MockRuntime.TransitionToState(XrSessionState.Visible, false), "Failed to transition to visible state");

            Assert.AreEqual(MockRuntime.sessionState, XrSessionState.Visible);
        }

        [UnityTest]
        public IEnumerator TransitionToStateForced ()
        {
            InitializeAndStart();
            yield return new WaitForXrFrame();

            Assert.IsFalse(MockRuntime.TransitionToState(XrSessionState.Synchronized, false), "Synchronized state must be an invalid transition for this test to be valid");
            Assert.IsTrue(MockRuntime.TransitionToState(XrSessionState.Synchronized, true), "Force state transition should not return false");

            yield return new WaitForXrFrame();

            Assert.IsTrue(MockRuntime.sessionState == XrSessionState.Synchronized);
        }

        [UnityTest]
        public IEnumerator CreateSessionFailure()
        {
            bool sawCreateSession = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionCreate))
                {
                    sawCreateSession = true;
                }

                return true;
            };

            MockRuntime.SetFunctionCallback("xrCreateSession", (name) => XrResult.RuntimeFailure);

            base.InitializeAndStart();

            yield return null;

            Assert.IsFalse(sawCreateSession);
        }

        static XrResult[] beginSessionSuccessResults = new XrResult[]
        {
            XrResult.Success,
            XrResult.LossPending
        };

        [UnityTest]
        public IEnumerator BeginSessionSuccessWithValues([ValueSource("beginSessionSuccessResults")]
            XrResult successResult)
        {
            var states = new List<XrSessionState>();
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionStateChange))
                {
                    var newState = (XrSessionState)((MockRuntime.XrSessionStateChangedParams) param).NewState;
                    states.Add(newState);
                }

                return true;
            };

            MockRuntime.SetFunctionCallback("xrBeginSession", (name) => successResult);

            base.InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRDisplaySubsystem>());

            switch (successResult)
            {
                case XrResult.Success:
                    Assert.IsTrue(states.Contains(XrSessionState.Ready));
                    Assert.IsTrue(states.Contains(XrSessionState.Synchronized));
                    Assert.IsTrue(states.Contains(XrSessionState.Visible));
                    Assert.IsTrue(states.Contains(XrSessionState.Focused));
                    break;

                case XrResult.LossPending:
                    Assert.IsTrue(states.Contains(XrSessionState.Ready));
                    Assert.IsFalse(states.Contains(XrSessionState.Synchronized));
                    Assert.IsFalse(states.Contains(XrSessionState.Visible));
                    Assert.IsFalse(states.Contains(XrSessionState.Focused));
                    break;
            }
        }

        [UnityTest]
        public IEnumerator BeginSessionFailure()
        {
            var states = new List<XrSessionState>();
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionStateChange))
                {
                    var newState = (XrSessionState)((MockRuntime.XrSessionStateChangedParams)param).NewState;
                    states.Add(newState);
                }

                return true;
            };

            MockRuntime.SetFunctionCallback("xrBeginSession", (name) => XrResult.RuntimeFailure);

            InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRDisplaySubsystem>());

            Assert.IsTrue(states.Contains(XrSessionState.Ready));
            Assert.IsFalse(states.Contains(XrSessionState.Synchronized));
            Assert.IsFalse(states.Contains(XrSessionState.Visible));
            Assert.IsFalse(states.Contains(XrSessionState.Focused));
        }

#if false
        // TEST WAS UNSTABLE, DISABLING FOR RELEASE ONLY
        [UnityTest]
        public IEnumerator TestRequestExitShutsdownSubsystems()
        {
            bool sawSessionDestroy = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionDestroy))
                {
                    sawSessionDestroy = true;
                }

                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRDisplaySubsystem>());

            var wait = new WaitForLoaderShutdown(Loader);
            MockDriver.RequestExitSession(MockRuntime.Instance.XrSession);

            yield return wait;

            Assert.IsTrue(sawSessionDestroy);
        }
#endif

        [UnityTest]
        public IEnumerator RequestExitSession ()
        {
            InitializeAndStart();

            // Wait for a single XrFrame to make sure OpenXR is up and running
            yield return new WaitForXrFrame();

            // Request the session exit which should shutdown OpenXR
            MockRuntime.RequestExitSession();

            // Wait for OpenXR to shutdown
            yield return new WaitForLoaderShutdown();
        }

        [UnityTest]
        public IEnumerator CauseInstanceLoss()
        {
            bool instanceLost = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnInstanceLossPending))
                {
                    instanceLost = true;
                }

                return true;
            };

            InitializeAndStart();

            yield return null;

            MockRuntime.CauseInstanceLoss();

            yield return new WaitForLoaderShutdown();

            Assert.IsTrue(instanceLost);
        }

        [UnityTest]
        public IEnumerator DisplayTransparent()
        {
            AddExtension(MockRuntime.XR_UNITY_mock_test);

            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnInstanceCreate))
                {
                    MockRuntime.ChooseEnvironmentBlendMode(XrEnvironmentBlendMode.Additive);
                }

                return true;
            };

            base.InitializeAndStart();

            yield return null;
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(displays);
            Assert.AreEqual(false, displays[0].displayOpaque);
        }

        [UnityTest]
        public IEnumerator DisplayOpaque()
        {
            base.InitializeAndStart();

            yield return null;
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(displays);
            Assert.AreEqual(true, displays[0].displayOpaque);
        }

        [UnityTest]
        public IEnumerator MultipleEnvironmentBlendModes()
        {
            //Mock available environmentBlendModes are Opaque and Additive in mock_runtime.cpp EnumerateEnvironmentBlendModes.
            AddExtension(MockRuntime.XR_UNITY_mock_test);
            base.InitializeAndStart();
            yield return null;

            //check default mode is Opaque.
            Assert.AreEqual(XrEnvironmentBlendMode.Opaque, MockRuntime.GetXrEnvironmentBlendMode());

            //Switch to another supported mode - Additive.
            MockRuntime.ChooseEnvironmentBlendMode(XrEnvironmentBlendMode.Additive);
            yield return new WaitForXrFrame(2);
            Assert.AreEqual(XrEnvironmentBlendMode.Additive, MockRuntime.GetXrEnvironmentBlendMode());

            //Set to unsupported mode - Alpha_blend
            MockRuntime.ChooseEnvironmentBlendMode(XrEnvironmentBlendMode.AlphaBlend);
            yield return new WaitForXrFrame(2);
            Assert.AreNotEqual(XrEnvironmentBlendMode.AlphaBlend, MockRuntime.GetXrEnvironmentBlendMode());
        }

        [UnityTest]
        public IEnumerator BoundaryPoints()
        {
            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRInputSubsystem>(), "Input subsystem failed to properly start!");

            MockRuntime.SetReferenceSpaceBounds(XrReferenceSpaceType.Stage, new Vector2 {x = 1.0f, y = 3.0f});

            yield return null;

            var input = Loader.GetLoadedSubsystem<XRInputSubsystem>();
            Assert.That(() => input, Is.Not.Null);

            input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

            yield return null;

            var points = new List<Vector3>();
            Assert.IsTrue(input.TryGetBoundaryPoints(points), "Failed to get boundary points!");
            Assert.That(() => points.Count, Is.EqualTo(4), "Incorrect number of boundary points received!");

            var comparer = new Vector3EqualityComparer(10e-6f);

            Assert.That(points[0], Is.EqualTo(new Vector3 {x = -0.5f, y = 0.0f, z = -1.5f}).Using(comparer));
            Assert.That(points[1], Is.EqualTo(new Vector3 {x = -0.5f, y = 0.0f, z = 1.5f}).Using(comparer));
            Assert.That(points[2], Is.EqualTo(new Vector3 {x = 0.5f, y = 0.0f, z = 1.5f}).Using(comparer));
            Assert.That(points[3], Is.EqualTo(new Vector3 {x = 0.5f, y = 0.0f, z = -1.5f}).Using(comparer));
        }

        [UnityTest]
        public IEnumerator NoBoundaryPoints ()
        {
            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRInputSubsystem>(), "Input subsystem failed to properly start!");

            var input = Loader.GetLoadedSubsystem<XRInputSubsystem>();
            Assert.That(() => input, Is.Not.Null);

            input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

            yield return null;

            var points = new List<Vector3>();
            input.TryGetBoundaryPoints(points);
            Assert.That(() => points.Count, Is.EqualTo(0), "Incorrect number of boundary points received!");
        }

        [UnityTest]
        public IEnumerator BoundryPointsForTrackingOrigin()
        {
            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRInputSubsystem>(), "Input subsystem failed to properly start!");

            MockRuntime.SetReferenceSpaceBounds(XrReferenceSpaceType.Stage, new Vector2 {x = 1.0f, y = 3.0f});

            yield return null;

            var input = Loader.GetLoadedSubsystem<XRInputSubsystem>();
            Assert.That(() => input, Is.Not.Null);

            input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

            yield return null;

            var points = new List<Vector3>();
            Assert.IsTrue(input.TryGetBoundaryPoints(points), "Failed to get boundary points!");
            Assert.That(() => points.Count, Is.EqualTo(4), "Incorrect number of boundary points received!");

            input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);

            yield return null;

            points.Clear();
            input.TryGetBoundaryPoints(points);
            Assert.That(() => points.Count, Is.EqualTo(0), "Incorrect number of boundary points received!");
        }

        [UnityTest]
        public IEnumerator BeforeFunctionCallbackSuccess()
        {
            var createInstanceCalled = false;

            MockRuntime.SetFunctionCallback("xrCreateInstance", (name) =>
            {
                createInstanceCalled = true;
                return XrResult.Success;
            });

            InitializeAndStart();

            yield return new WaitForXrFrame(1);

            Assert.IsTrue(createInstanceCalled, "xrCreateInstance callback was not called");
        }

        [Test]
        public void BeforeFunctionCallbackError ()
        {
            var createInstanceCalled = false;

            MockRuntime.SetFunctionCallback("xrCreateInstance", (name) =>
            {
                createInstanceCalled = true;
                return XrResult.RuntimeFailure;
            });

            LogAssert.ignoreFailingMessages = true;
            InitializeAndStart();
            LogAssert.ignoreFailingMessages = false;

            Assert.IsTrue(OpenXRLoaderBase.Instance == null, "OpenXR instance should have failed to initialize");

            Assert.IsTrue(createInstanceCalled, "xrCreateInstance callback was not called");
        }

        [UnityTest]
        public IEnumerator AfterFunctionCallback ( )
        {
            var createInstanceCalled = false;
            var createInstanceSuccess = false;

            MockRuntime.SetFunctionCallback("xrCreateInstance", (name, result) =>
            {
                createInstanceCalled = true;
                createInstanceSuccess = result == XrResult.Success;
            });

            InitializeAndStart();

            yield return new WaitForXrFrame(1);

            Assert.IsTrue(createInstanceCalled, "xrCreateInstance callback was not called");
            Assert.IsTrue(createInstanceSuccess, "xrCreateInstance result was not XR_SUCCESS");
        }

        [UnityTest]
        public IEnumerator CallbacksClearedOnLoaderShutdown()
        {
            MockRuntime.SetFunctionCallback("xrBeginSession", (func) => XrResult.Success);

            InitializeAndStart();

            yield return new WaitForXrFrame(1);

            StopAndShutdown();

            Assert.IsTrue(OpenXRLoader.Instance == null, "OpenXR should not be running");
            Assert.IsNull(MockRuntime.GetBeforeFunctionCallback("xrBeginSession"), "Callback should have been cleared when loader shut down");
        }
    }
}
