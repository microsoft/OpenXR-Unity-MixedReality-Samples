---
uid: arsubsystems-whats-new
---
# What's new in version 4.2

Summary of changes in AR Subsystems package version 4.2.

The main updates in this release include:

**Added**

- Added an [API to request temporal smoothing of the environment depth image](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystem.environmentDepthTemporalSmoothingRequested) for providers that support this feature.
- The runtime image data associated with an [XRReferenceImageLibrary](xref:UnityEngine.XR.ARSubsystems.XRReferenceImageLibrary) is now stored directly in the asset. This allows novel reference image libraries to be downloaded by an app that was not originally built with that library, for example, as an [asset bundle](xref:AssetBundlesIntro).
- A new [reference object](xref:UnityEngine.XR.ARSubsystems.XRReferenceObject) can be [added](xref:UnityEngine.XR.ARSubsystems.XRReferenceObjectLibrary.Add(UnityEngine.XR.ARSubsystems.XRReferenceObject)) to a [reference object library](xref:UnityEngine.XR.ARSubsystems.XRReferenceObjectLibrary) at runtime.

**Updated**

- The [XROcclusionSubsystemDescriptor](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor) provides a means to query for the capabilities of an [XROcclusionSubsystem](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystem). Previously, these capabilities were booleans, but some platforms may take a few frames to determine a capability. Those `bool` properties have been deprecated and replaced by properties that return a [Supported](xref:UnityEngine.XR.ARSubsystems.Supported) state, which includes a `Supported.Unknown` state to indicate support for the feature or capability is not yet known.
- Update documentation for [XROcclusionSubsystemDescriptor](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor) with notes describing the limitations of feature support detection.

**Fixed**

- Exclude tests from scripting API docs.

For a full list of changes and updates in this version, see the [AR Subsystems package changelog](xref:arsubsystems-changelog).
