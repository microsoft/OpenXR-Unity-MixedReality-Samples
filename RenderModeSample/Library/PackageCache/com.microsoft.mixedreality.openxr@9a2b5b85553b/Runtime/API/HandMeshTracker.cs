// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Represents different possible hand poses.
    /// </summary>
    /// <remarks>See https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandPoseTypeMSFT for more information.</remarks>
    public enum HandPoseType
    {
        /// <summary>
        /// Represents a hand pose provided by actual tracking of the user's hand.
        /// </summary>
        Tracked = 0,

        /// <summary>
        /// Represents a stable reference hand pose in a relaxed open hand shape.
        /// </summary>
        ReferenceOpenPalm,
    }

    /// <summary>
    /// Represents a user's hand and the ability to render a hand mesh representation of it.
    /// </summary>
    public class HandMeshTracker
    {
        /// <summary>
        /// The user's left hand.
        /// </summary>
        public static HandMeshTracker Left { get; } = new HandMeshTracker(Handedness.Left);

        /// <summary>
        /// The user's right hand.
        /// </summary>
        public static HandMeshTracker Right { get; } = new HandMeshTracker(Handedness.Right);

        private readonly HandTrackingFeaturePlugin m_feature = OpenXRSettings.Instance.GetFeature<HandTrackingFeaturePlugin>();
        private readonly NativeLibToken m_token = HandTrackingFeaturePlugin.nativeLibToken;
        private readonly Handedness m_handedness;

        private Vector3[] m_handMeshVertices = null;
        private Vector3[] m_handMeshNormals = null;
        private int[] m_handMeshIndices = null;

        private Mesh m_currentMesh = null;
        private uint m_indexBufferKey = 0;
        private ulong m_vertexBufferkey = 0;

        private HandMeshTracker(Handedness trackerHandedness)
        {
            m_handedness = trackerHandedness;
        }

        /// <summary>
        /// Tries to get the current location in world-space of the specified hand mesh.
        /// </summary>
        /// <param name="frameTime">Specify the <see cref="FrameTime"/> to locate the hand mesh.</param>
        /// <param name="pose">The current pose of the specified hand mesh.</param>
        /// <param name="handPoseType">The type of hand mesh pose to request. The tracked pose represents the actively tracked hand. The reference pose represents a stable hand pose in a relaxed open hand shape.</param>
        /// <returns>True if the pose is valid.</returns>
        public bool TryLocateHandMesh(FrameTime frameTime, out Pose pose, HandPoseType handPoseType = HandPoseType.Tracked)
        {
            pose = Pose.identity;
            if (m_feature == null || !m_feature.enabled)
            {
                return false;   // Hand tracking feature is not enabled
            }
            return NativeLib.TryLocateHandMesh(m_token, m_handedness, frameTime, handPoseType, out pose);
        }

        /// <summary>
        /// Retrieves the latest hand mesh information and build the current hand mesh in the passed-in mesh parameter.
        /// </summary>
        /// <param name="frameTime">Specify the <see cref="FrameTime"/> to locate the hand mesh.</param>
        /// <param name="handMesh">The mesh object to build the hand mesh in.</param>
        /// <param name="handPoseType">The type of hand mesh to request. The tracked pose represents the actively tracked hand. The reference pose represents a stable hand pose in a relaxed open hand shape.</param>
        /// <returns>True if the mesh was retrievable.</returns>
        public bool TryGetHandMesh(FrameTime frameTime, Mesh handMesh, HandPoseType handPoseType = HandPoseType.Tracked)
        {
            if (m_feature == null || !m_feature.enabled)
            {
                return false; // Hand tracking feature is not enabled. Return the tracker not tracking.
            }

            try
            {
                if (m_handMeshVertices == null || m_handMeshNormals == null || m_handMeshIndices == null)
                {
                    if (NativeLib.TryGetHandMeshBufferSizes(m_token, out uint maxVertexCount, out uint maxIndexCount))
                    {
                        m_handMeshVertices = new Vector3[maxVertexCount];
                        m_handMeshNormals = new Vector3[maxVertexCount];
                        m_handMeshIndices = new int[maxIndexCount];
                    }
                    else
                    {
                        return false;
                    }
                }

                if (m_currentMesh != handMesh)
                {
                    m_currentMesh = handMesh;
                    m_indexBufferKey = 0;
                    m_vertexBufferkey = 0;
                }

                if (NativeLib.TryGetHandMesh(m_token, m_handedness, frameTime, handPoseType,
                    ref m_vertexBufferkey, out uint vertexCount, m_handMeshVertices, m_handMeshNormals,
                    ref m_indexBufferKey, out uint indexCount, m_handMeshIndices))
                {
                    // The NativeLib call will return a count of 0 if no change was made
                    if (vertexCount > 0)
                    {
                        handMesh.SetVertices(m_handMeshVertices, 0, (int)vertexCount);
                        handMesh.SetNormals(m_handMeshNormals, 0, (int)vertexCount);
                    }

                    if (indexCount > 0)
                    {
                        handMesh.SetTriangles(m_handMeshIndices, 0, (int)indexCount, 0);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.DllNotFoundException)
            {
                return false;
            }
        }
    }
}
