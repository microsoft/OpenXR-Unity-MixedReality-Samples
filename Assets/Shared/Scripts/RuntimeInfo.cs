// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

        string opaque = "Unknown";
        if (displays.Count > 0)
        {
            opaque = displays[0].displayOpaque ? "Yes" : "No";
        }

        runtimeText.text = $"{Application.productName}\n" +
            $"Unity Version: {Application.unityVersion}\n" +
            $"Unity OpenXR Plugin Version: {OpenXRRuntime.pluginVersion}\n" +
            $"{OpenXRRuntime.name} {OpenXRRuntime.version}\n" +
            $"Display Opaque: {opaque}";
    }
}
