using System;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// The base class for all <see cref="ARTrackable{TSessionRelativeData,TTrackable}"/> types.
    /// </summary>
    /// <remarks>
    /// A "trackable" is something that is tracked in the physical environment. These include:
    /// - <see cref="ARAnchor"/>
    /// - <see cref="AREnvironmentProbe"/>
    /// - <see cref="ARFace"/>
    /// - <see cref="ARHumanBody"/>
    /// - <see cref="ARParticipant"/>
    /// - <see cref="ARPlane"/>
    /// - <see cref="ARPointCloud"/>
    /// - <see cref="ARRaycast"/>
    /// - <see cref="ARTrackedImage"/>
    /// - <see cref="ARTrackedObject"/>
    /// </remarks>
    public abstract class ARTrackable : MonoBehaviour { }

    /// <summary>
    /// A generic component for trackables. A "trackable" is a feature in the physical
    /// environment that can be detected and tracked by an XR device.
    /// </summary>
    /// <typeparam name="TSessionRelativeData">The raw, session-relative data type used to update this trackable.</typeparam>
    /// <typeparam name="TTrackable">The concrete class which derives from <see cref="ARTrackable{TSessionRelativeData, TTrackable}"/>.</typeparam>
    public class ARTrackable<TSessionRelativeData, TTrackable> : ARTrackable
        where TSessionRelativeData : struct, ITrackable
        where TTrackable : ARTrackable<TSessionRelativeData, TTrackable>
    {
        [SerializeField]
        [Tooltip("If true, this component's GameObject will be removed immediately when this trackable is removed.")]
        bool m_DestroyOnRemoval = true;

        /// <summary>
        /// If true, this component's <c>GameObject</c> will be removed immediately when the XR device reports this trackable is no longer tracked.
        /// </summary>
        /// <remarks>
        /// Setting this to false will keep the <c>GameObject</c> around. You might want to do this, for example,
        /// if you have custom removal logic, such as a fade out.
        /// </remarks>
        public bool destroyOnRemoval
        {
            get => m_DestroyOnRemoval;
            set => m_DestroyOnRemoval = value;
        }

        /// <summary>
        /// The <c>TrackableId</c> associated with this trackable. <c>TrackableId</c>s
        /// are typically unique to a particular session.
        /// </summary>
        public TrackableId trackableId => sessionRelativeData.trackableId;

        /// <summary>
        /// The tracking state associated with this trackable.
        /// </summary>
        public TrackingState trackingState => sessionRelativeData.trackingState;

        /// <summary>
        /// Pending means the trackable was added manually (usually via an <c>AddTrackable</c>-style method
        /// on its manager) but has not yet been reported as added.
        /// </summary>
        public bool pending { get; internal set; }

        /// <summary>
        /// The session-relative data associated with this trackable.
        /// </summary>
        protected TSessionRelativeData sessionRelativeData { get; private set; }

        /// <summary>
        /// Invoked just after the session-relative data has been set.
        /// The <c>GameObject</c>'s transform has already been updated.
        /// You may override this method to perform further updates specific
        /// to the derived trackable.
        /// </summary>
        protected internal virtual void OnAfterSetSessionRelativeData()
        { }

        internal void SetSessionRelativeData(TSessionRelativeData data) => sessionRelativeData = data;

        internal Pose sessionRelativePose => sessionRelativeData.pose;
    }
}
