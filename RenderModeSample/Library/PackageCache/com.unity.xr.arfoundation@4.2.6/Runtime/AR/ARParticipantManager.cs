using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A manager for <see cref="ARParticipant"/>s. Creates, updates, and removes
    /// <c>GameObject</c>s in response to other users in a multi-user collaborative session.
    /// </summary>
    [DefaultExecutionOrder(ARUpdateOrder.k_ParticipantManager)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARSessionOrigin))]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARParticipantManager) + ".html")]
    public sealed class ARParticipantManager : ARTrackableManager<
        XRParticipantSubsystem,
        XRParticipantSubsystemDescriptor,
        XRParticipantSubsystem.Provider,
        XRParticipant,
        ARParticipant>
    {
        [SerializeField]
        [Tooltip("(Optional) Instantiates this prefab for each participant.")]
        GameObject m_ParticipantPrefab;

        /// <summary>
        /// (Optional) Instantiates this Prefab for each participant. If <c>null</c>, an empty <c>GameObject</c>
        /// with a <see cref="ARParticipant"/> component is instantiated instead.
        /// </summary>
        public GameObject participantPrefab
        {
            get => m_ParticipantPrefab;
            set => m_ParticipantPrefab = value;
        }

        /// <summary>
        /// Invoked when participants have changed (been added, updated, or removed).
        /// </summary>
        public event Action<ARParticipantsChangedEventArgs> participantsChanged;

        /// <summary>
        /// Attempt to retrieve an existing <see cref="ARParticipant"/> by <paramref name="trackableId"/>.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> of the participant to retrieve.</param>
        /// <returns>The <see cref="ARParticipant"/> with <paramref name="trackableId"/>, or <c>null</c> if it does not exist.</returns>
        public ARParticipant GetParticipant(TrackableId trackableId)
        {
            if (m_Trackables.TryGetValue(trackableId, out ARParticipant participant))
                return participant;

            return null;
        }

        /// <summary>
        /// The Prefab which will be instantiated for each <see cref="ARParticipant"/>. Can be `null`.
        /// </summary>
        /// <returns>A Prefab to instantiate for each <see cref="ARParticipant"/>.</returns>
        protected override GameObject GetPrefab() => m_ParticipantPrefab;

        /// <summary>
        /// Invoked when the base class detects trackable changes.
        /// </summary>
        /// <param name="added">The list of added <see cref="ARParticipant"/>s.</param>
        /// <param name="updated">The list of updated <see cref="ARParticipant"/>s.</param>
        /// <param name="removed">The list of removed <see cref="ARParticipant"/>s.</param>
        protected override void OnTrackablesChanged(
            List<ARParticipant> added,
            List<ARParticipant> updated,
            List<ARParticipant> removed)
        {
            if (participantsChanged != null)
            {
                using (new ScopedProfiler("OnParticipantsChanged"))
                participantsChanged(
                    new ARParticipantsChangedEventArgs(
                        added,
                        updated,
                        removed));
            }
        }

        /// <summary>
        /// The name to be used for the <c>GameObject</c> whenever a new participant is detected.
        /// </summary>
        protected override string gameObjectName => "ARParticipant";
    }
}
