using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Manages an <c>XRRaycastSubsystem</c>, exposing raycast functionality in AR Foundation. Use this component
    /// to raycast against trackables (that is, detected features in the physical environment) when they do not have
    /// a presence in the Physics world.
    /// </summary>
    [DefaultExecutionOrder(ARUpdateOrder.k_RaycastManager)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARSessionOrigin))]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARRaycastManager) + ".html")]
    public sealed class ARRaycastManager : ARTrackableManager<
        XRRaycastSubsystem, XRRaycastSubsystemDescriptor,
        XRRaycastSubsystem.Provider,
        XRRaycast, ARRaycast>
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each raycast.")]
        GameObject m_RaycastPrefab;

        /// <summary>
        /// If not null, this prefab will be instantiated for each <see cref="ARRaycast"/>.
        /// </summary>
        public GameObject raycastPrefab
        {
            get => m_RaycastPrefab;
            set => m_RaycastPrefab = value;
        }

        /// <summary>
        /// Cast a ray from a point in screen space against trackables, that is, detected features such as planes.
        /// </summary>
        /// <param name="screenPoint">The point, in device screen pixels, from which to cast.</param>
        /// <param name="hitResults">Contents are replaced with the raycast results, if successful.</param>
        /// <param name="trackableTypes">(Optional) The types of trackables to cast against.</param>
        /// <returns>True if the raycast hit a trackable in the <paramref name="trackableTypes"/></returns>
        #region ARRaycastManager_Raycast_screenPoint
        public bool Raycast(
            Vector2 screenPoint,
            List<ARRaycastHit> hitResults,
            TrackableType trackableTypes = TrackableType.AllTypes)
        #endregion
        {
            if (subsystem == null)
                return false;

            if (hitResults == null)
                throw new ArgumentNullException("hitResults");

            var nativeHits = m_RaycastViewportDelegate(screenPoint, trackableTypes, Allocator.Temp);
            var originTransform = sessionOrigin.camera != null ? sessionOrigin.camera.transform : sessionOrigin.trackablesParent;
            return TransformAndDisposeNativeHitResults(nativeHits, hitResults, originTransform.position);
        }

        /// <summary>
        /// Cast a <c>Ray</c> against trackables, that is, detected features such as planes.
        /// </summary>
        /// <param name="ray">The <c>Ray</c>, in Unity world space, to cast.</param>
        /// <param name="hitResults">Contents are replaced with the raycast results, if successful.</param>
        /// <param name="trackableTypes">(Optional) The types of trackables to cast against.</param>
        /// <returns>True if the raycast hit a trackable in the <paramref name="trackableTypes"/></returns>
        #region ARRaycastManager_Raycast_ray
        public bool Raycast(
            Ray ray,
            List<ARRaycastHit> hitResults,
            TrackableType trackableTypes = TrackableType.AllTypes)
        #endregion
        {
            if (subsystem == null)
                return false;

            if (hitResults == null)
                throw new ArgumentNullException(nameof(hitResults));

            var sessionSpaceRay = sessionOrigin.trackablesParent.InverseTransformRay(ray);
            var nativeHits = m_RaycastRayDelegate(sessionSpaceRay, trackableTypes, Allocator.Temp);
            return TransformAndDisposeNativeHitResults(nativeHits, hitResults, ray.origin);
        }

        /// <summary>
        /// Creates an <see cref="ARRaycast"/> that updates automatically. <see cref="ARRaycast"/>s will
        /// continue to update until you remove them with <see cref="RemoveRaycast(ARRaycast)"/> or disable
        /// this component.
        /// </summary>
        /// <param name="screenPoint">A point on the screen, in pixels.</param>
        /// <param name="estimatedDistance">The estimated distance to the intersection point.
        /// This can be used to determine a potential intersection before the environment has been fully mapped.</param>
        /// <returns>A new <see cref="ARRaycast"/> if successful; otherwise `null`.</returns>
        #region ARRaycastManager_AddRaycast_screenPoint
        public ARRaycast AddRaycast(Vector2 screenPoint, float estimatedDistance)
        #endregion
        {
            if (subsystem == null)
                return null;

            var normalizedScreenPoint = new Vector2(
                Mathf.Clamp01(screenPoint.x / Screen.width),
                Mathf.Clamp01(screenPoint.y / Screen.height));

            if (subsystem.TryAddRaycast(normalizedScreenPoint, estimatedDistance, out XRRaycast sessionRelativeData))
            {
                return CreateTrackableImmediate(sessionRelativeData);
            }

            if (sessionOrigin.camera && subsystem.TryAddRaycast(ScreenPointToSessionSpaceRay(screenPoint), estimatedDistance, out sessionRelativeData))
            {
                return CreateTrackableImmediate(sessionRelativeData);
            }

            return null;
        }

        /// <summary>
        /// Removes an existing <see cref="ARRaycast"/>.
        /// </summary>
        /// <param name="raycast">The <see cref="ARRaycast"/> to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="raycast"/> is `null`.</exception>
        public void RemoveRaycast(ARRaycast raycast)
        {
            if (raycast == null)
                throw new ArgumentNullException(nameof(raycast));

            subsystem?.RemoveRaycast(raycast.trackableId);
        }

        /// <summary>
        /// Gets the Prefab that should be instantiated for each <see cref="ARRaycast"/>. Can be `null`.
        /// </summary>
        /// <returns>The Prefab that should be instantiated for each <see cref="ARRaycast"/>.</returns>
        protected override GameObject GetPrefab() => m_RaycastPrefab;

        /// <summary>
        /// Allows AR managers to register themselves as a raycaster.
        /// Raycasters can be used as a fallback method if the AR platform does
        /// not support raycasting using arbitrary <c>Ray</c>s.
        /// </summary>
        /// <param name="raycaster">A raycaster implementing the IRaycast interface.</param>
        internal void RegisterRaycaster(IRaycaster raycaster)
        {
            if (!m_Raycasters.Contains(raycaster))
                m_Raycasters.Add(raycaster);
        }

        /// <summary>
        /// Unregisters a raycaster previously registered with <see cref="RegisterRaycaster(IRaycaster)"/>.
        /// </summary>
        /// <param name="raycaster">A raycaster to use as a fallback, if needed.</param>
        internal void UnregisterRaycaster(IRaycaster raycaster)
        {
            if (m_Raycasters != null)
                m_Raycasters.Remove(raycaster);
        }

        /// <summary>
        /// Invoked just after the subsystem has been `Start`ed. Used to set raycast delegates internally.
        /// </summary>
        protected override void OnAfterStart()
        {
            var desc = subsystem.subsystemDescriptor;
            if (desc.supportsViewportBasedRaycast)
            {
                m_RaycastViewportDelegate = RaycastViewport;
            }
            else
            {
                m_RaycastViewportDelegate = RaycastViewportAsRay;
            }

            if (desc.supportsWorldBasedRaycast)
            {
                m_RaycastRayDelegate = RaycastRay;
            }
            else
            {
                m_RaycastRayDelegate = RaycastFallback;
            }

            var raycasters = GetComponents(typeof(IRaycaster));
            foreach (var raycaster in raycasters)
                RegisterRaycaster((IRaycaster)raycaster);
        }

        Ray ScreenPointToSessionSpaceRay(Vector2 screenPoint)
        {
            var worldSpaceRay = sessionOrigin.camera.ScreenPointToRay(screenPoint);
            return sessionOrigin.trackablesParent.InverseTransformRay(worldSpaceRay);
        }

        NativeArray<XRRaycastHit> RaycastViewportAsRay(
            Vector2 screenPoint,
            TrackableType trackableTypeMask,
            Allocator allocator)
        {
            if (sessionOrigin.camera == null)
                return new NativeArray<XRRaycastHit>(0, allocator);

            return m_RaycastRayDelegate(ScreenPointToSessionSpaceRay(screenPoint), trackableTypeMask, allocator);
        }

        NativeArray<XRRaycastHit> RaycastViewport(
            Vector2 screenPoint,
            TrackableType trackableTypeMask,
            Allocator allocator)
        {
            screenPoint.x = Mathf.Clamp01(screenPoint.x / Screen.width);
            screenPoint.y = Mathf.Clamp01(screenPoint.y / Screen.height);
            return subsystem.Raycast(screenPoint, trackableTypeMask, allocator);
        }

        NativeArray<XRRaycastHit> RaycastRay(
            Ray ray,
            TrackableType trackableTypeMask,
            Allocator allocator)
        {
            return subsystem.Raycast(ray, trackableTypeMask, allocator);
        }

        static int RaycastHitComparer(ARRaycastHit lhs, ARRaycastHit rhs)
        {
            return lhs.CompareTo(rhs);
        }

        NativeArray<XRRaycastHit> RaycastFallback(
            Ray ray,
            TrackableType trackableTypeMask,
            Allocator allocator)
        {
            s_NativeRaycastHits.Clear();
            int count = 0;
            foreach (var raycaster in m_Raycasters)
            {
                var hits = raycaster.Raycast(ray, trackableTypeMask, Allocator.Temp);
                if (hits.IsCreated)
                {
                    s_NativeRaycastHits.Add(hits);
                    count += hits.Length;
                }
            }

            var allHits = new NativeArray<XRRaycastHit>(count, allocator);
            int dstIndex = 0;
            foreach (var hitArray in s_NativeRaycastHits)
            {
                if (hitArray.Length > 0)
                {
                    NativeArray<XRRaycastHit>.Copy(hitArray, 0, allHits, dstIndex, hitArray.Length);
                    dstIndex += hitArray.Length;
                }

                if (hitArray.IsCreated)
                {
                    hitArray.Dispose();
                }
            }

            return allHits;
        }

        bool TransformAndDisposeNativeHitResults(
            NativeArray<XRRaycastHit> nativeHits,
            List<ARRaycastHit> managedHits,
            Vector3 rayOrigin)
        {
            managedHits.Clear();
            if (!nativeHits.IsCreated)
                return false;

            var planeManager = GetComponent<ARPlaneManager>();

            using (nativeHits)
            {
                // Results are in "trackables space", so transform results back into world space
                foreach (var nativeHit in nativeHits)
                {
                    float distanceInWorldSpace = (nativeHit.pose.position - rayOrigin).magnitude;

                    // Attempt to look up the trackable
                    ARTrackable trackable = null;

                    // Planes
                    if ((nativeHit.hitType & TrackableType.Planes) != 0)
                    {
                        if (planeManager)
                        {
                            trackable = planeManager.GetPlane(nativeHit.trackableId);
                        }
                    }

                    managedHits.Add(new ARRaycastHit(nativeHit, distanceInWorldSpace, sessionOrigin.trackablesParent, trackable));
                }

                managedHits.Sort(s_RaycastHitComparer);
                return managedHits.Count > 0;
            }
        }

        /// <summary>
        /// Invoked just after a <see cref="ARRaycast"/> has been updated.
        /// </summary>
        /// <param name="raycast">The <see cref="ARRaycast"/> being updated.</param>
        /// <param name="sessionRelativeData">The new data associated with the raycast. All spatial
        /// data is is session-relative space.</param>
        protected override void OnAfterSetSessionRelativeData(ARRaycast raycast, XRRaycast sessionRelativeData)
        {
            var planeManager = GetComponent<ARPlaneManager>();
            raycast.plane = planeManager ? planeManager.GetPlane(sessionRelativeData.hitTrackableId) : null;
        }

        /// <summary>
        /// The name of the `GameObject` for each instantiated <see cref="ARRaycast"/>.
        /// </summary>
        protected override string gameObjectName => "ARRaycast";

        static Comparison<ARRaycastHit> s_RaycastHitComparer = RaycastHitComparer;

        static List<NativeArray<XRRaycastHit>> s_NativeRaycastHits = new List<NativeArray<XRRaycastHit>>();

        Func<Vector2, TrackableType, Allocator, NativeArray<XRRaycastHit>> m_RaycastViewportDelegate;

        Func<Ray, TrackableType, Allocator, NativeArray<XRRaycastHit>> m_RaycastRayDelegate;

        List<IRaycaster> m_Raycasters = new List<IRaycaster>();
    }
}
