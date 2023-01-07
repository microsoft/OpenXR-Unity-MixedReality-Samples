// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    internal static class OpenXRAnchorStoreFactory
    {
        private static Task<OpenXRAnchorStore> m_anchorStoreLoadTask = null;

        private static ulong m_currentOpenxrSession;
        public static Task<OpenXRAnchorStore> LoadAnchorStoreAsync(XRAnchorSubsystem anchorSubsystem)
        {
            if (!(anchorSubsystem is AnchorSubsystem))
            {
                Debug.LogWarning($"LoadAnchorStoreAsync: subsystem is not of type Microsoft.MixedReality.AnchorSubsystem. type: {anchorSubsystem.GetType()}");
                return Task.FromResult<OpenXRAnchorStore>(null);
            }
            // Load anchor store only once per OpenXR session. And load again if the session changes. This is specifically added to support loading anchor store in subsequent app remoting connections. 
            if(m_anchorStoreLoadTask == null || (m_anchorStoreLoadTask.IsCompleted && m_anchorStoreLoadTask.Result == null) || m_currentOpenxrSession != OpenXRContext.Current.Session)
            {
                MixedRealityFeaturePlugin feature = OpenXRSettings.Instance.GetFeature<MixedRealityFeaturePlugin>();
                if (feature != null && feature.enabled)
                {
                    if (OpenXRContext.Current.Session == 0)
                    {
                        Debug.LogWarning("LoadAnchorStoreAsync: Cannot load anchor store without a valid XR session.");
                        return Task.FromResult<OpenXRAnchorStore>(null);
                        // After this return, m_anchorStoreLoadTask is still null, so next call will try again.
                    }
                    m_currentOpenxrSession = OpenXRContext.Current.Session;
                    m_anchorStoreLoadTask = Task.Run(() =>
                    {
                        bool nativeAnchorStoreLoaded = NativeLib.LoadAnchorStore(OpenXRAnchorStore.nativeLibToken);// Blocking, potentially long call
                        if (!nativeAnchorStoreLoaded)
                        {
                            Debug.LogWarning("LoadAnchorStoreAsync: The anchor store is not supported; either the feature is not enabled, or the related OpenXR extensions are not supported");
                            return null;
                        }
                        return new OpenXRAnchorStore();
                    });
                }
                else
                {
                    Debug.LogWarning($"LoadAnchorStoreAsync: The anchor store is not supported; {MixedRealityFeaturePlugin.featureName} is not enabled");
                    m_anchorStoreLoadTask = Task.FromResult<OpenXRAnchorStore>(null);
                }
            }
            return m_anchorStoreLoadTask;
        }
    }

    internal class OpenXRAnchorStore
    {
        internal static readonly NativeLibToken nativeLibToken = MixedRealityFeaturePlugin.nativeLibToken;
        private List<string> m_persistedAnchorNamesCache;
        private bool m_persistedAnchorNamesCacheDirty = true;
        private readonly object m_persistedAnchorNamesCacheLock = new object();

        public IReadOnlyList<string> PersistedAnchorNames
        {
            get
            {
                lock (m_persistedAnchorNamesCacheLock)
                {
                    if (m_persistedAnchorNamesCacheDirty)
                    {
                        UpdatePersistedAnchorNames();
                        m_persistedAnchorNamesCacheDirty = false;
                    }
                    return m_persistedAnchorNamesCache;
                }
            }
        }

        private void UpdatePersistedAnchorNames()
        {
            lock (m_persistedAnchorNamesCacheLock)
            {
                uint numPersisted = 0;
                m_persistedAnchorNamesCache = new List<string>();
                numPersisted = NativeLib.GetNumPersistedAnchorNames(nativeLibToken);
                for (uint i = 0; i < numPersisted; i++)
                {
                    // A persisted anchor with a name > 255 chars does not appear
                    // to be supported by the anchor store winrt implementation.
                    StringBuilder stringBuilder = new StringBuilder(255);
                    NativeLib.GetPersistedAnchorName(nativeLibToken, i, stringBuilder, (uint)stringBuilder.Capacity);
                    m_persistedAnchorNamesCache.Add(stringBuilder.ToString());
                }
            }
        }

        public TrackableId LoadAnchor(string name)
        {
            Guid persistedAnchor = NativeLib.LoadPersistedAnchor(nativeLibToken, name);
            return FeatureUtils.ToTrackableId(persistedAnchor);
        }

        public bool TryPersistAnchor(string name, TrackableId trackableId)
        {
            bool anchorPersisted = false;

            lock (m_persistedAnchorNamesCacheLock)
            {
                m_persistedAnchorNamesCacheDirty = true;
                anchorPersisted = NativeLib.TryPersistAnchor(nativeLibToken, name, FeatureUtils.ToGuid(trackableId));
            }
            return anchorPersisted;
        }

        public void UnpersistAnchor(string name)
        {
            lock (m_persistedAnchorNamesCacheLock)
            {
                m_persistedAnchorNamesCacheDirty = true;
                NativeLib.UnpersistAnchor(nativeLibToken, name);
            }
        }

        public void Clear()
        {
            lock (m_persistedAnchorNamesCacheLock)
            {
                m_persistedAnchorNamesCacheDirty = true;
                NativeLib.ClearPersistedAnchors(nativeLibToken);
            }
        }
    }

} // namespace Microsoft.MixedReality.OpenXR
