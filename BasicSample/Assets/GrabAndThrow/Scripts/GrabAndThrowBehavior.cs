using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class GrabAndThrowBehavior : MonoBehaviour
{
    //public BoxCollider VolumeBox;
    private bool[] m_wasRigidBodyPicked = { false, false };
    private bool canHoldRigidBody = false;
    private Rigidbody rigidBody;
    //private float distanceFromRigidBody;
    public BoxCollider VolumeBox;
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
                /*int maxColliders = 10;
                Collider[] hitColliders = new Collider[maxColliders];
                int numColliders = Physics.OverlapSphereNonAlloc(devicePosition, allowedDistanceFromRigidBody, hitColliders);
                for (int j = 0; j < numColliders; j++)
                {
                    if(hitColliders[j].name == "Stick")
                    {
                        canHoldRigidBody = true;
                    }
                }*/

                /*distanceFromRigidBody = Vector3.Distance(transform.position, devicePosition);
                if(distanceFromRigidBody < GRABBABLE_RANGE)
                {
                    Debug.Log(distanceFromRigidBody);
                    canHoldRigidBody = true;
                }*/

                Vector3 positionInVolumeCoordinates = VolumeBox.transform.InverseTransformPoint(devicePosition);
                if (Mathf.Abs(positionInVolumeCoordinates.x) < 0.05f &&
                    Mathf.Abs(positionInVolumeCoordinates.y) < 0.05f &&
                    Mathf.Abs(positionInVolumeCoordinates.z) < 0.5f)
                {
                    canHoldRigidBody = true;
                }

                if (m_wasRigidBodyPicked[i] && !isDeviceTapped)
                {
                    // Release the rigidbody using velocity
                    // Note that Unity's velocity is not expected to match the input's velocity, this is yet to be addressed 
                    rigidBody.velocity = deviceVelocity;
                    rigidBody.angularVelocity = deviceAngularVelocity;
                    rigidBody.isKinematic = false;
                    m_wasRigidBodyPicked[i] = false;
                }

                if (isDeviceTapped && canHoldRigidBody)
                {
                    // Pick the rigidbody
                    m_wasRigidBodyPicked[i] = true;
                    rigidBody.isKinematic = true;
                    transform.position = devicePosition;
                    transform.rotation = deviceRotation;
                    canHoldRigidBody = false;
                }
            }
        }
    }
}
