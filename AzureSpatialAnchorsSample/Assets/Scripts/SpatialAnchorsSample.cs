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

        [SerializeField]
        [Tooltip("SpatialAnchorManager instance to use for this demo. This is required.")]
        private SpatialAnchorManager m_spatialAnchorManager = null;

        private List<GameObject> m_foundOrCreatedAnchorObjects = new List<GameObject>(); // list of created or found anchors
        private List<string> m_createdSpatialAnchorIDs = new List<string>();
        protected CloudSpatialAnchorWatcher m_currentWatcher;

        /// <summary>
        /// Ensure this sample scene is properly configured, then create the spatial anchor manager session.
        /// </summary>
        private async void Start()
        {
            // Ensure the ARAnchorManager is properly setup for this sample
            ARAnchorManager arAnchorManager = GetComponent<ARAnchorManager>();
            if (!arAnchorManager.enabled || arAnchorManager.subsystem == null)
            {
                Debug.Log($"ARAnchorManager not enabled or available; sample anchor functionality will not be enabled.");
                return;
            }
            if (arAnchorManager.anchorPrefab != null)
            {
                // When using ASA, ARAnchors are managed internally and should not be instantiated with custom behaviors
                Debug.LogError("The anchor prefab for ARAnchorManager must be set to null.");
                return;
            }

            // Ensure the SpatialAnchorManager is properly setup for this sample
            if (m_spatialAnchorManager == null)
            {
                Debug.Log($"{nameof(m_spatialAnchorManager)} reference has not been set. Make sure it has been added to the scene and wired up to {this.name}.");
                return;
            }
            if (string.IsNullOrWhiteSpace(m_spatialAnchorManager.SpatialAnchorsAccountId) ||
                string.IsNullOrWhiteSpace(m_spatialAnchorManager.SpatialAnchorsAccountKey) ||
                string.IsNullOrWhiteSpace(m_spatialAnchorManager.SpatialAnchorsAccountDomain))
            {
                Debug.Log($"{nameof(SpatialAnchorManager.SpatialAnchorsAccountId)}, {nameof(SpatialAnchorManager.SpatialAnchorsAccountKey)} and {nameof(SpatialAnchorManager.SpatialAnchorsAccountDomain)} must be set on {nameof(SpatialAnchorManager)}");
                return;
            }

            // Ensure this component's references are properly setup for this sample
            if (m_sampleSpatialAnchorPrefab == null)
            {
                Debug.Log($"{nameof(m_sampleSpatialAnchorPrefab)} reference has not been set. Make sure it has been added to the scene and wired up to {this.name}.");
                return;
            }

            await SetupAndCreateSession();
        }

        private async Task SetupAndCreateSession()
        {
            m_spatialAnchorManager.LogDebug += (sender, args) => Debug.Log($"Debug: {args.Message}");
            m_spatialAnchorManager.Error += (sender, args) => Debug.LogError($"Error: {args.ErrorMessage}");
            m_spatialAnchorManager.AnchorLocated += SpatialAnchorManagerAnchorLocated;
            m_spatialAnchorManager.LocateAnchorsCompleted += (sender, args) => Debug.Log("Locate anchors completed!");

            Debug.Log($"Creating session...");
            try
            {
                await m_spatialAnchorManager.CreateSessionAsync();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
            Debug.Log($"Session created! Tap the button below to start the session.");
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
        /// Start the session and begin searching for anchors previously saved.
        /// </summary>
        public async void StartSession()
        {
            if (m_spatialAnchorManager == null)
            {
                Debug.Log($"spatialAnchorManager does not exist yet!");
                return;
            }

            if (m_spatialAnchorManager.IsSessionStarted)
            {
                Debug.Log("Cannot start session; session already started.");
                return;
            }

            // Start the session which we created during Start()
            Debug.Log("Starting session...");
            try
            {
                await m_spatialAnchorManager.StartSessionAsync();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
            Debug.Log($"Session started! Air tap to create a local anchor.");

            // And create the watcher to look for any created anchors
            if (m_createdSpatialAnchorIDs.Count > 0)
            {
                Debug.Log($"Creating watcher to look for {m_createdSpatialAnchorIDs.Count} spatial anchors");
                AnchorLocateCriteria anchorLocateCriteria = new AnchorLocateCriteria();
                anchorLocateCriteria.Identifiers = m_createdSpatialAnchorIDs.ToArray();
                m_spatialAnchorManager.Session.CreateWatcher(anchorLocateCriteria);
                Debug.Log($"Watcher created!");
            }
        }

        /// <summary>
        /// When this sample is destroyed, ensure the session is cleaned up.
        /// </summary>
        private void OnDestroy() => StopAndCleanupSession();

        /// <summary>
        /// Stop the session and cleanup any anchors created.
        /// </summary>
        public void StopAndCleanupSession()
        {
            // Clean up the sample scenario, destroying any anchors
            foreach (GameObject gameObject in m_foundOrCreatedAnchorObjects)
            {
                Destroy(gameObject);
            }
            m_foundOrCreatedAnchorObjects.Clear();

            // Stop the session
            if (m_spatialAnchorManager == null)
            {
                Debug.Log($"SpatialAnchorManager does not exist!");
                return;
            }

            if (!m_spatialAnchorManager.IsSessionStarted)
            {
                Debug.Log("Cannot stop session; session has not started.");
                return;
            }

            Debug.Log("Stopping session...");
            m_spatialAnchorManager.StopSession();
            m_spatialAnchorManager.Session.Reset();

            if (m_currentWatcher != null)
            {
                m_currentWatcher.Stop();
                m_currentWatcher = null;
            }
            Debug.Log("Session stopped!");
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
                    continue;

                if (isTapping && !m_wasHandAirTapping[i])
                    OnAirTapped(device);

                m_wasHandAirTapping[i] = isTapping;
            }
        }

        /// <summary>
        /// When an air tap is detected, check for a nearby anchor. If there is an anchor nearby,
        /// we save it to or delete it from the cloud. If there is no anchor nearby, we create one.
        /// </summary>
        public void OnAirTapped(InputDevice device)
        {
            if (m_spatialAnchorManager == null || !m_spatialAnchorManager.IsSessionStarted)
                return;

            Vector3 position;
            if (!device.TryGetFeatureValue(CommonUsages.devicePosition, out position))
                return;

            // First, check if there is a nearby anchor to save/delete.
            if (m_foundOrCreatedAnchorObjects.Count > 0)
            {
                var (distance, closestObject) = m_foundOrCreatedAnchorObjects.Aggregate(
                    new Tuple<float, GameObject>(Mathf.Infinity, null),
                    (minPair, gameObject) =>
                    {
                        Vector3 gameObjectPosition = gameObject.transform.position;
                        float dist = (position - gameObjectPosition).magnitude;
                        return dist < minPair.Item1 ? new Tuple<float, GameObject>(dist, gameObject) : minPair;
                    });

                if (distance < 0.1f)
                {
                    if (closestObject.GetComponent<SampleSpatialAnchor>().Persisted)
                    {
                        DeleteAnchorFromCloudAsync(closestObject);
                    }
                    else
                    {
                        SaveAnchorToCloudAsync(closestObject);
                    }
                    return;
                }
            }

            // If there's no anchor nearby, create a new one.
            Vector3 headPosition;
            if (!InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
                headPosition = Vector3.zero;

            Debug.Log($"Creating new local anchor: {position:F3}");
            Quaternion orientationTowardsHead = Quaternion.LookRotation(position - headPosition, Vector3.up);
            GameObject newGameObject = Instantiate(m_sampleSpatialAnchorPrefab, position, orientationTowardsHead);
            m_foundOrCreatedAnchorObjects.Add(newGameObject);
        }

        private async void DeleteAnchorFromCloudAsync(GameObject gameObject)
        {
            CloudNativeAnchor cloudNativeAnchor = gameObject.GetComponent<CloudNativeAnchor>();
            CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;

            Debug.Log($"Deleting cloud anchor: {cloudSpatialAnchor.Identifier}");

            m_createdSpatialAnchorIDs.Remove(cloudSpatialAnchor.Identifier);
            await m_spatialAnchorManager.DeleteAnchorAsync(cloudSpatialAnchor);
            Destroy(cloudNativeAnchor);

            gameObject.GetComponent<SampleSpatialAnchor>().Persisted = false;
            gameObject.GetComponent<SampleSpatialAnchor>().Identifier = "";

            Debug.Log($"Cloud anchor deleted!");
        }

        private async void SaveAnchorToCloudAsync(GameObject gameObject)
        {
            CloudNativeAnchor cloudNativeAnchor = gameObject.AddComponent<CloudNativeAnchor>();

            await cloudNativeAnchor.NativeToCloud();
            CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;

            // In this sample app, the cloud anchors are deleted explicitly.
            // Here, we show how to set an anchor to expire automatically.
            cloudSpatialAnchor.Expiration = DateTimeOffset.Now.AddDays(7);

            while (!m_spatialAnchorManager.IsReadyForCreate)
            {
                await Task.Delay(1000);
                float createProgress = m_spatialAnchorManager.SessionStatus.RecommendedForCreateProgress;
                Debug.Log($"Move your device to capture more environment data: {createProgress:0%}");
            }

            Debug.Log($"Saving cloud anchor...");

            try
            {
                // Now that the cloud spatial anchor has been prepared, we can try the actual save here.
                await m_spatialAnchorManager.CreateAnchorAsync(cloudSpatialAnchor);

                bool saveSucceeded = cloudSpatialAnchor != null;
                if (!saveSucceeded)
                {
                    Debug.LogError("Failed to save, but no exception was thrown.");
                    return;
                }

                Debug.Log($"Saved cloud anchor: {cloudSpatialAnchor.Identifier}");
                gameObject.GetComponent<SampleSpatialAnchor>().Identifier = cloudSpatialAnchor.Identifier;
                gameObject.GetComponent<SampleSpatialAnchor>().Persisted = true;
                m_createdSpatialAnchorIDs.Add(cloudSpatialAnchor.Identifier);
            }
            catch (Exception exception)
            {
                Debug.Log("Failed to save anchor: " + exception.ToString());
                Debug.LogException(exception);
            }
        }

    }
}
