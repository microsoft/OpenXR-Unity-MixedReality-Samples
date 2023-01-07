using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Manages reference points.
    /// </summary>
    /// <remarks>
    /// <para>Use this component to programmatically add, remove, or query for
    /// reference points. Reference points are `Pose`s in the world
    /// which will be periodically updated by an AR device as its understanding
    /// of the world changes.</para>
    /// <para>Subscribe to changes (added, updated, and removed) via the
    /// <see cref="ARReferencePointManager.referencePointsChanged"/> event.</para>
    /// </remarks>
    /// <seealso cref="ARTrackableManager{TSubsystem,TSubsystemDescriptor,TProvider,TSessionRelativeData,TTrackable}"/>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARSessionOrigin))]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARReferencePointManager) + ".html")]
    [Obsolete("ARReferencePointManager has been deprecated. Use ARAnchorManager instead (UnityUpgradable) -> UnityEngine.XR.ARFoundation.ARAnchorManager", true)]
    public sealed class ARReferencePointManager : ARTrackableManager<
        XRReferencePointSubsystem,
        XRReferencePointSubsystemDescriptor,
        XRReferencePointSubsystem.Provider,
        XRReferencePoint,
        ARReferencePoint>
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each instantiated reference point.")]
        GameObject m_ReferencePointPrefab;

        /// <summary>
        /// This Prefab will be instantiated for each <see cref="ARReferencePoint"/>. Can be `null`.
        /// </summary>
        [Obsolete("ARReferencePointManger.referencePointPrefab has been renamed. Use ARAnchorManager.anchorPrefab instead (UnityUpgradable) -> UnityEngine.XR.ARFoundation.ARAnchorManager.anchorPrefab", true)]
        public GameObject referencePointPrefab
        {
            get => m_ReferencePointPrefab;
            set => m_ReferencePointPrefab = value;
        }

        /// <summary>
        /// Invoked once per frame to communicate changes to reference points, including
        /// new reference points, the update of existing reference points, and the removal
        /// of previously existing reference points.
        /// </summary>
        [Obsolete("ARReferencePointManger.referencePointsChanged has been renamed. Use ARAnchorManager.anchorsChanged instead (UnityUpgradable) -> UnityEngine.XR.ARFoundation.ARAnchorManager.anchorsChanged", true)]
        public event Action<ARReferencePointsChangedEventArgs> referencePointsChanged;

        /// <summary>
        /// Attempts to add an <see cref="ARReferencePoint"/> with the given <c>Pose</c>.
        /// </summary>
        /// <remarks>
        /// If <see cref="ARTrackableManager{TSubsystem,TSubsystemDescriptor,TProvider,TSessionRelativeData,TTrackable}.GetPrefab()"/>
        /// is not null, a new instance of that Prefab will be instantiated. Otherwise, a
        /// new <c>GameObject</c> will be created. In either case, the resulting
        /// <c>GameObject</c> will have an <see cref="ARReferencePoint"/> component on it.
        /// </remarks>
        /// <param name="pose">The pose, in Unity world space, of the <see cref="ARReferencePoint"/>.</param>
        /// <returns>A new <see cref="ARReferencePoint"/> if successful, otherwise <c>null</c>.</returns>
        [Obsolete("ARReferencePointManger.AddReferencePoint() has been deprecated. Use ARAnchorManager.AddAnchor() instead (UnityUpgradable) -> UnityEngine.XR.ARFoundation.ARAnchorManager.AddAnchor(*)", true)]
        public ARReferencePoint AddReferencePoint(Pose pose)
        {
            if (!enabled)
                throw new InvalidOperationException("Cannot create a reference point from a disabled reference point manager.");

            if (subsystem == null)
                throw new InvalidOperationException("Reference point manager has no subsystem. Enable the manager first.");

            var sessionRelativePose = sessionOrigin.trackablesParent.InverseTransformPose(pose);

            // Add the reference point to the XRReferencePointSubsystem
            XRReferencePoint sessionRelativeData;
            if (subsystem.TryAddReferencePoint(sessionRelativePose, out sessionRelativeData))
                return CreateTrackableImmediate(sessionRelativeData);

            return null;
        }

        /// <summary>
        /// Attempts to create a new reference point that is attached to an existing <see cref="ARPlane"/>.
        /// </summary>
        /// <param name="plane">The <see cref="ARPlane"/> to attach to.</param>
        /// <param name="pose">The initial <c>Pose</c>, in Unity world space, of the reference point.</param>
        /// <returns>A new <see cref="ARReferencePoint"/> if successful, otherwise <c>null</c>.</returns>
        [Obsolete("ARReferencePointManger.AttachReferencePoint() has been deprecated. Use ARAnchorManager.AttachAnchor() instead (UnityUpgradable) -> UnityEngine.XR.ARFoundation.ARAnchorManager.AttachAnchor(*)", true)]
        public ARReferencePoint AttachReferencePoint(ARPlane plane, Pose pose)
        {
            if (!enabled)
                throw new InvalidOperationException("Cannot create a reference point from a disabled reference point manager.");

            if (subsystem == null)
                throw new InvalidOperationException("Reference point manager has no subsystem. Enable the manager first.");

            if (plane == null)
                throw new ArgumentNullException("plane");

            var sessionRelativePose = sessionOrigin.trackablesParent.InverseTransformPose(pose);
            XRReferencePoint sessionRelativeData;
            if (subsystem.TryAttachReferencePoint(plane.trackableId, sessionRelativePose, out sessionRelativeData))
                return CreateTrackableImmediate(sessionRelativeData);

            return null;
        }

        /// <summary>
        /// Attempts to remove an <see cref="ARReferencePoint"/>.
        /// </summary>
        /// <param name="referencePoint">The reference point you wish to remove.</param>
        /// <returns>
        /// <c>True</c> if the reference point was successfully removed.
        /// <c>False</c> usually means the reference point is not longer tracked by the system.
        /// </returns>
        [Obsolete("ARReferencePointManger.RemoveReferencePoint() has been deprecated. Use ARAnchorManager.RemoveAnchor() instead (UnityUpgradable) -> UnityEngine.XR.ARFoundation.ARAnchorManager.RemoveAnchor(*)", true)]
        public bool RemoveReferencePoint(ARReferencePoint referencePoint)
        {
            if (!enabled)
                throw new InvalidOperationException("Cannot create a reference point from a disabled reference point manager.");

            if (subsystem == null)
                throw new InvalidOperationException("Reference point manager has no subsystem. Enable the manager first.");

            if (referencePoint == null)
                throw new ArgumentNullException("referencePoint");

            if (subsystem.TryRemoveReferencePoint(referencePoint.trackableId))
            {
                DestroyPendingTrackable(referencePoint.trackableId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="ARReferencePoint"/> with given <paramref name="trackableId"/>,
        /// or <c>null</c> if it does not exist.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> of the <see cref="ARReferencePoint"/> to retrieve.</param>
        /// <returns>The <see cref="ARReferencePoint"/> with <paramref name="trackableId"/> or <c>null</c> if it does not exist.</returns>
        [Obsolete("ARReferencePointManger.GetReferencePoint() has been deprecated. Use ARAnchorManager.GetAnchor() instead (UnityUpgradable) -> UnityEngine.XR.ARFoundation.ARAnchorManager.GetAnchor(*)", true)]
        public ARReferencePoint GetReferencePoint(TrackableId trackableId)
        {
            ARReferencePoint referencePoint;
            if (m_Trackables.TryGetValue(trackableId, out referencePoint))
                return referencePoint;

            return null;
        }

        /// <summary>
        /// Gets the Prefab that will be instantiated for each <see cref="ARReferencePoint"/>.
        /// </summary>
        /// <returns>The Prefab that will be instantiated for each <see cref="ARReferencePoint"/>.</returns>
        protected override GameObject GetPrefab() => m_ReferencePointPrefab;

        /// <summary>
        /// The name given to each `GameObject` associated with each <see cref="ARReferencePoint"/>.
        /// </summary>
        protected override string gameObjectName => "ReferencePoint";

        /// <summary>
        /// Invoked when the base class detects trackable changes.
        /// </summary>
        /// <param name="added">The list of added <see cref="ARReferencePoint"/>s.</param>
        /// <param name="updated">The list of updated <see cref="ARReferencePoint"/>s.</param>
        /// <param name="removed">The list of removed <see cref="ARReferencePoint"/>s.</param>
        protected override void OnTrackablesChanged(
            List<ARReferencePoint> added,
            List<ARReferencePoint> updated,
            List<ARReferencePoint> removed)
        {
            if (referencePointsChanged != null)
            {
                using (new ScopedProfiler("OnReferencePointsChanged"))
                referencePointsChanged(
                    new ARReferencePointsChangedEventArgs(
                        added,
                        updated,
                        removed));
            }
        }
    }
}
