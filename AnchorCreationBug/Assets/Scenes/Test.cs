// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.Tests

{
    internal static class Test
    {
        internal static void PrintTestMessage(string message,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            Debug.Log($"[{caller}, {lineNumber}] Info : {message}");
        }

        internal static void VerifyTrue(bool value, string message,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            if (!value)
            {
                Debug.LogError($"[{caller}, {lineNumber}] Error : {message}");
            }
        }

        internal static bool GetTrackingState(XRNode xRNode)
        {
            var inputDevice = InputDevices.GetDeviceAtXRNode(xRNode);
            if (inputDevice.isValid && inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked))
            {
                return tracked;
            }
            return false;
        }
    }
}