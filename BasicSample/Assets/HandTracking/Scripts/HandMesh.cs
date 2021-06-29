// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class HandMesh : MonoBehaviour
    {
        [SerializeField, Tooltip("The hand this mesh should represent.")]
        private Handedness handedness;

        private Mesh mesh;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private HandMeshTracker handMeshTracker;

        private void Awake()
        {
            handMeshTracker = handedness == Handedness.Left ? HandMeshTracker.Left : HandMeshTracker.Right;
            meshFilter = gameObject.GetComponent<MeshFilter>();
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            mesh = meshFilter.mesh;
        }

        private void Update()
        {
            if (mesh == null)
            {
                return;
            }

            if (handMeshTracker.TryGetHandMesh(FrameTime.OnUpdate, mesh))
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