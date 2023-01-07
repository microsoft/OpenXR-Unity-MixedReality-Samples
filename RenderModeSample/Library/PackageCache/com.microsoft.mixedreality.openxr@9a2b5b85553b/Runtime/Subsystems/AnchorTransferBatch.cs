// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.XR.ARSubsystems;

#if WINDOWS_UWP
using UnityEngine;
using Windows.Perception.Spatial;
#endif // WINDOWS_UWP

namespace Microsoft.MixedReality.OpenXR
{
    internal enum SerializationCompletionReason
    {
        Succeeded = 0,
        NotSupported = 1,
        AccessDenied = 2,
        UnknownError = 3
    }

    internal class AnchorTransferBatch
    {
#if WINDOWS_UWP
        private readonly List<string> m_anchorIds = new List<string>();
        private Dictionary<string, SpatialAnchor> m_spatialAnchors = null;
#endif // WINDOWS_UWP

        public IReadOnlyList<string> AnchorNames =>
#if WINDOWS_UWP
            m_anchorIds;
#else
            Array.Empty<string>();
#endif // WINDOWS_UWP

        public bool AddAnchor(TrackableId trackableId, string name)
        {
#if WINDOWS_UWP
            m_spatialAnchors ??= new Dictionary<string, SpatialAnchor>();

            if (!m_anchorIds.Contains(name)
                && AnchorConverter.ToPerceptionSpatialAnchor(trackableId) is SpatialAnchor spatialAnchor
                && spatialAnchor != null)
            {
                m_anchorIds.Add(name);
                m_spatialAnchors.Add(name, spatialAnchor);
                return true;
            }
#endif // WINDOWS_UWP

            return false;
        }

        public void RemoveAnchor(string name)
        {
#if WINDOWS_UWP
            m_anchorIds?.Remove(name);
            m_spatialAnchors?.Remove(name);
#endif // WINDOWS_UWP
        }

        public void Clear()
        {
#if WINDOWS_UWP
            m_anchorIds?.Clear();
            m_spatialAnchors?.Clear();
#endif // WINDOWS_UWP
        }

        public TrackableId LoadAnchor(string name)
        {
#if WINDOWS_UWP
            if (m_spatialAnchors?.Count == 0)
            {
                Debug.LogWarning($"No anchors have been imported yet. Call {nameof(ImportAsync)} before calling {nameof(LoadAnchor)}");
                return TrackableId.invalidId;
            }

            if (m_spatialAnchors.TryGetValue(name, out SpatialAnchor spatialAnchor))
            {
                return AnchorConverter.CreateFromPerceptionSpatialAnchor(spatialAnchor);
            }
#endif // WINDOWS_UWP

            return TrackableId.invalidId;
        }

        public TrackableId LoadAndReplaceAnchor(string name, TrackableId trackableId)
        {
#if WINDOWS_UWP
            if (m_spatialAnchors?.Count == 0)
            {
                Debug.LogWarning($"No anchors have been imported yet. Call {nameof(ImportAsync)} before calling {nameof(LoadAndReplaceAnchor)}");
                return TrackableId.invalidId;
            }

            if (m_spatialAnchors.TryGetValue(name, out SpatialAnchor spatialAnchor))
            {
                return AnchorConverter.ReplaceSpatialAnchor(spatialAnchor, trackableId);
            }
#endif // WINDOWS_UWP

            return TrackableId.invalidId;
        }

        public async Task<SerializationCompletionReason> ExportAsync(Stream output)
        {
#if WINDOWS_UWP
#pragma warning disable CS0618 // Turn this off, so we can use the deprecated SpatialAnchorTransferManager
            SpatialPerceptionAccessStatus access = await SpatialAnchorTransferManager.RequestAccessAsync();
            if (access != SpatialPerceptionAccessStatus.Allowed)
            {
                Debug.LogError($"{nameof(SpatialAnchorTransferManager)} access not granted: {access}");
                return SerializationCompletionReason.AccessDenied;
            }

            if (m_spatialAnchors?.Count == 0)
            {
                Debug.LogError("No anchors to export!");
                return SerializationCompletionReason.UnknownError;
            }
            else
            {
                bool success = await SpatialAnchorTransferManager.TryExportAnchorsAsync(m_spatialAnchors, output.AsOutputStream());
                return success ? SerializationCompletionReason.Succeeded : SerializationCompletionReason.UnknownError;
            }
#pragma warning restore CS0618
#else
            await Task.CompletedTask;
            return SerializationCompletionReason.NotSupported;
#endif // WINDOWS_UWP
        }

        public async Task<SerializationCompletionReason> ImportAsync(Stream input)
        {
#if WINDOWS_UWP
#pragma warning disable CS0618 // Turn this off, so we can use the deprecated SpatialAnchorTransferManager
            SpatialPerceptionAccessStatus access = await SpatialAnchorTransferManager.RequestAccessAsync();
            if (access != SpatialPerceptionAccessStatus.Allowed)
            {
                Debug.LogError($"{nameof(SpatialAnchorTransferManager)} access not granted: {access}");
                return SerializationCompletionReason.AccessDenied;
            }

            IReadOnlyDictionary<string, SpatialAnchor> importedAnchors = await SpatialAnchorTransferManager.TryImportAnchorsAsync(input.AsInputStream());
            if (importedAnchors != null)
            {
                m_spatialAnchors = new Dictionary<string, SpatialAnchor>(importedAnchors.Count);
                foreach (KeyValuePair<string, SpatialAnchor> anchor in importedAnchors)
                {
                    m_spatialAnchors.Add(anchor.Key, anchor.Value);
                }

                m_anchorIds.AddRange(m_spatialAnchors.Keys);
                return SerializationCompletionReason.Succeeded;
            }
            else
            {
                return SerializationCompletionReason.UnknownError;
            }
#pragma warning restore CS0618
#else
            await Task.CompletedTask;
            return SerializationCompletionReason.NotSupported;
#endif // WINDOWS_UWP
        }
    }
}
