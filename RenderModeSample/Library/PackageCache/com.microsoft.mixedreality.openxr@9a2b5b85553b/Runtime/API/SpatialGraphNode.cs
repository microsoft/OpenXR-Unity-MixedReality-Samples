// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// A spatial graph node represents a spatially tracked point provided by the driver.
    /// </summary>
    /// <remarks>
    /// There are two types of spatial graph nodes: static and dynamic.
    ///
    /// A static spatial graph node tracks the pose of a fixed location in the world.
    /// The tracking of static nodes may slowly adjust the pose over time for better accuracy
    /// but the pose is relatively stable in the short term, such as between rendering frames.
    ///
    /// A dynamic spatial graph node tracks the pose of a physical object that moves
    /// continuously relative to reference spaces. The pose of a dynamic spatial graph node
    /// can be very different within the duration of a rendering frame.
    /// </remarks>
    public class SpatialGraphNode
    {
        /// <summary>
        /// Creating a SpatialGraphNode with given static node id, or return null upon failure.
        /// </summary>
        /// <remarks>
        /// The application typically obtains the Guid for the static node
        /// from other spatial graph driver APIs. For example, a static node id
        /// representing the tracking of a QR code can be obtained from HoloLens 2 QR code library.
        /// </remarks>
        /// <param name="id">A GUID represents a spatial graph static node.</param>
        /// <returns>Returns either a valid SpatialGraphNode object if succeeded 
        /// or null if the given static node id cannot be found at the moment.</returns>
        public static SpatialGraphNode FromStaticNodeId(System.Guid id)
        {
            if (NativeLib.TryCreateSpaceFromStaticNodeId(token, id, out ulong spaceId))
            {
                return new SpatialGraphNode()
                {
                    Id = id,
                    m_spaceId = spaceId,
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creating a SpatialGraphNode with given dynamic node id, or return null upon failure.
        /// </summary>
        /// <remarks>
        /// The application typically obtains the Guid for the dynamic node
        /// from other spatial graph driver APIs. For example, a dynamic node id
        /// representing the tracking of the Photo and Video camera on HoloLens 2
        /// can be obtained from media foundation APIs for the camera.
        /// </remarks>
        /// <param name="id">A GUID represents a spatial graph dynamic node.</param>
        /// <returns>Returns either a valid SpatialGraphNode object if succeeded 
        /// or null if the given dynamic node id cannot be found at the moment.</returns>
        public static SpatialGraphNode FromDynamicNodeId(System.Guid id)
        {
            if (NativeLib.TryCreateSpaceFromDynamicNodeId(token, id, out ulong spaceId))
            {
                return new SpatialGraphNode()
                {
                    Id = id,
                    m_spaceId = spaceId,
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the Guid of the SpatialGraphNode
        /// </summary>
        public System.Guid Id { get; private set; } = System.Guid.Empty;

        /// <summary>
        /// Locate the SpatialGraphNode at the given frame time.
        /// The returned pose is in the current Unity scene origin space.
        /// </summary>
        /// <returns>
        /// Return true if the output pose is valid to use, or return false indicating the node lost tracking.
        /// </returns>
        /// <remarks>
        /// This function is typically used to locate the spatial graph node used in Unity's render pipeline
        /// at either OnUpdate or OnBeforeRender callbacks.  Providing the correct input frameTime
        /// allows the runtime to provide correct motion prediction of the tracked node to the display time
        /// of the current rendering frame.
        /// </remarks>
        /// <param name="frameTime">Specify the <see cref="FrameTime"/> to locate the spatial graph node.</param>
        /// <param name="pose">Output the pose when the function returns true.  Discard the value if the function returns false.</param>
        public bool TryLocate(FrameTime frameTime, out Pose pose)
        {
            return NativeLib.TryLocateSpatialGraphNodeSpace(token, m_spaceId, frameTime, out pose);
        }

        /// <summary>
        /// Locate the SpatialGraphNode at the given QPC time.
        /// The returned pose is in the current Unity scene origin space.
        /// </summary>
        /// <returns>
        /// Return true if the output pose is valid to use, or return false indicating the node lost tracking.
        /// </returns>
        /// <remarks>
        /// This function is typically used to locate the spatial graph node using historical timestamp
        /// obtained from other spatial graph APIs, for example the qpcTime of a IMFSample from media
        /// foundation APIs representing the time when a Photo and Video camera captured the image.
        /// Providing an accurate qpcTime from the camera sensor allows the runtime to locate precisely
        /// where the dynamic node was tracked when the image was taken.
        /// </remarks>
        /// <param name="qpcTime">Specify the QPC (i.e. query performance counter) time to locate the spatial graph node.</param>
        /// <param name="pose">Output the pose when the function returns true.  Discard the value if the function returns false.</param>
        public bool TryLocate(long qpcTime, out Pose pose)
        {
            return NativeLib.TryLocateSpatialGraphNodeSpace(token, m_spaceId, qpcTime, out pose);
        }

        private SpatialGraphNode() { }
        private ulong m_spaceId = 0;
        private const NativeLibToken token = NativeLibToken.HoloLens;
    }
}
