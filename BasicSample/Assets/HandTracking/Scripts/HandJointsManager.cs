// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class HandJointsManager : MonoBehaviour
    {
        private enum HandJointSource
        {
            FeatureUsage,
            OpenXRExtension,
        }

        [SerializeField, Tooltip("The hand joint source for the left hand.")]
        private HandJointSource leftHandJointSource = HandJointSource.FeatureUsage;

        [SerializeField, Tooltip("The hand joint source for the right hand.")]
        private HandJointSource rightHandJointSource = HandJointSource.FeatureUsage;

        [SerializeField, Tooltip("The prefab to use for rendering hand joints in the scene. (optional)")]
        private GameObject handJointPrefab = null;

        private Hand leftHand = null;
        private Hand rightHand = null;

        private static readonly HandJointLocation[] HandJointLocations = new HandJointLocation[HandTracker.JointCount];

        private void OnEnable()
        {
            Application.onBeforeRender += Application_onBeforeRender;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= Application_onBeforeRender;
            leftHand?.DisableHandJoints();
            rightHand?.DisableHandJoints();
        }

        private void Application_onBeforeRender()
        {
            if (leftHandJointSource == HandJointSource.OpenXRExtension)
            {
                UpdateHandJointsUsingOpenXRExtension(HandTracker.Left, leftHand, FrameTime.OnBeforeRender);
            }
            if (rightHandJointSource == HandJointSource.OpenXRExtension)
            {
                UpdateHandJointsUsingOpenXRExtension(HandTracker.Right, rightHand, FrameTime.OnBeforeRender);
            }
        }

        private void Start()
        {
            leftHand = new Hand(handJointPrefab);
            rightHand = new Hand(handJointPrefab);
        }

        private void Update()
        {
            if (leftHandJointSource == HandJointSource.OpenXRExtension)
            {
                UpdateHandJointsUsingOpenXRExtension(HandTracker.Left, leftHand, FrameTime.OnUpdate);
            }
            else
            {
                UpdateHandJointsUsingFeatureUsage(InputDeviceCharacteristics.Left, leftHand);
            }

            if (rightHandJointSource == HandJointSource.OpenXRExtension)
            {
                UpdateHandJointsUsingOpenXRExtension(HandTracker.Right, rightHand, FrameTime.OnUpdate);
            }
            else
            {
                UpdateHandJointsUsingFeatureUsage(InputDeviceCharacteristics.Right, rightHand);
            }
        }

        private static void UpdateHandJointsUsingFeatureUsage(InputDeviceCharacteristics flag, Hand hand)
        {
            List<InputDevice> inputDeviceList = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking, inputDeviceList);
            UnityEngine.XR.Hand xrHand = default;
            foreach (InputDevice device in inputDeviceList)
            {
                if (device.characteristics.HasFlag(flag) && device.TryGetFeatureValue(CommonUsages.handData, out xrHand))
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

        private static void UpdateHandJointsUsingOpenXRExtension(HandTracker handTracker, Hand hand, FrameTime frameTime)
        {
            if (handTracker.TryLocateHandJoints(frameTime, HandJointLocations))
            {
                hand?.UpdateHandJoints(HandJointLocations);
            }
            else
            {
                hand?.DisableHandJoints();
            }
        }
    }
}
