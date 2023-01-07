using System;
using Unity.Collections;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Base class for a raycast subsystem.
    /// </summary>
    /// <remarks>
    /// This abstract class should be implemented by an XR provider and instantiated using the <c>SubsystemManager</c>
    /// to enumerate the available <see cref="XRRaycastSubsystemDescriptor"/>s.
    /// </remarks>
    public class XRRaycastSubsystem
        : TrackingSubsystem<XRRaycast, XRRaycastSubsystem, XRRaycastSubsystemDescriptor, XRRaycastSubsystem.Provider>
    {
        /// <summary>
        /// Constructor. Do not invoke directly; use the <c>SubsystemManager</c>
        /// to enumerate the available <see cref="XRRaycastSubsystemDescriptor"/>s
        /// and call <c>Create</c> on the desired descriptor.
        /// </summary>
        public XRRaycastSubsystem() { }

        /// <summary>
        /// Get the changes to the tracked raycasts (arrays of added, updated and removed) since the last call to this
        /// method.
        /// </summary>
        /// <param name="allocator">An [allocator](https://docs.unity3d.com/ScriptReference/Unity.Collections.Allocator.html)
        /// to use for the returned container.</param>
        /// <returns>The set of changes since the last call to this method. The caller owns the data and is responsible
        /// for calling <see cref="TrackableChanges{T}.Dispose"/> on it.</returns>
        public override TrackableChanges<XRRaycast> GetChanges(Allocator allocator)
        {
            var changes = provider.GetChanges(XRRaycast.defaultValue, allocator);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            m_ValidationUtility.ValidateAndDisposeIfThrown(changes);
#endif
            return changes;
        }

        /// <summary>
        /// Attempts to add a new persistent raycast. The raycast will be updated automatically until
        /// this subsystem is stopped or destroyed, or the the raycast is removed with
        /// <see cref="RemoveRaycast(TrackableId)"/>.
        /// </summary>
        /// <param name="screenPoint">A point on the screen, in normalized screen coorindates (0, 0)..(1, 1).</param>
        /// <param name="estimatedDistance">The estimated distance to the raycast target. For example, an average
        /// human height might be used to estimate the distance to the floor.</param>
        /// <param name="raycast">The newly added raycast. All spatial data is relative to the session origin.</param>
        /// <returns>`True` if the raycast was successfully added, or `false` otherwise.</returns>
        public bool TryAddRaycast(Vector2 screenPoint, float estimatedDistance, out XRRaycast raycast) => provider.TryAddRaycast(screenPoint, estimatedDistance, out raycast);

        /// <summary>
        /// Attempts to add a new persistent raycast. The raycast will be updated automatically until
        /// this subsystem is stopped or destroyed, or the the raycast is removed with
        /// <see cref="RemoveRaycast(TrackableId)"/>.
        /// </summary>
        /// <param name="ray">A [ray](https://docs.unity3d.com/ScriptReference/Ray.html) relative to the session origin defining the raycast.</param>
        /// <param name="estimatedDistance">The estimated distance to the raycast target. For example, an average
        /// human height might be used to estimate the distance to the floor.</param>
        /// <param name="raycast">The newly added raycast. All spatial data is relative to the session origin.</param>
        /// <returns>`True` if the raycast was successfully added, or `false` otherwise.</returns>
        public bool TryAddRaycast(Ray ray, float estimatedDistance, out XRRaycast raycast) => provider.TryAddRaycast(ray, estimatedDistance, out raycast);

        /// <summary>
        /// Removes an existing raycast by its <see cref="TrackableId"/>.
        /// </summary>
        /// <param name="trackableId">The unique identifier for the raycast to remove.</param>
        public void RemoveRaycast(TrackableId trackableId) => provider.RemoveRaycast(trackableId);

        /// <summary>
        /// Casts <paramref name="ray"/> against trackables specified with <paramref name="trackableTypeMask"/>.
        /// </summary>
        /// <param name="ray">A ray in session space.</param>
        /// <param name="trackableTypeMask">The types of trackables to test for ray intersections.</param>
        /// <param name="allocator">The <c>Allocator</c> used to allocate the returned <c>NativeArray</c>.</param>
        /// <returns>A <c>NativeArray</c> of all the resulting ray intersections.</returns>
        public NativeArray<XRRaycastHit> Raycast(
            Ray ray,
            TrackableType trackableTypeMask,
            Allocator allocator)
        {
            return provider.Raycast(XRRaycastHit.defaultValue, ray, trackableTypeMask, allocator);
        }

        /// <summary>
        /// Casts a ray originating from <paramref name="screenPoint"/> against trackables specified with <paramref name="trackableTypeMask"/>.
        /// </summary>
        /// <param name="screenPoint">A point on the screen in normalized screen coordinates (0, 0) - (1, 1).</param>
        /// <param name="trackableTypeMask">The types of trackables to test for ray intersections.</param>
        /// <param name="allocator">The <c>Allocator</c> used to allocate the returned <c>NativeArray</c>.</param>
        /// <returns>A <c>NativeArray</c> of all the resulting ray intersections.</returns>
        public NativeArray<XRRaycastHit> Raycast(
            Vector2 screenPoint,
            TrackableType trackableTypeMask,
            Allocator allocator)
        {
            return provider.Raycast(XRRaycastHit.defaultValue, screenPoint, trackableTypeMask, allocator);
        }

        /// <summary>
        /// An interface to be implemented by providers of this subsystem.
        /// </summary>
        public class Provider : SubsystemProvider<XRRaycastSubsystem>
        {
            /// <summary>
            /// Called when the subsystem is started. Will not be called again until <see cref="Stop"/>.
            /// </summary>
            public override void Start() { }

            /// <summary>
            /// Called when the subsystem is stopped. Will not be called before <see cref="Start"/>.
            /// </summary>
            public override void Stop() { }

            /// <summary>
            /// Called when the subsystem is destroyed. <see cref="Stop"/> will be called first if the subsystem is running.
            /// </summary>
            public override void Destroy() { }

            /// <summary>
            /// Adds a new persistent raycast. Persistent raycasts should be updated automatically until this
            /// provider is stopped or destroyed or the raycast is removed with
            /// <see cref="RemoveRaycast(TrackableId)"/>.
            /// </summary>
            /// <param name="screenPoint">A position on the screen in normalized screen coordinates (0, 0)..(1, 1).</param>
            /// <param name="estimatedDistance">The estimated distance to the raycast target.</param>
            /// <param name="raycast">The newly added raycast. All spatial data should be reported relative to the session origin.</param>
            /// <returns>`True` if the raycast was added; otherwise `false`.</returns>
            public virtual bool TryAddRaycast(Vector2 screenPoint, float estimatedDistance, out XRRaycast raycast)
            {
                raycast = XRRaycast.defaultValue;
                return false;
            }

            /// <summary>
            /// Adds a new persistent raycast. Persistent raycasts should be updated automatically until this
            /// provider is stopped or destroyed or the raycast is removed with
            /// <see cref="RemoveRaycast(TrackableId)"/>.
            /// </summary>
            /// <param name="ray">A ray in session space defining the raycast.</param>
            /// <param name="estimatedDistance">The estimated distance to the raycast target.</param>
            /// <param name="raycast">The newly added raycast. All spatial data should be reported relative to the session origin.</param>
            /// <returns>`True` if the raycast was added; otherwise `false`.</returns>
            public virtual bool TryAddRaycast(Ray ray, float estimatedDistance, out XRRaycast raycast)
            {
                raycast = XRRaycast.defaultValue;
                return false;
            }

            /// <summary>
            /// Removes a raycast previously added with
            /// <see cref="TryAddRaycast(UnityEngine.Vector2,float,out UnityEngine.XR.ARSubsystems.XRRaycast)"/>
            /// or
            /// <see cref="TryAddRaycast(UnityEngine.Ray,float,out UnityEngine.XR.ARSubsystems.XRRaycast)"/>
            /// </summary>
            /// <param name="trackableId">The unique identifier associated with the raycast to remove.</param>
            public virtual void RemoveRaycast(TrackableId trackableId) { }

            /// <summary>
            /// Get the changes to raycasts (arrays of added, updated, and removed) since the last call to this method.
            /// </summary>
            /// <param name="defaultRaycast">A default value for <see cref="XRRaycast"/>s. For backwards compatibility,
            /// this should be used to initialize the returned
            /// [NativeArray](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html)s.</param>
            /// <param name="allocator">The [Allocator](https://docs.unity3d.com/ScriptReference/Unity.Collections.Allocator.html)
            /// to use when allocating the returned [NativeArray](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html)s.</param>
            /// <returns>Arrays of added, updated, and removed raycasts since the last call to this method. The
            /// changes should be allocated using <paramref name="allocator"/>. The caller owns the memory.</returns>
            public virtual TrackableChanges<XRRaycast> GetChanges(XRRaycast defaultRaycast, Allocator allocator) => default;

            /// <summary>
            /// Performs a raycast from an arbitrary ray against the types
            /// specified by <paramref name="trackableTypeMask"/>. Results
            /// should be sorted by distance from the ray origin.
            /// </summary>
            /// <param name="defaultRaycastHit">The default raycast hit that should be used as a template when populating the returned <c>NativeArray</c>.</param>
            /// <param name="ray">A ray in session space from which to raycast.</param>
            /// <param name="trackableTypeMask">The types to raycast against.</param>
            /// <param name="allocator">The allocator with which to allocate the returned <c>NativeArray</c>.</param>
            /// <returns>A <c>NativeArray</c> of all the resulting ray intersections.</returns>
            public virtual NativeArray<XRRaycastHit> Raycast(
                XRRaycastHit defaultRaycastHit,
                Ray ray,
                TrackableType trackableTypeMask,
                Allocator allocator)
            {
                throw new NotSupportedException("Raycasting using a Ray is not supported.");
            }

            /// <summary>
            /// Performs a raycast from the camera against the types
            /// specified by <paramref name="trackableTypeMask"/>. Results
            /// should be sorted by distance from the ray origin.
            /// </summary>
            /// <param name="defaultRaycastHit">The default raycast hit that should be used as a template when populating the returned <c>NativeArray</c>.</param>
            /// <param name="screenPoint">A point on the screen in normalized (0..1) coordinates.</param>
            /// <param name="trackableTypeMask">The types to raycast against.</param>
            /// <param name="allocator">The allocator with which to allocate the returned <c>NativeArray</c>.</param>
            /// <returns>A <c>NativeArray</c> of all the resulting ray intersections.</returns>
            public virtual NativeArray<XRRaycastHit> Raycast(
                XRRaycastHit defaultRaycastHit,
                Vector2 screenPoint,
                TrackableType trackableTypeMask,
                Allocator allocator)
            {
                throw new NotSupportedException("Raycasting using a screen point is not supported.");
            }
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ValidationUtility<XRRaycast> m_ValidationUtility = new ValidationUtility<XRRaycast>();
#endif
    }
}
