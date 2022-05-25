using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class HapticsSupport : MonoBehaviour
    {
        private readonly List<InputDevice> controllerInputDevices = new List<InputDevice>();

        // Update is called once per frame
        void Update()
        {
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, controllerInputDevices);

            foreach (InputDevice controller in controllerInputDevices)
            {
                if (controller.TryGetFeatureValue(CommonUsages.trigger, out float trigger) && trigger > 0)
                {
                    if (controller.TryGetHapticCapabilities(out HapticCapabilities hapticCapabilities) && hapticCapabilities.supportsImpulse)
                    {
                        controller.SendHapticImpulse(0, trigger);
                    }
                }
            }
        }
    }
}
