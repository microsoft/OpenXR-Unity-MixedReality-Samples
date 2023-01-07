---
uid: arfoundation-migration-guide-2
---
# Migration guide

This guide covers the differences between AR Foundation 2.x and 3.x.

## AR Pose Driver

There is a new component: `ARPoseDriver`. This component was added to provide similar functionality to the `TrackedPoseDriver` and remove the dependency on the `com.unity.xr.legacyinputhelpers` package for AR Foundation. You can now use either the `ARPoseDriver` or the `TrackedPoseDriver` to drive a GameObject's local position and orientation according to the device's tracking information. Having both the `ARPoseDriver` and the `TrackedPoseDriver` components enabled on the same GameObject is not recommended as the behaviour is undefined.

## Image tracking

The image tracking manager `ARTrackedImageManager` has a `referenceLibrary` property on it to set the reference image library (the set of images to detect in the environment). Previously, this was an `XRReferenceImageLibrary`. Now, it is an `IReferenceImageLibrary`, and `XRReferenceImageLibrary` implements `IReferenceImageLibrary`. If your code previously set the the `referenceLibrary` property to an `XRReferenceImageLibrary`, it should continue to work as before. However, if you previously treated the `referenceLibrary` as an `XRReferenceImageLibrary`, you have to attempt to cast it to a `XRReferenceImageLibrary`.

In the Editor, this is always an `XRReferenceImageLibrary`. However, at runtime with image tracking enabled, `ARTrackedImageManager.referenceLibrary` returns a new type, `RuntimeReferenceImageLibrary`. This still behaves like an `XRReferenceImageLibrary` (for instance, you can enumerate its reference images), and it might also have additional functionality (see `MutableRuntimeReferenceImageLibrary`).

## Background shaders

The `ARCameraBackground` has been updated to support the [Universal Render Pipeline (URP)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest) when that package is present. This involved a breaking change to the `XRCameraSubsystem`: the property `shaderName` is now `cameraMaterial`. Most developers didn't need to access this directly because the shader name was only used by AR Foundation to construct the background material. That functionality has now moved to the `XRCameraSubsystem`.

## Point clouds

The [ARPointCloud](point-cloud-manager.md) properties [positions](xref:UnityEngine.XR.ARFoundation.ARPointCloud.positions), [confidenceValues](xref:UnityEngine.XR.ARFoundation.ARPointCloud.confidenceValues), and [identifiers](xref:UnityEngine.XR.ARFoundation.ARPointCloud.identifiers) have changed from returning [NativeArray](xref:Unity.Collections.NativeArray`1)s. The `ARPointCloud` manages the memory contained in these `NativeArray`s, so callers should only be able to see a `NativeSlice` (that is, you should not be able to [Dispose](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.Dispose.html) of the `NativeArray`).

Additionally, these arrays aren't necessarily present. Previously, you could check for their existence with [`NativeArray<T>.IsCreated`](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.IsCreated.html). `NativeSlice` doesn't have an `IsCreated` property, so these properties have been made nullable.

## Face tracking

The [ARFaceManager](face-manager.md)'s `supported` property has been removed. If face tracking is not supported, the manager's subsystem is null. This was done for consistency as no other manager has this property. If a manager's subsystem is null after enabling the manager, that generally means the subsystem is not supported.

## Reference Points renamed to Anchors

To align with industry standard terminology, "Reference Points" are now named "Anchors":

| **Old class** | **New class** |
| --------- | --------- |
| `ARReferencePointManager` | `ARAnchorManager` |
| `ARReferencePoint` | `ARAnchor` |

When you open an existing Project that used the old reference point API, Unity prompts you with the option to automatically update your scripts to the new API.
