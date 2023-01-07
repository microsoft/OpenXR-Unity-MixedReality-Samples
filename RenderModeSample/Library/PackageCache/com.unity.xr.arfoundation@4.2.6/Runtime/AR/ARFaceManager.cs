using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Serialization;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Creates, updates, and removes <c>GameObject</c>s with <see cref="ARFace"/> components under the
    /// <see cref="ARSessionOrigin"/>'s <see cref="ARSessionOrigin.trackablesParent"/>.
    /// </summary>
    /// <remarks>
    /// When enabled, this component subscribes to <see cref="ARFaceManager.facesChanged"/> event to update face data.
    /// If this component is disabled, and there are no other subscribers to those events,
    /// face detection will be disabled on the device.
    /// </remarks>
    [RequireComponent(typeof(ARSessionOrigin))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ARUpdateOrder.k_FaceManager)]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARFaceManager) + ".html")]
    public sealed class ARFaceManager : ARTrackableManager<
        XRFaceSubsystem,
        XRFaceSubsystemDescriptor,
        XRFaceSubsystem.Provider,
        XRFace,
        ARFace>
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each created face.")]
        GameObject m_FacePrefab;

        /// <summary>
        /// Getter/setter for the Face Prefab.
        /// </summary>
        public GameObject facePrefab
        {
            get => m_FacePrefab;
            set => m_FacePrefab = value;
        }

        [SerializeField]
        [Tooltip("The maximum number of faces to track simultaneously.")]
        int m_MaximumFaceCount = 1;

        /// <summary>
        /// Get or set the requested maximum number of faces to track simultaneously
        /// </summary>
        public int requestedMaximumFaceCount
        {
            get => subsystem?.requestedMaximumFaceCount ?? m_MaximumFaceCount;
            set
            {
                m_MaximumFaceCount = value;
                if (enabled && subsystem != null)
                {
                    subsystem.requestedMaximumFaceCount = value;
                }
            }
        }

        /// <summary>
        /// Get or set the maximum number of faces to track simultaneously. This method is obsolete.
        /// Use <see cref="currentMaximumFaceCount"/> or <see cref="requestedMaximumFaceCount"/> instead.
        /// </summary>
        [Obsolete("Use requestedMaximumFaceCount or currentMaximumFaceCount instead. (2020-01-14)")]
        public int maximumFaceCount
        {
            get => subsystem?.currentMaximumFaceCount ?? m_MaximumFaceCount;
            set => requestedMaximumFaceCount = value;
        }

        /// <summary>
        /// Get the maximum number of faces to track simultaneously.
        /// </summary>
        public int currentMaximumFaceCount => subsystem?.currentMaximumFaceCount ?? 0;

        /// <summary>
        /// Get the supported number of faces that can be tracked simultaneously. This value
        /// might change when the configuration changes.
        /// </summary>
        public int supportedFaceCount => subsystem?.supportedFaceCount ?? 0;

        /// <summary>
        /// Raised for each new <see cref="ARFace"/> detected in the environment.
        /// </summary>
        public event Action<ARFacesChangedEventArgs> facesChanged;

        /// <summary>
        /// Attempts to retrieve an <see cref="ARFace"/>.
        /// </summary>
        /// <param name="faceId">The <c>TrackableId</c> associated with the <see cref="ARFace"/>.</param>
        /// <returns>The <see cref="ARFace"/>if found. <c>null</c> otherwise.</returns>
        public ARFace TryGetFace(TrackableId faceId)
        {
            m_Trackables.TryGetValue(faceId, out ARFace face);
            return face;
        }

        /// <summary>
        /// Invoked just before calling `Start` on the Subsystem. Used to set the `requestedMaximumFaceCount`
        /// on the subsystem.
        /// </summary>
        protected override void OnBeforeStart()
        {
            subsystem.requestedMaximumFaceCount = m_MaximumFaceCount;
        }

        /// <summary>
        /// Invoked just after a <see cref="ARFace"/> has been updated.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="sessionRelativeData"></param>
        protected override void OnAfterSetSessionRelativeData(
            ARFace face,
            XRFace sessionRelativeData)
        {
            face.UpdateMesh(subsystem);

            if (subsystem.subsystemDescriptor.supportsEyeTracking)
                face.UpdateEyes();
        }

        /// <summary>
        /// Invoked when the base class detects trackable changes.
        /// </summary>
        /// <param name="added">The list of added <see cref="ARFace"/>s.</param>
        /// <param name="updated">The list of updated <see cref="ARFace"/>s.</param>
        /// <param name="removed">The list of removed <see cref="ARFace"/>s.</param>
        protected override void OnTrackablesChanged(
            List<ARFace> added,
            List<ARFace> updated,
            List<ARFace> removed)
        {
            if (facesChanged != null)
            {
                using (new ScopedProfiler("OnFacesChanged"))
                facesChanged(new ARFacesChangedEventArgs(added, updated, removed));
            }
        }

        /// <summary>
        /// Get the Prefab that will be instantiated for each <see cref="ARFace"/>. Can be `null`.
        /// </summary>
        /// <returns>The prefab that will be instantiated for each <see cref="ARFace"/>.</returns>
        protected override GameObject GetPrefab() => m_FacePrefab;

        /// <summary>
        /// The name assigned to each `GameObject` belonging to each <see cref="ARFace"/>.
        /// </summary>
        protected override string gameObjectName => "ARFace";
    }
}
