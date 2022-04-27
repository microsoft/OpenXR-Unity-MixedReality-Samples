// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class RuntimeInfo : MonoBehaviour, ITextProvider
    {
        private const int m_countToNextUpdate = 60;
        private int m_countTillNextUpdate = 0;
        private string m_text;

        string ITextProvider.UpdateText()
        {
            if (m_countTillNextUpdate-- <= 0)
            {
                m_countTillNextUpdate = m_countToNextUpdate;

                Version mrPluginVersion = typeof(OpenXRContext).Assembly.GetName().Version;
                string runtimeName = string.IsNullOrEmpty(OpenXRRuntime.name)
                    ? "OpenXR Runtime is not available."
                    : $"{OpenXRRuntime.name} {OpenXRRuntime.version}";

                m_text = $"{Application.productName}\n" +
                    $"Unity Version: {Application.unityVersion}\n" +
                    $"Unity OpenXR Plugin Version: {OpenXRRuntime.pluginVersion}\n" +
                    $"Mixed Reality OpenXR Plugin {mrPluginVersion}\n" +
                    $"{runtimeName}\n" +
                    $"{GetDisplayInfo()}";
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
    }
}
