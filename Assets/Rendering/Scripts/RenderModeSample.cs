// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// Changes and displays the current status of the render mode.
    /// </summary>
    public class RenderModeSample : MonoBehaviour
    {
        private TextMesh text;

        private void Awake() => text = GetComponent<TextMesh>();

        private void Update()
        {
            string renderModeString = "Not recognized";
            if (OpenXRSettings.Instance.renderMode == OpenXRSettings.RenderMode.SinglePassInstanced)
                renderModeString = "Single-Pass Instanced";
            else if (OpenXRSettings.Instance.renderMode == OpenXRSettings.RenderMode.MultiPass)
                renderModeString = "Multi-Pass";

            text.text = $"Current render mode: {renderModeString}";
        }

        private void TrySetRenderMode(OpenXRSettings.RenderMode renderMode) => OpenXRSettings.Instance.renderMode = renderMode;
        public void SetRenderModeMultiPass() => TrySetRenderMode(OpenXRSettings.RenderMode.MultiPass);
        public void SetRenderModeSinglePassInstanced() => TrySetRenderMode(OpenXRSettings.RenderMode.SinglePassInstanced);
    }
}