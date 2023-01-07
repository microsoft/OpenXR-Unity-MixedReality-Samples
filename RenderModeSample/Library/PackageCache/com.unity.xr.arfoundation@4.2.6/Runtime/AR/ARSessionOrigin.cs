using System;
#if USE_LEGACY_INPUT_HELPERS
using UnityEngine.SpatialTracking;
#endif

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// An <c>ARSessionOrigin</c> is the parent for an AR setup. It contains a <c>Camera</c> and
    /// any <c>GameObject</c>s created from detected features, such as planes or point clouds.
    /// </summary>
    /// <remarks>
    /// Session space vs. Unity space
    ///
    /// Because an AR device will be used to drive the <c>Camera</c>'s position and rotation,
    /// you cannot directly place the <c>Camera</c> at an arbitrary position in the Unity scene.
    /// Instead, you should position the <c>ARSessionOrigin</c>. This makes the <c>Camera</c>
    /// (and any detected features) relative to that as a result.
    ///
    /// It is important to keep the <c>Camera</c> and detected features in the same space relative to
    /// each other (otherwise, detected features like planes won't appear in the correct place relative
    /// to the <c>Camera</c>). The space relative to the AR device's starting position is called 
    /// "session space" or "device space". For example, when the AR session begins, the device might
    /// report its position as (0, 0, 0). Detected features, such as planes, will be reported relative
    /// to this starting position. The purpose of the <c>ARSessionOrigin</c> is to convert the session space
    /// to Unity world space.
    ///
    /// To facilitate this, the <c>ARSessionOrigin</c> creates a new <c>GameObject</c> called "Trackables"
    /// as a sibling of its <c>Camera</c>. This should be the parent <c>GameObject</c> for all
    /// detected features.
    ///
    /// At runtime, a typical scene graph might look like this:
    /// - AR Session Origin
    ///     - Camera
    ///     - Trackables
    ///         - Detected plane 1
    ///         - Detected plane 2
    ///         - Point cloud
    ///         - etc...
    ///
    /// You can access the "trackables" <c>GameObject</c> with <see cref="trackablesParent"/>.
    ///
    /// Note that the <c>localPosition</c> and <c>localRotation</c> of detected trackables
    /// remain in real-world meters relative to the AR device's starting position and rotation.
    ///
    /// Scale
    ///
    /// If you want to scale the content rendered by the <c>ARSessionOrigin</c> you should apply
    /// the scale to the <c>ARSessionOrigin</c>'s transform. This is preferrable to scaling
    /// the content directly, which can have undesirable side effects. Physics and NavMeshes,
    /// for example, do not perform well when scaled very small.
    /// </remarks>
    [DisallowMultipleComponent]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARSessionOrigin) + ".html")]
    public class ARSessionOrigin : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The Camera to associate with the AR device.")]
        Camera m_Camera;

        /// <summary>
        /// The <c>Camera</c> to associate with the AR device. It must be a child of this <c>ARSessionOrigin</c>.
        /// </summary>
        /// <remarks>
        /// The <c>Camera</c> should update its position and rotation according to the AR device.
        /// This is typically accomplished by adding a <c>TrackedPoseDriver</c> component to the
        /// <c>Camera</c>.
        /// </remarks>
#if UNITY_EDITOR
        public new Camera camera
#else
        public Camera camera
