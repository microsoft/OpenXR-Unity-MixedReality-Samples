using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Serialization;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A manager for <see cref="ARPlane"/>s. Creates, updates, and removes
    /// <c>GameObject</c>s in response to detected surfaces in the physical
    /// environment.
    /// </summary>
    [DefaultExecutionOrder(ARUpdateOrder.k_PlaneManager)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARSessionOrigin))]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARPlaneManager) + ".html")]
    public sealed class ARPlaneManager : ARTrackableManager<
        XRPlaneSubsystem,
        XRPlaneSubsystemDescriptor,
        XRPlaneSubsystem.Provider,
        BoundedPlane,
        ARPlane>, IRaycaster
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each created plane.")]
        GameObject m_PlanePrefab;

        /// <summary>
        /// Getter or setter for the Plane Prefab.
        /// </summary>
        public GameObject planePrefab
        {
            get => m_PlanePrefab;
            set => m_PlanePrefab = value;
        }

        [SerializeField, PlaneDetectionModeMask]
        [Tooltip("The types of planes to detect.")]
        PlaneDetectionMode m_DetectionMode = (PlaneDetectionMode)(-1);

        /// <summary>
        /// Get or set the <c>PlaneDetectionMode</c> to use for plane detection.
        /// This property is obsolete.
        /// Use <see cref="requestedDetectionMode"/> or <see cref="currentDetectionMode"/> instead.
        /// </summary>
        [Obsolete("Use requestedDetectionMode or currentDetectionMode instead")]
        public PlaneDetectionMode detectionMode
        {
            get => m_DetectionMode;
            set => requestedDetectionMode = value;
        }

        /// <summary>
        /// Get or set the requested plane detection mode.
        /// </summary>
        public PlaneDetectionMode requestedDetectionMode
        {
            get => subsystem?.requestedPlaneDetectionMode ?? m_DetectionMode;
            set
            {
                m_DetectionMode = value;
                if (enabled && subsystem != null)
                {
                    subsystem.requestedPlaneDetectionMode = value;
                }
            }
        }

        /// <summary>
        /// Get the current plane detection mode in use by the subsystem.
        /// </summary>
        public PlaneDetectionMode currentDetectionMode => subsystem?.currentPlaneDetectionMode ?? PlaneDetectionMode.None;

        /// <summary>
        /// Invoked when planes have changed (been added, updated, or removed).
        /// </summary>
        public event Action<ARPlanesChangedEventArgs> planesChanged;

        /// <summary>
        /// Attempt to retrieve an existing <see cref="ARPlane"/> by <paramref name="trackableId"/>.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> of the plane to retrieve.</param>
        /// <returns>The <see cref="ARPlane"/> with <paramref name="trackableId"/>, or <c>null</c> if it does not exist.</returns>
        public ARPlane GetPlane(TrackableId trackableId) => m_Trackables.TryGetValue(trackableId, out ARPlane plane) ? plane : null;

        /// <summary>
        /// Performs a raycast against all currently tracked planes.
        /// </summary>
        /// <param name="ray">The ray, in Unity world space, to cast.</param>
        /// <param name="trackableTypeMask">A mask of raycast types to perform.</param>
        /// <param name="allocator">The <c>Allocator</c> to use when creating the returned <c>NativeArray</c>.</param>
        /// <returns>
        /// A new <c>NativeArray</c> of raycast results allocated with <paramref name="allocator"/>.
        /// The caller owns the memory and is responsible for calling <c>Dispose</c> on the <c>NativeArray</c>.
        /// </returns>
        /// <seealso cref="ARRaycastManager.Raycast(Ray, List{ARRaycastHit}, TrackableType)"/>
        /// <seealso cref="ARRaycastManager.Raycast(Vector2, List{ARRaycastHit}, TrackableType)"/>
        public NativeArray<XRRaycastHit> Raycast(
            Ray ray,
            TrackableType trackableTypeMask,
            Allocator allocator)
        {
            // No plane types requested; early out.
            if ((trackableTypeMask & TrackableType.Planes) == TrackableType.None)
                return new NativeArray<XRRaycastHit>(0, allocator);

            var trackableCollection = trackables;

            // Allocate a buffer that is at least large enough to contain a hit against every plane
            var hitBuffer = new NativeArray<XRRaycastHit>(trackableCollection.count, Allocator.Temp);
            try
            {
                int count = 0;
                foreach (var plane in trackableCollection)
                {
                    TrackableType trackableTypes = TrackableType.None;

                    var normal = plane.transform.localRotation * Vector3.up;
                    var infinitePlane = new Plane(normal, plane.transform.localPosition);
                    float distance;
                    if (!infinitePlane.Raycast(ray, out distance))
                        continue;

                    // Pose in session space
                    var pose = new Pose(
                        ray.origin + ray.direction * distance,
                        plane.transform.localRotation);

                    if ((trackableTypeMask & TrackableType.PlaneWithinInfinity) != TrackableType.None)
                        trackableTypes |= TrackableType.PlaneWithinInfinity;

                    // To test the rest, we need the intersection point in plane space
                    var hitPositionPlaneSpace3d = Quaternion.Inverse(plane.transform.localRotation) * (pose.position - plane.transform.localPosition);
                    var hitPositionPlaneSpace = new Vector2(hitPositionPlaneSpace3d.x, hitPositionPlaneSpace3d.z);

                    var estimatedOrWithinBounds = TrackableType.PlaneWithinBounds | TrackableType.PlaneEstimated;
                    if ((trackableTypeMask & estimatedOrWithinBounds) != TrackableType.None)
                    {
                        var differenceFromCenter = hitPositionPlaneSpace - plane.centerInPlaneSpace;
                        if ((Mathf.Abs(differenceFromCenter.x) <= plane.extents.x) &&
                            (Mathf.Abs(differenceFromCenter.y) <= plane.extents.y))
                        {
                            trackableTypes |= (estimatedOrWithinBounds & trackableTypeMask);
                        }
                    }

                    if ((trackableTypeMask & TrackableType.PlaneWithinPolygon) != TrackableType.None)
                    {
                        if (WindingNumber(hitPositionPlaneSpace, plane.boundary) != 0)
                            trackableTypes |= TrackableType.PlaneWithinPolygon;
                    }

                    if (trackableTypes != TrackableType.None)
                    {
                        hitBuffer[count++] = new XRRaycastHit(
                            plane.trackableId,
                            pose,
                            distance,
                            trackableTypes);
                    }
                }

                // Finally, copy to return value
                var hitResults = new NativeArray<XRRaycastHit>(count, allocator);
                NativeArray<XRRaycastHit>.Copy(hitBuffer, hitResults, count);
                return hitResults;
            }
            finally
            {
                hitBuffer.Dispose();
            }
        }

        static float GetCrossDirection(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        // See http://geomalgorithms.com/a03-_inclusion.html
        static int WindingNumber(
            Vector2 positionInPlaneSpace,
            NativeArray<Vector2> boundaryInPlaneSpace)
        {
            int windingNumber = 0;
            Vector2 point = positionInPlaneSpace;
            for (int i = 0; i < boundaryInPlaneSpace.Length; ++i)
            {
                int j = (i + 1) % boundaryInPlaneSpace.Length;
                Vector2 vi = boundaryInPlaneSpace[i];
                Vector2 vj = boundaryInPlaneSpace[j];

                if (vi.y <= point.y)
                {
                    if (vj.y > point.y)                                     // an upward crossing
                    {
                        if (GetCrossDirection(vj - vi, point - vi) < 0f)    // P left of edge
                            ++windingNumber;
                    }
                    // have  a valid up intersect
                }
                else
                {                                                           // y > P.y (no test needed)
                    if (vj.y <= point.y)                                    // a downward crossing
                    {
                        if (GetCrossDirection(vj - vi, point - vi) > 0f)    // P right of edge
                            --windingNumber;
                    }
                    // have  a valid down intersect
                }
            }

            return windingNumber;
        }

        /// <summary>
        /// Get the Prefab which will be instantiated for each <see cref="ARPlane"/>. Can be `null`.
        /// </summary>
        /// <returns>The Prefab which will be instantiated for each <see cref="ARPlane"/>.</returns>
        protected override GameObject GetPrefab() => m_PlanePrefab;

        /// <summary>
        /// Invoked just before `Start`ing the plane subsystem. Used to set the subsystem's
        /// `requestedPlaneDetectionMode`.
        /// </summary>
        protected override void OnBeforeStart()
        {
            subsystem.requestedPlaneDetectionMode = m_DetectionMode;
        }

        /// <summary>
        /// Invoked just after each <see cref="ARPlane"/> is updated.
        /// </summary>
        /// <param name="plane">The <see cref="ARPlane"/> being updated.</param>
        /// <param name="sessionRelativeData">The new data associated with the plane. All spatial
        /// data is is session-relative space.</param>
        protected override void OnAfterSetSessionRelativeData(
            ARPlane plane,
            BoundedPlane sessionRelativeData)
        {
            ARPlane subsumedByPlane;
            if (m_Trackables.TryGetValue(sessionRelativeData.subsumedById, out subsumedByPlane))
            {
                plane.subsumedBy = subsumedByPlane;
            }
            else
            {
                plane.subsumedBy = null;
            }

            plane.UpdateBoundary(subsystem);
        }

        /// <summary>
        /// Invoked when the base class detects trackable changes.
        /// </summary>
        /// <param name="added">The list of added <see cref="ARPlane"/>s.</param>
        /// <param name="updated">The list of updated <see cref="ARPlane"/>s.</param>
        /// <param name="removed">The list of removed <see cref="ARPlane"/>s.</param>
        protected override void OnTrackablesChanged(
            List<ARPlane> added,
            List<ARPlane> updated,
            List<ARPlane> removed)
        {
            if (planesChanged != null)
            {
                using (new ScopedProfiler("OnPlanesChanged"))
                planesChanged(
                    new ARPlanesChangedEventArgs(
                        added,
                        updated,
                        removed));
            }
        }

        /// <summary>
        /// The name to be used for the <c>GameObject</c> whenever a new plane is detected.
        /// </summary>
        protected override string gameObjectName => "ARPlane";

        /// <summary>
        /// Invoked when Unity enables this `MonoBehaviour`. Used to register with the <see cref="ARRaycastManager"/>.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (subsystem != null)
            {
                var raycastManager = GetComponent<ARRaycastManager>();
                if (raycastManager != null)
                    raycastManager.RegisterRaycaster(this);
            }
        }

        /// <summary>
        /// Invoked when Unity disables this `MonoBehaviour`. Used to unregister with the <see cref="ARRaycastManager"/>.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            var raycastManager = GetComponent<ARRaycastManager>();
            if (raycastManager != null)
                raycastManager.UnregisterRaycaster(this);
        }
    }
}
