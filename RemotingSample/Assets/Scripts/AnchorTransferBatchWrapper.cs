// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// This class wraps <see cref="XRAnchorTransferBatch"/> to maintain a consistent API shape with Unity's legacy WorldAnchorTransferBatch.
    /// Use <see cref="XRAnchorTransferBatch"/> in favor of this script if you aren't dependent on migrating legacy code.
    /// </summary>
    /// <remarks>Use of this class requires an <see cref="ARAnchorManager"/> in the scene or some other manual management of an XRAnchorSubsystem.</remarks>
    public class AnchorTransferBatchWrapper
    {
        private XRAnchorTransferBatch anchorTransferBatch = new XRAnchorTransferBatch();

        /// <summary>
        /// Gets the number of anchors in this XRAnchorTransferBatch.
        /// </summary>
        public int anchorCount => anchorTransferBatch.AnchorNames.Count;

        /// <summary>
        /// Gets all of the identifiers currently mapped in this AnchorTransferBatchWrapper.
        /// </summary>
        public string[] GetAllIds() => anchorTransferBatch.AnchorNames.ToArray();

        /// <summary>
        /// Gets all of the identifiers currently mapped in this AnchorTransferBatchWrapper.
        /// If the target array is not large enough to contain all the identifiers, then only those identifiers that fit within the array will be stored and the return value will equal the size of the array.
        /// You can detect this condition by checking for a return value less than AnchorTransferBatchWrapper.anchorCount.
        /// </summary>
        public int GetAllIds(string[] ids)
        {
            Array.Clear(ids, 0, ids.Length);
            IReadOnlyList<string> names = anchorTransferBatch.AnchorNames;
            int idCount = Math.Min(ids.Length, names.Count);
            for (int i = 0; i < idCount; i++)
            {
                ids[i] = names[i];
            }
            return idCount;
        }

        /// <summary>
        /// Adds an ARAnchor to the batch with the specified identifier.
        /// </summary>
        public bool AddWorldAnchor(string id, ARAnchor anchor) => anchorTransferBatch.AddAnchor(anchor.trackableId, id);

        /// <summary>
        /// Cleans up the XRAnchorTransferBatch.
        /// </summary>
        public void Dispose() => anchorTransferBatch.Clear();

        /// <summary>
        /// Ensures the specified GameObject has an <see cref="ARAnchor"/> attached.
        /// Then, replaces the ARAnchor's underlying platform anchor with the imported one.
        /// </summary>
        public ARAnchor LockObject(string id, GameObject go)
        {
            if (anchorTransferBatch.AnchorNames.Count == 0)
            {
                Debug.LogWarning($"No anchors have been imported yet. Call {nameof(XRAnchorTransferBatch.ImportAsync)} before calling {nameof(LockObject)}");
                return null;
            }

            if (!go.TryGetComponent(out ARAnchor anchor))
            {
                anchor = go.AddComponent<ARAnchor>();
            }

            anchorTransferBatch.LoadAndReplaceAnchor(id, anchor.trackableId);
            return anchor;
        }

        /// <summary>
        /// The handler for when some data is available from serialization.
        /// </summary>
        /// <remarks>The data from an export may be provided in chunks. Each chunk is ordered and must be in the correct order when restored.</remarks>
        public delegate void SerializationDataAvailableDelegate(byte[] data);

        /// <summary>
        /// The handler for when serialization is completed.
        /// </summary>
        public delegate void SerializationCompleteDelegate(SerializationCompletionReason completionReason);

        /// <summary>
        /// The handler for when deserialization has completed.
        /// </summary>
        public delegate void DeserializationCompleteDelegate(SerializationCompletionReason completionReason, AnchorTransferBatchWrapper deserializedAnchorTransferBatch);

        /// <summary>
        /// Exports the input AnchorTransferBatchWrapper into a byte array which can be passed to AnchorTransferBatchWrapper.ImportAsync to restore the original AnchorTransferBatchWrapper.
        /// </summary>
        public static void ExportAsync(AnchorTransferBatchWrapper transferBatch, SerializationDataAvailableDelegate onDataAvailable, SerializationCompleteDelegate onCompleted) => ExportAsyncInternal(transferBatch, onDataAvailable, onCompleted);

        private static async void ExportAsyncInternal(AnchorTransferBatchWrapper transferBatch, SerializationDataAvailableDelegate onDataAvailable, SerializationCompleteDelegate onCompleted)
        {
            Stream exportStream = await XRAnchorTransferBatch.ExportAsync(transferBatch.anchorTransferBatch);
            byte[] exportBytes = null;
            if (exportStream != null)
            {
                if (!(exportStream is MemoryStream memoryStream))
                {
                    memoryStream = new MemoryStream();
                    exportStream.CopyTo(memoryStream);
                }
                exportBytes = memoryStream.ToArray();
                onDataAvailable?.Invoke(exportBytes);
            }
            onCompleted?.Invoke(exportBytes?.Length > 0 ? SerializationCompletionReason.Succeeded : SerializationCompletionReason.UnknownError);
        }

        /// <summary>
        /// Imports the provided bytes into a AnchorTransferBatchWrapper.
        /// </summary>
        public static void ImportAsync(byte[] serializedData, DeserializationCompleteDelegate onComplete) => ImportAsyncInternal(serializedData, onComplete);

        /// <summary>
        /// Imports the provided bytes into a AnchorTransferBatchWrapper.
        /// </summary>
        public static void ImportAsync(byte[] serializedData, int offset, int length, DeserializationCompleteDelegate onComplete) => ImportAsyncInternal(serializedData, onComplete, offset, length);

        private static async void ImportAsyncInternal(byte[] serializedData, DeserializationCompleteDelegate onComplete, int offset = 0, int length = -1)
        {
            if (length < 0)
            {
                length = serializedData.Length;
            }

            AnchorTransferBatchWrapper anchorTransferBatchWrapper = new AnchorTransferBatchWrapper
            {
                anchorTransferBatch = await XRAnchorTransferBatch.ImportAsync(new MemoryStream(serializedData, offset, length))
            };
            onComplete?.Invoke(anchorTransferBatchWrapper.anchorTransferBatch != null ? SerializationCompletionReason.Succeeded : SerializationCompletionReason.UnknownError, anchorTransferBatchWrapper);
        }
    }

    public enum SerializationCompletionReason
    {
        Succeeded = 0,
        NotSupported = 1,
        AccessDenied = 2,
        UnknownError = 3
    }
}
