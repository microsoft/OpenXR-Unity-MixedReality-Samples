// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Updates the mesh bounding volume every frame to follow the user's head.
    /// </summary>
    public class MeshFollowUser : MonoBehaviour
    {
        [SerializeField]
        private ARMeshManager meshManager = null;

        [SerializeField]
        private Vector3 boundingExtents = Vector3.one * 3;

        private void Update()
        {
            meshManager.subsystem.SetBoundingVolume(Camera.main.transform.position, boundingExtents);
        }
    }
}
