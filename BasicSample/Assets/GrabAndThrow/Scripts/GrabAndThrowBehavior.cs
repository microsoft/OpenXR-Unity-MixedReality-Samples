// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
	[RequireComponent(typeof(Rigidbody))]
	public class GrabAndThrowBehavior : MonoBehaviour
	{
		private const float PICKABLE_RANGE = 0.5f;

		[SerializeField]
		public BoxCollider PickupDetectionCollider;

		private bool[] m_wasDeviceTapped = { false, false };
		private Rigidbody rigidBody;

		private XRNode? handHeldBy = null;
		private Transform handSpace;
		private Vector3 positionInHandSpace;
		private Quaternion rotationInHandSpace;

		private void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
			GameObject handSpaceGameObject = new GameObject("Hand Space");
			handSpace = Instantiate(handSpaceGameObject, Vector3.zero, Quaternion.identity).transform;
		}

		// Update is called once per frame
		private void Update()
		{
			// Repeat this update once for each hand
			for (int i = 0; i < 2; i++)
			{
				XRNode handNode = (i == 0) ? XRNode.RightHand : XRNode.LeftHand;
				InputDevice device = InputDevices.GetDeviceAtXRNode(handNode);
				bool deviceHasData = device.TryGetFeatureValue(CommonUsages.isTracked, out bool deviceIsTracked);
				deviceHasData &= device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isDeviceTapped);
				deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
				deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 deviceAngularVelocity);
				deviceHasData &= device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition);
				deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation);

				if (deviceHasData && deviceIsTracked)
				{
					handSpace.SetPositionAndRotation(devicePosition, deviceRotation);

					// Check if we need to grab the object
					if (handNode != handHeldBy && isDeviceTapped && !m_wasDeviceTapped[i])
					{
						Vector3 positionInVolumeCoordinates = PickupDetectionCollider.transform.InverseTransformPoint(devicePosition);
						bool canBePickedUp = false;
						if (Mathf.Abs(positionInVolumeCoordinates.x) < PICKABLE_RANGE &&
							Mathf.Abs(positionInVolumeCoordinates.y) < PICKABLE_RANGE &&
							Mathf.Abs(positionInVolumeCoordinates.z) < PICKABLE_RANGE)
						{
							canBePickedUp = true;
						}

						if (canBePickedUp)
						{
							Grab(handNode, devicePosition, deviceRotation);
						}
					}

					// Check if we need to update the object position (holding it)
					if (handHeldBy == handNode)
					{
						transform.position = handSpace.TransformPoint(positionInHandSpace);
						transform.rotation = deviceRotation * rotationInHandSpace;
					}

					// Check if we need to drop the object
					if (handNode == handHeldBy && !isDeviceTapped)
					{
						Drop(devicePosition, deviceVelocity, deviceAngularVelocity);
					}

					m_wasDeviceTapped[i] = isDeviceTapped;
				}
			}
		}

		private void Grab(XRNode handNode, Vector3 devicePosition, Quaternion deviceRotation)
		{
			rigidBody.isKinematic = true;
			handHeldBy = handNode;

			// Record the offset from the hand pose to the object origin pose
			positionInHandSpace = handSpace.InverseTransformPoint(rigidBody.position);
			rotationInHandSpace = Quaternion.Inverse(deviceRotation) * rigidBody.rotation;
		}

		// Release the rigidbody, deriving the object's velocity from the current hand velocity and object pose.
		private void Drop(Vector3 devicePosition, Vector3 deviceVelocity, Vector3 deviceAngularVelocity)
		{
			// As the object is released, the hand and object share an angular velocity.
			rigidBody.angularVelocity = -deviceAngularVelocity;

			// As the object is released, we apply two velocities - the velocity of the hand relative to the world, 
			// and the velocity of the object's center of mass (assumed at the origin) relative to the hand.
			rigidBody.velocity = deviceVelocity + Vector3.Cross(rigidBody.angularVelocity, rigidBody.position - devicePosition);

			rigidBody.isKinematic = false;
			handHeldBy = null;
		}
	}
}