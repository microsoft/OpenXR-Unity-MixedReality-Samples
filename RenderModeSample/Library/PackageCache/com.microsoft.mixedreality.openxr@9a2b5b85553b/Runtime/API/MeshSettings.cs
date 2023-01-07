// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// The type of mesh to request from the XRMeshSubsystem.
    /// </summary>
    public enum MeshType
    {
        ///<summary>
        /// A mesh intended for visualization.
        ///</summary>
        Visual = 1,

        ///<summary>
        /// A mesh intended for collision detection.
        ///</summary>
        Collider = 2,
    }

    /// <summary>
    /// The level of detail of visual mesh to request from the XRMeshSubsystem.
    /// </summary>
    /// <remarks>Has no effect on the collider mesh.</remarks>
    public enum VisualMeshLevelOfDetail
    {
        /// <summary>
        /// Coarse mesh level of detail with roughly 100 triangles per cubic meter.
        /// </summary>
        Coarse = 1,
        /// <summary>
        /// Medium mesh level of detail with roughly 400 triangles per cubic meter.
        /// </summary>
        Medium = 2,
        /// <summary>
        /// Fine mesh level of detail with roughly 2000 triangles per cubic meter.
        /// </summary>
        Fine = 3,
        /// <summary>
        /// Unlimited mesh level of detail with no guarantee as to the number of triangles per cubic meter.
        /// </summary>
        Unlimited = 4,
    }

    /// <summary>
    /// The compute consistency to request from the XRMeshSubsystem.
    /// </summary>
    public enum MeshComputeConsistency
    {
        /// <summary>
        /// A watertight, globally consistent snapshot, not limited to observable objects in
        /// the scanned regions.
        /// </summary>
        ConsistentSnapshotComplete = 1,
        /// <summary>
        /// A non-watertight snapshot, limited to observable objects in the scanned regions.
        /// The returned mesh may not be globally optimized for completeness, and therefore
        /// may be returned faster in some scenarios.
        /// </summary>
        ConsistentSnapshotIncompleteFast = 2,
        /// <summary>
        /// A mesh optimized for lower-latency occlusion uses. The returned mesh may not be
        /// globally consistent and might be adjusted piecewise independently.
        /// </summary>
        OcclusionOptimized = 3,
    }

    /// <summary>
    /// Settings describing the quality and type of mesh to be provided.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct MeshComputeSettings
    {
        private MeshType meshType;
        private VisualMeshLevelOfDetail visualMeshLevelOfDetail;
        private MeshComputeConsistency meshComputeConsistency;

        /// <summary>
        /// Deprecated. The XRMeshSubsystem only supplies visual meshes. Use a <see cref="UnityEngine.MeshCollider"/> or the method <see cref="XRMeshSubsystem.GenerateMeshAsync(MeshId, UnityEngine.Mesh, UnityEngine.MeshCollider, MeshVertexAttributes, Action{MeshGenerationResult})"/> to get collider meshes as needed.
        /// </summary>
        /// <remarks>Defaults to <see cref="MeshType.Visual"/>.</remarks>
        [Obsolete("Obsolete; only visual meshes are supplied through the mesh subsystem. Use a MeshCollider or the method XRMeshSubsystem.GenerateMeshAsync to get collider meshes as needed.")]
        public MeshType MeshType
        {
            get => meshType != 0 ? meshType : MeshType.Visual;
            set => meshType = MeshType.Visual;
        }

        /// <summary>
        /// Get or set the level of detail of visual mesh to request from the XRMeshSubsystem.
        /// </summary>
        /// <remarks>Defaults to <see cref="VisualMeshLevelOfDetail.Coarse"/>.</remarks>
        public VisualMeshLevelOfDetail VisualMeshLevelOfDetail
        {
            get => visualMeshLevelOfDetail != 0 ? visualMeshLevelOfDetail : VisualMeshLevelOfDetail.Coarse;
            set => visualMeshLevelOfDetail = value;
        }

        /// <summary>
        /// Get or set the compute consistency to request from the XRMeshSubsystem.
        /// </summary>
        /// <remarks>Defaults to <see cref="MeshComputeConsistency.OcclusionOptimized"/>.</remarks>
        public MeshComputeConsistency MeshComputeConsistency
        {
            get => meshComputeConsistency != 0 ? meshComputeConsistency : MeshComputeConsistency.OcclusionOptimized;
            set => meshComputeConsistency = value;
        }
    }

    namespace ARSubsystems
    {
        /// <summary>
        /// Additional functionality for the mesh subsystem.
        /// </summary>
        public static class MeshSubsystemExtensions
        {
            /// <summary>
            /// Change the settings for future meshes given by the XRMeshSubsystem.
            /// </summary>
            /// <param name="subsystem">The <see cref="XRMeshSubsystem"/> to receive the settings</param>
            /// <param name="settings">The mesh compute settings to be set.</param>
            /// <returns>Returns true if the setting is successfully changed to the given value.  Returns false otherwise.  </returns>
            public static bool TrySetMeshComputeSettings(this XRMeshSubsystem subsystem, MeshComputeSettings settings)
            {
                return InternalMeshSettings.TrySetMeshComputeSettings(settings);
            }
        }
    }

    /// <summary>
    /// Static entry point for updating the mesh compute settings.
    /// </summary>
    public static class MeshSettings
    {
        /// <summary>
        /// Change the settings for future meshes given by the XRMeshSubsystem.
        /// </summary>
        /// <param name="settings">The mesh compute settings to be set.</param>
        /// <returns>Returns true if the setting is successfully changed to the given value.  Returns false otherwise.  </returns>
        public static bool TrySetMeshComputeSettings(MeshComputeSettings settings)
        {
            return InternalMeshSettings.TrySetMeshComputeSettings(settings);
        }
    }
}
