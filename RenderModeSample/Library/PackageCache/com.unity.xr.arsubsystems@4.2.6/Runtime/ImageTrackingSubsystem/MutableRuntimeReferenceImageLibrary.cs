using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A reference image library that can be constructed and modified at runtime.
    /// </summary>
    /// <remarks>
    /// This differs from an <see cref="XRReferenceImageLibrary"/>, which can only be constructed in the Editor and is
    /// immutable at runtime.
    ///
    /// > [!IMPORTANT]
    /// > Implementors: <see cref="XRImageTrackingSubsystem"/> providers must implement this class for their provider if
    /// > <see cref="XRImageTrackingSubsystemDescriptor.supportsMutableLibrary"/> is `true` to provide the functionality
    /// > to support runtime mutable libraries.
    /// >
    /// > This is not something consumers of the AR Subsystems package should implement.
    /// </remarks>
    /// <seealso cref="XRImageTrackingSubsystem.CreateRuntimeLibrary(XRReferenceImageLibrary)"/>
    public abstract class MutableRuntimeReferenceImageLibrary : RuntimeReferenceImageLibrary
    {
        /// <summary>
        /// This method should schedule a [Unity Job](xref:JobSystem) which adds an image to this reference image
        /// library.
        /// </summary>
        /// <param name="imageBytes">The raw image bytes in <paramref name="format"/>. Assume the bytes will be valid
        /// until the job completes.</param>
        /// <param name="sizeInPixels">The width and height of the image, in pixels.</param>
        /// <param name="format">The format of <paramref name="imageBytes"/>. The format has already been validated with
        /// <see cref="IsTextureFormatSupported(TextureFormat)"/>.</param>
        /// <param name="referenceImage">The <see cref="XRReferenceImage"/> data associated with the image to add to the
        /// library. This includes information like physical dimensions, associated
        /// [Texture2D](xref:UnityEngine.Texture2D) (optional), and string name.</param>
        /// <param name="inputDeps">Input dependencies for the add image job.</param>
        /// <returns>A [JobHandle](xref:Unity.Jobs.JobHandle) which can be used
        /// to chain together multiple tasks or to query for completion.</returns>
        /// <seealso cref="ScheduleAddImageJob(NativeSlice{byte}, Vector2Int, TextureFormat, XRReferenceImage, JobHandle)"/>
        protected abstract JobHandle ScheduleAddImageJobImpl(
            NativeSlice<byte> imageBytes,
            Vector2Int sizeInPixels,
            TextureFormat format,
            XRReferenceImage referenceImage,
            JobHandle inputDeps);

        /// <summary>
        /// Derived classes should call this to create an <see cref="AddReferenceImageJobState"/> to be returned by
        /// its implementation of <see cref="ScheduleAddImageWithValidationJobImpl"/>.
        /// </summary>
        /// <param name="handle">A handle to the job state. This should be unique to this job state.</param>
        /// <param name="jobHandle">The [JobHandle](xref:Unity.Jobs.JobHandle) associated with the add job state.</param>
        /// <returns>Returns a new <see cref="AddReferenceImageJobState"/> that contains information about the state of
        /// an add image job.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <see cref="supportsValidation"/> is true and
        /// <paramref name="handle"/> is `IntPtr.Zero`.</exception>
        protected AddReferenceImageJobState CreateAddJobState(IntPtr handle, JobHandle jobHandle)
        {
            if (supportsValidation && handle == IntPtr.Zero)
                throw new ArgumentException($"{nameof(handle)} must be non-zero if {nameof(supportsValidation)} is true.", nameof(handle));

            return new AddReferenceImageJobState(handle, jobHandle, this);
        }

        /// <summary>
        /// Get the status of an <see cref="AddReferenceImageJobState"/>.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > Implementors: If <see cref="supportsValidation"/> is `true`, then you must also implement this method.
        /// </remarks>
        /// <param name="state">The state whose status should be retrieved.</param>
        /// <returns>Returns the <see cref="AddReferenceImageJobStatus"/> of an existing
        /// <see cref="AddReferenceImageJobState"/>.</returns>
        /// <exception cref="System.NotImplementedException">Thrown if the <paramref name="state"/> has a non-zero
        /// <see cref="System.IntPtr"/> (non-zero means the concrete class should have overriden this method).
        /// </exception>
        protected internal virtual AddReferenceImageJobStatus GetAddReferenceImageJobStatus(AddReferenceImageJobState state)
        {
            if (state.AsIntPtr() == IntPtr.Zero)
            {
                return state.jobHandle.IsCompleted
                    ? AddReferenceImageJobStatus.Success
                    : AddReferenceImageJobStatus.Pending;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// (Read Only) <c>True</c> if this <see cref="MutableRuntimeReferenceImageLibrary"/> supports the validation of images
        /// when added via <see cref="ScheduleAddImageWithValidationJob"/>.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > Implementors: If this is `true`, then your implementation must also override:
        /// > - <see cref="ScheduleAddImageWithValidationJobImpl"/>
        /// > - <see cref="GetAddReferenceImageJobStatus"/>
        /// </remarks>
        public virtual bool supportsValidation => false;

        /// <summary>
        /// This method should schedule a [Unity Job](xref:JobSystem) which adds an image to this reference image
        /// library.
        /// </summary>
        /// <param name="imageBytes">The raw image bytes in <paramref name="format"/>. You can assume the bytes are
        /// valid until the job completes.</param>
        /// <param name="sizeInPixels">The width and height of the image, in pixels.</param>
        /// <param name="format">The format of <paramref name="imageBytes"/>. The format has already been validated with
        /// <see cref="IsTextureFormatSupported(TextureFormat)"/>.</param>
        /// <param name="referenceImage">The <see cref="XRReferenceImage"/> data associated with the image to add to the
        /// library. This includes information like physical dimensions, associated
        /// [Texture2D](xref:UnityEngine.Texture2D) (optional), and string name.</param>
        /// <param name="inputDeps">A [JobHandle](xref:Unity.Jobs.JobHandle) that represents input dependencies for the add
        /// image job.</param>
        /// <returns>Returns an <see cref="AddReferenceImageJobState"/> which contains the state of the asynchronous
        /// image addition.</returns>
        /// <exception cref="System.NotImplementedException">Thrown by this base class implementation. If
        /// <see cref="supportsValidation"/> is `true`, then this method should be implemented by the derived class.
        /// </exception>
        protected virtual AddReferenceImageJobState ScheduleAddImageWithValidationJobImpl(
            NativeSlice<byte> imageBytes,
            Vector2Int sizeInPixels,
            TextureFormat format,
            XRReferenceImage referenceImage,
            JobHandle inputDeps) => throw new NotImplementedException();

        /// <summary>
        /// Asynchronously adds an image to this library.
        /// </summary>
        /// <remarks>
        /// Image addition can take some time (for example, several frames) due to the processing that must occur to insert
        /// the image into the library. This is done using the [Unity Job System](xref:JobSystem). The returned
        /// <see cref="AddReferenceImageJobState"/> allows your to query for the status of the job, and, if the job completed,
        /// whether it was successful. The job state includes the [JobHandle](xref:Unity.Jobs.JobHandle) which can be
        /// used to chain together multiple tasks.
        ///
        /// This job, like all [Unity jobs](xref:JobSystem), can have dependencies (using the
        /// <paramref name="inputDeps"/>). This can be useful, for example, if <paramref name="imageBytes"/> is the
        /// output of another job. If you are adding multiple images to the library, it is not necessary to pass a
        /// previous <see cref="ScheduleAddImageWithValidationJob"/>'s `JobHandle` as the input dependency to the next
        /// <see cref="ScheduleAddImageWithValidationJob"/>.
        ///
        /// The <paramref name="imageBytes"/> must be valid until this job completes. The caller is responsible for
        /// managing its memory. You can use the resulting `JobHandle` to schedule a job that deallocates the
        /// <paramref name="imageBytes"/>.
        /// </remarks>
        /// <param name="imageBytes">The raw image bytes in <paramref name="format"/>.</param>
        /// <param name="sizeInPixels">The width and height of the image, in pixels.</param>
        /// <param name="format">The format of <paramref name="imageBytes"/>. Test for and enumerate supported formats
        /// with <see cref="supportedTextureFormatCount"/>, <see cref="GetSupportedTextureFormatAt(int)"/>, and
        /// <see cref="IsTextureFormatSupported(TextureFormat)"/>.</param>
        /// <param name="referenceImage">The <see cref="XRReferenceImage"/> data associated with the image to add to the
        /// library. This includes information like physical dimensions, associated
        /// [Texture2D](xref:UnityEngine.Texture2D) (optional), and string name. The
        /// <see cref="XRReferenceImage.guid"/> must be set to zero (<see cref="System.Guid.Empty"/>). A new guid is
        /// automatically generated for the new image.</param>
        /// <param name="inputDeps">(Optional) Input dependencies for the add image job.</param>
        /// <returns>Returns an <see cref="AddReferenceImageJobState"/> that can be used to query the status of the job.
        /// The <see cref="AddReferenceImageJobState"/> can be used to determine whether the image was successfully
        /// added. Invalid images will be not be added to the reference image library.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="imageBytes"/> does not contain any
        /// bytes.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="referenceImage"/>'s
        /// <see cref="XRReferenceImage.guid"/> is not <see cref="System.Guid.Empty"/>.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="referenceImage"/>'s
        /// <see cref="XRReferenceImage.name"/> is `null`.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="referenceImage"/>'s
        /// <see cref="XRReferenceImage.specifySize"/> is `true` but
        /// <paramref name="referenceImage"/>.<see cref="XRReferenceImage.size"/> contains values less than or equal
        /// to zero.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the <paramref name="format"/> is not supported.
        /// You can query for support using <see cref="IsTextureFormatSupported"/>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="sizeInPixels"/> contains
        /// values less than or equal to zero.</exception>
        public AddReferenceImageJobState ScheduleAddImageWithValidationJob(
            NativeSlice<byte> imageBytes,
            Vector2Int sizeInPixels,
            TextureFormat format,
            XRReferenceImage referenceImage,
            JobHandle inputDeps = default)
        {
            ValidateAndThrow(imageBytes, sizeInPixels, format, ref referenceImage);

            return supportsValidation
                ? ScheduleAddImageWithValidationJobImpl(imageBytes, sizeInPixels, format, referenceImage, inputDeps)
                : CreateAddJobState(IntPtr.Zero, ScheduleAddImageJobImpl(imageBytes, sizeInPixels, format, referenceImage, inputDeps));
        }

        /// <summary>
        /// Asynchronously adds an image to this library.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Image addition can take some time (several frames) due to extra processing that
        /// must occur to insert the image into the library. This is done using
        /// the [Unity Job System](xref:JobSystem). The returned
        /// [JobHandle](xref:Unity.Jobs.JobHandle) can be used
        /// to chain together multiple tasks or to query for completion, but can be safely discarded if you do not need it.
        /// </para><para>
        /// This job, like all Unity jobs, can have dependencies (using the <paramref name="inputDeps"/>). This can be useful,
        /// for example, if <paramref name="imageBytes"/> is the output of another job. If you are adding multiple
        /// images to the library, it is not necessary to pass a previous <c>ScheduleAddImageJob</c> JobHandle as the input
        /// dependency to the next <c>ScheduleAddImageJob</c>; they can be processed concurrently.
        /// </para><para>
        /// The <paramref name="imageBytes"/> must be valid until this job completes. The caller is responsible for managing its memory.
        /// </para>
        /// </remarks>
        /// <param name="imageBytes">The raw image bytes in <paramref name="format"/>.</param>
        /// <param name="sizeInPixels">The width and height of the image, in pixels.</param>
        /// <param name="format">The format of <paramref name="imageBytes"/>. Test for and enumerate supported formats with
        /// <see cref="supportedTextureFormatCount"/>, <see cref="GetSupportedTextureFormatAt(int)"/>, and <see cref="IsTextureFormatSupported(TextureFormat)"/>.</param>
        /// <param name="referenceImage">The <see cref="XRReferenceImage"/> data associated with the image to add to the library.
        /// This includes information like physical dimensions, associated <c>Texture2D</c> (optional), and string name.
        /// The <see cref="XRReferenceImage.guid"/> must be set to zero (empty).
        /// A new guid is automatically generated for the new image.</param>
        /// <param name="inputDeps">(Optional) input dependencies for the add image job.</param>
        /// <returns>A [JobHandle](xref:Unity.Jobs.JobHandle) which can be used
        /// to chain together multiple tasks or to query for completion. Can be safely discarded.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="imageBytes"/> does not contain any bytes.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="referenceImage"/><c>.guid</c> is not <c>Guid.Empty</c>.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="referenceImage"/><c>.name</c> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="referenceImage"/><c>.specifySize</c> is <c>true</c> and <paramref name="referenceImage"/><c>.size.x</c> contains a value less than or equal to zero.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the <paramref name="format"/> is not supported. You can query for support using <see cref="IsTextureFormatSupported"/>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="sizeInPixels"/><c>.x</c> or <paramref name="sizeInPixels"/><c>.y</c> is less than or equal to zero.</exception>
        [Obsolete("Use " + nameof(ScheduleAddImageWithValidationJob) + " instead. (2020-10-20)")]
        public JobHandle ScheduleAddImageJob(
            NativeSlice<byte> imageBytes,
            Vector2Int sizeInPixels,
            TextureFormat format,
            XRReferenceImage referenceImage,
            JobHandle inputDeps = default)
        {
            ValidateAndThrow(imageBytes, sizeInPixels, format, ref referenceImage);
            return ScheduleAddImageJobImpl(imageBytes, sizeInPixels, format, referenceImage, inputDeps);
        }

        void ValidateAndThrow(NativeSlice<byte> imageBytes, Vector2Int sizeInPixels, TextureFormat format, ref XRReferenceImage referenceImage)
        {
            unsafe
            {
                if (imageBytes.GetUnsafePtr() == null)
                    throw new ArgumentException($"{nameof(imageBytes)} does not contain any bytes.", nameof(imageBytes));
            }

            if (!referenceImage.guid.Equals(Guid.Empty))
                throw new ArgumentException($"{nameof(referenceImage)}.{nameof(referenceImage.guid)} must be empty (all zeroes).", $"{nameof(referenceImage)}.{nameof(referenceImage.guid)}");

            // Generate and assign a new guid for the new image
            referenceImage.m_SerializedGuid = GenerateNewGuid();

            if (string.IsNullOrEmpty(referenceImage.name))
                throw new ArgumentNullException($"{nameof(referenceImage)}.{nameof(referenceImage.name)}");

            if (referenceImage.specifySize && referenceImage.size.x <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(referenceImage)}.{nameof(referenceImage.size)}", referenceImage.size.x, $"Invalid physical image dimensions.");

            if (!IsTextureFormatSupported(format))
                throw new InvalidOperationException($"The texture format {format} is not supported by the current image tracking subsystem.");

            if (sizeInPixels.x <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(sizeInPixels)}.{nameof(sizeInPixels.x)}", sizeInPixels.x, "Image width must be greater than zero.");

            if (sizeInPixels.y <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(sizeInPixels)}.{nameof(sizeInPixels.y)}", sizeInPixels.y, "Image height must be greater than zero.");
        }

        /// <summary>
        /// The number of texture formats that are supported for image addition.
        /// </summary>
        public abstract int supportedTextureFormatCount { get; }

        /// <summary>
        /// Returns the supported texture format at <paramref name="index"/>. Useful for enumerating the supported texture formats for image addition.
        /// </summary>
        /// <param name="index">The index of the format to retrieve.</param>
        /// <returns>The supported format at <paramref name="index"/>.</returns>
        public TextureFormat GetSupportedTextureFormatAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"{nameof(index)} must be greater than or equal to zero.");

            if (index >= supportedTextureFormatCount)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"{nameof(index)} must be less than {nameof(supportedTextureFormatCount)} ({supportedTextureFormatCount}).");

            return GetSupportedTextureFormatAtImpl(index);
        }

        /// <summary>
        /// Derived methods should return the [TextureFormat](xref:UnityEngine.TextureFormat) at the given <paramref name="index"/>.
        /// <paramref name="index"/> has already been validated to be within [0..<see cref="supportedTextureFormatCount"/>].
        /// </summary>
        /// <param name="index">The index of the format to retrieve.</param>
        /// <returns>The supported format at <paramref name="index"/>.</returns>
        protected abstract TextureFormat GetSupportedTextureFormatAtImpl(int index);

        /// <summary>
        /// Determines whether the given <paramref name="format"/> is supported.
        /// </summary>
        /// <param name="format">The [TextureFormat](xref:UnityEngine.TextureFormat) to test.</param>
        /// <returns><c>true</c> if <paramref name="format"/> is supported for image addition; otherwise, <c>false</c>.</returns>
        public bool IsTextureFormatSupported(TextureFormat format)
        {
            int n = supportedTextureFormatCount;
            for (int i = 0; i < n; ++i)
            {
                if (GetSupportedTextureFormatAtImpl(i) == format)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an enumerator for this collection of reference images. This allows this image library to act as a collection in a <c>foreach</c> statement.
        /// The <see cref="Enumerator"/> is a <c>struct</c>, so no garbage is generated.
        /// </summary>
        /// <returns>An enumerator that can be used in a <c>foreach</c> statement.</returns>
        public Enumerator GetEnumerator() => new Enumerator(this);

        // Converts a System.Guid into two ulongs
        static unsafe SerializableGuid GenerateNewGuid()
        {
            var newGuid = Guid.NewGuid();
            var trackableId = *(TrackableId*)&newGuid;
            return new SerializableGuid(trackableId.subId1, trackableId.subId2);
        }

        /// <summary>
        /// An enumerator to be used in a <c>foreach</c> statement.
        /// </summary>
        public struct Enumerator : IEquatable<Enumerator>
        {
            internal Enumerator(MutableRuntimeReferenceImageLibrary lib)
            {
                m_Library = lib;
                m_Index = -1;
            }

            MutableRuntimeReferenceImageLibrary m_Library;

            int m_Index;

            /// <summary>
            /// Moves to the next element in the collection.
            /// </summary>
            /// <returns><c>true</c> if <see cref="Current"/> is valid after this call.</returns>
            public bool MoveNext() => ++m_Index < m_Library.count;

            /// <summary>
            /// The current <see cref="XRReferenceImage"/>.
            /// </summary>
            public XRReferenceImage Current => m_Library[m_Index];

            /// <summary>
            /// Disposes of the enumerator. This method does nothing.
            /// </summary>
            public void Dispose() {}

            /// <summary>
            /// Generates a hash code suitable for use in a `Dictionary` or `HashSet`.
            /// </summary>
            /// <returns>A hash code of this Enumerator.</returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = ReferenceEquals(m_Library, null) ? 0 : m_Library.GetHashCode();
                    return hash * 486187739 + m_Index.GetHashCode();
                }
            }

            /// <summary>
            /// Compares for equality.
            /// </summary>
            /// <param name="obj">The <c>object</c> to compare against.</param>
            /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="Enumerator"/> and <see cref="Equals(Enumerator)"/> is <c>true</c>.</returns>
            public override bool Equals(object obj) => (obj is Enumerator) && Equals((Enumerator)obj);

            /// <summary>
            /// Compares for equality.
            /// </summary>
            /// <param name="other">The other enumerator to compare against.</param>
            /// <returns><c>true</c> if the other enumerator is equal to this one.</returns>
            public bool Equals(Enumerator other)
            {
                return
                    ReferenceEquals(m_Library, other.m_Library) &&
                    (m_Index == other.m_Index);
            }

            /// <summary>
            /// Compares for equality.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>The same as <see cref="Equals(Enumerator)"/>.</returns>
            public static bool operator ==(Enumerator lhs, Enumerator rhs) => lhs.Equals(rhs);

            /// <summary>
            /// Compares for inequality.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>The negation of <see cref="Equals(Enumerator)"/>.</returns>
            public static bool operator !=(Enumerator lhs, Enumerator rhs) => !lhs.Equals(rhs);
        }
    }
}
