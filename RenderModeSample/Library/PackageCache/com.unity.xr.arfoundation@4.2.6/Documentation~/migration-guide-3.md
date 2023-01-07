---
uid: arfoundation-migration-guide-3
---
# Migration guide

This guide covers the differences between AR Foundation 3.x and 4.x.

## Camera and tracking node selection

In previous versions of AR Foundation, you couldn't explicitly select which camera provided the camera texture used for pass-through video, and you couldn't indicate the desired type of tracking (for example, 3 or 6 degrees of freedom). With AR Foundation 4, you can now
* Explicitly request a particular camera. AR Foundation distinguishes between "world-facing" and "user-facing" cameras. On a phone, this corresponds to the rear-facing and selfie cameras, respectively.
* Explicitly request the type of tracking.

In previous versions of AR Foundation, this was an implicit choice. For example, if you enabled face detection, you would likely get a 3dof (rotation only) session which used the user-facing camera.

ARKit 3 added modes of operation that were not possible to express with previous versions of AR Foundation. In particular, ARKit 3 supports both 3 DOF (degrees of freedom) and 6 DOF tracking when using the user-facing camera to perform face tracking, as well as a mode that uses the world-facing camera for the video feed but also uses the user-facing camera to perform face tracking.

This table represents the face tracking support AR Foundation 3 could provide:

|              | 3 DOF  | 6 DOF |
| ------       | ------ | ----- |
| User  Camera |   ✓    |       |
| World Camera |        |   ✓*  |

\* This was only available by enabling another feature which required the world-facing camera, such as plane detection.

This table represents what ARKit3 supports:

|               | 3 DOF  | 6 DOF |
| ------        | ------ | ----- |
| User Camera   |   ✓    |   ✓   |
| World Camera  |        |   ✓   |

As you can see, there was no way to use the user-facing camera with 6 dof tracking, and it was difficult to use face tracking with the world-facing camera.

Not all features are available simultaneously. For instance, on ARCore, face and plane detection are mutually exclusive features; you cannot have both at the same time. On ARKit, it depends on the hardware. However, there was no way to know what would happen if you enabled a particular combination of features. Because face tracking using the world-facing camera requires particular hardware only available on newer iOS devices, there was no way to query for support in a generic way.

### Configuration chooser

To solve this problem, we've added methods to enumerate each discrete mode of operation (called a "configuration descriptor") and list its capabilities. All capabilities in a configuration descriptor are simultaneously available on the current device.

AR Foundation 4 also introduces the concept of a "configuration chooser", which uses the current set of requested features (for example, "auto focus", "plane detection", and "face detection") and selects the best available configuration descriptor. This also allows you to query what would happen if you were to enable a particular set of features before enabling them.

The default configuration chooser selects the configuration that supports the most number of requested features, but you can also implement a custom configuration chooser to perform your own logic. See [`XRSessionSubsystem.configurationChooser`](xref:UnityEngine.XR.ARSubsystems.XRSessionSubsystem.configurationChooser) in the ARSubsystems package.

### Adapting an existing project

The breaking change here is mostly behavioral. In previous versions, face tracking tended to trump other features, so enabling face tracking would give you a 3dof user-facing camera experience. Now, you must be explicit, and the default mode is 6 DOF, world-facing. This means previous apps which used face tracking might not work the same as they did before. In addition to enabling face tracking, you might also need to specify the camera and tracking mode to achieve the same result. Note that there is a "don't care" mode, which means tracking will not be a consideration when choosing the configuration.

In AR Foundation 4, the `ARCameraManager` (a component on a Camera in your scene) controls which hardware camera is requested, and the `ARSession` component controls which tracking mode is requested. There is also a new scripting API for these modes.

## Requested vs. Current

