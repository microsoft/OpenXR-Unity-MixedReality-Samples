---
uid: arfoundation-changelog
---
# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [4.2.6] - 2022-09-11

### Fixed

- Fixed an issue where both `ARRaycastManager` and `ARPointCloudManager` could attempt to copy invalid `NativeArray<T>`s when using the raycast fallback API.
- Fixed [issue 960](https://github.com/Unity-Technologies/arfoundation-samples/issues/960) where `ARPointCloudManager.Append` could incorrectly throw an `ArgumentException`. For developers with custom implementations of `XRRaycastSubsystem.Provider` which do not support raycasting with an arbitrary ray, fallback raycasts including hits from point clouds will now correctly return all values.
- Fixed [issue 963](https://github.com/Unity-Technologies/arfoundation-samples/issues/963) where `ARRaycastManager.RaycastFallback` would sometimes return incomplete results. Developers with custom implementations of `XRRaycastSubsystem.Provider` which do not support raycasting with an arbitrary ray will now see correct results from `ARRaycastManager.Raycast(Ray, TrackableType, Allocator)`.

## [4.2.3] - 2022-03-01

### Fixed

- Fixed [issue 1392753](https://issuetracker.unity3d.com/issues/flickering-slash-frozen-textures-when-ar-occlusion-manager-is-disabled) where occlusion texture would remain frozen instead of being cleared when the occlusion manager is disabled.

## [4.2.2] - 2021-12-09

### Fixed

- Fixed an issue where creating an [ARSessionOrigin](xref:UnityEngine.XR.ARFoundation.ARSessionOrigin) would not tag the newly created camera as "MainCamera"
- [ARMeshManager](xref:UnityEngine.XR.ARFoundation.ARMeshManager) no longer throws `ArgumentNullException`s ("Value cannot be null") when exiting play mode in the editor.
- Fixed [Issue 1353859](https://issuetracker.unity3d.com/issues/ar-each-time-an-arface-is-added-three-new-gameobjects-are-created-in-the-scene-and-never-destroyed): During face tracking, each `ARFace` may create three additional GameObjects to represent the left eye, right eye, and fixation point. Previously, this would also and incorreclty create three additional, superfluous GameObjects at the origin. These additional GameObjects are no longer created.
- Fixed [issue 878](https://github.com/Unity-Technologies/arfoundation-samples/issues/878) where `XROcclusionSubsystem` would use `Provider.environmentDepthCpuImageApi` instead of `Provider.environmentDepthConfidenceCpuImageApi` when acquiring the Environment Depth Confidence CPU Image.

## [4.2.1] - 2021-10-06

### Fixed

- Fixed a missing dependency on built-in [particle system](https://docs.unity3d.com/2021.2/Documentation/ScriptReference/UnityEngine.ParticleSystemModule.html) module.

## [4.2.0] - 2021-08-11

No changes

## [4.2.0-pre.12] - 2021-07-08

### Added

- Added support new [XRMeshSubsystem](xref:UnityEngine.XR.XRMeshSubsystem) interface available in 2021.2, which allows providers to specify a separate transform for each mesh.
- Added methods to get the [raw](xref:UnityEngine.XR.ARFoundation.AROcclusionManager.TryAcquireRawEnvironmentDepthCpuImage(UnityEngine.XR.ARSubsystems.XRCpuImage@)) and [smoothed](xref:UnityEngine.XR.ARFoundation.AROcclusionManager.TryAcquireSmoothedEnvironmentDepthCpuImage(UnityEngine.XR.ARSubsystems.XRCpuImage@)) depth images independently.

### Fixed

- Fixed a `NullReferenceException` when accessing mesh information from an [ARFace](xref:UnityEngine.XR.ARFoundation.ARFace) in the Editor. The exception was thrown from ``Unity.Collections.NativeArray`1[T].CheckElementReadAccess``.

## [4.2.0-pre.10] - 2021-06-28

No changes

## [4.2.0-pre.9] - 2021-05-27

No changes

## [4.2.0-pre.8] - 2021-05-20

### Changed

- [Environment depth temporal smoothing](xref:UnityEngine.XR.ARFoundation.AROcclusionManager.environmentDepthTemporalSmoothingRequested) now defaults to true, which is more consistent with the behavior prior to the introduction of this option.

## [4.2.0-pre.7] - 2021-05-13

### Added

- Add support for temporal smoothing of the environment depth image. See [AROcclusionManager.environmentDepthTemporalSmoothingRequested](xref:UnityEngine.XR.ARFoundation.AROcclusionManager.environmentDepthTemporalSmoothingRequested).

### Fixed

- Fixed incorrect validation in [ARMeshManager](xref:UnityEngine.XR.ARFoundation.ARMeshManager) which prevented its use in a prefab.

## [4.2.0-pre.5] - 2021-04-07

### Fixed

- Fixed [warning messages regarding cameraDepthTarget when using URP rendering](https://issuetracker.unity3d.com/issues/urp-ar-foundation-warning-message-regarding-cameradepthtarget-with-urp-slash-ar-foundation-projects) issue.
- Fixed [camera background doesn't work when stacking cameras with URP rendering](https://issuetracker.unity3d.com/issues/arfoundation-arkit-ios-urp-arbackgroundrenderreature-doesnt-work-correctly-when-stacking-overlay-cameras) issue.
- Fixed [enabling depth texture causes the camera to render black](https://issuetracker.unity3d.com/issues/ios-ar-foundation-urp-enabling-depth-texture-causes-the-camera-to-render-black) issue.
- Fixed an issue which caused [black reflections on iOS when using AREnvironmentProbeManager](https://github.com/Unity-Technologies/arfoundation-samples/issues/749).

## [4.2.0-pre.4] - 2021-03-19

### Fixed

- Clarified documentation for [ARCameraManager.TryGetInstrincis](xref:UnityEngine.XR.ARFoundation.ARCameraManager.TryGetIntrinsics(UnityEngine.XR.ARSubsystems.XRCameraIntrinsics@)).

## [4.2.0-pre.3] - 2021-03-15

No changes

## [4.2.0-pre.2] - 2021-03-03

### Added

- Added options to the [ARPlaneMeshVisualizer component](xref:UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer) to control when MeshRenderer and LineRenderer components should be enabled. See
  - [ARPlaneMeshVisualizer.trackingStateVisibilityThreshold](xref:UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer.trackingStateVisibilityThreshold)
  - [ARPlaneMeshVisualizer.hideSubsumed](xref:UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer.hideSubsumed)

### Changed

- Added description for occlusion on the main AR Foundation manual page, and edit the human segmentations description.
- The [ARSession](xref:UnityEngine.XR.ARFoundation.ARSession) optionally sets the application's `vSyncCount` and `targetFrameRate` (see [matchFrameRateRequested](xref:UnityEngine.XR.ARFoundation.ARSession.matchFrameRateRequested)). In earlier versions, the original values were not restored when the ARSession was disabled. The behavior has changed so that the original values are restored when the ARSession disabled if the frame rate was set while the ARSession was enabled.
- Updated [XR Plug-in Management](https://docs.unity3d.com/Packages/com.unity.xr.management@4.0) dependency to 4.0.
- The minimum Unity version for this package is now 2020.3.

### Fixed

- Added missing icon to the [AROcclusionManager](xref:UnityEngine.XR.ARFoundation.AROcclusionManager).
- Replaced uses of [PostProcessScene](xref:UnityEditor.Callbacks.PostProcessSceneAttribute) attribute with [IProcessSceneWithReport](xref:UnityEditor.Build.IProcessSceneWithReport) callback mechanism. This prevents scene hashes from changing when adding the AR Foundation package.
- Added `MODULE_URP_10_OR_NEWER` and updated use of obsolete `ScriptableRenderer.cameraDepth` API to fix issues related to API upgrades that occur in builds with URP 10.0 and above

## [4.1.3] - 2021-01-05

### Changed

- The [ARCameraBackground](xref:UnityEngine.XR.ARFoundation.ARCameraBackground) component now sets the camera's [field of view](xref:UnityEngine.Camera.fieldOfView). Because the `ARCameraBackground` already overrides the camera's [projection matrix](xref:UnityEngine.Camera.projectionMatrix), this has no effect on ARFoundation. However, any code that reads the camera's [fieldOfView](xref:UnityEngine.Camera.fieldOfView) property will now read the correct value.
- Minor documentation formatting fixes.
- Update the inspector for the ARSession component to include a help box describing the function of the [Match Frame Rate](xref:UnityEngine.XR.ARFoundation.ARSession.matchFrameRateRequested) option.

## [4.1.1] - 2020-11-11

### Changed

- Released package for Unity 2021.1

## [4.1.0-preview.13] - 2020-11-09

### Fixed

- Remove fix made in 4.1.0-preview.10 for URP 10+ rendering. URP 10 now has a proper fix for this issue.

## [4.1.0-preview.12] - 2020-11-02

### Fixed

- Trackable managers no longer attempt to query subsystems for trackable changes when the subsystems are stopped. This fixes the console error that will occur with MagicLeap:
<pre>
InvalidOperationException: Can't call "GetChanges" without "Start"ing the anchor subsystem!
</pre>

## [4.1.0-preview.11] - 2020-10-22

### Added

- Added a new extension method [ScheduleAddImageWithValidationJob](xref:UnityEngine.XR.ARFoundation.MutableRuntimeReferenceImageLibraryExtensions.ScheduleAddImageWithValidationJob(UnityEngine.XR.ARSubsystems.MutableRuntimeReferenceImageLibrary,UnityEngine.Texture2D,System.String,System.Nullable{System.Single},Unity.Jobs.JobHandle)) for the [MutableRuntimeReferenceImageLibrary](xref:UnityEngine.XR.ARSubsystems.MutableRuntimeReferenceImageLibrary) which adds new reference images to a reference library only after validating that the new reference image is suitable for image tracking. The new method returns a new type [AddReferenceImageJobState](xref:UnityEngine.XR.ARSubsystems.AddReferenceImageJobState) which you can use to determine whether the image was successfully added.

### Changed

- **Anchors:** Previously, [ARAnchors](xref:UnityEngine.XR.ARFoundation.ARAnchor) were added and removed by calling [AddAnchor](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.AddAnchor(UnityEngine.Pose)) and [RemoveAnchor](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.RemoveAnchor(UnityEngine.XR.ARFoundation.ARAnchor)), respectively. Now, you can add an anchor by simply adding an `ARAnchor` component to any GameObject, e.g., by calling [AddComponent](xref:UnityEngine.GameObject.AddComponent(System.Type)). The GameObject may exist anywhere in your scene hierarchy; it need not be under the [ARSessionOrigin](xref:UnityEngine.XR.ARFoundation.ARSessionOrigin). Similarly, remove an anchor by [destroying](xref:UnityEngine.Object.Destroy(UnityEngine.Object)) the `ARAnchor` component (or its GameObject). See the [manual entry for anchors](xref:arfoundation-anchor-manager) for more details.
- **Environment Probes:** Previously, [environment probes](xref:UnityEngine.XR.ARFoundation.AREnvironmentProbe) were added and removed by calling [AddEnvironmentProbe](xref:UnityEngine.XR.ARFoundation.AREnvironmentProbeManager.AddEnvironmentProbe(UnityEngine.Pose,UnityEngine.Vector3,UnityEngine.Vector3)) and [RemoveEnvironmentProbe](xref:UnityEngine.XR.ARFoundation.AREnvironmentProbeManager.RemoveEnvironmentProbe(UnityEngine.XR.ARFoundation.AREnvironmentProbe)), respectively. Now, you can add an anchor by simply adding an `AREnvironmentProbe` component to any GameObject, e.g., by calling [AddComponent](xref:UnityEngine.GameObject.AddComponent(System.Type)). The GameObject may exist anywhere in your scene hierarchy; it need not be under the [ARSessionOrigin](xref:UnityEngine.XR.ARFoundation.ARSessionOrigin). Similarly, remove an environment probe by [destroying](xref:UnityEngine.Object.Destroy(UnityEngine.Object)) the `AREnvironmentProbe` component (or its GameObject). See the [manual entry for environment probes](xref:arfoundation-environment-probe-manager) for more details.

### Fixed

- Fixed in an incorrect `Debug.Assert` regarding a texture descriptor changing from `None` to something other than `None`. A typical error message (only seen in Development Builds) was
<pre>
Texture descriptor dimension should not change from None to Tex2D.
</pre>
- Fix a null reference exception in the occlusion manager when setting the environment depth mode or occlusion preference mode when the device does not support the occlusion subsystem.

## [4.1.0-preview.10] - 2020-10-12

### Changed

- Changed internal uses of [Debug.Assert](xref:UnityEngine.Debug.Assert(System.Boolean,System.Object)) to a custom version that does not allocate the message string unless the assert fails. This reduces per-frame GC allocations when `UNITY_ASSERTIONS` are enabled (typically in development builds).
- Update [XR Plug-in Management](https://docs.unity3d.com/Packages/com.unity.xr.management@3.2) to 3.2.16.

### Fixed

- Minor documentation fixes.
- Fix an issue with background rendering with URP 10+ rendering with a Final Blit pass.

## [4.1.0-preview.9] - 2020-09-25

### Added

- Added new sections in the manual documentation for [image tracking](xref:arfoundation-tracked-image-manager) and [anchors](xref:arfoundation-anchor-manager).
- Added a [trackable](xref:UnityEngine.XR.ARFoundation.ARRaycastHit.trackable) property to the [ARRaycastHit](xref:UnityEngine.XR.ARFoundation.ARRaycastHit) which will refer to the [ARTrackable](xref:UnityEngine.XR.ARFoundation.ARTrackable) hit by the raycast, or `null` if no trackable was hit.

### Fixed

- Fix several documentation links.
- Fix [AROcclusionManager](xref:UnityEngine.XR.ARFoundation.AROcclusionManager)'s handling of depth when non-unit scale is applied.

## [4.1.0-preview.7] - 2020-08-26

### Changed

- Added a warning when an `ARSession` is used in a scene (or scenes), but no providers are selected in [XR Plug-in Management](https://docs.unity3d.com/Packages/com.unity.xr.management@3.2) for the current build platform.

### Fixed

- Updated "[Copying the Camera Texture to a Render Texture when accessing the camera image on the GPU](xref:arfoundation-manual#copying-the-camera-texture-to-a-render-texture-when-accessing-the-camera-image-on-the-gpu)" documentation to use a '[Command Buffer](xref:UnityEngine.Rendering.CommandBuffer)' instead of a '[Graphics.Blit()](xref:uid: UnityEngine.Graphics.Blit(UnityEngine.Texture,UnityEngine.RenderTexture))' to fix an issue where blit would not work on certain devices.
- Fixed a `NullReferenceException` which would happen if you invoked [`ARSession.CheckAvailability()`](xref:UnityEngine.XR.ARFoundation.ARSession.CheckAvailability) when the ARSession's GameObject was disabled and had never been enabled.

## [4.1.0-preview.6] - 2020-07-27

No changes

## [4.1.0-preview.5] - 2020-07-16

### Changed

- Updated ["Configuring the AR Camera background using a Scriptable Render Pipeline"](xref:arfoundation-ar-camera-background-with-scriptable-render-pipeline) documentation for further clarity on setup steps.
- Added documentation for the `ARMeshManager`.

### Fixed

- The choice of whether to use the legacy or Universal render pipeline was based on an incorrect GraphicsSettings parameter (`renderPipelineAsset` instead of `currentRenderPipeline`). This meant that certain quality settings may not have been respected properly.
- Fixed a bug where it was possible for the ARSession component to set the [Application.targetFrameRate](xref:UnityEngine.Application.targetFrameRate) even when [`matchFrameRateRequested`](xref:UnityEngine.XR.ARFoundation.ARSession.matchFrameRateRequested) was `false`.

## [4.1.0-preview.3] - 2020-07-09

### Changed

- Update [XR Plug-in Management](https://docs.unity3d.com/Packages/com.unity.xr.management@3.2) to 3.2.13.

### Fixed

- Minor documentation fixes.

## [4.1.0-preview.2] - 2020-06-24

### Changed

- Updating the documentation for new package versions.

## [4.1.0-preview.1] - 2020-06-23

### Added

- Add support for environment depth through the AROcclusionManager/XROcclusionSubsystem.

### Changed

- Minimum Unity version for this package is now 2019.4.

## [4.0.2] - 2020-05-29

### Added

- The `ARCameraManager` now invokes `XRCameraSubsystem.OnBeforeBackgroundRender` immediately before rendering the camera background.
- Added a helper utility, `LoaderUtility`, for interacting with `XR Management` and added a section to the [migration guide](xref:arfoundation-migration-guide-3#xr-plug-in-management) explaining how to use it.

### Changed

- Changed `XRCameraImage` to `XRCpuImage` along with APIs that used this type (e.g., `ARCameraManager.TryGetLatestImage`). See the [migration guide](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.0/manual/migration-guide-3.html#xrcameraimage-is-now-xrcpuimage) for more details.
- The `ARMeshManager` no longer creates and destroys an [`XRMeshSubsystem`](https://docs.unity3d.com/ScriptReference/XR.XRMeshSubsystem.html). Instead, it relies on [XR Management](https://docs.unity3d.com/Packages/com.unity.xr.management@3.2/manual/index.html) to create and destroy the subsystem. The `ARMeshManager` still starts and stops it.

### Fixed

- Fixed a bug which could throw an `ArgumentNullException`, typically on application shutdown:
    ```
    ArgumentNullException: Value cannot be null.
    Parameter name: _unity_self
    ```

## [4.0.0-preview.3] - 2020-05-04

### Added

- Add support for tracked raycasts. A tracked raycast is repeated and updated automatically. See [ARRaycastManager.AddRaycast](xref:UnityEngine.XR.ARFoundation.ARRaycastManager.AddRaycast*).
- `ARCameraFrameReceivedEventArgs` now provides a camera grain texture along with an associated intensity value which can be used to add image noise characteristics to virtual content. The usage of these properties may vary by platform, so please consult the documentation of the specific provider plugin when using this feature.

### Changed

- Updating dependency on com.unity.xr.management to 3.2.10.

### Fixed

- Fixed all broken or missing scripting API documentation.

## [4.0.0-preview.1] - 2020-02-26

### Added

- Improved performance of face mesh updates by using the [new mesh APIs in 2019.3 which accept NativeArrays](xref:UnityEngine.Mesh.SetVertices(Unity.Collections.NativeArray`1<T>,System.Int32,System.Int32)). This avoids a copy of the `NativeArray` to `List`.

### Changed

- See the [Migration Guide](xref:arfoundation-migration-guide-3).
- AR Foundation now relies on [XR Management](https://docs.unity3d.com/Packages/com.unity.xr.management@3.2/manual/index.html) to initialize subsystems. If your project's configuration does not enable an XR Loader appropriate for your target platforms then the underlying subsystems AR Foundation depends on will not be available. Previously AR Foundation would attempt to initialize subsystems itself in the absence of a valid XR Management configuration.

## [3.1.3] - 2020-04-13

### Fixed

- The documentation for the `ARFace` scripting API for accessing left eye, right eye, and fixation point incorrectly referred to a nullable value type, when in fact the returned type is a [Transform](xref:UnityEngine.Transform). This has been fixed.
- Eliminating shader compiler errors that started with Unity 2020.1 and that originate from the ARKit package.

## [3.1.0-preview.8] - 2020-03-12

No changes

## [3.1.0-preview.7] - 2020-03-03

### Fixed

- Patched a memory leak by destroying the camera textures when the camera manager is disabled.

## [3.1.0-preview.6] - 2020-01-21

### Fixed

- Fixed memory leak when accessing the `humanStencilTexture` and `humanDepthTexture` properties of the `AROcclusionManager`.

## [3.1.0-preview.4] - 2020-01-16

### Added

- Updated documentation.

## [3.1.0-preview.3] - 2019-12-20

### Added

- Added support for HDR light estimation to ARCameraManager.
- Added script updater support for facilitating the rename of Reference Points to Anchors

### Fixed

- Fix `HelpURL`s on `MonoBehaviour`s to point to the 3.1 version of the documentation.
- Added `FormerlySerializedAs` to `ARAnchorManager.m_AnchorPrefab` field to preserve any prefab specified when it was the `ARReferencePointManager`.
- Fixed issue that broke the camera background rendering when URP post-processing was enabled on the AR camera.

## [3.1.0-preview.1] - 2019-11-21

### Added

- Added the new [AROcclusionManager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@3.1/api/UnityEngine.XR.ARFoundation.AROcclusionManager.html) with human segmentation functionality (on some iOS hardware).

## [3.0.1] - 2019-11-18

### Changed

- Renaming the concept of `Reference Points` to `Anchors`. The Unity script updater should convert any references to `ARReferencePointManager`, `ARReferencePoint`, and `ARReferencePointsChangedEventArgs` the next time the project is loaded into the Editor.

### Fixed

- Removed references to LWRP from the documentation.

## [3.0.0-preview.6] - 2019-11-14

### Changed

- The following virtual methods on the `ARCameraBackground` have been removed:
  - `ConfigureLegacyRenderPipelineBackgroundRendering`
  - `TeardownLegacyRenderPipelineBackgroundRendering`

  Previously, `ConfigureLegacyRenderPipelineBackgroundRendering` configured a `CommandBuffer` and added the command buffer to the camera. If you had overriden this method previously, you can override the `legacyCameraEvents` property to specify the camera events which use the CommandBuffer, and `ConfigureLegacyCommandBuffer(CommandBuffer)` to configure the command buffer. There is no need to override `TeardownLegacyRenderPipelineBackgroundRendering` anymore because the `ARCameraBackground` remembers which `CameraEvent`s the buffer was added to so it can remove them later.

### Fixed

- Fixed an issue which could cause the background camera texture to stop functioning correctly on certain devices running OpenGLES3.
- Previously, when using LWRP or UniversalRP, the requested culling mode was ignored. This resulted in an incorrect culling mode when using the front facing camera with ARCore (e.g., during face detection). This has been fixed.

## [3.0.0-preview.4] - 2019-10-22

### Added

- Querying the [`ARCameraManager`'s `focusMode`](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@3.1/api/UnityEngine.XR.ARFoundation.ARCameraManager.html#UnityEngine_XR_ARFoundation_ARCameraManager_focusMode) will return the actual camera focus value rather than a cached value.
- Added `classification` property on `ARPlane` which may return contextual information about planes. See [PlaneClassification](https://docs.unity3d.com/Packages/com.unity.xr.arsubsystems@3.1/api/UnityEngine.XR.ARSubsystems.PlaneClassification.html) for the list of all possible classifications.
- Added `supportsClassification` property to [XRPlaneSubsystemDescriptor](https://docs.unity3d.com/Packages/com.unity.xr.arsubsystems@3.1/api/UnityEngine.XR.ARSubsystems.XRPlaneSubsystemDescriptor.html).
- Add getter for the current AR frame rate to the `ARSession`. See [`ARSession.frameRate`](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@3.1/api/UnityEngine.XR.ARFoundation.ARSession.html#UnityEngine_XR_ARFoundation_ARSession_frameRate).
- [`ARFace.supportedFaceCount`](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@3.1/api/UnityEngine.XR.ARFoundation.ARFaceManager.html#UnityEngine_XR_ARFoundation_ARFaceManager_supportedFaceCount) would throw if face tracking was not supported. Now it returns 0.

### Fixed

- Fixed an issue with the `ARSessionOrigin` which could produce incorrect behavior with non-identity transforms.
- [`ARPlane.center`](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@3.1/api/UnityEngine.XR.ARFoundation.ARPlane.html#UnityEngine_XR_ARFoundation_ARPlane_center) was computed incorrectly (though the result was close to the correct answer). This has been fixed.

## [3.0.0-preview.3] - 2019-09-26

### Added

- If ["match frame rate"](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@3.1/api/UnityEngine.XR.ARFoundation.ARSession.html#UnityEngine_XR_ARFoundation_ARSession_matchFrameRate) is enabled, the `ARSession` now checks for changes to the recommended frame rate each frame.

### Changed

- Some properties on `ARPointCloud` changed from `NativeArray`s to nullable `NativeSlice`s. See the [migration guide](xref:arfoundation-migration-guide-2#point-clouds) for more details.
- The `ARFaceManager.supported` property has been removed. If face tracking is not supported, the manager's subsystem will be null. This was done for consistency as no other manager has this property. If a manager's subsystem is null after enabling the manager, that generally means the subsystem is not supported.

### Fixed

- Fixed a typo in [Face Tracking](xref:arfoundation-face-manager) documentation which incorrectly referred to planes instead of faces. Also updated the screenshot of the ARFaceManager to reflect the "Maximum Face Count" field.

## [3.0.0-preview.2] - 2019-09-05

### Added

- Converted dependency on Legacy Input Helpers package to optional dependency.  ARFoundation will continue to support the use of [TrackedPoseDriver](https://docs.unity3d.com/2018.3/Documentation/ScriptReference/SpatialTracking.TrackedPoseDriver.html) and the `UnityEngine.SpatialTracking` namespace but there is no longer a dependency on that package.  ARFoundation can instead use the `ARTrackedPoseDriver` self-contained component to surface the device's pose.

### Fixed

- Fix issue where having the camera clearFlags property set to Skybox would break background rendering.
- Fix issue where screen would flash green before the first camera frame was received.

## [3.0.0-preview.1] - 2019-08-21

### Added

- Added support for eye tracking.
- Added support for [XRParticipantSubsystem](https://docs.unity3d.com/Packages/com.unity.xr.arsubsystems@3.1/manual/participant-subsystem.html), which can track other users in a multi-user collaborative session.
- Added support for exposure duration
- Added support for exposure offset
- Add support for Lightweight Render Pipeline and Universal Render Pipeline.
- Add support for height scale estimatation for the 3D human body subsystem.

### Added

- Update the Legacy Input Helpers package to 1.3.7. This has no impact on existing functionality in ARFoundation, but should help with compatibility when using other packages.
- Add `IEquatable` to `TrackableCollection`.
- Previously, the `ARTrackedImageManager` and `ARTrackedObjectManager` would throw an exception if enabled without a valid reference library. These managers no longer throw; now, they disable themselves. This makes it possible to add one of these managers at runtime, since Unity will automatically invoke `OnEnable` before you have a chance to set a valid reference library. If you create one of these managers at runtime, you will need to set a valid reference library and then re-enable it.

### Fixed

- Fixed the issue where native providers were not being reinstantiated after being destroyed when loading/reloading a new scene.

## [2.2.0-preview.3] - 2019-07-16

### Added

- Added support for [XR Management](https://docs.unity3d.com/Packages/com.unity.xr.management@2.0/manual/index.html).
- Add support for `ARSession.notTrackingReason`.
- Add an option to synchronize the Unity frame with the AR session's update. See `ARSession.matchFrameRate`.
- Add an `ARMeshManager` to interface with the [`XRMeshSubsystem`](https://docs.unity3d.com/ScriptReference/Experimental.XR.XRMeshSubsystem.html). This is useful for XR Plugins that can generate meshes at runtime.

### Changed

- Previously, a trackable manager's changed event (e.g., `ARPlaneSubsystem.planesChanged`) was invoked every frame even if nothing had changed. Now, it is only invoked on frames when a change has occurred.

## [2.2.0-preview.2] - 2019-06-05

### Added

- Adding support for ARKit 3 functionality: Human pose estimation, human segmentation images, session collaboration, multiple face tracking, and tracking a face (with front camera) while in world tracking (with rear camera).

## [2.1.0-preview.4] - 2019-06-03

### Added

- Added a convenience method `ARTrackableManager.SetTrackablesActive(bool)` which will call `SetActive` on each trackable's `GameObject`. This is useful, for example, to disable all `ARPlane`s after disabling the `ARPlaneManager`.
- Fix NativeArray handling when `ENABLE_UNITY_COLLECTIONS_CHECKS` is set. This fixes an issue when running ARFoundation in the Editor. This is not set in the Player, so it is not an issue when running on device.

## [2.1.0-preview.3] - 2019-05-22

### Fixed

- Fix documentation links.

## [2.1.0-preview.2] - 2019-05-07

### Added

- Add support for image tracking (`XRImageTrackingSubsystem`).
- Add support for environment probes (`XREnvironmentProbeSubsystem`).
- Add support for face tracking (`XRFaceSubsystem`).
- Add support for object tracking (`XRObjectTrackingSubsystem`).

## [2.0.1] - 2019-03-05

### Changed

- See the [Migration Guide](xref:arfoundation-migration-guide-1).

### Fixed

- Add dependency on Legacy Input Helpers, which were moved from Unity to a package in 2019.1.

## [1.0.0-preview.22] - 2018-12-13

### Fixed

- Fix package dependency

## [1.0.0-preview.21] - 2018-12-13

### Added

- Added Face Tracking support:  `ARFaceManager`, `ARFace`, `ARFaceMeshVisualizer` and related scripts. See documentation for usage.

### Added

- Plane detection modes: Add ability to selectively enable detection for horizontal, vertical, or both types of planes. The `ARPlaneManager` includes a new setting, which defaults to both.
- Add support for setting the camera focus mode. Added a new component, `ARCameraOptions`, which lets you set the focus mode.

### Changed

- The light estimation option, which was previously on the `ARSession` component, has been moved to the new `ARCameraOptions` component.

## [1.0.0-preview.20] - 2018-11-06

### Added

- Support reference points reported without a corresponding `TryAddReferencePoint` call. This handles reference points that are added by some other means, e.g., loaded from a serialized session.

## [1.0.0-preview.19] - 2018-10-10

### Added

- Added support for `XRCameraExtensions` API to get the raw camera image data on the CPU. See the [manual documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/manual/cpu-camera-image.html) for more information.

## [1.0.0-preview.18] - 2018-09-13

### Added

- Added `ARPlane.normal` to get the `ARPlane`'s normal in world space.

### Added

- Added LWRP support by allowing `ARCameraBackground` to use a background renderer that overrides the default functionality.  This works in conjunction with some *LWRPSupport* files (see [arfoundation-samples](https://github.com/Unity-Technologies/arfoundation-samples)) that will live in your LWRP project.

### Changed

- The `ARPlaneManager`, `ARPointCloudManager`, and `ARReferencePointManager` all instantiate either prefabs that you specify or `GameObject`s with at least an `ARPlane`, `ARPointCloud`, or `ARReferencePoint`, respectively, on them. The instantiated `GameObject`'s layer was set to match the `ARSessionOrigin`, overwriting the layer specified in the prefab. This has been changed so that if a prefab is specified, no changes to the layer are made. If no prefab is specified, then a new `GameObject` is created, and its layer will match that of the `ARSessionOrigin`'s `GameObject`.

### Fixed

- The `ARPlaneMeshVisualizer` did not disable its visible components (`MeshRenderer` and `LineRenderer`) when disabled. This has been fixed.

## [1.0.0-preview.17] - 2018-08-02

### Fixed

- Add `FormerlySerializedAs` attribute to serialized field changes to `ARCameraBackground`.

## [1.0.0-preview.16] - 2018-07-26

### Changed

- Removed static constructor from `ARSubsystemManager`. This allows access to the manager without forcing the creation of the subsystems, making the initialization more flexible.
- The API for overriding the material has been refactored. Previously, a custom material could be set with the `ARCameraBackground.material` setter, but this might be overwritten later if the option to override was disabled.
- Rename: `overrideMaterial` is now `useCustomMaterial`
- New member: `customMaterial`
- The following properties are now private:
  - `material` setter (getter is still public)
  - `backgroundRenderer`
- Use the `ARCameraBackground.material` getter to get the currently active material.

### Fixed

- `ARPlane.vertexChangedThreshold` is no longer allowed to be negative.
- The `ARCameraBackground` component did not reset the projection matrix on disable, leading to stretching or other distortion. This has been fixed.
- The `ARCameraBackground` component did not properly handle an overriden material. This has been fixed (see API Changes below).
- The `ARPlaneMeshGenerators` was meant to generate a triangle fan about the center of the plane. However, indices were instead generated as a fan about one of the bounary points. The visual result is similar, but can lead to long thin triangles. This has been fixed to use the plane's center point as intended.
- Update for compatibility with 2018.3
- If the `ARPlaneMeshVisualizer` has a `LineRenderer` on it, then it will be scaled to match the `ARPlane`'s scale, making the visual width invariant under differing `ARSessionOrigin` scales.
- When the `ARPointCloudManager` instantiated a point cloud prefab, it did not change its transform. If it was not identity, then feature points would appear in unexpected places. It now instantiates the point cloud prefab with identity transform.
- The menu item "GameObject > XR > AR Default Point Cloud" created a `GameObject` which used a particle system whose "Scaling Mode" was set to "Local". If used as the `ARPointCloudManager`'s point cloud prefab, this would produce odd results when the `ARSessionOrigin` was scaled. The correct setting is "Hierarchy", and the utility now creates a particle system with the correct setting.

## [1.0.0-preview.14] - 2018-07-17

### Added

- Added `ARSession.Reset()` to destroy the current AR Session and establish a new one. This destroys all trackables, such as planes.
- Added an `ARSubsystemManager.sessionDestroyed` event. The `ARPlaneManager`, `ARPointCloudManager`, and `ARReferencePointManager` subscribe to this event, and they remove their trackables when the `ARSession` is destroyed.

### Fixed

- Fixed a bug in the `ARCameraBackground` which would not render the camera texture properly if the `ARSession` had been destroyed and then recreated or if the `ARCameraBackground` had been disabled and then re-enabled.
- `ARSubsystemManager.systemState`'s setter was not private, allowing user code to change the system state. The setter is now private.
- `ARPlane.trackingState` returned a cached value, which was incorrect if the `ARSession` was no longer active.

## [1.0.0-preview.13] - 2018-07-03

### Added

- Added a getter on the `ARPointCloudManager` for the instantiated `ARPointCloud`.
- Added UVs to the `Mesh` generated by the `ARPlaneMeshVisualizer`.
- Added a `meshUpdated` event to the `ARPlaneMeshVisualizer`, which lets you customize the generated `Mesh`.
- Added AR Icons.

### Changed

- Refactored out plane mesh generation functionality into a new static class `ARPlaneMeshGenerators`.

### Fixed

- Fixed a bug where point clouds did not stop rendering when disabled.

## [1.0.0-preview.12] - 2018-06-14

### Added

- Add color correction value to `LightEstimationData`.
- Add availability and AR install support

### Changed

- Improve lifecycle reporting: remove public members `ARSubsystemManager.availability` and `ARSubsystemManager.trackingState`. Combine with `ARSubsystemManager.systemState` and the public event `ARSubsystemManager.systemStateChanged`.
- Docs improvements
- Move `ParticleSystem` to the top of the `ARDebugPointCloudVisualizer`
- Update documentation: ARSession image and written description.
- Rename `ARBackgroundRenderer` to `ARCameraBackground`
- Unify `ARSessionState` & `ARSystemState` enums
- Change dependency to `ARExtension` 1.0.0-preview.2
- Significant rework to startup lifecycle & asynchronous availability check
- Rename ARUtilities to ARFoundation
- This package is now called `ARFoundation`.
- `ARSessionOrigin` no longer requires its `Camera` to be a child of itself.

### Removed

- Remove "timeout" from AR Session
- Removed `ARPlaceOnPlane`, `ARMakeAppearOnPlane`, `ARPlaneMeshVisualizer`, and `ARPointCloudParticleVisualizer` as these were deemed sample content.
- Removed setup wizard.
- Renamed `ARRig` to `ARSessionOrigin`.

