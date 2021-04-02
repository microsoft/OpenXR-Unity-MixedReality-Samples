// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class FeatureUsageHandJointsManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The prefab to use for rendering hand joints in the scene. (optional)")]
        private GameObject handJointPrefab = null;

        private Hand leftHand = null;
        private Hand rightHand = null;

        private void OnDisable()
        {
            leftHand?.DisableHandJoints();
            rightHand?.DisableHandJoints();
        }

        private void Start()
        {
            leftHand = new Hand(handJointPrefab);
            rightHand = new Hand(handJointPrefab);
        }

        private void Update()
        {
            UpdateHandJoints(InputDeviceCharacteristics.Left, leftHand);
            UpdateHandJoints(InputDeviceCharacteristics.Right, rightHand);
        }

        private static void UpdateHandJoints(InputDeviceCharacteristics flag, Hand hand)
        {
            List<InputDevice> inputDeviceList = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking | flag, inputDeviceList);
            UnityEngine.XR.Hand xrHand = default;
            foreach (InputDevice device in inputDeviceList)
            {
                if (device.TryGetFeatureValue(CommonUsages.handData, out xrHand))
                {
                    break;
                }
            }

            if (xrHand != default)
            {
                hand?.UpdateHandJoints(xrHand);
            }
            else
            {
                // If we get here, we didn't successfully update hand joints for any tracked input device
                hand?.DisableHandJoints();
            }
        }
    }
}
