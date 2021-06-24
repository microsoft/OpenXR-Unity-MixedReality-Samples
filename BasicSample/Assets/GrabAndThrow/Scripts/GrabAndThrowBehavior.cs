using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class GrabAndThrowBehavior : MonoBehaviour
{
    //public BoxCollider VolumeBox;
    private bool[] m_wasRigidBodyPicked = { false, false };
    private bool canPickRigidBody = false;
    private Rigidbody rigidBody;
    private const float PICKABLE_RANGE = 0.5f;
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
                    Vector3 positionInVolumeCoordinates = VolumeBox.transform.InverseTransformPoint(devicePosition);
                    if (Mathf.Abs(positionInVolumeCoordinates.x) < PICKABLE_RANGE &&
                        Mathf.Abs(positionInVolumeCoordinates.y) < PICKABLE_RANGE &&
                        Mathf.Abs(positionInVolumeCoordinates.z) < PICKABLE_RANGE)
                    {
                        canPickRigidBody = true;
                    }
                    else
                    {
                        canPickRigidBody = false;
                    }

                    if(canPickRigidBody)
                    {
                        // Pick the rigidbody
                        m_wasRigidBodyPicked[i] = true;
                        rigidBody.isKinematic = true;
                        
                        //approach1: this approach picks the stick but sets center of mass position to the hand position instead of reflecting the position where it is picked from
                        transform.position = devicePosition;

                        // approach2: adding offset to the center of mass of the stick in x direction, similar approaches are followed in other directions and all directions. 
                        //transform.position = devicePosition + new Vector3(transform.position.x - devicePosition.x, 0, 0);

                        //approach3: calculated relative position of hand wrt the stick and added it to center of mass. it picks up from the point where it is supposed to pick it up but the stick kind of weighs down
                        //transform.position = devicePosition + getRelativePosition(transform, devicePosition);

                        transform.rotation = deviceRotation;
                        canPickRigidBody = false;
                    }
                }
            }
        }
    }
    private Vector3 getRelativePosition(Transform origin, Vector3 position) 
    {
        Vector3 distance = position - origin.position;
        Vector3 relativePosition = Vector3.zero;
        relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
        relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
        relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);
        
        return relativePosition;
    }
}
