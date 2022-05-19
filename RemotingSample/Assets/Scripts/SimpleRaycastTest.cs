using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// Raycast to all planes in the scene, creating a temporary visual for each raycast hit.
    /// </summary>

    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class SimpleRaycastTest : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] m_raycastRayIndicators = { null, null };

        [SerializeField]
        private GameObject[] m_raycastHitIndicators = { null, null };

        private static readonly InputFeatureUsage<Vector3> PointerPosition = new InputFeatureUsage<Vector3>("PointerPosition");
        private static readonly InputFeatureUsage<Quaternion> PointerRotation = new InputFeatureUsage<Quaternion>("PointerRotation");

        private ARPlaneManager m_planeManager;
        private ARRaycastManager m_raycastManager;

        private void Awake()
        {
            m_planeManager = GetComponent<ARPlaneManager>();
            m_raycastManager = GetComponent<ARRaycastManager>();
        }

        private void Update()
        {
            // Raycasting and updating the scene for each hand
            for (int i = 0; i < 2; i++)
            {
                InputDevice device = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.LeftHand : XRNode.RightHand);
                if (!device.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked) || !isTracked ||
                    !device.TryGetFeatureValue(PointerPosition, out Vector3 handPosition) ||
                    !device.TryGetFeatureValue(PointerRotation, out Quaternion handRotation))
                {
                    m_raycastRayIndicators[i].SetActive(false);
                    continue;
                }

                // Update the raycast ray visuals
                m_raycastRayIndicators[i].transform.SetPositionAndRotation(handPosition, handRotation);
                m_raycastRayIndicators[i].SetActive(true);

                // Try to raycast to planes
                Vector3 handForward = new Pose(handPosition, handRotation).forward;
                List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
                if (m_raycastManager.Raycast(new Ray(handPosition, handForward), raycastHits, TrackableType.PlaneWithinPolygon))
                {
                    // Raycast hits are sorted by distance, so the first one will be the closest hit.
                    var raycastHit = raycastHits[0];

                    // Update the raycast hit visuals
                    m_raycastHitIndicators[i].transform.SetPositionAndRotation(raycastHit.pose.position, raycastHit.pose.rotation);
                    m_raycastHitIndicators[i].SetActive(true);
                }
                else
                {
                    m_raycastHitIndicators[i].SetActive(false);
                }
            }
        }
    }
}
