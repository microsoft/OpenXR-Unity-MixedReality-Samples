// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Raycast to all planes in the scene, creating an anchor for each raycast hit.
    /// </summary>

    [RequireComponent(typeof(ARAnchorManager))]
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class PlacementOnPlanes : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] m_raycastRayIndicators = { null, null };

        private static readonly InputFeatureUsage<Vector3> PointerPosition = new InputFeatureUsage<Vector3>("PointerPosition");
        private static readonly InputFeatureUsage<Quaternion> PointerRotation = new InputFeatureUsage<Quaternion>("PointerRotation");
        private bool[] m_wasHandTapping = { true, true };

        private ARPlaneManager m_planeManager;
        private ARRaycastManager m_raycastManager;
        private ARAnchorManager m_anchorManager;

        // Only planes which are being hovered or have anchors on them should be visible.
        private TrackableId[] m_hoveredPlanes = { TrackableId.invalidId, TrackableId.invalidId };
        private HashSet<TrackableId> m_planesWithAnchors = new HashSet<TrackableId>();

        private void Awake()
        {
            m_planeManager = GetComponent<ARPlaneManager>();
            m_raycastManager = GetComponent<ARRaycastManager>();
            m_anchorManager = GetComponent<ARAnchorManager>();
        }

        private void Update()
        {
            // Deselect the previously hovered planes
            foreach (TrackableId id in m_hoveredPlanes)
            {
                if (id == TrackableId.invalidId)
                    continue;

                ARPlane plane = m_planeManager.GetPlane(id);
                if (plane == null)
                    continue;

                // Don't hide planes which have anchors on them
                if (m_planesWithAnchors.Contains(id))
                    continue;

                plane.gameObject.SetActive(false);
            }

            // Raycasting and updating the scene for each hand
            for (int i = 0; i < 2; i++)
            {
                InputDevice device = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.LeftHand : XRNode.RightHand);
                if (!device.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked) || !isTracked ||
                    !device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isHandTapping) ||
                    !device.TryGetFeatureValue(PointerPosition, out Vector3 handPosition) ||
                    !device.TryGetFeatureValue(PointerRotation, out Quaternion handRotation))
                {
                    m_raycastRayIndicators[i].SetActive(false);
                    m_hoveredPlanes[i] = TrackableId.invalidId;
                    m_wasHandTapping[i] = true; // Prevent detecting a tap when we begin tracking the hand again
                    continue;
                }

                bool handTappedThisUpdate = isHandTapping && !m_wasHandTapping[i];
                m_wasHandTapping[i] = isHandTapping;

                m_raycastRayIndicators[i].transform.SetPositionAndRotation(handPosition, handRotation);
                m_raycastRayIndicators[i].SetActive(true);

                Vector3 handForward = new Pose(handPosition, handRotation).forward;
                List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
                if (m_raycastManager.Raycast(new Ray(handPosition, handForward), raycastHits, TrackableType.PlaneWithinPolygon))
                {
                    // Raycast hits are sorted by distance, so the first one will be the closest hit.
                    var raycastHit = raycastHits[0];
                    ARPlane plane = raycastHit.trackable as ARPlane;

                    plane.gameObject.SetActive(true);
                    m_hoveredPlanes[i] = plane.trackableId;

                    if (handTappedThisUpdate)
                    {
                        m_anchorManager.AttachAnchor(plane, raycastHit.pose);
                        m_planesWithAnchors.Add(plane.trackableId);
                    }
                }
                else
                {
                    m_hoveredPlanes[i] = TrackableId.invalidId;
                }
            }
        }
    }
}
