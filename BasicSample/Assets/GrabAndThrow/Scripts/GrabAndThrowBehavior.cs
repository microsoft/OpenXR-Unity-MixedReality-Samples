using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class GrabAndThrowBehavior : MonoBehaviour
{
    private bool[] m_wasRigidBodyPicked = { false, false };
    private const float allowedDistanceFromRigidBody = 1f;
    private float distanceFromRigidBody;
    private bool canHoldRigidBody = true;
    private Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Get trigger and velocity data from the left device
        for (int i = 0; i < 2; i++)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.RightHand : XRNode.LeftHand);
            bool deviceHasData = device.TryGetFeatureValue(CommonUsages.isTracked, out bool deviceIsTracked);
            deviceHasData &= device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isDeviceTapped);
            deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
            deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 deviceAngularVelocity);
            deviceHasData &= device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition);
            deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation);

            if (deviceHasData && deviceIsTracked)
            {
                distanceFromRigidBody = Vector3.Distance(transform.localPosition, devicePosition);
                if(distanceFromRigidBody > allowedDistanceFromRigidBody)
                {
                    canHoldRigidBody = false;
                }
                if(canHoldRigidBody)
                {
                    if (m_wasRigidBodyPicked[i] && !isDeviceTapped)
                    {
                        // Release the rigidbody using velocity
                        // Note that Unity's velocity is not expected to match the input's velocity, this is yet to be addressed 
                        rigidBody.velocity = deviceVelocity;
                        rigidBody.angularVelocity = deviceAngularVelocity;
                        rigidBody.isKinematic = false;
                        m_wasRigidBodyPicked[i] = false;
                    }

                    if (isDeviceTapped)
                    {
                        // Pick the rigidbody
                        m_wasRigidBodyPicked[i] = true;
                        transform.localPosition = devicePosition;
                        transform.localRotation = deviceRotation;
                        rigidBody.isKinematic = true;
                    }

                }

            }
        }
    }
}
