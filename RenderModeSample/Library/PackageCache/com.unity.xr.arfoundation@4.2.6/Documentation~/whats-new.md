---
uid: arfoundation-whats-new
---
# What's new in version 4.2

Summary of changes in AR Foundation package version 4.2.

The main updates in this release include:

**Added**

- Add support for temporal smoothing of the environment depth image. This allows you to request that the environment depth image be smoothed over time. Not all providers support this feature. You can query for support with [XROcclusionSubsystemDescriptor.environmentDepthTemporalSmoothingSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthTemporalSmoothingSupported).
- Added options to the [ARPlaneMeshVisualizer component](xref:UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer) to control when MeshRenderer and LineRenderer components should be enabled. See
  - [ARPlaneMeshVisualizer.trackingStateVisibilityThreshold](xref:UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer.trackingStateVisibilityThreshold)
  - [ARPlaneMeshVisualizer.hideSubsumed](xref:UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer.hideSubsumed)

**Updated**

- Add description for occlusion on the main AR Foundation manual page, and edit the human segmentations description.
- The [ARSession](xref:UnityEngine.XR.ARFoundation.ARSession) optionally sets the application's `vSyncCount` and `targetFrameRate` (see [matchFrameRateRequested](xref:UnityEngine.XR.ARFoundation.ARSession.matchFrameRateRequested)). In earlier versions, the original values were not restored when the ARSession was disabled. The behavior has changed so that the original values are restored when the ARSession was disabled if the frame rate was set while the ARSession was enabled.
- Update [XR Plug-in Management](https://docs.unity3d.com/Packages/com.unity.xr.management@4.0) dependency to 4.0.

**Fixed**

- Added missing icon to the [AROcclusionManager](xref:UnityEngine.XR.ARFoundation.AROcclusionManager).
- Replace uses of [PostProcessScene](xref:UnityEditor.Callbacks.PostProcessSceneAttribute) attribute with [IProcessSceneWithReport](xref:UnityEditor.Build.IProcessSceneWithReport) callback mechanism. This prevents scene hashes from changing when adding the AR Foundation package.

For a full list of changes and updates in this version, see the [AR Foundation package changelog](xref:arfoundation-changelog).
