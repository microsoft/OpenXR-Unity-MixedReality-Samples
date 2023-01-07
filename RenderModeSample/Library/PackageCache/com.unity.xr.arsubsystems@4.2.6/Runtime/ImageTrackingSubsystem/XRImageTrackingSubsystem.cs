using System;
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A subsystem for detecting and tracking a preconfigured set of images in the environment.
    /// </summary>
    public class XRImageTrackingSubsystem
        : TrackingSubsystem<XRTrackedImage, XRImageTrackingSubsystem, XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem.Provider>
    {
        /// <summary>
        /// Constructs a subsystem. Do not invoke directly; call <c>Create</c> on the <see cref="XRImageTrackingSubsystemDescriptor"/> instead.
        /// </summary>
        public XRImageTrackingSubsystem() { }

        /// <summary>
        /// Starts the subsystem, that is, starts detecting images in the scene. <see cref="imageLibrary"/> must not be null.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if <see cref="imageLibrary"/> is null.</exception>
        protected override void OnStart()
        {
            if (m_ImageLibrary == null)
                throw new InvalidOperationException("Cannot start image tracking without an image library.");

            provider.imageLibrary = m_ImageLibrary;

            base.OnStart();
        }

        /// <summary>
        /// Stops the subsystem, that is, stops detecting and tracking images.
        /// </summary>
        protected sealed override void OnStop()
        {
            base.OnStop();
            provider.imageLibrary = null;
        }

        /// <summary>
        /// Get or set the reference image library. This is the set of images to look for in the environment.
        /// </summary>
        /// <remarks>
        /// A <see cref="RuntimeReferenceImageLibrary"/> is created at runtime and can be modifiable
        /// (see <see cref="MutableRuntimeReferenceImageLibrary"/>). You can create a <see cref="RuntimeReferenceImageLibrary"/>
        /// from an <see cref="XRReferenceImageLibrary"/> using
        /// <see cref="CreateRuntimeLibrary(XRReferenceImageLibrary"/>.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if the subsystem has been started, and you attempt to set the image library to null.</exception>
        /// <seealso cref="OnStart()"/>
        /// <seealso cref="OnStop()"/>
        /// <seealso cref="XRReferenceImageLibrary"/>
        /// <seealso cref="MutableRuntimeReferenceImageLibrary"/>
        public RuntimeReferenceImageLibrary imageLibrary
        {
            get => m_ImageLibrary;
            set
            {
                if (m_ImageLibrary == value)
                    return;

                if (running && value == null)
                    throw new ArgumentNullException(nameof(value), "Cannot set imageLibrary to null while subsystem is running.");

                m_ImageLibrary = value;

                if (running)
                    provider.imageLibrary = m_ImageLibrary;
            }
        }

        /// <summary>
        /// Creates a <see cref="RuntimeReferenceImageLibrary"/> from an existing <see cref="XRReferenceImageLibrary"/>,
        /// or an empty library if <paramref name="serializedLibrary"/> is <c>null</c>.
        /// Use this to construct the runtime representation of an <see cref="XRReferenceImageLibrary"/>.
        /// </summary>
        /// <remarks>
        /// If the subsystem supports runtime mutable libraries
        /// (see <see cref="XRImageTrackingSubsystemDescriptor.supportsMutableLibrary"/>), then the returned
        /// library will be a <see cref="MutableRuntimeReferenceImageLibrary"/>.
        /// </remarks>
        /// <param name="serializedLibrary">An existing <see cref="XRReferenceImageLibrary"/> created at edit time, or <c>null</c> to create an empty image library.</param>
        /// <returns>A new <see cref="RuntimeReferenceImageLibrary"/> representing the deserialized version of <paramref name="serializedLibrary"/>
        /// or an empty library if <paramref name="serializedLibrary"/> is <c>null</c>.</returns>
        /// <seealso cref="RuntimeReferenceImageLibrary"/>
        public RuntimeReferenceImageLibrary CreateRuntimeLibrary(XRReferenceImageLibrary serializedLibrary)
        {
            var library = provider.CreateRuntimeLibrary(serializedLibrary);
            Assert.IsFalse(ReferenceEquals(library, null));
            return library;
        }

        /// <summary>
        /// Retrieve the changes in the state of tracked images (added, updated, and removed) since the last call to <c>GetChanges</c>.
        /// </summary>
        /// <param name="allocator">The allocator to use for the returned set of changes.</param>
        /// <returns>The set of tracked image changes (added, updated, and removed) since the last call to this method.</returns>
        public override TrackableChanges<XRTrackedImage> GetChanges(Allocator allocator)
        {
            var changes = provider.GetChanges(XRTrackedImage.defaultValue, allocator);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            m_ValidationUtility.ValidateAndDisposeIfThrown(changes);
#endif
            return changes;
        }

        /// <summary>
        /// The requested maximum number of moving images to track.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the subsystem does not support tracking moving images.
        /// Check for support of this feature with <see cref="XRImageTrackingSubsystemDescriptor.supportsMovingImages"/>.
        /// </exception>
        public int requestedMaxNumberOfMovingImages
        {
            get => provider.requestedMaxNumberOfMovingImages;
            set => provider.requestedMaxNumberOfMovingImages = value;
        }

        /// <summary>
        /// The current maximum number of moving images to track.
        /// This can be different from <see cref="requestedMaxNumberOfMovingImages"/>.
        /// </summary>
        public int currentMaxNumberOfMovingImages => provider.currentMaxNumberOfMovingImages;

        /// <summary>
        /// Methods to implement by the implementing provider.
        /// </summary>
        public abstract class Provider : SubsystemProvider<XRImageTrackingSubsystem>
        {
            /// <summary>
            /// Get the changes to the tracked images (added, updated, and removed) since the last call to this method.
            /// </summary>
            /// <param name="defaultTrackedImage">An <see cref="XRTrackedImage"/> populated with default values.
            /// The implementation should first fill arrays of added, updated, and removed with copies of this
            /// before copying in its own values. This guards against additional fields added to the <see cref="XRTrackedImage"/> in the future.</param>
            /// <param name="allocator">The allocator to use for the returned data.</param>
            /// <returns>The set of changes to tracked images (added, updated, and removed) since the last call to this method.</returns>
            public abstract TrackableChanges<XRTrackedImage> GetChanges(XRTrackedImage defaultTrackedImage, Allocator allocator);

            /// <summary>
            /// Sets the set of images to search for in the environment.
            /// </summary>
            /// <remarks>
            /// Setting this to <c>null</c> implies the subsystem should stop detecting and tracking images.
            /// </remarks>
            public abstract RuntimeReferenceImageLibrary imageLibrary { set; }

            /// <summary>
            /// Creates a <see cref="RuntimeReferenceImageLibrary"/> from an existing <see cref="XRReferenceImageLibrary"/>,
            /// or an empty library if <paramref name="serializedLibrary"/> is <c>null</c>.
            /// </summary>
            /// <param name="serializedLibrary">A <see cref="XRReferenceImageLibrary"/> to deserialize.</param>
            /// <returns>The runtime version of <paramref name="serializedLibrary"/> or an empty library if <paramref name="serializedLibrary"/> is <c>null</c>.</returns>
            public abstract RuntimeReferenceImageLibrary CreateRuntimeLibrary(XRReferenceImageLibrary serializedLibrary);

            /// <summary>
            /// The requested maximum number of moving images to track in real time.
            /// </summary>
            /// <remarks>
            /// Must be implemented if <see cref="XRImageTrackingSubsystemDescriptor.supportsMovingImages"/> is <c>true</c>;
            /// otherwise, this property will never be set and doesn't need to be implemented.
            /// </remarks>
            /// <exception cref="System.NotSupportedException">Thrown if not overridden by the derived class.</exception>
            public virtual int requestedMaxNumberOfMovingImages
            {
                get => 0;
                set => throw new NotSupportedException("This subsystem does not track moving images.");
            }

            /// <summary>
            /// The current maximum number of moving images to track in realtime.
            /// </summary>
            /// <remarks>
            /// Must be implemented if <see cref="XRImageTrackingSubsystemDescriptor.supportsMovingImages"/> is <c>true</c>.
            /// </remarks>
            public virtual int currentMaxNumberOfMovingImages => 0;
        }

        RuntimeReferenceImageLibrary m_ImageLibrary;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ValidationUtility<XRTrackedImage> m_ValidationUtility =
            new ValidationUtility<XRTrackedImage>();
#endif
    }
}