In previous versions of AR Foundation, several features had simple getters and setters. For instance, the `ARCameraManager` had an `autoFocusMode` property that you could set. This, however, did not distinguish between what had been requested and what the actual mode was. Note that auto focus mode might not supported in the current state (ARKit's `ARFaceTrackingConfiguration`, for instance, does not support auto focus).

AR Foundation 3 was inconsistent in its handling of such properties. Some properties only had setters, and, of those that had getters, some getters returned what had been requested, while others returned the actual value by querying the underlying platform SDK.

AR Foundation 4 makes this explicit. Parameters now have "requested" and "current" properties. "Requested" properties are both gettable and settable, while the "current" property is read-only. The "current" properties tell you what is actually enabled in the underlying SDK, not just what you have requested.

| Component                 | Previous                                  | Requested                             | Actual                            |
| ---------                 | --------                                  | ---------                             | ------                            |
| ARCameraManager           | `autoFocusMode`                           | `autoFocusRequested`                  | `autoFocusEnabled`                |
| ARCameraManager           | `lightEstimationMode`                     | `requestedLightEstimation`            | `currentLightEstimation`          |
| ARCameraManager           | --                                        | `requestedFacingDirection`            | `currentFacingDirection`          |
| ARSession                 | --                                        | `requestedTrackingMode`               | `currentTrackingMode`             |
| ARSession                 | `matchFrameRate`                          | `matchFrameRateRequested`             | `matchFrameRateEnabled`           |
| ARHumanBodyManager        | `humanBodyPose2DEstimationEnabled`        | `pose2DRequested`                     | `pose2DEnabled`                   |
| ARHumanBodyManager        | `humanBodyPose3DEstimationEnabled`        | `pose3DRequested`                     | `pose3DEnabled`                   |
| ARHumanBodyManager        | `humanBodyPose3DScaleEstimationEnabled`   | `pose3DScaleEstimationRequested`      | `pose3DScaleEstimationEnabled`    |
| ARFaceManager             | `maximumFaceCount`                        | `requestedMaximumFaceCount`           | `currentMaximumFaceCount`         |
| AROcclusionManager        | `humanSegmentationDepthMode`              | `requestedHumanDepthMode`             | `currentHumanDepthMode`           |
| AROcclusionManager        | `humanSegmentationDepthMode`              | `requestedHumanStencilMode`           | `currentHumanStencilMode`         |
| ARPlaneManager            | `planeDetectionMode`                      | `requestedPlaneDetectionMode`         | `currentPlaneDetectionMode`       |
| ARTrackedImageManager     | `maxNumberOfMovingImages`                 | `requestedMaxNumberOfMovingImages`    | `currentMaxNumberOfMovingImages`  |
| AREnvironmentProbeManager | `automaticPlacement`                      | `automaticPlacementRequested`         | `automaticPlacementEnabled`       |
| AREnvironmentProbeManager | `environmentTextureHDR`                   | `environmentTextureHDRRequested`      | `environmentTextureHDREnabled`    |

## `XRCameraImage` is now `XRCpuImage`

The [Texture2D](xref:UnityEngine.Texture2D)s that represent the pass-through video are typically GPU textures. Computer vision or other CPU-side processing applications require access to the raw pixels on the CPU; however, the normal `Texture2D` APIs for reading pixels do not work unless the data is first transferred back from the GPU, which can be inefficient. Fortunately, AR frameworks like ARCore and ARKit provide a way to access the raw pixel data on the CPU without the inefficient GPU readback, and AR Foundation provided this data as an `XRCameraImage`.

This "camera image" API still exists, but it has been generalized and renamed from `XRCameraImage` to [XRCpuImage](xref:UnityEngine.XR.ARSubsystems.XRCpuImage). The API is very similar, but it can now be used to read other types of image data, such as the human depth and human stencil buffers provided by the [AROcclusionManager](xref:UnityEngine.XR.ARFoundation.AROcclusionManager).

### Changes

* `XRCameraImage` is now [XRCpuImage](xref:UnityEngine.XR.ARSubsystems.XRCpuImage)
* `XRCameraImage.FormatSupported` was a static method which determined whether the camera image could be converted to a given [TextureFormat](xref:UnityEngine.TextureFormat). Since `XRCpuImage` can handle different types of images, that method is now an instance method on the `XRCpuImage`.
* `ARCameraManager.TryGetLatestImage(out XRCameraImage)` is now [TryAcquireLatestCpuImage(out XRCpuImage)](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystem.Provider.TryAcquireLatestCpuImage(UnityEngine.XR.ARSubsystems.XRCpuImage.Cinfo@)). Note the parameter change from `XRCameraImage` to `XRCpuImage` and the method name more accurately describes the lifetime and caller responsibility of the `XRCpuImage` resource (though its behavior remains unchanged).
* The [AROcclusionManager](xref:UnityEngine.XR.ARFoundation.AROcclusionManager) has two new methods:
    * [TryAcquireHumanStencilCpuImage(out XRCpuImage)](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystem.Provider.TryAcquireHumanStencilCpuImage(UnityEngine.XR.ARSubsystems.XRCpuImage.Cinfo@))
    * [TryAcquireHumanDepthCpuImage(out XRCpuImage)](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystem.Provider.TryAcquireHumanDepthCpuImage(UnityEngine.XR.ARSubsystems.XRCpuImage.Cinfo@))

### Sample

The ["CpuImages" sample](https://github.com/Unity-Technologies/arfoundation-samples#cpuimages) from the AR Foundation Samples GitHub repo shows how to use the CPU image API and has been updated to include the color camera image and the human depth and human stencil images.

## XR Plug-in Management

AR Foundation now depends on [XR Plug-in Management](https://docs.unity3d.com/Packages/com.unity.xr.management@4.0/manual/index.html). This affects both edit and runtime:

### Edit time setup

Provider plug-ins must be enabled before AR Foundation can use them. You can enable specific plug-in providers for each target platform from the **XR Plug-in Management** window (menu: **Edit** &gt; **Project Settings** &gt; **XR Plug-in Management**).

![XR Plug-in Management](images/enable-arcore-plugin.png "XR Plug-in Management")

### Runtime

In previous versions of AR Foundation, each manager-component (for example, `ARSession`, `ARPlaneManager`, and`ARPointCloudManager`) fully controlled the lifecycle of each subsystem (a subsystem is the platform agnostic interface implemented by each provider package).

In AR Foundation 4, XR Plug-in Management controls the creation and destruction of the subsystems. AR Foundation's components still start and stop the subsystems, but do not create or destroy them:

| MonoBehaviour Event   | Subsystem |
| --------------------- | --------- |
| OnEnable              | Start     |
| OnDisable             | Stop      |

This means, for instance, that destroying an `ARSession` component pauses but does not destroy the session. This can be desirable when, for example, switching between two AR scenes. However, it is a change from the previous behavior. Destroying an `ARSession` and recreating it pauses and then resume the same session.

If you want to completely destroy the session, you need to destroy the subsystem. This means calling [Deinitialize](xref:UnityEngine.XR.Management.XRLoader.Deinitialize) on an [XRLoader](xref:UnityEngine.XR.Management.XRLoader) from XR Plug-in Management.

There is a new utility in AR Foundation to facilitate this: [LoaderUtility](xref:UnityEngine.XR.AR Foundation.LoaderUtility). Call

```csharp
LoaderUtility.Initialize();
```

or

```csharp
LoaderUtility.Deinitialize();
```

to create or destroy subsystems, respectively.
