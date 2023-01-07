// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Provides the ability to build up a batch of anchors and export them to a binary stream for transfer.
    /// Typically on a second device, it then supports importing the transfer stream and loading in the original batch of anchors.
    /// </summary>
    /// <remarks>Use of this class requires an ARAnchorManager in the scene or some other manual management of an XRAnchorSubsystem.</remarks>
    public class XRAnchorTransferBatch
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public XRAnchorTransferBatch() : this(new AnchorTransferBatch()) { }

        private XRAnchorTransferBatch(AnchorTransferBatch anchorTransferBatch)
        {
            m_anchorTransferBatch = anchorTransferBatch;
        }

        private readonly AnchorTransferBatch m_anchorTransferBatch;

        /// <summary>
        /// Provides a list of all identifiers currently mapped in this AnchorTransferBatch.
        /// </summary>
        public IReadOnlyList<string> AnchorNames => m_anchorTransferBatch.AnchorNames;

        /// <summary>
        /// Tries to convert and add an anchor with the corresponding <see cref="TrackableId"/> to an export list.
        /// </summary>
        /// <remarks>Call <see cref="ExportAsync"/> to get the transferable anchor data.</remarks>
        /// <param name="trackableId">The <see cref="TrackableId"/> of an anchor to be exported.</param>
        /// <param name="name">A string to identify this anchor upon import to another device.</param>
        /// <returns>Whether the anchor was successfully converted into a Perception SpatialAnchor and added to the export list.</returns>
        public bool AddAnchor(TrackableId trackableId, string name) => m_anchorTransferBatch.AddAnchor(trackableId, name);

        /// <summary>
        /// Removes an anchor from the transfer batch. Doesn't remove the existing Unity anchor, if one is present.
        /// </summary>
        /// <remarks>After an anchor is removed from the transfer batch, it will still be valid and locatable in the current session.</remarks>
        /// <param name="name">The name of the anchor to be removed from the transfer batch.</param>
        public void RemoveAnchor(string name) => m_anchorTransferBatch.RemoveAnchor(name);

        /// <summary>
        /// Removes all anchors from the transfer batch. Doesn't remove any existing Unity anchors, if present.
        /// </summary>
        /// <remarks>After the anchors are cleared from the transfer batch, they will still be valid and locatable in the current session.</remarks>
        public void Clear() => m_anchorTransferBatch.Clear();

        /// <summary>
        /// Attempts to load a specified anchor from the transfer batch and reports it to Unity as an XRAnchor/ARAnchor.
        /// </summary>
        /// <remarks>It's then typically recommended to use an ARAnchorManager to access the resulting Unity anchor.</remarks>
        /// <param name="name">The anchor's identifier from the transfer batch.</param>
        /// <returns>The <see cref="TrackableId"/> of the resulting Unity anchor if successfully loaded, or TrackableId.invalidId if the given name is not found.</returns>
        public TrackableId LoadAnchor(string name) => m_anchorTransferBatch.LoadAnchor(name);

        /// <summary>
        /// Attempts to load a specified anchor from the transfer batch and replace the specified Unity anchor's tracking data with the new anchor.
        /// </summary>
        /// <param name="name">The anchor's identifier from the transfer batch.</param>
        /// <param name="trackableId">The existing Unity anchor to update to track this new spatial anchor.</param>
        /// <returns>The <see cref="TrackableId"/> of the resulting Unity anchor (usually the same as the passed-in parameter) if successfully loaded, 
        /// or TrackableId.invalidId if the given name is not found.</returns>
        public TrackableId LoadAndReplaceAnchor(string name, TrackableId trackableId) => m_anchorTransferBatch.LoadAndReplaceAnchor(name, trackableId);

        /// <summary>
        /// Exports any anchors added via <see cref="AddAnchor"/> into a Stream for transfer. Use <see cref="ImportAsync(Stream)"/> for reading this Stream.
        /// </summary>
        /// <param name="anchorTransferBatch">The anchor transfer batch instance to export from. This instance should have had anchors added before attempting export.</param>
        /// <returns>A task which, when completed, will contain the exported array, or null if the export was unsuccessful.</returns>
        public static async Task<Stream> ExportAsync(XRAnchorTransferBatch anchorTransferBatch)
        {
            MemoryStream output = new MemoryStream();
            SerializationCompletionReason reason = await anchorTransferBatch.m_anchorTransferBatch.ExportAsync(output);

            if (reason == SerializationCompletionReason.Succeeded)
            {
                return output;
            }

            return null;
        }

        /// <summary>
        /// Imports the provided Stream into an <see cref="XRAnchorTransferBatch"/>.
        /// </summary>
        /// <param name="inputStream">The streamed data representing the result of a call to <see cref="ExportAsync(XRAnchorTransferBatch)"/>. This stream must be readable.</param>
        /// <returns>A task which, when completed, will contain the resulting XRAnchorTransferBatch, or null if the import was unsuccessful.</returns>
        public static async Task<XRAnchorTransferBatch> ImportAsync(Stream inputStream)
        {
            AnchorTransferBatch anchorTransfer = new AnchorTransferBatch();
            SerializationCompletionReason reason = await anchorTransfer.ImportAsync(inputStream);

            if (reason == SerializationCompletionReason.Succeeded)
            {
                return new XRAnchorTransferBatch(anchorTransfer);
            }

            return null;
        }
    }
}
