// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    [RequireComponent(typeof(ARPlaneManager))]
    public class PlanesSample : MonoBehaviour
    {
        [SerializeField]
        private TextMesh m_sampleText;
        private ARPlaneManager m_arPlaneManager;

        private void Awake()
        {
            m_arPlaneManager = GetComponent<ARPlaneManager>();
            if (!m_arPlaneManager)
                Debug.Log($"ARPlaneManager not available; sample plane functionality is not enabled.");
        }

        private void Update() => m_sampleText.text = $"Plane Detection Sample Scene\nPlanes found: {m_arPlaneManager.trackables.count}";

        /// <summary>
        /// Set the plane detection mode of the associated ARPlaneManager. This function takes an int for easier expression of combinations of flags in the editor for Unity events on components.
        /// </summary>
        public void SetPlaneDetectionMode(int planeDetectionMode)
        {
            if (!m_arPlaneManager)
                return;

            m_arPlaneManager.requestedDetectionMode = (PlaneDetectionMode)planeDetectionMode;
        }
    }
}
