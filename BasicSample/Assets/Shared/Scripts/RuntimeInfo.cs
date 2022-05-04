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

        private void Start()
        {
            arSession = FindObjectOfType<ARSession>();
        }

        private void Update()
        {
            if (m_frameCountSinceLastUpdate-- <= 0)
            {
                m_frameCountSinceLastUpdate = m_frameCountToUpdateFrame;

                var trackingMode = (arSession == null ? "Tracking unknown" : arSession.currentTrackingMode.ToString());
                var info = $"{Application.productName}\n" +
                    $"Unity Version: {Application.unityVersion}\n" +
                    $"Unity OpenXR Plugin Version: {OpenXRRuntime.pluginVersion}\n" +
                    $"Mixed Reality OpenXR Plugin {mrPluginVersion}\n" +
                    $"{OpenXRRuntime.name} {OpenXRRuntime.version}\n" +
                    $"{GetDisplayInfo()}\n" +
                    $"AR Session State: {ARSession.state}, {trackingMode}\n" +
                    $"{GetTrackingOriginMode()}\n" +
                    $"{GetTrackingStates()}";

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

        private static string GetTrackingStates()
        {
            return $"Head: {GetTrackingState(XRNode.Head)} Left Hand: {GetTrackingState(XRNode.LeftHand)} Right Hand: {GetTrackingState(XRNode.RightHand)}";
        }

        private static string GetTrackingState(XRNode xRNode)
        {
            var trackingState = "Not found";
            var inputDevice = InputDevices.GetDeviceAtXRNode(xRNode);
            if (inputDevice.isValid && inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked))
            {
                trackingState = tracked ? "Tracked" : "Not tracked";
            }

            return trackingState;
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
