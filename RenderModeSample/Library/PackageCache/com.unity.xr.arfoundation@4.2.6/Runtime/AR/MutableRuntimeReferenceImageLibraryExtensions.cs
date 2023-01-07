using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Extension methods for
    /// [MutableRuntimeReferenceImageLibrary](xref:UnityEngine.XR.ARSubsystems.MutableRuntimeReferenceImageLibrary).
    /// </summary>
    public static class MutableRuntimeReferenceImageLibraryExtensions
    {
        /// <summary>
        /// Asynchronously adds <paramref name="texture"/> to <paramref name="library"/>.
        /// </summary>
        /// <remarks>
        /// This method is deprecated. Use <see cref="ScheduleAddImageWithValidationJob"/> instead.
        ///
        /// Image addition can take some time (several frames) due to extra processing that must occur to insert the
        /// image into the library. This is done using the [Unity Job System](xref:JobSystem). The returned
        /// [JobHandle](xref:Unity.Jobs.JobHandle) can be used to chain together multiple tasks or to query for
        /// completion, but you can safely discarded if you do not need it.
        ///
        /// This job, like all Unity jobs, can have dependencies (using the <paramref name="inputDeps"/>). If you are
        /// adding multiple images to the library, it is not necessary to pass a previous
        /// <see cref="ScheduleAddImageJob"/> `JobHandle` as the input dependency to the next
        /// <see cref="ScheduleAddImageJob"/>; they can be processed concurrently.
        ///
        /// The bytes of the <paramref name="texture"/> are copied, so you can safely destroy the texture
        /// after this method returns.
        /// </remarks>
        /// <param name="library">The <see cref="MutableRuntimeReferenceImageLibrary"/> being extended.</param>
        /// <param name="texture">The [Texture2D](xref:UnityEngine.Texture2D) to use as image target.</param>
        /// <param name="name">The name of the image.</param>
        /// <param name="widthInMeters">The physical width of the image, in meters.</param>
        /// <param name="inputDeps">Input job dependencies (optional).</param>
        /// <returns>A [JobHandle](xref:Unity.Jobs.JobHandle) which can be used to chain together multiple tasks or to
        ///     query for completion. Can be safely discarded.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref cref="library"/> is `null`.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="texture"/> is `null`.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="texture"/> is not readable.</exception>
        [Obsolete("Use " + nameof(ScheduleAddImageWithValidationJob) + " instead. (2020-10-20)")]
        public static JobHandle ScheduleAddImageJob(
            this MutableRuntimeReferenceImageLibrary library,
            Texture2D texture,
            string name,
            float? widthInMeters,
            JobHandle inputDeps = default) =>
            ScheduleAddImageWithValidationJob(library, texture, name, widthInMeters, inputDeps).jobHandle;

        /// <summary>
        /// Asynchronously adds <paramref name="texture"/> to <paramref name="library"/>.
        /// </summary>
        /// <remarks>
        /// Image addition can take some time (several frames) due to extra processing that must occur to insert the
        /// image into the library. This is done using the [Unity Job System](xref:JobSystem). The returned
        /// [AddReferenceImageJobState](xref:UnityEngine.XR.ARSubsystems.AddReferenceImageJobState) can be used to query
        /// for job completion and whether the addition was successful.
        ///
        /// The bytes of the <paramref name="texture"/> are copied, so the texture may be safely
        /// destroyed after this method returns.
        /// </remarks>
        /// <param name="library">The <see cref="MutableRuntimeReferenceImageLibrary"/> being extended.</param>
        /// <param name="texture">The [Texture2D](xref:UnityEngine.Texture2D) to use as image target.</param>
        /// <param name="name">The name of the image.</param>
        /// <param name="widthInMeters">The physical width of the image, in meters.</param>
        /// <param name="inputDeps">Input job dependencies (optional).</param>
        /// <returns>Returns an [AddReferenceImageJobState](xref:UnityEngine.XR.ARSubsystems.AddReferenceImageJobState)
        /// that can be used to query the status of the job. The `AddReferenceImageJobState` can be used to
        /// determine whether the image was successfully added. If image validity can be determined, invalid images
        /// will be not be added to the reference image library.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref cref="library"/> is `null`.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="texture"/> is `null`.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="texture"/> is not readable.</exception>
        public static AddReferenceImageJobState ScheduleAddImageWithValidationJob(
            this MutableRuntimeReferenceImageLibrary library,
            Texture2D texture,
            string name,
            float? widthInMeters,
            JobHandle inputDeps = default)
        {
            if (ReferenceEquals(library, null))
                throw new ArgumentNullException(nameof(library));

            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (!texture.isReadable)
                throw new InvalidOperationException("The texture must be readable to be used as the source for a reference image.");

            var imageBytesCopy = new NativeArray<byte>(texture.GetRawTextureData<byte>(), Allocator.Persistent);
            try
            {
                Vector2? size = null;
                if (widthInMeters.HasValue)
                {
                    size = new Vector2(widthInMeters.Value, widthInMeters.Value * texture.height / texture.width);
                }

                var referenceImage = new XRReferenceImage(SerializableGuid.empty, SerializableGuid.empty, size, name, texture);
                var state = library.ScheduleAddImageWithValidationJob(imageBytesCopy, new Vector2Int(texture.width, texture.height), texture.format, referenceImage, inputDeps);
                new DeallocateJob { data = imageBytesCopy }.Schedule(state.jobHandle);
                return state;
            }
            catch
            {
                imageBytesCopy.Dispose();
                throw;
            }
        }

        struct DeallocateJob : IJob
        {
            [DeallocateOnJobCompletion]
            public NativeArray<byte> data;
            public void Execute() { }
        }
    }
}
