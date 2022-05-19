// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.OpenXR.Sample;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

public class InternalRuntimeInfo : MonoBehaviour, ITextProvider
{
    private const int m_countToUpdateFrame = 10;
    private int m_countUntilNextUpdate = 0;
    private string m_text;

    private void Update()
    {
        if (m_countUntilNextUpdate-- <= 0)
        {
            m_countUntilNextUpdate = m_countToUpdateFrame;

            m_text =
                $"\nPFN_xrGetInstanceProcAddr = 0x{OpenXRContext.Current.PFN_xrGetInstanceProcAddr:X16}";
        }
    }

    string ITextProvider.UpdateText()
    {
        return m_text;
    }
}
