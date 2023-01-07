using System;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Represents an Anchor tracked by an XR device.
    /// </summary>
    /// <remarks>
    /// An anchor is a pose in the physical environment that is tracked by an XR device.
    /// As the device refines its understanding of the environment, anchors will be
    /// updated, helping you to keep virtual content connected to a real-world position and orientation.
    /// </remarks>
    [DefaultExecutionOrder(ARUpdateOrder.k_Anchor)]
    [DisallowMultipleComponent]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARAnchor) + ".html")]
    public sealed class ARAnchor : ARTrackable<XRAnchor, ARAnchor>
    {
        /// <summary>
        /// Get the native pointer associated with this <see cref="ARAnchor"/>.
        /// </summary>
        /// <remarks>
        /// The data pointed to by this pointer is implementation defined. While its
        /// lifetime is also implementation defined, it should be valid until at least
        /// the next <see cref="ARSession"/> update.
        /// </remarks>
        public IntPtr nativePtr => sessionRelativeData.nativePtr;

        /// <summary>
        /// Get the session identifier from which this anchor originated.
        /// </summary>
        public Guid sessionId => sessionRelativeData.sessionId;

        void OnEnable()
        {
            if (ARAnchorManager.instance is ARAnchorManager manager)
            {
                manager.TryAddAnchor(this);
            }
            else
            {
                pending = true;
            }
        }

        void Update()
        {
            if (trackableId.Equals(TrackableId.invalidId) && ARAnchorManager.instance is ARAnchorManager manager)
            {
                manager.TryAddAnchor(this);
            }
        }

        void OnDisable()
        {
            if (ARAnchorManager.instance is ARAnchorManager manager)
            {
                manager.TryRemoveAnchor(this);
            }
        }
    }
}
