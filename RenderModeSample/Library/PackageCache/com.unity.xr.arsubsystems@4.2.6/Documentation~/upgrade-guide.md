---
uid: arsubsystems-upgrade-guide
---
# Upgrading to AR Subsystems version 4.2

To upgrade to AR Subsystems package version 4.2, you need to do the following:

- Use new methods to query for occlusion subsystem support.
- Use Unity 2020.3 or newer.

**Use new methods to query for occlusion subsystem support**

The [XROcclusionSubsystemDescriptor](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor) provides a means to query for the capabilities of an [XROcclusionSubsystem](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystem). Previously, these capabilities were booleans, but some platforms may take a few frames to determine a capability. Those `bool` properties have been deprecated in favor of properties that return a [Supported](xref:UnityEngine.XR.ARSubsystems.Supported) state, which includes a `Supported.Unknown` state to indicate support for the feature or capability is not yet known. These properties are:

- [humanSegmentationStencilImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.humanSegmentationStencilImageSupported)
- [humanSegmentationDepthImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.humanSegmentationDepthImageSupported)
- [environmentDepthImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthImageSupported)
- [environmentDepthConfidenceImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthConfidenceImageSupported)

**Use Unity 2020.3 or newer**

This version of the package requires Unity 2020.3 or newer.
