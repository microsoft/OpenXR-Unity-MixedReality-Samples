// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class RuntimeInfo : MonoBehaviour, ITextProvider
    {
        private const int m_countToNextUpdate = 60;
        private int m_countTillNextUpdate = 0;
        private string m_text;
		
        private void Start()
        {
            arSession = FindObjectOfType<ARSession>();
        }


        string ITextProvider.UpdateText()
        {
            if (m_countTillNextUpdate-- <= 0)
            {
                m_countTillNextUpdate = m_countToNextUpdate;

                Version mrPluginVersion = typeof(OpenXRContext).Assembly.GetName().Version;
                string runtimeName = string.IsNullOrEmpty(OpenXRRuntime.name)
                    ? "OpenXR Runtime is not available."
                    : $"{OpenXRRuntime.name} {OpenXRRuntime.version}";
                var trackingMode = (arSession == null ? "Tracking unknown" : arSession.currentTrackingMode.ToString());
                m_text = $"{Application.productName}\n" +
                    $"Unity Version: {Application.unityVersion}\n" +
                    $"Unity OpenXR Plugin Version: {OpenXRRuntime.pluginVersion}\n" +
                    $"Mixed Reality OpenXR Plugin {mrPluginVersion}\n" +
                    $"{runtimeName}\n" +
                    $"{GetDisplayInfo()}\n" +
                    $"AR Session State: {ARSession.state}, {trackingMode}\n" +
                    $"{GetTrackingOriginMode()}\n" +
                    $"{GetTrackingStates()}";
            }
            return m_text;
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

    }
}
