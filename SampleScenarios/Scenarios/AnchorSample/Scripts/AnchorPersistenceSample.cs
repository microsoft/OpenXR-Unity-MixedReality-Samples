﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.ARFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

#if USE_ARFOUNDATION_5_OR_NEWER
using ARSessionOrigin = Unity.XR.CoreUtils.XROrigin;
#else
using ARSessionOrigin = UnityEngine.XR.ARFoundation.ARSessionOrigin;
#endif

#if ENABLE_WINMD_SUPPORT
using Windows.Perception.Spatial;
#endif

namespace Microsoft.MixedReality.OpenXR.Sample
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
        private bool m_placeAndReload = false;

#if ENABLE_WINMD_SUPPORT
        private int m_externalAnchorCount = 0;
#endif

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

#if USE_MICROSOFT_OPENXR_PLUGIN_1_9_OR_NEWER
            m_anchorStore = await XRAnchorStore.LoadAnchorStoreAsync(m_arAnchorManager.subsystem);
#else
            m_anchorStore = await XRAnchorStore.LoadAsync(m_arAnchorManager.subsystem);
#endif
            if (m_anchorStore == null)
            {
                Debug.Log("XRAnchorStore not available, sample anchor persistence functionality will not be enabled.");
                return;
            }

            LoadAnchors();
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

        private void LoadAnchors()
        {
            // Request all persisted anchors be loaded once the anchor store is loaded.
            foreach (string name in m_anchorStore.PersistedAnchorNames)
            {
                // When a persisted anchor is requested from the anchor store, LoadAnchor returns the TrackableId which
                // the anchor will use once it is loaded. To later recognize and recall the names of these anchors after
                // they have loaded, this dictionary stores the TrackableIds.
                TrackableId trackableId = m_anchorStore.LoadAnchor(name);
                if (trackableId == TrackableId.invalidId)
                {
                    Debug.LogError($"Failed to load anchor {name} from XRAnchorStore.");
                }
                else
                {
                    m_incomingPersistedAnchors.Add(trackableId, name);
                }
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

        private async void LateUpdate()
        {
            // Air taps for anchor creation are handled in LateUpdate() to avoid race conditions with air taps to enable/disable anchor creation.
            for (int i = 0; i < 2; i++)
            {
                InputDevice device = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.RightHand : XRNode.LeftHand);

                bool isTapping = IsTapping(device);
                if (isTapping && !m_wasTapping[i])
                {
                    await OnAirTapped(device);
                }
                m_wasTapping[i] = isTapping;
            }


            m_airTapToCreateEnabledChangedThisUpdate = false;
        }

        public async Task OnAirTapped(InputDevice device)
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

                Pose pose = new Pose(position, Quaternion.LookRotation(position - headPosition, Vector3.up));

                // Check if we should reload after placing
                if (m_placeAndReload)
                {
                    m_placeAndReload = false;
                    await PlaceAndReload(pose);
                }
                else
                {
                    AddAnchor(pose);
                }
            }
        }

        public void AddAnchor(Pose pose)
        {
            XRAnchor newAnchor;
            m_arAnchorManager.subsystem.TryAddAnchor(pose, out newAnchor);

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
                if (!m_arAnchorManager.subsystem.TryRemoveAnchor(anchor.trackableId))
                {
                    Debug.LogError($"Failed to remove anchor {anchor.trackableId}");
                }
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

        public void EnablePlaceAndReload()
        {
            m_placeAndReload = true;
        }

        private async Task PlaceAndReload(Pose pose)
        {

            if (m_arAnchorManager.subsystem == null)
            {
                throw new Exception(message: "ARAnchorManager subsystem not active.");
            }
#if ENABLE_WINMD_SUPPORT
            try
            {
                Debug.Log($"Creating coodinate system");
                SpatialCoordinateSystem spatialCoordinateSystem = Microsoft.MixedReality.OpenXR.PerceptionInterop.GetSceneCoordinateSystem(Pose.identity) as SpatialCoordinateSystem;
                SpatialAnchor sa = SpatialAnchor.TryCreateRelativeTo(spatialCoordinateSystem, ToSystem(pose.position), ToSystem(pose.rotation));
                if (sa == null)
                {
                    Debug.LogWarning($"Failed to create SpatialAnchor.");
                }

                SpatialAnchorStore spatialAnchorStore = SpatialAnchorManager.RequestStoreAsync().AsTask().Result;
                if (spatialAnchorStore != null)
                {
                    bool saved = spatialAnchorStore.TrySave("ExternalAnchor" + m_externalAnchorCount++, sa);
                    Debug.Log($"{(saved ? "successfully saved" : "failed to save")} anchor via Windows.Perception.Spatial APIs.");
                    GC.SuppressFinalize(spatialAnchorStore);
                }
                else
                {
                    Debug.Log($"SpatialAnchorStore was null.");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#else
            Debug.Log("Loading via Windows.Perception.Spatial APIs not supported on this platform.");
#endif

            ClearSceneAnchors();
            m_incomingPersistedAnchors.Clear();


            if (!(await m_anchorStore?.TryReloadAnchorStoreAsync()))
            {
                throw new Exception(message: "Failed to load native anchors. Look for the XR logs for more information.");
            }

            LoadAnchors();
        }

         
        private System.Numerics.Vector3 ToSystem(UnityEngine.Vector3 v)
        {
            return new System.Numerics.Vector3(v.x, v.y, -v.z);
        }

        private System.Numerics.Quaternion ToSystem(UnityEngine.Quaternion q)
        {
            return new System.Numerics.Quaternion(-q.x, -q.y, q.z, q.w);
        }
    }
}