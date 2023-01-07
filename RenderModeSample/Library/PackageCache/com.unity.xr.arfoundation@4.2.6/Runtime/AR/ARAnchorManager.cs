using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Manages anchors.
    /// </summary>
    /// <remarks>
    /// <para>Use this component to programmatically add, remove, or query for
    /// anchors. Anchors are <c>Pose</c>s in the world
    /// which will be periodically updated by an AR device as its understanding
    /// of the world changes.</para>
    /// <para>Subscribe to changes (added, updated, and removed) via the
    /// <see cref="ARAnchorManager.anchorsChanged"/> event.</para>
    /// </remarks>
    /// <seealso cref="ARTrackableManager{TSubsystem,TSubsystemDescriptor,TProvider,TSessionRelativeData,TTrackable}"/>
    [DefaultExecutionOrder(ARUpdateOrder.k_AnchorManager)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARSessionOrigin))]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARAnchorManager) + ".html")]
    public sealed class ARAnchorManager : ARTrackableManager<
        XRAnchorSubsystem,
        XRAnchorSubsystemDescriptor,
        XRAnchorSubsystem.Provider,
        XRAnchor,
        ARAnchor>
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each instantiated anchor.")]
        [FormerlySerializedAs("m_ReferencePointPrefab")]
        GameObject m_AnchorPrefab;

        /// <summary>
        /// This prefab will be instantiated for each <see cref="ARAnchor"/>. May be `null`.
        /// </summary>
        /// <remarks>
        /// The purpose of this property is to extend the functionality of <see cref="ARAnchor"/>s.
        /// It is not the recommended way to instantiate content associated with an <see cref="ARAnchor"/>.
        /// See [Anchoring content](xref:arfoundation-anchor-manager#anchoring-content) for more details.
        /// </remarks>
        public GameObject anchorPrefab
        {
            get => m_AnchorPrefab;
            set => m_AnchorPrefab = value;
        }

        /// <summary>
        /// Invoked once per frame to communicate changes to anchors, including
        /// new anchors, the update of existing anchors, and the removal
        /// of previously existing anchors.
        /// </summary>
        public event Action<ARAnchorsChangedEventArgs> anchorsChanged;

        /// <summary>
        /// Attempts to add an <see cref="ARAnchor"/> with the given <c>Pose</c>.
        /// </summary>
        /// <remarks>
        /// If <see cref="ARTrackableManager{TSubsystem,TSubsystemDescriptor,TProvider,TSessionRelativeData,TTrackable}.GetPrefab()"/>
        /// is not null, a new instance of that prefab will be instantiated. Otherwise, a
        /// new <c>GameObject</c> will be created. In either case, the resulting
        /// <c>GameObject</c> will have an <see cref="ARAnchor"/> component on it.
        /// </remarks>
        /// <param name="pose">The pose, in Unity world space, of the <see cref="ARAnchor"/>.</param>
        /// <returns>A new <see cref="ARAnchor"/> if successful, otherwise <c>null</c>.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if this `MonoBehaviour` is not enabled.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the underlying subsystem is `null`.</exception>
        [Obsolete("Add an anchor using AddComponent<" + nameof(ARAnchor) + ">(). (2020-10-06)")]
        public ARAnchor AddAnchor(Pose pose)
        {
            if (!enabled)
                throw new InvalidOperationException("Cannot create an anchor from a disabled anchor manager.");

            if (subsystem == null)
                throw new InvalidOperationException("Anchor manager has no subsystem. Enable the manager first.");

            var sessionRelativePose = sessionOrigin.trackablesParent.InverseTransformPose(pose);

            // Add the anchor to the XRAnchorSubsystem
            if (subsystem.TryAddAnchor(sessionRelativePose, out var sessionRelativeData))
            {
                return CreateTrackableImmediate(sessionRelativeData);
            }

            return null;
        }

        internal bool TryAddAnchor(ARAnchor anchor)
        {
            if (!CanBeAddedToSubsystem(anchor))
                return false;

            var t = anchor.transform;
            var sessionRelativePose = sessionOrigin.trackablesParent.InverseTransformPose(new Pose(t.position, t.rotation));

            // Add the anchor to the XRAnchorSubsystem
            if (subsystem.TryAddAnchor(sessionRelativePose, out var sessionRelativeData))
            {
                CreateTrackableFromExisting(anchor, sessionRelativeData);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to create a new anchor that is attached to an existing <see cref="ARPlane"/>.
        /// </summary>
        /// <param name="plane">The <see cref="ARPlane"/> to which to attach.</param>
        /// <param name="pose">The initial <c>Pose</c>, in Unity world space, of the anchor.</param>
        /// <returns>A new <see cref="ARAnchor"/> if successful, otherwise <c>null</c>.</returns>
        public ARAnchor AttachAnchor(ARPlane plane, Pose pose)
        {
            if (!enabled)
                throw new InvalidOperationException("Cannot create an anchor from a disabled anchor manager.");

            if (subsystem == null)
                throw new InvalidOperationException("Anchor manager has no subsystem. Enable the manager first.");

            if (plane == null)
                throw new ArgumentNullException(nameof(plane));

            var sessionRelativePose = sessionOrigin.trackablesParent.InverseTransformPose(pose);
            if (subsystem.TryAttachAnchor(plane.trackableId, sessionRelativePose, out var sessionRelativeData))
            {
                return CreateTrackableImmediate(sessionRelativeData);
            }

            return null;
        }

        /// <summary>
        /// Attempts to remove an <see cref="ARAnchor"/>.
        /// </summary>
        /// <param name="anchor">The anchor you wish to remove.</param>
        /// <returns>
        /// <c>True</c> if the anchor was successfully removed.
        /// <c>False</c> usually means the anchor is not longer tracked by the system.
        /// </returns>
        [Obsolete("Call Destroy() on the " + nameof(ARAnchor) + " component to remove it. (2020-10-06)")]
        public bool RemoveAnchor(ARAnchor anchor)
        {
            if (!enabled)
                throw new InvalidOperationException("Cannot create an anchor from a disabled anchor manager.");

            if (subsystem == null)
                throw new InvalidOperationException("Anchor manager has no subsystem. Enable the manager first.");

            if (anchor == null)
                throw new ArgumentNullException(nameof(anchor));

            if (subsystem.TryRemoveAnchor(anchor.trackableId))
            {
                DestroyPendingTrackable(anchor.trackableId);
                return true;
            }

            return false;
        }

        internal bool TryRemoveAnchor(ARAnchor anchor)
        {
            if (anchor == null)
                throw new ArgumentNullException(nameof(anchor));

            if (subsystem == null)
                return false;

            if (subsystem.TryRemoveAnchor(anchor.trackableId))
            {
                if (m_PendingAdds.ContainsKey(anchor.trackableId))
                {
                    m_PendingAdds.Remove(anchor.trackableId);
                    m_Trackables.Remove(anchor.trackableId);
                }

                anchor.pending = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="ARAnchor"/> with given <paramref name="trackableId"/>,
        /// or <c>null</c> if it does not exist.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> of the <see cref="ARAnchor"/> to retrieve.</param>
        /// <returns>The <see cref="ARAnchor"/> with <paramref name="trackableId"/> or <c>null</c> if it does not exist.</returns>
        public ARAnchor GetAnchor(TrackableId trackableId)
        {
            if (m_Trackables.TryGetValue(trackableId, out var anchor))
                return anchor;

            return null;
        }

        /// <summary>
        /// Get the prefab to instantiate for each <see cref="ARAnchor"/>.
        /// </summary>
        /// <returns>The prefab to instantiate for each <see cref="ARAnchor"/>.</returns>
        protected override GameObject GetPrefab() => m_AnchorPrefab;

        /// <summary>
        /// The name to assign to the `GameObject` instantiated for each <see cref="ARAnchor"/>.
        /// </summary>
        protected override string gameObjectName => "Anchor";

        /// <summary>
        /// Invoked when the base class detects trackable changes.
        /// </summary>
        /// <param name="added">The list of added anchors.</param>
        /// <param name="updated">The list of updated anchors.</param>
        /// <param name="removed">The list of removed anchors.</param>
        protected override void OnTrackablesChanged(
            List<ARAnchor> added,
            List<ARAnchor> updated,
            List<ARAnchor> removed)
        {
            if (anchorsChanged != null)
            {
                using (new ScopedProfiler("OnAnchorsChanged"))
                {
                    anchorsChanged(new ARAnchorsChangedEventArgs(
                        added,
                        updated,
                        removed));
                }
            }
        }
    }
}
