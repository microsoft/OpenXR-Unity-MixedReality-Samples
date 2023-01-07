---
uid: arsubsystems-raycast-subsystem
---
# XR raycast subsystem

Raycasts allow you to perform hit testing against AR-specific features. They use the same concept as the [Physics.Raycast](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html), but raycast targets don't require a presence in the physics world.

There are two types of raycasts:
- [Screen point](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystem.Raycast(UnityEngine.Vector2,UnityEngine.XR.ARSubsystems.TrackableType,Unity.Collections.Allocator))
- [Arbitrary ray](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystem.Raycast(UnityEngine.Ray,UnityEngine.XR.ARSubsystems.TrackableType,Unity.Collections.Allocator))

Some implementations only support one or the other. You can check for support with [XRRaycastSubsystemDescriptor](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor).
