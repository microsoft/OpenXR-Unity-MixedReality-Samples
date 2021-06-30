// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class FollowEyeGaze : MonoBehaviour
    {
        [SerializeField, Tooltip("The material to use when eye gaze isn't tracked")]
        private Material untrackedMaterial = null;

        private static readonly List<InputDevice> InputDeviceList = new List<InputDevice>();
        private InputDevice eyeTrackingDevice = default(InputDevice);
        private Renderer materialRenderer = null;
        private Material trackedMaterial = null;

        /// <summary>
        /// Toggles the enabled state of this script to actively follow eye gaze or not.
        /// </summary>
        public void ToggleFollow() => gameObject.SetActive(!gameObject.activeSelf);

        private void Awake()
        {
            materialRenderer = GetComponent<Renderer>();
            if (materialRenderer != null)
            {
                trackedMaterial = materialRenderer.material;
            }
        }

        private void Update()
        {
            if (!eyeTrackingDevice.isValid)
            {
                InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, InputDeviceList);
                if (InputDeviceList.Count > 0)
                {
                    eyeTrackingDevice = InputDeviceList[0];
                }

                if (!eyeTrackingDevice.isValid)
                {
                    Debug.LogWarning($"Unable to acquire eye tracking device. Have permissions been granted?");
                    return;
                }
            }

            // Gets gaze data from the device.
            bool hasData = eyeTrackingDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked);
            hasData &= eyeTrackingDevice.TryGetFeatureValue(EyeTrackingUsages.gazePosition, out Vector3 position);
            hasData &= eyeTrackingDevice.TryGetFeatureValue(EyeTrackingUsages.gazeRotation, out Quaternion rotation);

            if (isTracked && hasData)
            {
                if (materialRenderer != null)
                {
                    materialRenderer.material = trackedMaterial;
                }

                transform.localPosition = position + (rotation * Vector3.forward);
                transform.localRotation = rotation;
            }
            else
            {
                if (materialRenderer != null)
                {
                    materialRenderer.material = untrackedMaterial;
                }
            }
        }
    }
}