#endif
        {
            get { return m_Camera; }
            set { m_Camera = value; }
        }

        /// <summary>
        /// The parent <c>Transform</c> for all "trackables" (for example, planes and feature points).
        /// </summary>
        public Transform trackablesParent { get; private set; }

        /// <summary>
        /// Invoked during
        /// [Application.onBeforeRender](xref:UnityEngine.Application.onBeforeRender(UnityEngine.Events.UnityAction))
        /// whenever the <see cref="trackablesParent"/> [transform](xref:UnityEngine.Transform) changes.
        /// </summary>
        public event Action<ARTrackablesParentTransformChangedEventArgs> trackablesParentTransformChanged;

        GameObject m_ContentOffsetGameObject;

        Transform contentOffsetTransform
        {
            get
            {
                if (m_ContentOffsetGameObject == null)
                {
                    // Insert a GameObject directly below the rig
                    m_ContentOffsetGameObject = new GameObject("Content Placement Offset");
                    m_ContentOffsetGameObject.transform.SetParent(transform, false);

                    // Re-parent any children of the ARSessionOrigin
                    for (var i = 0; i < transform.childCount; ++i)
                    {
                        var child = transform.GetChild(i);
                        if (child != m_ContentOffsetGameObject.transform)
                        {
                            child.SetParent(m_ContentOffsetGameObject.transform, true);
                            --i; // Decrement because childCount is also one less.
                        }
                    }
                }

                return m_ContentOffsetGameObject.transform;
            }
        }

        /// <summary>
        /// Makes <paramref name="content"/> appear to be placed at <paramref name="position"/> with orientation <paramref name="rotation"/>.
        /// </summary>
        /// <param name="content">The <c>Transform</c> of the content you wish to affect.</param>
        /// <param name="position">The position you wish the content to appear at. This could be
        /// a position on a detected plane, for example.</param>
        /// <param name="rotation">The rotation the content should appear to be in, relative
        /// to the <c>Camera</c>.</param>
        /// <remarks>
        /// This method does not actually change the <c>Transform</c> of content; instead,
        /// it updates the <c>ARSessionOrigin</c>'s <c>Transform</c> so the content
        /// appears to be at the given position and rotation. This is useful for placing AR
        /// content onto surfaces when the content itself cannot be moved at runtime.
        /// For example, if your content includes terrain or a NavMesh, it cannot
        /// be moved or rotated dynamically.
        /// </remarks>
        public void MakeContentAppearAt(Transform content, Vector3 position, Quaternion rotation)
        {
            MakeContentAppearAt(content, position);
            MakeContentAppearAt(content, rotation);
        }

        /// <summary>
        /// Makes <paramref name="content"/> appear to be placed at <paramref name="position"/>.
        /// </summary>
        /// <param name="content">The <c>Transform</c> of the content you wish to affect.</param>
        /// <param name="position">The position you wish the content to appear at. This could be
        /// a position on a detected plane, for example.</param>
        /// <remarks>
        /// This method does not actually change the <c>Transform</c> of content; instead,
        /// it updates the <c>ARSessionOrigin</c>'s <c>Transform</c> so the content
        /// appears to be at the given position.
        /// </remarks>
        public void MakeContentAppearAt(Transform content, Vector3 position)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            // Adjust the "point of interest" transform to account
            // for the actual position we want the content to appear at.
            contentOffsetTransform.position += transform.position - position;

            // The ARSessionOrigin's position needs to match the content's pivot. This is so
            // the entire ARSessionOrigin rotates around the content (so the impression is that
            // the content is rotating, not the rig).
            transform.position = content.position;
        }

        /// <summary>
        /// Makes <paramref name="content"/> appear to have orientation <paramref name="rotation"/> relative to the <c>Camera</c>.
        /// </summary>
        /// <param name="content">The <c>Transform</c> of the content you wish to affect.</param>
        /// <param name="rotation">The rotation the content should appear to be in, relative
        /// to the <c>Camera</c>.</param>
        /// <remarks>
        /// This method does not actually change the <c>Transform</c> of content; instead,
        /// it updates the <c>ARSessionOrigin</c>'s <c>Transform</c> so that the content
        /// appears to be in the requested orientation.
        /// </remarks>
        public void MakeContentAppearAt(Transform content, Quaternion rotation)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            // Since we aren't rotating the content, we need to perform the inverse
            // operation on the ARSessionOrigin. For example, if we want the
            // content to appear to be rotated 90 degrees on the Y axis, we should
            // rotate our rig -90 degrees on the Y axis.
            transform.rotation = Quaternion.Inverse(rotation) * content.rotation;
        }

        void Awake()
        {
            // This will be the parent GameObject for any trackables (such as planes) for which
            // we want a corresponding GameObject.
            trackablesParent = (new GameObject("Trackables")).transform;
            trackablesParent.SetParent(transform, false);
            trackablesParent.localPosition = Vector3.zero;
            trackablesParent.localRotation = Quaternion.identity;
            trackablesParent.localScale = Vector3.one;

            if (camera)
            {
                var arPoseDriver = camera.GetComponent<ARPoseDriver>();
#if USE_LEGACY_INPUT_HELPERS
                var trackedPoseDriver = camera.GetComponent<TrackedPoseDriver>();

                // Warn if not using a ARPoseDriver or a TrackedPoseDriver
                if (arPoseDriver == null && trackedPoseDriver == null)
                {
                    Debug.LogWarning(
                        $"Camera \"{camera.name}\" does not use a AR Pose Driver or a Tracked Pose Driver, " +
                        "so its transform will not be updated by an XR device.  In order for this to be " +
                        "updated, please add either an AR Pose Driver or a Tracked Pose Driver.");
                }

                // If we are using an TrackedPoseDriver, and the user hasn't chosen "make relative"
                // then warn if the camera has a non-identity transform (since it will be overwritten).
                else if ((trackedPoseDriver != null && !trackedPoseDriver.UseRelativeTransform))
                {
                    var cameraTransform = camera.transform;
                    if ((cameraTransform.localPosition != Vector3.zero) || (cameraTransform.localRotation != Quaternion.identity))
                    {
                        Debug.LogWarning(
                            $"Camera \"{camera.name}\" has a non-identity transform " +
                            $"(position = {cameraTransform.localPosition}, rotation = {cameraTransform.localRotation}). " +
                            "The camera's local position and rotation will be overwritten by the XR device, " +
                            "so this starting transform will have no effect. Tick the \"Make Relative\" " +
                            "checkbox on the camera's Tracked Pose Driver to apply this starting transform.");
                    }
                }
                // If using ARPoseDriver then it will get overwritten no matter what
                else
                {
                    var cameraTransform = camera.transform;
                    if ((cameraTransform.localPosition != Vector3.zero) || (cameraTransform.localRotation != Quaternion.identity))
                    {
                        Debug.LogWarning(
                            $"Camera \"{camera.name}\" has a non-identity transform " +
                            $"(position = {cameraTransform.localPosition}, rotation = {cameraTransform.localRotation}). " +
                            "The camera's local position and rotation will be overwritten by the XR device.");
                    }
                }
#else // !(USE_LEGACY_INPUT_HELPERS
                if (arPoseDriver == null)
                {
                    Debug.LogWarning(
                        $"Camera \"{camera.name}\" does not use a AR Pose Driver, so its transform will not be updated by " +
                        "an XR device.  In order for this to be updated, please add an AR Pose Driver component.");
                }
#endif // USE_LEGACY_INPUT_HELPERS
            }
        }

        Pose GetCameraOriginPose()
        {
            var localOriginPose = Pose.identity;
            var parent = camera.transform.parent;

#if USE_LEGACY_INPUT_HELPERS
            var trackedPoseDriver = camera.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver)
            {
                localOriginPose = trackedPoseDriver.originPose;
            }
