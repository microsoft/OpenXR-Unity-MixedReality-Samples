// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.ARFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary> 
    /// This sample detects air taps, creating new unpersisted anchors at the locations. Air tapping 
    /// again near these anchors toggles their persistence, backed by the <c>XRAnchorStore</c>.
    /// </summary>

    [RequireComponent(typeof(ARAnchorManager))]
    public class AnchorsSample : MonoBehaviour
    {

        [SerializeField]
        private GameObject m_anchorsContainer;

        private bool[] m_wasTapping = { true, true };
        private bool m_airTapToCreateEnabled = true;
        private bool m_airTapToCreateEnabledChangedThisUpdate = false;
        public void ToggleAirTapToCreateEnabled()
        {
            m_airTapToCreateEnabled = !m_airTapToCreateEnabled;
            m_airTapToCreateEnabledChangedThisUpdate = true;
        }

        private ARAnchorManager m_arAnchorManager;
        private List<ARAnchor> m_anchors = new List<ARAnchor>();
        private XRAnchorStore m_anchorStore = null;
        private Dictionary<TrackableId, string> m_incomingPersistedAnchors = new Dictionary<TrackableId, string>();

        async void Start()
        {
            m_arAnchorManager = GetComponent<ARAnchorManager>();
            if (!m_arAnchorManager.enabled || m_arAnchorManager.subsystem == null)
            {
                Debug.Log($"ARAnchorManager not enabled or available; sample anchor functionality will not be enabled.");
                return;
            }
            m_arAnchorManager.anchorsChanged += AnchorsChanged;

            m_anchorStore = await XRAnchorStore.LoadAsync(m_arAnchorManager.subsystem);
            if (m_anchorStore == null)
            {
                Debug.Log("XRAnchorStore not available, sample anchor persistence functionality will not be enabled.");
                return;
            }

            // Request all persisted anchors be loaded once the anchor store is loaded.
            foreach (string name in m_anchorStore.PersistedAnchorNames)
            {
                // When a persisted anchor is requested from the anchor store, LoadAnchor returns the TrackableId which
                // the anchor will use once it is loaded. To later recognize and recall the names of these anchors after
                // they have loaded, this dictionary stores the TrackableIds.
                TrackableId trackableId = m_anchorStore.LoadAnchor(name);
                m_incomingPersistedAnchors.Add(trackableId, name);
            }
        }

        public void AnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
        {
            foreach (var added in eventArgs.added)
            {
                // Keep any anchors created by this scene contained within the scene, for multi-scene management.
                added.transform.SetParent(m_anchorsContainer.transform, worldPositionStays: true);

#if !AR_FOUNDATION_4_1_1_OR_LATER
                // TryAddAnchor returns the anchor upon success, but it must also be reported in the next
                // AnchorsChanged update. These double adds are ignored, but other added anchors are processed.
                if (m_anchors.Contains(added)) continue;
#endif
                Debug.Log($"Anchor added from changed event: {added.trackableId}, OpenXR Handle: {added.GetOpenXRHandle()}");
                m_anchors.Add(added);

                // If this anchor being added was requested from the anchor store, it is recognized here
                if (m_incomingPersistedAnchors.ContainsKey(added.trackableId))
                {
                    added.GetComponent<SampleAnchor>().Name = m_incomingPersistedAnchors[added.trackableId];
                    added.GetComponent<SampleAnchor>().Persisted = true;
                    m_incomingPersistedAnchors.Remove(added.trackableId);
                }
            }

            foreach (var removed in eventArgs.removed)
            {
                Debug.Log($"Anchor removed from changed event: {removed.trackableId}");
                m_anchors.Remove(removed);
            }
        }

        private void LateUpdate()
        {
            // Air taps for anchor creation are handled in LateUpdate() to avoid race conditions with air taps to enable/disable anchor creation.
            for (int i = 0; i < 2; i++)
            {
                InputDevice device = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.RightHand : XRNode.LeftHand);
                bool isTapping;
                if (!device.TryGetFeatureValue(CommonUsages.primaryButton, out isTapping))
                    continue;

                if (isTapping && !m_wasTapping[i])
                    OnAirTapped(device);

                m_wasTapping[i] = isTapping;
            }

            m_airTapToCreateEnabledChangedThisUpdate = false;
        }

        public void OnAirTapped(InputDevice device)
        {
            if (!m_arAnchorManager.enabled || m_arAnchorManager.subsystem == null)
            {
                return;
            }

            Vector3 position;
            if (!device.TryGetFeatureValue(CommonUsages.devicePosition, out position))
                return;

            // First, check if there is a nearby anchor to persist/forget.
            if (m_anchors.Count > 0)
            {
                var (distance, closestAnchor) = m_anchors.Aggregate(
                    new Tuple<float, ARAnchor>(Mathf.Infinity, null),
                    (minPair, anchor) =>
                    {
                        float dist = (position - anchor.transform.position).magnitude;
                        return dist < minPair.Item1 ? new Tuple<float, ARAnchor>(dist, anchor) : minPair;
                    });

                if (distance < 0.1f)
                {
                    ToggleAnchorPersistence(closestAnchor);
                    return;
                }
            }

            // If there's no anchor nearby, create a new one.
            // If an air tap to enable/disable anchor creation just occurred, the tap is ignored here.
            if (m_airTapToCreateEnabled && !m_airTapToCreateEnabledChangedThisUpdate)
            {
                Vector3 headPosition;
                if (!InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
                    headPosition = Vector3.zero;

                AddAnchor(new Pose(position, Quaternion.LookRotation(position - headPosition, Vector3.up)));
            }
        }

        public void AddAnchor(Pose pose)
        {
#if AR_FOUNDATION_4_1_1_OR_LATER
            Debug.Log($"Instantiating new GameObject containing an ARAnchor");
            Instantiate(m_arAnchorManager.anchorPrefab, pose.position, pose.rotation);
#else
            ARAnchor newAnchor = m_arAnchorManager.AddAnchor(pose);
            if (newAnchor == null)
                Debug.Log($"Anchor creation failed");
            else
            {
                Debug.Log($"Anchor created: {newAnchor.trackableId}");
                m_anchors.Add(newAnchor);
            }
#endif
        }

        public void ToggleAnchorPersistence(ARAnchor anchor)
        {
            if (m_anchorStore == null)
            {
                Debug.Log($"Anchor Store was not available.");
                return;
            }

            SampleAnchor sampleAnchor = anchor.GetComponent<SampleAnchor>();
            if (!sampleAnchor.Persisted)
            {
                // For the purposes of this sample, randomly generate a name for the saved anchor.
                string newName = $"anchor/{Guid.NewGuid().ToString().Substring(0, 4)}";

                bool succeeded = m_anchorStore.TryPersistAnchor(anchor.trackableId, newName);
                if (!succeeded)
                {
                    Debug.Log($"Anchor could not be persisted: {anchor.trackableId}");
                    return;
                }

                Debug.Log($"Anchor persisted: {anchor.trackableId}");
                sampleAnchor.Name = newName;
                sampleAnchor.Persisted = true;
            }
            else
            {
                m_anchorStore.UnpersistAnchor(anchor.GetComponent<SampleAnchor>().Name);
                Debug.Log($"Anchor forgotten: {anchor.trackableId}");
                sampleAnchor.Name = "";
                sampleAnchor.Persisted = false;
            }
        }
    }
}
