// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Checks for haptics support in the controller and if supported, it sends an impulse when the trigger is pressed.
    /// </summary>
    public class ApplyHaptics : MonoBehaviour
    {
        private readonly List<InputDevice> controllerHapticDevices = new List<InputDevice>();

        private void InitializeDevices()
        {
            controllerHapticDevices.Clear();
            List<InputDevice> controllerInputDevices = new List<InputDevice>();

            // Get all controller input devices
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, controllerInputDevices);

            // Filter devices with haptics capabilities
            foreach (InputDevice controller in controllerInputDevices)
            {
                if (controller.TryGetHapticCapabilities(out HapticCapabilities hapticCapabilities) && hapticCapabilities.supportsImpulse)
                {
                    controllerHapticDevices.Add(controller);
                }
            }
        }

        private void OnDeviceConnected(InputDevice device)
        {
            InitializeDevices();
        }

        private void OnDeviceDisconnected(InputDevice device)
        {
            InitializeDevices();
        }

        private void Start()
        {
            InitializeDevices();
        }

        private void OnEnable()
        {
            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;
        }

        // Update is called once per frame
        private void Update()
        {
            foreach (InputDevice controller in controllerHapticDevices)
            {   
                if (controller.TryGetFeatureValue(CommonUsages.trigger, out float trigger) && trigger > 0)
                {
                    // send haptics impulse with channel set to 0 and amplitude set to the trigger value
                    controller.SendHapticImpulse(0, trigger);
                }
            }
        }
    }
}
