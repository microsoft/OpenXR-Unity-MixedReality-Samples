// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR;

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
		private class ObjectHeldData
		{
			public Rigidbody rigidbody;
			public Vector3 positionInHandSpace;
			public Quaternion rotationInHandSpace;
		}

		// Information describing the state of one of the hands
		private class HandData
		{
			public bool wasTapped = false;
			public Transform transform;
			public ObjectHeldData objectHeld = null;
		}

		private HandData m_leftHandData = new HandData();
		private HandData m_rightHandData = new HandData();
		private ThrowableObject[] m_throwableObjects;

		private void Awake()
		{
			// Create two empty GameObjects for their Transforms. These Transforms are useful for calculating and
			// maintaining offsets between the hands and the objects when grabbing and releasing them.
			GameObject leftHandSpaceGameObject = new GameObject("Left Hand Space");
			m_leftHandData.transform = Instantiate(leftHandSpaceGameObject, Vector3.zero, Quaternion.identity).transform;
			GameObject rightHandSpaceGameObject = new GameObject("Right Hand Space");
			m_rightHandData.transform = Instantiate(rightHandSpaceGameObject, Vector3.zero, Quaternion.identity).transform;

			// Find potential objects to pick up, searching the children of this gameObject.
			m_throwableObjects = GetComponentsInChildren<ThrowableObject>();
		}

		void Update()
		{
			UpdateForHand(XRNode.LeftHand, m_leftHandData);
			UpdateForHand(XRNode.RightHand, m_rightHandData);
		}

		private bool? TryGetIsGrabbing(InputDevice device)
		{
			bool isGrabbing;

			if (device.TryGetFeatureValue(CommonUsages.triggerButton, out isGrabbing))
			{
				return isGrabbing;
			}
			else if (device.TryGetFeatureValue(CommonUsages.gripButton, out isGrabbing))
			{
				return isGrabbing;
			}
			else if (device.TryGetFeatureValue(CommonUsages.primaryButton, out isGrabbing))
			{
				return isGrabbing;
			}
			return null;
		}

		// For each given hand: Grab, Move, or Release a GameObject
		void UpdateForHand(XRNode handNode, HandData handData)
		{
			InputDevice device = InputDevices.GetDeviceAtXRNode(handNode);
			bool deviceHasData = device.TryGetFeatureValue(CommonUsages.isTracked, out bool deviceIsTracked);
			bool? isGrabbing = TryGetIsGrabbing(device);
			bool isDeviceTapped = isGrabbing ?? false;
			deviceHasData &= (isGrabbing != null);
			deviceHasData &= device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition);
			deviceHasData &= device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation);

			if (!deviceHasData || !deviceIsTracked)
			{
				return;
			}

			// Update the hand's transform for use below
			handData.transform.SetPositionAndRotation(devicePosition, deviceRotation);

			// If this hand should grab an object, look for one nearby
			if (handData.objectHeld == null && isDeviceTapped && !handData.wasTapped)
			{
				ThrowableObject nearestObject = FindNearestObject(m_throwableObjects, devicePosition);
				if (nearestObject != null)
				{
					// Grab the object
					handData.objectHeld = new ObjectHeldData { rigidbody = nearestObject.GetComponent<Rigidbody>() };
					handData.objectHeld.rigidbody.isKinematic = true;

					// Record the offset from the hand pose to the object origin pose
					handData.objectHeld.positionInHandSpace = handData.transform.InverseTransformPoint(handData.objectHeld.rigidbody.position);
					handData.objectHeld.rotationInHandSpace = Quaternion.Inverse(deviceRotation) * handData.objectHeld.rigidbody.rotation;
				}
			}

			// If this hand is holding an object, update its pose. 
			if (handData.objectHeld != null)
			{
				handData.objectHeld.rigidbody.position = handData.transform.TransformPoint(handData.objectHeld.positionInHandSpace);
				handData.objectHeld.rigidbody.rotation = deviceRotation * handData.objectHeld.rotationInHandSpace;
			}

			// If this hand should release the object, derive the new object velocity using the current hand velocity and relative object pose.
			if (handData.objectHeld != null && !isDeviceTapped)
			{
				// Get the device's velocity and angular velocity
				bool hasData = device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
				hasData &= device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 deviceAngularVelocity);
				if (!hasData)
				{
					deviceVelocity = Vector3.zero;
					deviceAngularVelocity = Vector3.zero;
				}

				// As the object is released, the hand and object share an angular velocity.
				handData.objectHeld.rigidbody.angularVelocity = deviceAngularVelocity;

				// As the object is released, we apply two velocities - the velocity of the hand relative to the world, 
				// and the velocity of the object's center of mass relative to the hand.
				handData.objectHeld.rigidbody.velocity = deviceVelocity +
					Vector3.Cross(handData.objectHeld.rigidbody.angularVelocity, handData.objectHeld.rigidbody.worldCenterOfMass - devicePosition);
				handData.objectHeld.rigidbody.isKinematic = false;
				handData.objectHeld = null;
			}

			handData.wasTapped = isDeviceTapped;
		}

		ThrowableObject FindNearestObject(ThrowableObject[] objects, Vector3 position)
		{
			ThrowableObject nearestObject = null;
			float lowestDist = Mathf.Infinity;

			foreach (var throwable in objects)
			{
				foreach (var collider in throwable.colliders)
				{
					float dist = Vector3.Distance(position, collider.ClosestPoint(position));
					if (dist < GRAB_RANGE && dist < lowestDist)
					{
						lowestDist = dist;
						nearestObject = throwable;
					}
				}
			}

			return nearestObject;
		}
	}
}
