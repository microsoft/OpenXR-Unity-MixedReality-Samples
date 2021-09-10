// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Updates the mesh bounding volume every frame to follow the user's head.
    /// </summary>
    public class ARMeshUpdateFollowsUser : MonoBehaviour
    {
        [SerializeField]
        private ARMeshManager meshManager = null;

        [SerializeField]
        private Vector3 boundingExtents = Vector3.one * 3;

        private void Awake()
        {
            if (meshManager == null && !TryGetComponent(out meshManager))
            {
                Debug.LogError($"No {nameof(ARMeshManager)} was provided to {nameof(ARMeshUpdateFollowsUser)} on {name}.");
            }
        }

        private void Update()
        {
            if (meshManager != null)
            {
                meshManager.subsystem?.SetBoundingVolume(Camera.main.transform.position, boundingExtents);
            }
        }
    }
}
