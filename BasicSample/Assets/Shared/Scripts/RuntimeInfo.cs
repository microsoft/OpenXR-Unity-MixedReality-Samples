// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class RuntimeInfo : MonoBehaviour
    {
        [SerializeField]
        private TextMesh runtimeText = null;

        [SerializeField]
        private ARSession arSession = null;

        private void Update()
        {
            if (m_frameCountSinceLastUpdate-- <= 0)
            {
                m_frameCountSinceLastUpdate = m_frameCountToUpdateFrame;
                arSession = FindObjectOfType<ARSession>();

                var trackingMode = (arSession == null ? "Tracking unknown" : arSession.currentTrackingMode.ToString());
                var info = $"{Application.productName}\n" +
                    $"Unity Version: {Application.unityVersion}\n" +
                    $"Unity OpenXR Plugin Version: {OpenXRRuntime.pluginVersion}\n" +
                    $"Mixed Reality OpenXR Plugin {mrPluginVersion}\n" +
                    $"{OpenXRRuntime.name} {OpenXRRuntime.version}\n" +
                    $"{GetDisplayInfo()}\n" +
                    $"AR Session State: {ARSession.state}, {trackingMode}\n" +
                    $"{GetTrackingOriginMode()}\n" +
                    $"{GetTrackingInfo()}";

                if (runtimeText.text != info)
                {
                    runtimeText.text = info;
                }
            }
        }

        private static string GetDisplayInfo()
        {
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(displays);

            if (displays.Count == 0)
            {
                return "No XR display";
            }
            else if (displays.Count > 1)
            {
                return "More than one XR displays";
            }
            else
            {
                var display = displays[0];
                var opaque = display.displayOpaque ? "Opaque" : "Transparent";
                var renderMode = OpenXRSettings.Instance.renderMode;
                var depthMode = OpenXRSettings.Instance.depthSubmissionMode;

                return $"{opaque}, {renderMode}, {depthMode}";
            }
        }

        private static string GetTrackingInfo()
        {
            var leftHandTracked = "Not tracked";
            var rightHandTracked = "Not tracked";
            var headTracked = "Not tracked";
            var leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            var rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            var headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (leftHandDevice.isValid && leftHandDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool lTracked) && lTracked)
            {
                leftHandTracked = "Tracked";
            }

            if (rightHandDevice.isValid && rightHandDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool rTracked) && rTracked)
            {
                rightHandTracked = "Tracked";
            }

            if (headDevice.isValid && headDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool hTracked) && hTracked)
            {
                headTracked = "Tracked";
            }
            return $"Left Hand: {leftHandTracked} Right Hand: {rightHandTracked} Head: {headTracked}";
        }

        private static string GetTrackingOriginMode()
        {
            XRInputSubsystem inputSubsystem = GetXRInputSubsystem();
            if (inputSubsystem == null)
            {
                return "Tracking origin mode: Unknown";
            }
            else
            {
                return $"Tracking origin mode: {inputSubsystem.GetTrackingOriginMode()}";
            }
        }

        private static XRInputSubsystem GetXRInputSubsystem()
        {
            XRGeneralSettings xrSettings = XRGeneralSettings.Instance;
            if (xrSettings == null)
            {
                Debug.LogWarning($"GetXRInputSubsystem: XRGeneralSettings is null.");
                return null;
            }

            XRManagerSettings xrManager = xrSettings.Manager;
            if (xrManager == null)
            {
                Debug.LogWarning($"GetXRInputSubsystem: XRManagerSettings is null.");
                return null;
            }

            XRLoader xrLoader = xrManager.activeLoader;
            if (xrLoader == null)
            {
                Debug.LogWarning($"GetXRInputSubsystem: XRLoader is null.");
                return null;
            }

            XRInputSubsystem xrInputSubsystem = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();
            if (xrInputSubsystem == null)
            {
                Debug.LogWarning($"GetXRInputSubsystem: XRInputSubsystem is null.");
                return null;
            }
            return xrInputSubsystem;
        }

        private readonly static Version mrPluginVersion = typeof(OpenXRContext).Assembly.GetName().Version;
        private readonly static Version mrtkVersion = typeof(MixedRealityToolkit).Assembly.GetName().Version;
        private const int m_frameCountToUpdateFrame = 60;
        private int m_frameCountSinceLastUpdate = 0;
    }
}
