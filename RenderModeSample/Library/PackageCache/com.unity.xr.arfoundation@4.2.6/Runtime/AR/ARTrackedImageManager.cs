using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine.Serialization;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A manager for <see cref="ARTrackedImage"/>s. Uses the <c>XRImageTrackingSubsystem</c>
    /// to recognize and track 2D images in the physical environment.
    /// </summary>
    [DefaultExecutionOrder(ARUpdateOrder.k_TrackedImageManager)]
    [RequireComponent(typeof(ARSessionOrigin))]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARTrackedImageManager) + ".html")]
    public sealed class ARTrackedImageManager : ARTrackableManager<
        XRImageTrackingSubsystem,
        XRImageTrackingSubsystemDescriptor,
        XRImageTrackingSubsystem.Provider,
        XRTrackedImage,
        ARTrackedImage>
    {
        [SerializeField]
        [FormerlySerializedAs("m_ReferenceLibrary")]
        [Tooltip("The library of images which will be detected and/or tracked in the physical environment.")]
        XRReferenceImageLibrary m_SerializedLibrary;

        /// <summary>
        /// Get or set the reference image library (that is, the set of images to search for in the physical environment).
        /// </summary>
        /// <remarks>
        /// An <c>IReferenceImageLibrary</c> can be either an <c>XRReferenceImageLibrary</c>
        /// or a <c>RuntimeReferenceImageLibrary</c>. <c>XRReferenceImageLibrary</c>s can only be
        /// constructed in the Editor and are immutable at runtime. A <c>RuntimeReferenceImageLibrary</c>
        /// is the runtime representation of a <c>XRReferenceImageLibrary</c> and can be mutable
        /// at runtime (see <c>MutableRuntimeReferenceImageLibrary</c>).
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">Thrown if the <see cref="referenceLibrary"/> is set to <c>null</c> while image tracking is enabled.</exception>
        public IReferenceImageLibrary referenceLibrary
        {
            get
            {
                if (subsystem != null)
                {
                    return subsystem.imageLibrary;
                }
                else
                {
                    return m_SerializedLibrary;
                }
            }

            set
            {
                if (value == null && subsystem != null && subsystem.running)
                    throw new InvalidOperationException("Cannot set a null reference library while image tracking is enabled.");

                if (value is XRReferenceImageLibrary serializedLibrary)
                {
                    m_SerializedLibrary = serializedLibrary;
                    if (subsystem != null)
                        subsystem.imageLibrary = subsystem.CreateRuntimeLibrary(serializedLibrary);
                }
                else if (value is RuntimeReferenceImageLibrary runtimeLibrary)
                {
                    m_SerializedLibrary = null;
                    EnsureSubsystemInstanceSet();

                    if (subsystem != null)
                        subsystem.imageLibrary = runtimeLibrary;
                }

                if (subsystem != null)
                    UpdateReferenceImages(subsystem.imageLibrary);
            }
        }

        /// <summary>
        /// Creates a <c>UnityEngine.XR.ARSubsystems.RuntimeReferenceImageLibrary</c> from an existing
        /// <c>UnityEngine.XR.ARSubsystems.XRReferenceImageLibrary</c>
        /// or an empty library if <paramref name="serializedLibrary"/> is <c>null</c>.
        /// Use this to construct reference image libraries at runtime. If the library is of type
        /// <c>MutableRuntimeReferenceImageLibrary</c>, it is modifiable at runtime.
        /// </summary>
        /// <param name="serializedLibrary">An existing <c>XRReferenceImageLibrary</c>, or <c>null</c> to create an empty mutable image library.</param>
        /// <returns>A new <c>RuntimeReferenceImageLibrary</c> representing the deserialized version of <paramref name="serializedLibrary"/> or an empty library if <paramref name="serializedLibrary"/> is <c>null</c>.</returns>
        /// <exception cref="System.NotSupportedException">Thrown if there is no subsystem. This usually means image tracking is not supported.</exception>
        public RuntimeReferenceImageLibrary CreateRuntimeLibrary(XRReferenceImageLibrary serializedLibrary = null)
        {
            EnsureSubsystemInstanceSet();

            if (subsystem == null)
                throw new NotSupportedException("No image tracking subsystem found. This usually means image tracking is not supported.");

            return subsystem.CreateRuntimeLibrary(serializedLibrary);
        }

        [SerializeField]
        [Tooltip("The maximum number of moving images to track in realtime. Not all implementations support this feature.")]
        int m_MaxNumberOfMovingImages;

        /// <summary>
        /// The maximum number of moving images to track in real time.
        /// This property is obsolete.
        /// Use <see cref="requestedMaxNumberOfMovingImages"/>
        /// or  <see cref="currentMaxNumberOfMovingImages"/> instead.
        /// </summary>
        [Obsolete("Use requestedMaxNumberOfMovingImages or currentMaxNumberOfMovingImages instead. (2020-01-16)")]
        public int maxNumberOfMovingImages
        {
            get => m_MaxNumberOfMovingImages;
            set => requestedMaxNumberOfMovingImages = value;
        }

        bool supportsMovingImages => descriptor?.supportsMovingImages == true;

        /// <summary>
        /// The requested maximum number of moving images to track in real time. Support can vary between devices and providers. Check
        /// for support at runtime with <see cref="SubsystemLifecycleManager{TSubsystem,TSubsystemDescriptor,TProvider}.descriptor"/>'s
        /// `supportsMovingImages` property.
        /// </summary>
        public int requestedMaxNumberOfMovingImages
        {
            get => supportsMovingImages ? subsystem.requestedMaxNumberOfMovingImages : m_MaxNumberOfMovingImages;
            set
            {
                m_MaxNumberOfMovingImages = value;
                 if (enabled && (descriptor?.supportsMovingImages == true))
                {
                    subsystem.requestedMaxNumberOfMovingImages = value;
                }
            }
        }

        /// <summary>
        /// Get the maximum number of moving images to track in real time that is currently in use by the subsystem.
        /// </summary>
        public int currentMaxNumberOfMovingImages => supportsMovingImages ? subsystem.currentMaxNumberOfMovingImages : 0;

        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each detected image.")]
        GameObject m_TrackedImagePrefab;

        /// <summary>
        /// If not null, instantiates this Prefab for each detected image.
        /// </summary>
        /// <remarks>
        /// The purpose of this property is to extend the functionality of <see cref="ARTrackedImage"/>s.
        /// It is not the recommended way to instantiate content associated with an <see cref="ARTrackedImage"/>.
        /// See [Tracked Image Prefab](xref:arfoundation-tracked-image-manager#tracked-image-prefab) for more details.
        /// </remarks>
        public GameObject trackedImagePrefab
        {
            get => m_TrackedImagePrefab;
            set => m_TrackedImagePrefab = value;
        }

        /// <summary>
        /// Get the Prefab that will be instantiated for each <see cref="ARTrackedImage"/>.
        /// </summary>
        /// <returns>The Prefab that will be instantiated for each <see cref="ARTrackedImage"/>.</returns>
        protected override GameObject GetPrefab() => m_TrackedImagePrefab;

        /// <summary>
        /// Invoked once per frame with information about the <see cref="ARTrackedImage"/>s that have changed (that is, been added, updated, or removed).
        /// This happens just before <see cref="ARTrackedImage"/>s are destroyed, so you can set <c>ARTrackedImage.destroyOnRemoval</c> to <c>false</c>
        /// from this event to suppress this behavior.
        /// </summary>
        public event Action<ARTrackedImagesChangedEventArgs> trackedImagesChanged;

        /// <summary>
        /// The name to be used for the <c>GameObject</c> whenever a new image is detected.
        /// </summary>
        protected override string gameObjectName => nameof(ARTrackedImage);

        /// <summary>
        /// Sets the image library on the subsystem before Start() is called on the <c>XRImageTrackingSubsystem</c>.
        /// </summary>
        protected override void OnBeforeStart()
        {
            if (subsystem.imageLibrary == null && m_SerializedLibrary != null)
            {
                subsystem.imageLibrary = subsystem.CreateRuntimeLibrary(m_SerializedLibrary);
                m_SerializedLibrary = null;
            }

            UpdateReferenceImages(subsystem.imageLibrary);
            if (supportsMovingImages)
            {
                subsystem.requestedMaxNumberOfMovingImages = m_MaxNumberOfMovingImages;
            }

            enabled = (subsystem.imageLibrary != null);
#if DEVELOPMENT_BUILD
            if (subsystem.imageLibrary == null)
            {
                Debug.LogWarning($"{nameof(ARTrackedImageManager)} '{name}' was enabled but no reference image library is specified. To enable, set a valid reference image library and then re-enable this component.");
            }
#endif
        }

        bool FindReferenceImage(Guid guid, out XRReferenceImage referenceImage)
        {
            if (m_ReferenceImages.TryGetValue(guid, out referenceImage))
                return true;

            // If we are using a mutable library, then it's possible an image
            // has been added that we don't yet know about, so search the library.
            if (referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
            {
                foreach (var candidateImage in mutableLibrary)
                {
                    if (candidateImage.guid.Equals(guid))
                    {
                        referenceImage = candidateImage;
                        m_ReferenceImages.Add(referenceImage.guid, referenceImage);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Invoked just after updating each <see cref="ARTrackedImage"/>. Used to update the <see cref="ARTrackedImage.referenceImage"/>.
        /// </summary>
        /// <param name="image">The tracked image being updated.</param>
        /// <param name="sessionRelativeData">New data associated with the tracked image. Spatial data is
        /// relative to the <see cref="ARSessionOrigin"/>.</param>
        protected override void OnAfterSetSessionRelativeData(
            ARTrackedImage image,
            XRTrackedImage sessionRelativeData)
        {
            if (FindReferenceImage(sessionRelativeData.sourceImageId, out XRReferenceImage referenceImage))
            {
                image.referenceImage = referenceImage;
            }
#if DEVELOPMENT_BUILD
            else
            {
                Debug.LogError($"Could not find reference image with guid {sessionRelativeData.sourceImageId}");
            }
#endif
        }

        /// <summary>
        /// Invokes the <see cref="trackedImagesChanged"/> event.
        /// </summary>
        /// <param name="added">A list of images added this frame.</param>
        /// <param name="updated">A list of images updated this frame.</param>
        /// <param name="removed">A list of images removed this frame.</param>
        protected override void OnTrackablesChanged(
            List<ARTrackedImage> added,
            List<ARTrackedImage> updated,
            List<ARTrackedImage> removed)
        {
            if (trackedImagesChanged != null)
            {
                using (new ScopedProfiler("OnTrackedImagesChanged"))
                trackedImagesChanged(
                    new ARTrackedImagesChangedEventArgs(
                        added,
                        updated,
                        removed));
            }
        }

        void UpdateReferenceImages(RuntimeReferenceImageLibrary library)
        {
            if (library == null)
                return;

            int count = library.count;
            for (int i = 0; i < count; ++i)
            {
                var referenceImage = library[i];
                m_ReferenceImages[referenceImage.guid] = referenceImage;
            }
        }

        Dictionary<Guid, XRReferenceImage> m_ReferenceImages = new Dictionary<Guid, XRReferenceImage>();
    }
}
