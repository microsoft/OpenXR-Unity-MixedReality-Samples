// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
	/// <summary>
	/// Controls grab and release interactions for GameObjects in the scene with the ThrowableObject component.
	/// </summary>
	public class GrabAndThrowManager : MonoBehaviour
	{
		// The maximum allowed distance between the hand and an object to be grabbed.
		private const float GRAB_RANGE = 0.05f;

		// Information describing how an object is currently being held
		public class ObjectHeldData
		{
			public Rigidbody rigidbody;
			public Vector3 positionInHandSpace;
			public Quaternion rotationInHandSpace;
		}
		private ObjectHeldData[] objectsHeld = { null, null };
		private bool[] wasHandTapped = { false, false };

		private ThrowableObject[] throwableObjects;
		private Transform leftHandSpace;
		private Transform rightHandSpace;

		private void Awake()
		{
			// Create two empty GameObjects for their Transforms. These Transforms are useful for calculating and
			// maintaining offsets between the hands and the objects when grabbing and releasing them.
			GameObject leftHandSpaceGameObject = new GameObject("Left Hand Space");
			leftHandSpace = Instantiate(leftHandSpaceGameObject, Vector3.zero, Quaternion.identity).transform;
			GameObject rightHandSpaceGameObject = new GameObject("Right Hand Space");
			rightHandSpace = Instantiate(rightHandSpaceGameObject, Vector3.zero, Quaternion.identity).transform;

			// Find potential objects to pick up, searching the children of this gameObject.
			throwableObjects = GetComponentsInChildren<ThrowableObject>();
		}

		void Update()
		{
			UpdateForHand(XRNode.LeftHand, leftHandSpace, ref wasHandTapped[0], ref objectsHeld[0]);
			UpdateForHand(XRNode.RightHand, rightHandSpace, ref wasHandTapped[1], ref objectsHeld[1]);
		}

		// For each given hand: Grab, Move, or Release a GameObject
		void UpdateForHand(XRNode handNode, Transform handSpace, ref bool wasTapped, ref ObjectHeldData objectHeldData)
		{
			InputDevice device = InputDevices.GetDeviceAtXRNode(handNode);
			bool deviceHasData = device.TryGetFeatureValue(CommonUsages.isTracked, out bool deviceIsTracked);
			deviceHasData &= device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isDeviceTapped);
			deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
			deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 deviceAngularVelocity);
			deviceHasData &= device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition);
			deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation);

			if (!deviceHasData || !deviceIsTracked)
			{
				return;
			}

			handSpace.SetPositionAndRotation(devicePosition, deviceRotation);

			// If this hand should grab an object, look for one nearby
			if (objectHeldData == null && isDeviceTapped && !wasTapped)
			{
				ThrowableObject nearestObject = null;
				float lowestDist = Mathf.Infinity;
				foreach (var throwable in throwableObjects)
				{
					float dist = Vector3.Distance(devicePosition, throwable.collider.ClosestPoint(devicePosition));
					if (dist < GRAB_RANGE && dist < lowestDist)
					{
						lowestDist = dist;
						nearestObject = throwable;
					}
				}

				if (nearestObject != null)
				{
					// Grab the object
					objectHeldData = new ObjectHeldData { rigidbody = nearestObject.GetComponent<Rigidbody>() };
					objectHeldData.rigidbody.isKinematic = true;

					// Record the offset from the hand pose to the object origin pose
					objectHeldData.positionInHandSpace = handSpace.InverseTransformPoint(objectHeldData.rigidbody.position);
					objectHeldData.rotationInHandSpace = Quaternion.Inverse(deviceRotation) * objectHeldData.rigidbody.rotation;
				}
			}

			// If this hand is holding an object, update its position 
			if (objectHeldData != null)
			{
				objectHeldData.rigidbody.position = handSpace.TransformPoint(objectHeldData.positionInHandSpace);
				objectHeldData.rigidbody.rotation = deviceRotation * objectHeldData.rotationInHandSpace;
			}

			// If this hand should release the object, derive the new object velocity using the current hand velocity and relative object pose.
			if (objectHeldData != null && !isDeviceTapped)
			{
				// As the object is released, the hand and object share an angular velocity.
				objectHeldData.rigidbody.angularVelocity = deviceAngularVelocity;

				// As the object is released, we apply two velocities - the velocity of the hand relative to the world, 
				// and the velocity of the object's center of mass (assumed at the object origin) relative to the hand.
				objectHeldData.rigidbody.velocity = deviceVelocity +
					Vector3.Cross(objectHeldData.rigidbody.angularVelocity, objectHeldData.rigidbody.position - devicePosition);
				objectHeldData.rigidbody.isKinematic = false;
				objectHeldData = null;
			}

			wasTapped = isDeviceTapped;
		}
	}
}