#endif // USE_LEGACY_INPUT_HELPERS

            return parent
                ? parent.TransformPose(localOriginPose)
                : localOriginPose;
        }

        void OnEnable() => Application.onBeforeRender += OnBeforeRender;

        void OnDisable() => Application.onBeforeRender -= OnBeforeRender;

        void OnBeforeRender()
        {
            if (camera)
            {
                var pose = GetCameraOriginPose();
                trackablesParent.position = pose.position;
                trackablesParent.rotation = pose.rotation;
            }

            if (trackablesParent.hasChanged)
            {
                trackablesParentTransformChanged?.Invoke(
                    new ARTrackablesParentTransformChangedEventArgs(this, trackablesParent));
                trackablesParent.hasChanged = false;
            }
        }

#if UNITY_EDITOR && USE_LEGACY_INPUT_HELPERS
        void OnValidate()
        {
            if (camera)
            {
                if ((camera.GetComponent<TrackedPoseDriver>()?.enabled ?? false) &&
                    (camera.GetComponent<ARPoseDriver>()?.enabled ?? false))
                {
                    Debug.LogWarning(
                        $"Camera \"{camera.name}\" has an AR Pose Driver and a Tracked Pose Driver and both are enabled. " +
                        "This configuration will cause the camera transform to be updated from both components in a non-deterministic " +
                        "way. This is likely an unintended error.");
                }
            }
        }
#endif // UNITY_EDITOR && USE_LEGACY_INPUT_HELPERS
    }
}
