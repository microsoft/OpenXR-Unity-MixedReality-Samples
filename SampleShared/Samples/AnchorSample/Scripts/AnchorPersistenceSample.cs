// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.ARFoundation;
using Microsoft.MixedReality.OpenXR.Sample;
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
    [RequireComponent(typeof(ARSessionOrigin))]
    public class AnchorPersistenceSample : MonoBehaviour
    {
        private bool[] m_wasTapping = { true, true };
        private bool m_airTapToCreateEnabled = true;
        private bool m_airTapToCreateEnabledChangedThisUpdate = false;

        public void ToggleAirTapToCreateEnabled()
        {
            m_airTapToCreateEnabled = !m_airTapToCreateEnabled;
            m_airTapToCreateEnabledChangedThisUpdate = true;
        }

        private ARSessionOrigin m_arSessionOrigin; // Used for ARSessionOrigin.trackablesParent
        private ARAnchorManager m_arAnchorManager;
        private List<ARAnchor> m_anchors = new List<ARAnchor>();
        private XRAnchorStore m_anchorStore = null;
        private Dictionary<TrackableId, string> m_incomingPersistedAnchors = new Dictionary<TrackableId, string>();

        protected async void OnEnable()
        {
            // Set up references in this script to ARFoundation components on this GameObject.
            m_arSessionOrigin = GetComponent<ARSessionOrigin>();
            if (!TryGetComponent(out m_arAnchorManager) || !m_arAnchorManager.enabled || m_arAnchorManager.subsystem == null)
            {
                Debug.Log($"ARAnchorManager not enabled or available; sample anchor functionality will not be enabled.");
                return;
            }

            m_arAnchorManager.anchorsChanged += AnchorsChanged;

            m_anchorStore = await m_arAnchorManager.LoadAnchorStoreAsync();
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

        protected void OnDisable()
        {
            if (m_arAnchorManager != null)
            {
                m_arAnchorManager.anchorsChanged -= AnchorsChanged;
                m_anchorStore = null;
                m_incomingPersistedAnchors.Clear();
            }
        }

        private void AnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
        {
            foreach (var added in eventArgs.added)
            {
                Debug.Log($"Anchor added from ARAnchorsChangedEvent: {added.trackableId}, OpenXR Handle: {added.GetOpenXRHandle()}");
                ProcessAddedAnchor(added);
            }

            foreach (ARAnchor updated in eventArgs.updated)
            {
                if (updated.TryGetComponent(out PersistableAnchorVisuals sampleAnchorVisuals))
                {
                    sampleAnchorVisuals.TrackingState = updated.trackingState;
                }
            }

            foreach (var removed in eventArgs.removed)
            {
                Debug.Log($"Anchor removed: {removed.trackableId}");
                m_anchors.Remove(removed);
            }
        }

        private void ProcessAddedAnchor(ARAnchor anchor)
        {
            // If this anchor being added was requested from the anchor store, it is recognized here
            if (m_incomingPersistedAnchors.TryGetValue(anchor.trackableId, out string name))
            {
                if (anchor.TryGetComponent(out PersistableAnchorVisuals sampleAnchorVisuals))
                {
                    sampleAnchorVisuals.Name = name;
                    sampleAnchorVisuals.Persisted = true;
                    sampleAnchorVisuals.TrackingState = anchor.trackingState;
                }
                m_incomingPersistedAnchors.Remove(anchor.trackableId);
            }

            m_anchors.Add(anchor);
        }

        private bool IsTapping(InputDevice device)
        {
            bool isTapping;

            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out isTapping))
            {
                return isTapping;
            }
            else if (device.TryGetFeatureValue(CommonUsages.primaryButton, out isTapping))
            {
                return isTapping;
            }
            return false;
        }

        private void LateUpdate()
        {
            // Air taps for anchor creation are handled in LateUpdate() to avoid race conditions with air taps to enable/disable anchor creation.
            for (int i = 0; i < 2; i++)
            {
                InputDevice device = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.RightHand : XRNode.LeftHand);

                bool isTapping = IsTapping(device);
                if (isTapping && !m_wasTapping[i])
                {
                    OnAirTapped(device);
                }
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
#pragma warning disable 0618 // warning CS0618: 'ARAnchorManager.AddAnchor(Pose)' is obsolete
            ARAnchor newAnchor = m_arAnchorManager.AddAnchor(pose);
#pragma warning restore 0618
            if (newAnchor == null)
            {
                Debug.Log($"Anchor creation failed");
            }
            else
            {
                Debug.Log($"Anchor created: {newAnchor.trackableId}");
            }
        }

        public void ToggleAnchorPersistence(ARAnchor anchor)
        {
            if (m_anchorStore == null)
            {
                Debug.Log($"Anchor Store was not available.");
                return;
            }

            PersistableAnchorVisuals sampleAnchorVisuals = anchor.GetComponent<PersistableAnchorVisuals>();
            if (!sampleAnchorVisuals.Persisted)
            {
                // For the purposes of this sample, randomly generate a name for the saved anchor.
                string newName = $"anchor/{Guid.NewGuid().ToString().Substring(0, 4)}";

                bool succeeded = m_anchorStore.TryPersistAnchor(anchor.trackableId, newName);
                if (!succeeded)
                {
                    Debug.Log($"Anchor could not be persisted: {anchor.trackableId}");
                    return;
                }

                ChangeAnchorVisuals(anchor, newName, true);
            }
            else
            {
                m_anchorStore.UnpersistAnchor(sampleAnchorVisuals.Name);
                ChangeAnchorVisuals(anchor, "", false);
            }
        }

        public void AnchorStoreClear()
        {
            m_anchorStore.Clear();
            // Change visual for every anchor in the scene
            foreach (ARAnchor anchor in m_anchors)
            {
                ChangeAnchorVisuals(anchor, "", false);
            }
        }

        public void ClearSceneAnchors()
        {
            // Remove every anchor in the scene. This does not affect their persistence
            foreach (ARAnchor anchor in m_anchors)
            {
                m_arAnchorManager.subsystem.TryRemoveAnchor(anchor.trackableId);
            }
            m_anchors.Clear();
        }

        private void ChangeAnchorVisuals(ARAnchor anchor, string newName, bool isPersisted)
        {
            PersistableAnchorVisuals sampleAnchorVisuals = anchor.GetComponent<PersistableAnchorVisuals>();
            Debug.Log(isPersisted ? $"Anchor {anchor.trackableId} with name {newName} persisted" : $"Anchor {anchor.trackableId} with name {sampleAnchorVisuals.Name} unpersisted");
            sampleAnchorVisuals.Name = newName;
            sampleAnchorVisuals.Persisted = isPersisted;
        }
    }
}
