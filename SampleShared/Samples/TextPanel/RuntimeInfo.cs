// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class RuntimeInfo : MonoBehaviour, ITextProvider
    {
        private const int m_countToNextUpdate = 20;
        private int m_countTillNextUpdate = 0;
        private string m_text;
        private ARSession m_arSession;
        private XRInputSubsystem m_inputSubsystem;

        private void Start()
        {
            m_arSession = FindObjectOfType<ARSession>();
            m_inputSubsystem = GetXRInputSubsystem();
        }


        string ITextProvider.UpdateText()
        {
            if (m_countTillNextUpdate-- <= 0)
            {
                m_countTillNextUpdate = m_countToNextUpdate;

                string runtimeName = string.IsNullOrEmpty(OpenXRRuntime.name)
                    ? "OpenXR Runtime is not available."
                    : $"{OpenXRRuntime.name} {OpenXRRuntime.version}";

                m_text = $"{Application.productName}" +
                    $"\nUnity Version: {Application.unityVersion}" +
                    $"\nUnity OpenXR Plugin Version: {OpenXRRuntime.pluginVersion}" +
                    $"\nMixed Reality OpenXR Plugin {typeof(OpenXRContext).Assembly.GetName().Version}" +
                    $"\n{runtimeName}" +
                    $"\n{GetDisplayInfo()}" +
                    $"\nAR Session State: {ARSession.state}, {GetTrackingMode()}, {GetOriginMode()}" +
                    $"\nHead tracking state: {GetTrackingState(XRNode.Head)}" +
                    $"\nLeft Hand tracking state: {GetTrackingState(XRNode.LeftHand)}" +
                    $"\nRight Hand tracking state: {GetTrackingState(XRNode.RightHand)}";
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

        private string GetTrackingMode()
        {
            return m_arSession == null ? "Unknown Tracking Mode" : m_arSession.currentTrackingMode.ToString();
        }

        private string GetOriginMode()
        {
            return m_inputSubsystem == null ? "Unknown Origin Mode" : m_inputSubsystem.GetTrackingOriginMode().ToString();
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
