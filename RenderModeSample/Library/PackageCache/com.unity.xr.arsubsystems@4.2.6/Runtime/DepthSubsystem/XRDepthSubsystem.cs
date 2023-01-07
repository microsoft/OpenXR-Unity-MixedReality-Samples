using System;
using Unity.Collections;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// An abstract class that provides a generic API for low-level depth detection features.
    /// </summary>
    /// <remarks>
    /// This class can be used to access depth detection features in your app via accessing the generic API.
    /// It can also be extended to provide an implementation of a provider which provides the depth detection data
    /// to the higher level code.
    /// </remarks>
    public class XRDepthSubsystem
        : TrackingSubsystem<XRPointCloud, XRDepthSubsystem, XRDepthSubsystemDescriptor, XRDepthSubsystem.Provider>
    {

        /// <summary>
        /// Get the changes to point clouds (added, updated, and removed) since the last call to <see cref="GetChanges(Allocator)"/>.
        /// </summary>
        /// <param name="allocator">An <c>Allocator</c> to use when allocating the returned <c>NativeArray</c>s.</param>
        /// <returns>
        /// <see cref="TrackableChanges{T}"/> describing the point clouds that have been added, updated, and removed
        /// since the last call to <see cref="GetChanges(Allocator)"/>. The caller owns the memory allocated with <c>Allocator</c>.
        /// </returns>
        public override TrackableChanges<XRPointCloud> GetChanges(Allocator allocator)
        {
            var changes = provider.GetChanges(XRPointCloud.defaultValue, allocator);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            m_ValidationUtility.ValidateAndDisposeIfThrown(changes);
#endif
            return changes;
        }

        /// <summary>
        /// Retrieve point cloud data (positions, confidence values, and identifiers)
        /// for the point cloud with the given <paramref name="trackableId"/>.
        /// </summary>
        /// <param name="trackableId">The point cloud for which to retrieve data.</param>
        /// <param name="allocator">The allocator to use when creating the <c>NativeArray</c>s in the returned <see cref="XRPointCloudData"/>. <c>Allocator.Temp</c> is not supported; use <c>Allocator.TempJob</c> if you need temporary memory.</param>
        /// <returns>
        /// A new <see cref="XRPointCloudData"/> with newly allocated <c>NativeArray</c>s using <paramref name="allocator"/>.
        /// The caller owns the memory and is responsible for calling <see cref="XRPointCloudData.Dispose"/> on it.
        /// </returns>
        public XRPointCloudData GetPointCloudData(
            TrackableId trackableId,
            Allocator allocator)
        {
            if (allocator == Allocator.Temp)
                throw new InvalidOperationException("Allocator.Temp is not supported. Use Allocator.TempJob if you wish to use a temporary allocator.");

            if (allocator == Allocator.None)
                throw new InvalidOperationException("Allocator.None is not a valid allocator.");

            return provider.GetPointCloudData(trackableId, allocator);
        }

        /// <summary>
        /// The interface that each derived class must implement.
        /// </summary>
        public abstract class Provider : SubsystemProvider<XRDepthSubsystem>
        {
            /// <summary>
            /// Called when the subsystem starts. Will not be called again until <see cref="Stop"/>.
            /// </summary>
            public override void Start() { }

            /// <summary>
            /// Called when the subsystem stops. Will not be called before <see cref="Start"/>.
            /// </summary>
            public override void Stop() { }

            /// <summary>
            /// Called when the subsystem is destroyed. <see cref="Stop"/> will be called first if the subsystem is running.
            /// </summary>
            public override void Destroy() { }

            /// <summary>
            /// Get the changes to planes (added, updated, and removed) since the last call to
            /// <see cref="GetChanges(XRPointCloud,Allocator)"/>.
            /// </summary>
            /// <param name="defaultPointCloud">
            /// The default point cloud. This should be used to initialize the returned <c>NativeArray</c>s for backwards compatibility.
            /// See <see cref="TrackableChanges{T}.TrackableChanges(void*, int, void*, int, void*, int, T, int, Allocator)"/>.
            /// </param>
            /// <param name="allocator">An <c>Allocator</c> to use when allocating the returned <c>NativeArray</c>s.</param>
            /// <returns>
            /// <see cref="TrackableChanges{T}"/> describing the reference points that have been added, updated, and removed
            /// since the last call to <see cref="GetChanges(XRPointCloud, Allocator)"/>. The changes should be allocated using
            /// <paramref name="allocator"/>.
            /// </returns>
            public abstract TrackableChanges<XRPointCloud> GetChanges(XRPointCloud defaultPointCloud, Allocator allocator);

            /// <summary>
            /// Generate point cloud data (positions, confidence values, and identifiers)
            /// for the point cloud with the given <paramref name="trackableId"/>.
            /// </summary>
            /// <param name="trackableId">The point cloud for which to retrieve data.</param>
            /// <param name="allocator">The allocator to use when creating the <c>NativeArray</c>s in the returned <see cref="XRPointCloudData"/>.</param>
            /// <returns>
            /// A new <see cref="XRPointCloudData"/> with newly allocated <c>NativeArray</c>s using <paramref name="allocator"/>.
            /// The caller owns the memory and is responsible for calling <see cref="XRPointCloudData.Dispose"/> on it.
            /// </returns>
            public abstract XRPointCloudData GetPointCloudData(TrackableId trackableId, Allocator allocator);
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ValidationUtility<XRPointCloud> m_ValidationUtility =
            new ValidationUtility<XRPointCloud>();
#endif
    }
}
