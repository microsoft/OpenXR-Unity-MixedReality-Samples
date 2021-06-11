using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class GrabAndThrowBehavior : MonoBehaviour
{
    private bool[] m_wasTriggerPressed = { false, false };
    private const float SPEED_MULTIPLIER = 10;

    // Update is called once per frame
    private void Update()
    {
        // get trigger and velocity data from the left device
        for (int i = 0; i < 2; i++)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.RightHand : XRNode.LeftHand);
            bool deviceHasData = device.TryGetFeatureValue(CommonUsages.isTracked, out bool deviceIsTracked);
            deviceHasData &= device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isdeviceTapped);
            deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
            deviceHasData &= device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition);
            deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation);

            if (deviceHasData && deviceIsTracked)
            {
                if (m_wasTriggerPressed[i] && !isdeviceTapped)
                {
                    // release the cube using velocity
                    GetComponent<Rigidbody>().velocity = deviceVelocity * SPEED_MULTIPLIER;
                    GetComponent<Rigidbody>().isKinematic = false;
                    m_wasTriggerPressed[i] = false;
                }

                if (!m_wasTriggerPressed[0] && isdeviceTapped)
                {
                    // pick the cube
                    m_wasTriggerPressed[i] = true;
                    transform.localPosition = devicePosition + (deviceRotation * Vector3.forward);
                    transform.localRotation = deviceRotation;
                    GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }
    }
}
