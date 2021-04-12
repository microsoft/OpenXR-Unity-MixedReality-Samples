// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

public class RuntimeInfo : MonoBehaviour
{
    [SerializeField]
    private TextMesh runtimeText = null;

    private void Start()
    {
        var displays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(displays);

        var version = typeof(OpenXRContext).Assembly.GetName().Version;

        string opaque = "Unknown";
        if (displays.Count > 0)
        {
            opaque = displays[0].displayOpaque ? "Opaque" : "Transparent";
        }

        runtimeText.text = $"{Application.productName}\n" +
            $"Unity Version: {Application.unityVersion}\n" +
            $"Unity OpenXR Plugin Version: {OpenXRRuntime.pluginVersion}\n" +
            $"Mixed Reality OpenXR Plugin {version}\n" +
            $"{OpenXRRuntime.name} {OpenXRRuntime.version}\n" +
            $"Display {opaque}";
    }
}
