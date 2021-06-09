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
        // get trigger and velocity data from the left controller
        for (int i = 0; i < 2; i++)
        {
            InputDevice controller = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.RightHand : XRNode.LeftHand);
            bool controllerHasData = controller.TryGetFeatureValue(CommonUsages.isTracked, out bool controllerIsTracked);
            controllerHasData &= controller.TryGetFeatureValue(CommonUsages.trigger, out float controllerTriggerValue);
            controllerHasData &= controller.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 controllerVelocity);
            controllerHasData &= controller.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 controllerPosition);
            controllerHasData &= controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion controllerRotation);

            if (controllerHasData && controllerIsTracked)
            {
                if (m_wasTriggerPressed[i] && controllerTriggerValue == 0)
                {
                    // release the cube using velocity
                    GetComponent<Rigidbody>().velocity = controllerVelocity * SPEED_MULTIPLIER;
                    GetComponent<Rigidbody>().isKinematic = false;
                    m_wasTriggerPressed[i] = false;
                }

                if (!m_wasTriggerPressed[0] && controllerTriggerValue > 0.5)
                {
                    // pick the cube
                    m_wasTriggerPressed[i] = true;
                    transform.localPosition = controllerPosition + (controllerRotation * Vector3.forward);
                    transform.localRotation = controllerRotation;
                    GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }
    }
}
