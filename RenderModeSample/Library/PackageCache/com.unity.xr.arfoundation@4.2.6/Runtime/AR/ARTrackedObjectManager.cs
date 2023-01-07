using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A manager for <see cref="ARTrackedObject"/>s. Uses the <c>XRObjectTrackingSubsystem</c>
    /// to recognize and track 3D objects in the physical environment.
    /// </summary>
    [DefaultExecutionOrder(ARUpdateOrder.k_TrackedObjectManager)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARSessionOrigin))]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARTrackedObjectManager) + ".html")]
    public sealed class ARTrackedObjectManager : ARTrackableManager<
        XRObjectTrackingSubsystem,
        XRObjectTrackingSubsystemDescriptor,
        XRObjectTrackingSubsystem.Provider,
        XRTrackedObject,
        ARTrackedObject>
    {
        [SerializeField]
        [Tooltip("The library of objects which will be detected and/or tracked in the physical environment.")]
        XRReferenceObjectLibrary m_ReferenceLibrary;

        /// <summary>
        /// The <c>ARObjectLibrary</c> to use during object detection. This is the
        /// library of objects which will be detected and tracked in the physical environment.
        /// </summary>
        public XRReferenceObjectLibrary referenceLibrary
        {
            get => m_ReferenceLibrary;
            set
            {
                m_ReferenceLibrary = value;
                UpdateReferenceObjects();

                if (subsystem != null)
                {
                    subsystem.library = m_ReferenceLibrary;
                }
            }
        }

        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each detected object.")]
        GameObject m_TrackedObjectPrefab;

        /// <summary>
        /// If not null, instantiates this Prefab for each detected object.
        /// </summary>
        public GameObject trackedObjectPrefab
        {
            get => m_TrackedObjectPrefab;
            set => m_TrackedObjectPrefab = value;
        }

        /// <summary>
        /// Get the Prefab to instantiate for each <see cref="ARTrackedObject"/>.
        /// </summary>
        /// <returns>The prefab to instantiate for each <see cref="ARTrackedObject"/>.</returns>
        protected override GameObject GetPrefab() => m_TrackedObjectPrefab;

        /// <summary>
        /// Invoked once per frame with information about the <see cref="ARTrackedObject"/>s that have changed (that is, been added, updated, or removed).
        /// This happens just before <see cref="ARTrackedObject"/>s are destroyed, so you can set <c>ARTrackedObject.destroyOnRemoval</c> to <c>false</c>
        /// from this event to suppress this behavior.
        /// </summary>
        public event Action<ARTrackedObjectsChangedEventArgs> trackedObjectsChanged;

        /// <summary>
        /// The name to be used for the <c>GameObject</c> whenever a new object is detected.
        /// </summary>
        protected override string gameObjectName => "ARTrackedObject";

        /// <summary>
        /// Sets the object library on the subsystem before Start() is called on the base class.
        /// </summary>
        protected override void OnBeforeStart()
        {
            UpdateReferenceObjects();
            if (referenceLibrary != null)
            {
                subsystem.library = referenceLibrary;
            }
            else
            {
                enabled = false;
#if DEVELOPMENT_BUILD
                Debug.LogWarning($"{nameof(ARTrackedObjectManager)} '{name}' was enabled but no reference object library is specified. To enable, set a valid reference object library and then re-enable this component.");
#endif
            }
        }

        /// <summary>
        /// Invoked just after each <see cref="ARTrackedObject"/> has been updated.
        /// </summary>
        /// <param name="trackedObject">The <see cref="ARTrackedObject"/> being updated.</param>
        /// <param name="sessionRelativeData">New data associated with <paramref name="trackedObject"/>.
        /// All spatial data is relative to the <see cref="ARSessionOrigin"/>.</param>
        protected override void OnAfterSetSessionRelativeData(
            ARTrackedObject trackedObject,
            XRTrackedObject sessionRelativeData)
        {
            if (m_ReferenceLibrary != null && m_ReferenceObjectCount != m_ReferenceLibrary.count)
            {
                // Since reference libraries can change, make sure
                // our reference object dictionary is up to date.
                UpdateReferenceObjects();
            }

            var guid = sessionRelativeData.referenceObjectGuid;
            if (!m_ReferenceObjects.TryGetValue(guid, out var referenceObject))
            {
                Debug.LogError($"Could not find reference object with guid {guid}");
            }

            trackedObject.referenceObject = referenceObject;
        }

        /// <summary>
        /// Invokes the <see cref="trackedObjectsChanged"/> event.
        /// </summary>
        /// <param name="added">A list of objects added this frame.</param>
        /// <param name="updated">A list of objects updated this frame.</param>
        /// <param name="removed">A list of objects removed this frame.</param>
        protected override void OnTrackablesChanged(
            List<ARTrackedObject> added,
            List<ARTrackedObject> updated,
            List<ARTrackedObject> removed)
        {
            if (trackedObjectsChanged != null)
            {
                using (new ScopedProfiler("OnTrackedObjectsChanged"))
                trackedObjectsChanged(
                    new ARTrackedObjectsChangedEventArgs(
                        added,
                        updated,
                        removed));
            }
        }

        void UpdateReferenceObjects()
        {
            m_ReferenceObjects.Clear();
            m_ReferenceObjectCount = 0;
            if (m_ReferenceLibrary == null)
                return;

            foreach (var referenceObject in m_ReferenceLibrary)
            {
                m_ReferenceObjects[referenceObject.guid] = referenceObject;
            }

            m_ReferenceObjectCount = m_ReferenceObjects.Count;
        }

        int m_ReferenceObjectCount;

        Dictionary<Guid, XRReferenceObject> m_ReferenceObjects = new Dictionary<Guid, XRReferenceObject>();
    }
}
