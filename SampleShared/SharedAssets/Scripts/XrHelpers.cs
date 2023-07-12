// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public static class XrHelpers
    {
        public static XRInputSubsystem GetXRInputSubsystem()
        {
            XRGeneralSettings xrSettings = XRGeneralSettings.Instance;
            if (xrSettings != null)
            {
                XRManagerSettings xrManager = xrSettings.Manager;
                if (xrManager != null)
                {
                    XRLoader xrLoader = xrManager.activeLoader;
                    if (xrLoader != null)
                    {
                        return xrLoader.GetLoadedSubsystem<XRInputSubsystem>();
                    }
                }
            }
            return null;
        }
    }
}