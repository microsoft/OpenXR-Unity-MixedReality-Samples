using System;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Represents a reference point tracked by an XR device.
    /// </summary>
    /// <remarks>
    /// A reference point is a pose in the physical environment that is tracked by an XR device.
    /// As the device refines its understanding of the environment, reference points will be
    /// updated, helping you to keep virtual content connected to a real-world position and orientation.
    /// </remarks>
    [DefaultExecutionOrder(ARUpdateOrder.k_Anchor)]
    [DisallowMultipleComponent]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARReferencePoint) + ".html")]
    [Obsolete("ARReferencePoint has been deprecated. Use ARAnchor instead (UnityUpgradable) -> UnityEngine.XR.ARFoundation.ARAnchor", true)]
    public sealed class ARReferencePoint : ARTrackable<XRReferencePoint, ARReferencePoint>
    {
        /// <summary>
        /// Get the native pointer associated with this <see cref="ARReferencePoint"/>.
        /// </summary>
        /// <remarks>
        /// The data that this pointer points to is implementation defined. While its
        /// lifetime is also implementation defined, it should be valid until at least
        /// the next <see cref="ARSession"/> update.
        /// </remarks>
        public IntPtr nativePtr { get { return sessionRelativeData.nativePtr; } }

        /// <summary>
        /// Get the session identifier from which this reference point originated.
        /// </summary>
        public Guid sessionId { get { return sessionRelativeData.sessionId; } }
    }
}
