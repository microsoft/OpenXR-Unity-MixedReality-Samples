using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Tests;
using Assert = UnityEngine.Assertions.Assert;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class OpenXRRuntimeSelectorTests : OpenXRInputTestsBase
    {
        [Test]
        public void NoAvailableRuntimesTest()
        {
            List<OpenXRRuntimeSelector.RuntimeDetector> detectorList = OpenXRRuntimeSelector.GenerateRuntimeDetectorList();
            Assert.IsTrue(detectorList.Count > 0);
            Assert.IsTrue(detectorList[0] is OpenXRRuntimeSelector.SystemDefault, "First choice should always be SystemDefault");
            Assert.IsTrue(detectorList[detectorList.Count - 1] is OpenXRRuntimeSelector.OtherRuntime, "Last choice should always be Other");
        }

        [Test]
        public void BuiltInRuntimesAsAvailableRuntimesTest()
        {
            // Simulate what happens if the AvailableRuntimes registry key is empty.
            // WindowsMR, SteamVR, and Oculus should all be added to the list.
            Dictionary<string, int> runtimePathToValue = new Dictionary<string, int>();
            List<OpenXRRuntimeSelector.RuntimeDetector> detectorList = OpenXRRuntimeSelector.GenerateRuntimeDetectorList(runtimePathToValue);
            Assert.IsTrue(detectorList.Count > 0);
            Assert.IsTrue(detectorList[0] is OpenXRRuntimeSelector.SystemDefault);
            Assert.IsTrue(detectorList[detectorList.Count - 1] is OpenXRRuntimeSelector.OtherRuntime);

            bool foundWindowsMRDetector = false;
            bool foundSteamVRDetector = false;
            bool foundOculusDetector = false;
            foreach (var detector in detectorList)
            {
                foundWindowsMRDetector |= detector is OpenXRRuntimeSelector.WindowsMRDetector;
                foundSteamVRDetector |= detector is OpenXRRuntimeSelector.SteamVRDetector;
                foundOculusDetector |= detector is OpenXRRuntimeSelector.OculusDetector;
            }

            Assert.IsTrue(foundWindowsMRDetector);
            Assert.IsTrue(foundSteamVRDetector);
            Assert.IsTrue(foundOculusDetector);
        }

        [Test]
        public void DiscoveredAvailableRuntimesTest()
        {
            OpenXRRuntimeSelector.WindowsMRDetector windowsMRDetector = new OpenXRRuntimeSelector.WindowsMRDetector();
            OpenXRRuntimeSelector.SteamVRDetector steamVRDetector = new OpenXRRuntimeSelector.SteamVRDetector();
            OpenXRRuntimeSelector.OculusDetector oculusDetector = new OpenXRRuntimeSelector.OculusDetector();
            string enabledRuntime = "enabledRuntime";
            string disabledRuntime = "disabledRuntime";

            Dictionary<string, int> runtimePathToValue = new Dictionary<string, int>()
            {
                { windowsMRDetector.jsonPath, 0},
                { steamVRDetector.jsonPath, 1},
                { enabledRuntime, 0 },
                { disabledRuntime, 1 }
            };

            List<OpenXRRuntimeSelector.RuntimeDetector> detectorList = OpenXRRuntimeSelector.GenerateRuntimeDetectorList(runtimePathToValue);
            Assert.IsTrue(detectorList.Count > 0);
            Assert.IsTrue(detectorList[0] is OpenXRRuntimeSelector.SystemDefault);
            Assert.IsTrue(detectorList[detectorList.Count - 1] is OpenXRRuntimeSelector.OtherRuntime);

            HashSet<string> detectedJsons = new HashSet<string>();
            foreach (var detector in detectorList)
            {
                detectedJsons.Add(detector.jsonPath);
            }

            Assert.IsTrue(detectedJsons.Contains(windowsMRDetector.jsonPath));
            Assert.IsFalse(detectedJsons.Contains(steamVRDetector.jsonPath));
            Assert.IsTrue(detectedJsons.Contains(oculusDetector.jsonPath));
            Assert.IsTrue(detectedJsons.Contains(enabledRuntime));
            Assert.IsFalse(detectedJsons.Contains(disabledRuntime));
        }

        [Test]
        public void TestSelectOtherRuntime()
        {
            // Get the OtherRuntime choice (verify that it's the last detector)
            var runtimeDetector = OpenXRRuntimeSelector.RuntimeDetectors[OpenXRRuntimeSelector.RuntimeDetectors.Count - 1];
            OpenXRRuntimeSelector.OtherRuntime otherRuntime = runtimeDetector as OpenXRRuntimeSelector.OtherRuntime;
            Assert.IsTrue(otherRuntime != null);

            // Save data that this test will modify to restore state after the test finishes.
            string previousOtherJsonPath = otherRuntime.jsonPath;
            string previousActiveRuntime = Environment.GetEnvironmentVariable(OpenXRRuntimeSelector.RuntimeDetector.k_RuntimeEnvKey);
            string previousSelectedRuntime = Environment.GetEnvironmentVariable(OpenXRRuntimeSelector.k_SelectedRuntimeEnvKey);

            try
            {
                // Set the other runtime value and set this as the selected runtime.
                string jsonPath = "testJsonPath";
                otherRuntime.SetOtherRuntimeJsonPath(jsonPath);
                OpenXRRuntimeSelector.SetSelectedRuntime(jsonPath);

                // Exit Edit Mode. Active runtime and Selected runtime should have the json path.
                OpenXRRuntimeSelector.ActivateOrDeactivateRuntime(PlayModeStateChange.ExitingEditMode);
                string activeRuntime = Environment.GetEnvironmentVariable(OpenXRRuntimeSelector.RuntimeDetector.k_RuntimeEnvKey);
                string selectedRuntime = Environment.GetEnvironmentVariable(OpenXRRuntimeSelector.k_SelectedRuntimeEnvKey);
                Assert.AreEqual(activeRuntime, jsonPath);
                Assert.AreEqual(selectedRuntime, jsonPath);

                // Simulate a refresh of the runtime detectors when entering play mode.
                // The OtherRuntime should still have the jsonPath from before.
                OpenXRRuntimeSelector.RefreshRuntimeDetectorList();
                runtimeDetector = OpenXRRuntimeSelector.RuntimeDetectors[OpenXRRuntimeSelector.RuntimeDetectors.Count - 1];
                otherRuntime = runtimeDetector as OpenXRRuntimeSelector.OtherRuntime;
                Assert.AreEqual(otherRuntime.jsonPath, jsonPath);

                // Enter Edit Mode. Active runtime should be cleared, selected runtime should still have the json path.
                OpenXRRuntimeSelector.ActivateOrDeactivateRuntime(PlayModeStateChange.EnteredEditMode);
                activeRuntime = Environment.GetEnvironmentVariable(OpenXRRuntimeSelector.RuntimeDetector.k_RuntimeEnvKey);
                selectedRuntime = Environment.GetEnvironmentVariable(OpenXRRuntimeSelector.k_SelectedRuntimeEnvKey);
                Assert.AreNotEqual(activeRuntime, jsonPath);
                Assert.AreEqual(selectedRuntime, jsonPath);

                // Refresh again. OtherRuntime and selected runtime should still have the json path.
                OpenXRRuntimeSelector.RefreshRuntimeDetectorList();
                runtimeDetector = OpenXRRuntimeSelector.RuntimeDetectors[OpenXRRuntimeSelector.RuntimeDetectors.Count - 1];
                otherRuntime = runtimeDetector as OpenXRRuntimeSelector.OtherRuntime;
                Assert.AreEqual(otherRuntime.jsonPath, jsonPath);
            }
            finally
            {
                otherRuntime.SetOtherRuntimeJsonPath(previousOtherJsonPath);
                Environment.SetEnvironmentVariable(OpenXRRuntimeSelector.RuntimeDetector.k_RuntimeEnvKey, previousActiveRuntime);
                Environment.SetEnvironmentVariable(OpenXRRuntimeSelector.k_SelectedRuntimeEnvKey, previousSelectedRuntime);
            }
        }
    }
}