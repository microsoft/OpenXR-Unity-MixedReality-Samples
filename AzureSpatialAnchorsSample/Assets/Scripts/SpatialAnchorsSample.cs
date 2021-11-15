// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.SpatialAnchors; // for SessionUpdatedEventArgs
using Microsoft.Azure.SpatialAnchors.Unity; // For SpatialAnchorManager

using System; // for Enum
using System.Collections.Generic; // for List
using System.Linq; // for Aggregate
using System.Threading.Tasks; // for Task
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    [RequireComponent(typeof(ARAnchorManager))]
    [RequireComponent(typeof(SpatialAnchorManager))]
    public class SpatialAnchorsSample : MonoBehaviour
    {
        private bool[] m_wasHandAirTapping = { false, false };

        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject m_sampleSpatialAnchorPrefab = null;

        /// <summary>
        /// Used to manage Azure Spatial Anchor sessions and queries in a Unity scene
        /// </summary>
        private SpatialAnchorManager m_cloudSpatialAnchorManager;

        /// <summary>
        /// The SpatialAnchorManager must have its session created after the Start() method.
        /// Instead, we create it if it has not been created while starting the session.
        /// </summary>
        private bool m_cloudSpatialAnchorManagerSessionCreated = false;

        /// <summary>
        /// All anchor GameObjects found by or manually created in this demo. Used to ensure these are all
        /// destroyed when the session is stopped.
        /// </summary>
        private List<GameObject> m_foundOrCreatedAnchorObjects = new List<GameObject>();

        /// <summary>
        /// If a new cloud session is started after we have previously saved cloud anchors, this spatial
        /// anchor watcher will rediscover them.
        /// </summary>
        private CloudSpatialAnchorWatcher m_cloudSpatialAnchorWatcher;

        /// <summary>
        /// The Ids of all cloud spatial anchors created during this demo. Used to when creating a watcher
        /// to re-discover these anchors after stopping and starting the cloud session.
        /// </summary>
        private List<string> m_cloudSpatialAnchorIDs = new List<string>();

        /// <summary>
        /// Setup references to other components on this GameObject.
        /// </summary>
        private void Awake()
        {
            m_cloudSpatialAnchorManager = GetComponent<SpatialAnchorManager>();
        }

        /// <summary>
        /// Ensure this sample scene is properly configured, then create the spatial anchor manager session.
        /// </summary>
        private void Start()
        {
            // Ensure the ARAnchorManager is properly setup for this sample
            ARAnchorManager arAnchorManager = GetComponent<ARAnchorManager>();
            if (!arAnchorManager.enabled || arAnchorManager.subsystem == null)
            {
                Debug.LogError($"ARAnchorManager not enabled or available; sample anchor functionality will not be enabled.");
                return;
            }

            // Ensure anchor prefabs are properly setup for this sample
            if (arAnchorManager.anchorPrefab != null)
            {
                // When using ASA, ARAnchors are managed internally and should not be instantiated with custom behaviors
                Debug.LogError("The anchor prefab for ARAnchorManager must be set to null.");
                return;
            }
            if (m_sampleSpatialAnchorPrefab == null)
            {
                // Since the ARAnchorManager cannot have a prefab set, this script handles anchor prefab instantiation instead
                Debug.LogError($"{nameof(m_sampleSpatialAnchorPrefab)} reference has not been set. Make sure it has been added to the scene and wired up to {this.name}.");
                return;
            }

            // Ensure the SpatialAnchorManager is properly setup for this sample
            if (string.IsNullOrWhiteSpace(m_cloudSpatialAnchorManager.SpatialAnchorsAccountId) ||
                string.IsNullOrWhiteSpace(m_cloudSpatialAnchorManager.SpatialAnchorsAccountKey) ||
                string.IsNullOrWhiteSpace(m_cloudSpatialAnchorManager.SpatialAnchorsAccountDomain))
            {
                Debug.LogError($"{nameof(SpatialAnchorManager.SpatialAnchorsAccountId)}, {nameof(SpatialAnchorManager.SpatialAnchorsAccountKey)} and {nameof(SpatialAnchorManager.SpatialAnchorsAccountDomain)} must be set on {nameof(SpatialAnchorManager)}");
                return;
            }

            m_cloudSpatialAnchorManager.LogDebug += (sender, args) => Debug.Log($"Debug: {args.Message}");
            m_cloudSpatialAnchorManager.Error += (sender, args) => Debug.LogError($"Error: {args.ErrorMessage}");
            m_cloudSpatialAnchorManager.AnchorLocated += SpatialAnchorManagerAnchorLocated;
            m_cloudSpatialAnchorManager.LocateAnchorsCompleted += (sender, args) => Debug.Log("Locate anchors completed!");

            Debug.Log($"Tap the button below to create and start the session.");
        }

        /// <summary>
        /// Check for any air taps from either hand.
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < 2; i++)
            {
                InputDevice device = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.RightHand : XRNode.LeftHand);
                bool isTapping;
                if (!device.TryGetFeatureValue(CommonUsages.primaryButton, out isTapping))
                {
                    continue;
                }

                if (isTapping && !m_wasHandAirTapping[i])
                {
                    OnAirTapped(device);
                }

                m_wasHandAirTapping[i] = isTapping;
            }
        }

        /// <summary>
        /// When an air tap is detected, check for a nearby anchor. If there is an anchor nearby, we save
        /// it to or delete it from the cloud. If there is no anchor nearby, we create a new local anchor.
        /// </summary>
        private void OnAirTapped(InputDevice device)
        {
            if (m_cloudSpatialAnchorManager == null || !m_cloudSpatialAnchorManager.IsSessionStarted)
            {
                return;
            }

            Vector3 position;
            if (!device.TryGetFeatureValue(CommonUsages.devicePosition, out position))
            {
                return;
            }

            // First, check if there is a nearby anchor to save/delete
            GameObject nearestAnchor = FindNearestObject(m_foundOrCreatedAnchorObjects, position, 0.1f);
            if (nearestAnchor != null)
            {
                if (nearestAnchor.GetComponent<SampleSpatialAnchor>().Persisted)
                {
                    DeleteAnchorFromCloudAsync(nearestAnchor);
                }
                else
                {
                    SaveAnchorToCloudAsync(nearestAnchor);
                }
                return;
            }

            // If there's no anchor nearby, create a new local one
            Vector3 headPosition;
            if (!InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
                headPosition = Vector3.zero;

            Debug.Log($"Creating new local anchor: {position:F3}");
            Quaternion orientationTowardsHead = Quaternion.LookRotation(position - headPosition, Vector3.up);
            GameObject newGameObject = Instantiate(m_sampleSpatialAnchorPrefab, position, orientationTowardsHead);
            m_foundOrCreatedAnchorObjects.Add(newGameObject);
        }

        /// <summary>
        /// Find the nearest gameObject to a position from a list of gameObjects, within an acceptable maximumDistance.
        /// </summary>
        private GameObject FindNearestObject(List<GameObject> gameObjects, Vector3 position, float maximumDistance)
        {
            GameObject nearestObject = null;
            float nearestObjectDist = maximumDistance;

            foreach (var gameObject in gameObjects)
            {
                float dist = Vector3.Distance(position, gameObject.transform.position);
                if (dist < nearestObjectDist)
                {
                    nearestObject = gameObject;
                    nearestObjectDist = dist;
                }
            }

            return nearestObject;
        }

        public async void OnStartSessionButtonPressed() => await StartSessionAsync();

        public void OnStopSessionButtonPressed() => StopAndCleanupSession();

        private void OnDestroy() => StopAndCleanupSession(); // When this sample is destroyed, ensure the session is cleaned up.

        /// <summary>
        /// Start the session and begin searching for anchors previously saved.
        /// </summary>
        private async Task StartSessionAsync()
        {
            // CreateSessionAsync cannot be called during Start(), since the SpatialAnchorManager may not have Start()ed yet itself.
            // Instead, we ensure the session is created before we start it.
            if (!m_cloudSpatialAnchorManagerSessionCreated)
            {
                await m_cloudSpatialAnchorManager.CreateSessionAsync();
                m_cloudSpatialAnchorManagerSessionCreated = true;
            }

            if (m_cloudSpatialAnchorManager.IsSessionStarted)
            {
                Debug.LogWarning("Cannot start session; session already started.");
                return;
            }

            // Start the session
            Debug.Log("Starting session...");
            await m_cloudSpatialAnchorManager.StartSessionAsync();
            Debug.Log($"Session started! Air tap to create a local anchor.");

            // And create the watcher to look for any created anchors
            if (m_cloudSpatialAnchorIDs.Count > 0)
            {
                Debug.Log($"Creating watcher to look for {m_cloudSpatialAnchorIDs.Count} spatial anchors");
                AnchorLocateCriteria anchorLocateCriteria = new AnchorLocateCriteria();
                anchorLocateCriteria.Identifiers = m_cloudSpatialAnchorIDs.ToArray();
                m_cloudSpatialAnchorWatcher = m_cloudSpatialAnchorManager.Session.CreateWatcher(anchorLocateCriteria);
                Debug.Log($"Watcher created!");
            }
        }

        /// <summary>
        /// Stop the session and cleanup any anchors created.
        /// </summary>
        private void StopAndCleanupSession()
        {
            // Clean up the sample scenario, destroying any anchors
            foreach (GameObject gameObject in m_foundOrCreatedAnchorObjects)
            {
                Destroy(gameObject);
            }
            m_foundOrCreatedAnchorObjects.Clear();

            // Stop the session
            if (!m_cloudSpatialAnchorManager.IsSessionStarted)
            {
                Debug.LogWarning("Cannot stop session; session has not started.");
                return;
            }

            Debug.Log("Stopping session...");
            m_cloudSpatialAnchorManager.StopSession();
            m_cloudSpatialAnchorManager.Session.Reset();

            // Stop the watcher, if it exists
            if (m_cloudSpatialAnchorWatcher != null)
            {
                m_cloudSpatialAnchorWatcher.Stop();
                m_cloudSpatialAnchorWatcher = null;
            }
            Debug.Log("Session stopped!");
        }

        /// <summary>
        /// For each anchor located by the spatial anchor manager, instantiate and setup a corresponding GameObject.
        /// </summary>
        private void SpatialAnchorManagerAnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            Debug.Log($"Anchor recognized as a possible anchor {args.Identifier} {args.Status}");

            if (args.Status == LocateAnchorStatus.Located)
            {
                UnityDispatcher.InvokeOnAppThread(() =>
                {
                    CloudSpatialAnchor cloudSpatialAnchor = args.Anchor;
                    Pose anchorPose = cloudSpatialAnchor.GetPose();

                    GameObject anchorGameObject = Instantiate(m_sampleSpatialAnchorPrefab, anchorPose.position, anchorPose.rotation);
                    anchorGameObject.AddComponent<CloudNativeAnchor>().CloudToNative(cloudSpatialAnchor);
                    anchorGameObject.GetComponent<SampleSpatialAnchor>().Identifier = cloudSpatialAnchor.Identifier;
                    anchorGameObject.GetComponent<SampleSpatialAnchor>().Persisted = true;

                    m_foundOrCreatedAnchorObjects.Add(anchorGameObject);
                });
            }
        }

        /// <summary>
        /// Save a local ARAnchor as a cloud anchor.
        /// </summary>
        private async void SaveAnchorToCloudAsync(GameObject gameObject)
        {
            CloudNativeAnchor cloudNativeAnchor = gameObject.AddComponent<CloudNativeAnchor>();

            await cloudNativeAnchor.NativeToCloud();
            CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;

            // In this sample app, the cloud anchors are deleted explicitly.
            // Here, we show how to set an anchor to expire automatically.
            cloudSpatialAnchor.Expiration = DateTimeOffset.Now.AddDays(7);

            while (!m_cloudSpatialAnchorManager.IsReadyForCreate)
            {
                await Task.Delay(1000);
                float createProgress = m_cloudSpatialAnchorManager.SessionStatus.RecommendedForCreateProgress;
                Debug.Log($"Move your device to capture more environment data: {createProgress:0%}");
            }


            // Now that the cloud spatial anchor has been prepared, we can try the actual save here
            Debug.Log($"Saving cloud anchor...");
            await m_cloudSpatialAnchorManager.CreateAnchorAsync(cloudSpatialAnchor);

                bool saveSucceeded = cloudSpatialAnchor != null;
                if (!saveSucceeded)
                {
                    Debug.LogError("Failed to save, but no exception was thrown.");
                    return;
                }

            m_cloudSpatialAnchorIDs.Add(cloudSpatialAnchor.Identifier);
            Debug.Log($"Saved cloud anchor: {cloudSpatialAnchor.Identifier}");

            // Update the visuals of the gameobject
            gameObject.GetComponent<SampleSpatialAnchor>().Identifier = cloudSpatialAnchor.Identifier;
            gameObject.GetComponent<SampleSpatialAnchor>().Persisted = true;
        }

        /// <summary>
        /// Delete the cloud anchor corresponding to a local ARAnchor.
        /// </summary>
        private async void DeleteAnchorFromCloudAsync(GameObject gameObject)
            {
            CloudNativeAnchor cloudNativeAnchor = gameObject.GetComponent<CloudNativeAnchor>();
            CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;

            Debug.Log($"Deleting cloud anchor: {cloudSpatialAnchor.Identifier}");
            await m_cloudSpatialAnchorManager.DeleteAnchorAsync(cloudSpatialAnchor);
            m_cloudSpatialAnchorIDs.Remove(cloudSpatialAnchor.Identifier);
            Destroy(cloudNativeAnchor);
            Debug.Log($"Cloud anchor deleted!");

            // Update the visuals of the gameobject
            gameObject.GetComponent<SampleSpatialAnchor>().Persisted = false;
            gameObject.GetComponent<SampleSpatialAnchor>().Identifier = "";
        }
    }
}
