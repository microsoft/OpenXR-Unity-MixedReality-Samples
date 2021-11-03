// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class RuntimeInfo : MonoBehaviour
    {
        [SerializeField]
        private TextMesh runtimeText = null;

        private void Update()
        {
            if (m_frameCountSinceLastUpdate-- <= 0)
            {
                m_frameCountSinceLastUpdate = m_frameCountToUpdateFrame;

                var info = $"{Application.productName}\n" +
                    $"Unity Version: {Application.unityVersion}\n" +
                    $"Unity OpenXR Plugin Version: {OpenXRRuntime.pluginVersion}\n" +
                    $"Mixed Reality OpenXR Plugin {mrPluginVersion}\n" +
                    $"{OpenXRRuntime.name} {OpenXRRuntime.version}\n" +
                    $"{GetDisplayInfo()}";

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

        private readonly static Version mrPluginVersion = typeof(OpenXRContext).Assembly.GetName().Version;
        private readonly static Version mrtkVersion = typeof(MixedRealityToolkit).Assembly.GetName().Version;
        private const int m_frameCountToUpdateFrame = 60;
        private int m_frameCountSinceLastUpdate = 0;
    }
}
