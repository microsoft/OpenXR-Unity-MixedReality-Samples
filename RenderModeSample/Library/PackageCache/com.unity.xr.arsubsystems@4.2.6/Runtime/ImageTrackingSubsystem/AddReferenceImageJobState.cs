using System;
using Unity.Jobs;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the state of an asynchronous "add image job" scheduled by
    /// <see cref="MutableRuntimeReferenceImageLibrary.ScheduleAddImageWithValidationJob"/>.
    /// </summary>
    public readonly struct AddReferenceImageJobState : IEquatable<AddReferenceImageJobState>
    {
        readonly IntPtr m_Handle;

        readonly MutableRuntimeReferenceImageLibrary m_Library;

        internal AddReferenceImageJobState(IntPtr nativePtr, JobHandle jobHandle, MutableRuntimeReferenceImageLibrary library)
        {
            if (library == null)
                throw new ArgumentNullException(nameof(library));

            m_Handle = nativePtr;
            m_Library = library;
            this.jobHandle = jobHandle;
        }

        /// <summary>
        /// The [JobHandle](xref:Unity.Jobs.JobHandle) associated with the add job.
        /// </summary>
        public JobHandle jobHandle { get; }

        /// <summary>
        /// Gets the job state as an <see cref="System.IntPtr"/>.
        /// </summary>
        /// <returns>Returns this <see cref="AddReferenceImageJobState"/> as an <see cref="System.IntPtr"/>.</returns>
        public IntPtr AsIntPtr() => m_Handle;

        /// <summary>
        /// Casts this <see cref="AddReferenceImageJobState"/> to an <see cref="System.IntPtr"/>.
        /// </summary>
        /// <param name="state">The <see cref="AddReferenceImageJobState"/> to cast.</param>
        /// <returns>Returns the <see cref="System.IntPtr"/> associated with this
        /// <see cref="AddReferenceImageJobState"/>.</returns>
        public static explicit operator IntPtr(AddReferenceImageJobState state) => state.m_Handle;

        /// <summary>
        /// (Read Only) The status of the add job.
        /// </summary>
        public AddReferenceImageJobStatus status =>
            m_Library?.GetAddReferenceImageJobStatus(this) ?? AddReferenceImageJobStatus.None;

        /// <summary>
        /// Provides a string representation suitable for debug logging.
        /// </summary>
        /// <returns>A string representation of this <see cref="AddReferenceImageJobState"/>.</returns>
        public override string ToString() => $"(handle: {m_Handle.ToString()}, {nameof(status)}: {status})";

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>Returns a hash code for this <see cref="AddReferenceImageJobState"/>.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(
            m_Handle.GetHashCode(),
            HashCodeUtil.ReferenceHash(m_Library));

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>Returns `true` if <paramref name="obj"/> is of type <see cref="AddReferenceImageJobState"/>
        /// and is considered equal to this <see cref="AddReferenceImageJobState"/> using
        /// <see cref="Equals(AddReferenceImageJobState)"/>. Returns `false` otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is AddReferenceImageJobState other && Equals(other);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="AddReferenceImageJobState"/> to compare against.</param>
        /// <returns>Returns `true` if this <see cref="AddReferenceImageJobState"/> represents the same handle
        /// as <paramref name="other"/>. Returns `false` otherwise.</returns>
        public bool Equals(AddReferenceImageJobState other) =>
            m_Handle == other.m_Handle &&
            m_Library == other.m_Library;

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(AddReferenceImageJobState)"/>.
        /// </summary>
        /// <param name="lhs">The <see cref="AddReferenceImageJobState"/> to compare with <paramref name="rhs"/>.</param>
        /// <param name="rhs">The <see cref="AddReferenceImageJobState"/> to compare with <paramref name="lhs"/>.</param>
        /// <returns>Returns `true` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>.
        /// Returns `false` otherwise.</returns>
        public static bool operator ==(AddReferenceImageJobState lhs, AddReferenceImageJobState rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. This is the negation of <see cref="Equals(AddReferenceImageJobState)"/>.
        /// </summary>
        /// <param name="lhs">The <see cref="AddReferenceImageJobState"/> to compare with <paramref name="rhs"/>.</param>
        /// <param name="rhs">The <see cref="AddReferenceImageJobState"/> to compare with <paramref name="lhs"/>.</param>
        /// <returns>Returns `true` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>.
        /// Returns `false` otherwise.</returns>
        public static bool operator !=(AddReferenceImageJobState lhs, AddReferenceImageJobState rhs) => !lhs.Equals(rhs);
    }
}
