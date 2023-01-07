// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.ARFoundation
{
    /// <summary>
    /// Provides extension methods to use an ARAnchorManager to load an XRAnchorStore.
    /// </summary>
    public static class AnchorManagerExtensions
    {
        /// <summary>
        /// Use this ARAnchorManager to load an XRAnchorStore.
        /// </summary>
        /// <returns>
        /// A task which, when completed, will contain a valid XRAnchorStore, or will contain null if the anchor store could not be loaded.
        /// </returns>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> to receive the loaded anchors from the store.</param>
        /// <remarks>
        /// The anchor subsystem might not be available if the XR session is not initialized at start up. In this case, the returned anchor store might be null. 
        /// Make sure to reload the anchor store after the XR session is initialized.
        /// </remarks>
        public static Task<XRAnchorStore> LoadAnchorStoreAsync(this ARAnchorManager anchorManager)
        {
            return XRAnchorStore.LoadAnchorStoreAsync(anchorManager.subsystem);
        }
    }
}

namespace Microsoft.MixedReality.OpenXR.ARSubsystems
{
    /// <summary>
    /// Provides extension methods to use an XRAnchorSubsystem to load an XRAnchorStore.
    /// </summary>
    public static class AnchorSubsystemExtensions
    {
        /// <summary>
        /// Use this XRAnchorSubsystem to load an XRAnchorStore.
        /// </summary>
        /// <returns>
        /// A task which, when completed, will contain a valid XRAnchorStore, or contain null if the anchor store could not be loaded.
        /// </returns>
        /// <param name="anchorSubsystem">The <see cref="XRAnchorSubsystem"/> to receive the loaded anchors from the store.</param>
        /// <remarks>
        /// The anchor subsystem might not be available if the XR session is not initialized at start up. In this case, the returned anchor store might be null. 
        /// Make sure to reload the anchor store after the XR session is initialized.
        /// </remarks>
        public static Task<XRAnchorStore> LoadAnchorStoreAsync(this XRAnchorSubsystem anchorSubsystem)
        {
            return XRAnchorStore.LoadAnchorStoreAsync(anchorSubsystem);
        }
    }
}

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Handles persisting anchors from the scene to the anchor store, loading anchors from the anchor store to the scene,
    /// and managing anchors persisted in the anchor store.
    /// </summary>
    /// <remarks>
    /// Persisting and unpersisting anchors from the anchor store does not affect their state in the Unity scene.
    /// These operations only affect whether anchors will be available to load from the anchor store in the future.
    /// </remarks>
    public class XRAnchorStore
    {
        /// <summary>
        /// The names of all persisted anchors available in this anchor store. Each of these persisted anchors can be loaded with <see cref="LoadAnchor"/>.
        /// </summary>
        public IReadOnlyList<string> PersistedAnchorNames => m_openxrAnchorStore.PersistedAnchorNames;

        /// <summary>
        /// Begin loading an anchor from the anchor store into the Unity scene.
        /// On a future update of the ARAnchorManager or XRAnchorSubsystem, a new corresponding anchor will be created.
        /// </summary>
        /// <remarks>If the persisted anchor has already been loaded, the TrackableId for the existing anchor in the scene will be returned.</remarks>
        /// <returns>The TrackableId which the anchor will use once it is created.</returns>
        /// <param name="name">The name of the anchor to be loaded from the store.</param>
        public TrackableId LoadAnchor(string name) => m_openxrAnchorStore.LoadAnchor(name);

        /// <summary>
        /// Persist an anchor in the anchor store, where it can be retrieved using <see cref="LoadAnchor"/>.
        /// </summary>
        /// <returns>True if the anchor was successfully persisted, false if an error occurred.</returns>
        /// <param name="trackableId">The <see cref="TrackableId"/> of an anchor to be persisted in the store.</param>
        /// <param name="name">A string to identify this anchor if it's successfully persisted in the store.</param>
        public bool TryPersistAnchor(TrackableId trackableId, string name) => m_openxrAnchorStore.TryPersistAnchor(name, trackableId);

        /// <summary>
        /// Unpersist an anchor from the anchor store.
        /// </summary>
        /// <remarks>After an anchor is unpersisted from the store, it will still be valid and locatable in the current session.</remarks>
        /// <param name="name">The name of the anchor to be unpersist from the store.</param>
        public void UnpersistAnchor(string name) => m_openxrAnchorStore.UnpersistAnchor(name);

        /// <summary>
        /// Clear all persisted anchors from the anchor store.
        /// </summary>
        /// <remarks>After the anchors are cleared from the anchor store, they will still be valid and locatable in the current session.</remarks>
        public void Clear() => m_openxrAnchorStore.Clear();

        /// <summary>
        /// Deprecated: Uses an existing <see cref="XRAnchorSubsystem"/> to load an XRAnchorStore.
        /// </summary>
        /// <returns>
        /// A task which, when completed, will contain a valid XRAnchorStore, or contain null if the anchor store could not be loaded.
        /// </returns>
        /// <remarks>
        /// The anchor subsystem might not be available if the XR session is not initialized at start up. In this case, the returned anchor store might be null.
        /// Make sure to reload the anchor store after the XR session is initialized.
        /// </remarks>
        [Obsolete("This method is obsolete. Use the LoadAnchorStoreAsync() extension method on an XRAnchorSubsystem or an ARAnchorManager instead.", false)]
        public static async Task<XRAnchorStore> LoadAsync(XRAnchorSubsystem anchorSubsystem) => await LoadAnchorStoreAsync(anchorSubsystem);

        internal static async Task<XRAnchorStore> LoadAnchorStoreAsync(XRAnchorSubsystem anchorSubsystem)
        {
            OpenXRAnchorStore openxrAnchorStore = await OpenXRAnchorStoreFactory.LoadAnchorStoreAsync(anchorSubsystem);
            return openxrAnchorStore == null ? null : new XRAnchorStore(openxrAnchorStore);
        }

        internal XRAnchorStore(OpenXRAnchorStore openxrAnchorStore)
        {
            m_openxrAnchorStore = openxrAnchorStore;
        }

        private readonly OpenXRAnchorStore m_openxrAnchorStore;
    }
}
