using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Serialization;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// The manager for the human body subsystem.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ARUpdateOrder.k_HumanBodyManager)]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARHumanBodyManager) + ".html")]
    public sealed class ARHumanBodyManager :
        ARTrackableManager<XRHumanBodySubsystem, XRHumanBodySubsystemDescriptor, XRHumanBodySubsystem.Provider, XRHumanBody, ARHumanBody>
    {
        /// <summary>
        /// Whether 2D body pose tracking is enabled. This method is obsolete.
        /// Use <see cref="pose2DRequested"/>
        /// or  <see cref="pose2DEnabled"/> instead.
        /// </summary>
        [Obsolete("Use pose2DEnabled or pose2DRequested instead. 2020-01-14")]
        public bool humanBodyPose2DEstimationEnabled
        {
            get => m_Pose2D;
            set => pose2DRequested = value;
        }

        [SerializeField, FormerlySerializedAs("m_HumanBodyPose2DEstimationEnabled")]
        [Tooltip("Whether to estimate the 2D pose for any human bodies detected.")]
        bool m_Pose2D = true;

        /// <summary>
        /// Whether 2D human pose estimation is enabled. While
        /// <see cref="pose2DRequested"/> tells you whether 2D pose
        /// estimation has been requested, this property tells you
        /// whether 2D pose estimation is currently active in the subsystem.
        /// </summary>
        public bool pose2DEnabled => subsystem?.pose2DEnabled ?? false;

        /// <summary>
        /// Whether 2D human pose estimation is requested.
        /// </summary>
        /// <value>
        /// <c>true</c> if 2D human pose estimation is requested. Otherwise, <c>false</c>.
        /// </value>
        public bool pose2DRequested
        {
            get => subsystem?.pose2DRequested ?? m_Pose2D;
            set
            {
                m_Pose2D = value;
                if (enabled && subsystem != null)
                {
                    subsystem.pose2DRequested = value;
                }
            }
        }

        /// <summary>
        /// Whether 3D body pose tracking is enabled. This method is obsolete.
        /// Use <see cref="pose3DEnabled"/> or <see cref="pose3DRequested"/> instead.
        /// </summary>
        [Obsolete("Use pose3DEnabled or pose3DRequested instead. 2020-01-14")]
        public bool humanBodyPose3DEstimationEnabled
        {
            get => m_Pose3D;
            set => pose3DRequested = value;
        }

        [SerializeField, FormerlySerializedAs("m_HumanBodyPose3DEstimationEnabled")]
        [Tooltip("Whether to estimate the 3D pose for any human bodies detected.")]
        bool m_Pose3D = true;

        /// <summary>
        /// Whether 3D human pose estimation is requested.
        /// </summary>
        /// <value>
        /// <c>true</c> if 3D human pose estimation is requested. Otherwise, <c>false</c>.
        /// </value>
        public bool pose3DRequested
        {
            get => subsystem?.pose3DRequested ?? m_Pose3D;
            set
            {
                m_Pose3D = value;
                if (enabled && subsystem != null)
                {
                    subsystem.pose3DRequested = value;
                }
            }
        }

        /// <summary>
        /// Whether 3D human pose estimation is enabled.
        /// </summary>
        public bool pose3DEnabled => subsystem?.pose3DEnabled ?? false;

        /// <summary>
        /// Whether 3D body pose scale estimation is enabled. This method is obsolete.
        /// Use <see cref="pose3DScaleEstimationEnabled"/> or <see cref="pose3DScaleEstimationRequested"/> instead.
        /// </summary>
        [Obsolete("Use pose3DScaleEstimationRequested or pose3DScaleEstimationRequested instead. 2020-01-14")]
        public bool humanBodyPose3DScaleEstimationEnabled
        {
            get => m_Pose3DScaleEstimation;
            set => pose3DScaleEstimationRequested = value;
        }

        [SerializeField, FormerlySerializedAs("m_HumanBodyPose3DScaleEstimationEnabled")]
        [Tooltip("Whether to estimate the 3D pose for any human bodies detected.")]
        bool m_Pose3DScaleEstimation = false;

        /// <summary>
        /// Whether 3D human body scale estimation is requested.
        /// </summary>
        /// <value>
        /// <c>true</c> if 3D human body scale estimation is requested. Otherwise, <c>false</c>.
        /// </value>
        public bool pose3DScaleEstimationRequested
        {
            get => subsystem?.pose3DScaleEstimationRequested ?? m_Pose3DScaleEstimation;
            set
            {
                m_Pose3DScaleEstimation = value;
                if (enabled && subsystem != null)
                {
                    subsystem.pose3DScaleEstimationRequested = value;
                }
            }
        }

        /// <summary>
        /// Whether 3D human body scale estimation is enabled.
        /// </summary>
        public bool pose3DScaleEstimationEnabled => subsystem?.pose3DScaleEstimationEnabled ?? false;

        /// <summary>
        /// The Prefab object to instantiate at the location of the human body origin.
        /// </summary>
        /// <value>
        /// The Prefab object to instantiate at the location of the human body origin.
        /// </value>
        public GameObject humanBodyPrefab { get => m_HumanBodyPrefab; set => m_HumanBodyPrefab = value; }

        [SerializeField]
        [Tooltip("The prefab to instantiate at the origin for the detected human body if human body pose estimation is enabled.")]
        GameObject m_HumanBodyPrefab;

        /// <summary>
        /// The name for any generated GameObjects.
        /// </summary>
        /// <value>
        /// The name for any generated GameObjects.
        /// </value>
        protected override string gameObjectName => "ARHumanBody";

        /// <summary>
        /// The event that is fired when a change to the detected human bodies is reported.
        /// </summary>
        public event Action<ARHumanBodiesChangedEventArgs> humanBodiesChanged;

        /// <summary>
        /// Gets the Prefab object to instantiate at the location of the trackable.
        /// </summary>
        /// <returns>
        /// A GameObject to instantiate at the location of the trackable, or <c>null</c>.
        /// </returns>
        protected override GameObject GetPrefab() => m_HumanBodyPrefab;

        /// <summary>
        /// Get the human body matching the trackable identifier.
        /// </summary>
        /// <param name="trackableId">The trackable identifier for querying a human body trackable.</param>
        /// <returns>
        /// The human body trackable, if found. Otherwise, <c>null</c>.
        /// </returns>
        public ARHumanBody GetHumanBody(TrackableId trackableId) => m_Trackables.TryGetValue(trackableId, out ARHumanBody humanBody) ? humanBody : null;

        /// <summary>
        /// Gets the human body pose 2D joints for the current frame.
        /// </summary>
        /// <param name="allocator">The allocator to use for the returned array memory.</param>
        /// <returns>
        /// The array of body pose 2D joints.
        /// </returns>
        /// <remarks>
        /// The returned array might be empty if the system is not enabled for human body pose 2D or if the system
        /// does not detect a human in the camera image.
        /// </remarks>
        /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support human body
        /// pose 2D.</exception>
        public NativeArray<XRHumanBodyPose2DJoint> GetHumanBodyPose2DJoints(Allocator allocator)
            => ((subsystem == null) ? new NativeArray<XRHumanBodyPose2DJoint>(0, allocator)
                : subsystem.GetHumanBodyPose2DJoints(allocator));

        /// <summary>
        /// Callback before the subsystem is started (but after it is created).
        /// </summary>
        protected override void OnBeforeStart()
        {
            subsystem.pose2DRequested = m_Pose2D;
            subsystem.pose3DRequested = m_Pose3D;
            subsystem.pose3DScaleEstimationRequested = m_Pose3DScaleEstimation;
        }

        /// <summary>
        /// Callback as the manager is being destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_Trackables.Clear();
        }

        /// <summary>
        /// Callback after the session relative data has been set to update the skeleton for the human body.
        /// </summary>
        /// <param name="arBody">The human body trackable being updated.</param>
        /// <param name="xrBody">The raw human body data from the subsystem.</param>
        protected override void OnAfterSetSessionRelativeData(ARHumanBody arBody, XRHumanBody xrBody)
            => arBody.UpdateSkeleton(subsystem);

        /// <summary>
        /// Callback when the trackable deltas are being reported.
        /// </summary>
        /// <param name="added">The list of human bodies added to the set of trackables.</param>
        /// <param name="updated">The list of human bodies updated in the set of trackables.</param>
        /// <param name="removed">The list of human bodies removed to the set of trackables.</param>
        protected override void OnTrackablesChanged(List<ARHumanBody> added, List<ARHumanBody> updated, List<ARHumanBody> removed)
        {
            if (humanBodiesChanged != null)
            {
                using (new ScopedProfiler("OnHumanBodiesChanged"))
                humanBodiesChanged(new ARHumanBodiesChangedEventArgs(added, updated, removed));
            }
        }
    }
}
