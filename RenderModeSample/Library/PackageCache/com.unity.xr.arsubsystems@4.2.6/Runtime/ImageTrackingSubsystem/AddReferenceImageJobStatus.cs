using System;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the status of an <see cref="AddReferenceImageJobState"/>.
    /// </summary>
    /// <seealso cref="AddReferenceImageJobState.status"/>
    /// <seealso cref="MutableRuntimeReferenceImageLibrary.ScheduleAddImageWithValidationJob"/>
    public enum AddReferenceImageJobStatus
    {
        /// <summary>
        /// No status (for example, because the <see cref="AddReferenceImageJobState"/> was default-constructed).
        /// </summary>
        None,

        /// <summary>
        /// The job is pending.
        /// </summary>
        Pending,

        /// <summary>
        /// The reference image was added successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The reference image was not added because it is invalid.
        /// </summary>
        ErrorInvalidImage,

        /// <summary>
        /// The reference image was not added due to an unknown error.
        /// </summary>
        ErrorUnknown,
    }

    /// <summary>
    /// Extensions to the <see cref="AddReferenceImageJobStatus"/> `enum`.
    /// </summary>
    public static class AddReferenceImageJobStatusExtensions
    {
        /// <summary>
        /// Determines whether the <see cref="AddReferenceImageJobStatus"/> is
        /// <see cref="AddReferenceImageJobStatus.Pending"/>.
        /// </summary>
        /// <param name="status">The <see cref="AddReferenceImageJobStatus"/> being extended.</param>
        /// <returns>Returns `true` if <paramref name="status"/> is <see cref="AddReferenceImageJobStatus.Pending"/>.
        /// Returns `false` otherwise.</returns>
        public static bool IsPending(this AddReferenceImageJobStatus status) =>
            status == AddReferenceImageJobStatus.Pending;

        /// <summary>
        /// Determines whether the <see cref="AddReferenceImageJobStatus"/> has completed (successfully or not).
        /// </summary>
        /// <param name="status">The <see cref="AddReferenceImageJobStatus"/> being extended.</param>
        /// <returns>Returns `true` if <paramref name="status"/> is greater than
        /// <see cref="AddReferenceImageJobStatus.Pending"/>. Returns `false` otherwise.</returns>
        public static bool IsComplete(this AddReferenceImageJobStatus status) =>
            status > AddReferenceImageJobStatus.Pending;

        /// <summary>
        /// Determines whether the <see cref="AddReferenceImageJobStatus"/> has completed with an error.
        /// </summary>
        /// <param name="status">The <see cref="AddReferenceImageJobStatus"/> being extended.</param>
        /// <returns>Returns `true` if <paramref name="status"/> is greater than or equal to
        /// <see cref="AddReferenceImageJobStatus.ErrorInvalidImage"/>. Returns `false` otherwise.</returns>
        public static bool IsError(this AddReferenceImageJobStatus status) =>
            status >= AddReferenceImageJobStatus.ErrorInvalidImage;

        /// <summary>
        /// Determines whether the <see cref="AddReferenceImageJobStatus"/> has completed successfully.
        /// </summary>
        /// <param name="status">The <see cref="AddReferenceImageJobStatus"/> being extended.</param>
        /// <returns>Returns `true` if <paramref name="status"/> is <see cref="AddReferenceImageJobStatus.Success"/>.
        /// Returns `false` otherwise.</returns>
        public static bool IsSuccess(this AddReferenceImageJobStatus status) =>
            status == AddReferenceImageJobStatus.Success;
    }
}
