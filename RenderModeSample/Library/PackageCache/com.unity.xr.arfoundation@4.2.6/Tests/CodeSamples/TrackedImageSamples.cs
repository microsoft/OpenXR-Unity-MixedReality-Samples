using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    [TestFixture]
    class TrackedImageSamples
    {
        // Disable "field never assigned to"
        #pragma warning disable CS0649

        class EnumerateTrackables
        {
            #region trackedimage_subscribe_to_events
            [SerializeField]
            ARTrackedImageManager m_TrackedImageManager;

            void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;

            void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

            void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
            {
                foreach (var newImage in eventArgs.added)
                {
                    // Handle added event
                }

                foreach (var updatedImage in eventArgs.updated)
                {
                    // Handle updated event
                }

                foreach (var removedImage in eventArgs.removed)
                {
                    // Handle removed event
                }
            }
            #endregion

            #region trackedimage_enumerate_trackables
            void ListAllImages()
            {
                Debug.Log(
                    $"There are {m_TrackedImageManager.trackables.count} images being tracked.");

                foreach (var trackedImage in m_TrackedImageManager.trackables)
                {
                    Debug.Log($"Image: {trackedImage.referenceImage.name} is at " +
                              $"{trackedImage.transform.position}");
                }
            }
            #endregion

            #region trackedimage_get_by_trackableId
            ARTrackedImage GetImageAt(TrackableId trackableId)
            {
                return m_TrackedImageManager.trackables[trackableId];
            }
            #endregion
        }

        class ScheduleAddImageJob
        {
            #region trackedimage_ScheduleAddImageJob
            [SerializeField]
            ARTrackedImageManager m_TrackedImageManager;

            void AddImage(Texture2D imageToAdd)
            {
                if (m_TrackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
                {
                    mutableLibrary.ScheduleAddImageWithValidationJob(
                        imageToAdd,
                        "my new image",
                        0.5f /* 50 cm */);
                }
            }
            #endregion

            #region trackedimage_DeallocateOnJobCompletion
            struct DeallocateJob : IJob
            {
                [DeallocateOnJobCompletion]
                public NativeArray<byte> data;

                public void Execute() { }
            }

            void AddImage(NativeArray<byte> grayscaleImageBytes,
                          int widthInPixels, int heightInPixels,
                          float widthInMeters)
            {
                if (m_TrackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
                {
                    var aspectRatio = (float)widthInPixels / (float)heightInPixels;
                    var sizeInMeters = new Vector2(widthInMeters, widthInMeters * aspectRatio);
                    var referenceImage = new XRReferenceImage(
                        // Guid is assigned after image is added
                        SerializableGuid.empty,
                        // No texture associated with this reference image
                        SerializableGuid.empty,
                        sizeInMeters, "My Image", null);

                    var jobState = mutableLibrary.ScheduleAddImageWithValidationJob(
                        grayscaleImageBytes,
                        new Vector2Int(widthInPixels, heightInPixels),
                        TextureFormat.R8,
                        referenceImage);

                    // Schedule a job that deallocates the image bytes after the image
                    // is added to the reference image library.
                    new DeallocateJob { data = grayscaleImageBytes }.Schedule(jobState.jobHandle);
                }
                else
                {
                    // Cannot add the image, so dispose its memory.
                    grayscaleImageBytes.Dispose();
                }
            }
            #endregion
        }

        class CreateRuntimeLibrary
        {
            [SerializeField]
            ARTrackedImageManager m_TrackedImageManager;

            #region trackedimage_CreateRuntimeLibrary
            void AddImage(Texture2D imageToAdd)
            {
                var library = m_TrackedImageManager.CreateRuntimeLibrary();
                if (library is MutableRuntimeReferenceImageLibrary mutableLibrary)
                {
                    mutableLibrary.ScheduleAddImageWithValidationJob(
                        imageToAdd,
                        "my new image",
                        0.5f /* 50 cm */);
                }
            }
            #endregion

            #region trackedimage_supportsMutableLibrary
            bool DoesSupportMutableImageLibraries()
            {
                return m_TrackedImageManager.descriptor.supportsMutableLibrary;
            }
            #endregion
        }

        #pragma warning restore CS0649
    }
}
