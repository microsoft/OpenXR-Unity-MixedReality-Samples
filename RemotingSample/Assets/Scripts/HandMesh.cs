// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class HandMesh : MonoBehaviour
    {
        [SerializeField, Tooltip("The hand this mesh should represent.")]
        private Handedness handedness = Handedness.Left;

        private Mesh mesh;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private void Awake()
        {
            meshFilter = gameObject.GetComponent<MeshFilter>();
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            mesh = meshFilter.mesh;
        }

        private void Update()
        {
            HandMeshTracker handMeshTracker = handedness == Handedness.Left ? HandMeshTracker.Left : HandMeshTracker.Right;
            if (mesh != null && handMeshTracker.TryGetHandMesh(FrameTime.OnUpdate, mesh))
            {
                if (!meshRenderer.enabled)
                {
                    meshRenderer.enabled = true;
                }

                if (handMeshTracker.TryLocateHandMesh(FrameTime.OnUpdate, out Pose pose))
                {
                    transform.SetPositionAndRotation(pose.position, pose.rotation);
                }
            }
            else
            {
                if (meshRenderer.enabled)
                {
                    meshRenderer.enabled = false;
                }
            }
        }
    }
}