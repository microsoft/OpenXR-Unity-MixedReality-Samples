---
uid: arsubsystems-plane-subsystem
---
# XR plane subsystem

The plane subsystem detects flat, planar surfaces in the environment. It is a type of [tracking subsystem](xref:arsubsystems-manual#tracking-subsystems) and follows the same [GetChanges](xref:UnityEngine.XR.ARSubsystems.XRPlaneSubsystem.Provider.GetChanges(UnityEngine.XR.ARSubsystems.BoundedPlane,Unity.Collections.Allocator)) pattern to inform the user about changes to the state of tracked planes. Its trackable is [BoundedPlane](xref:UnityEngine.XR.ARSubsystems.BoundedPlane).

## Plane lifecycle

The plane subsystem informs the user about static surfaces. It's not meant to track moving or otherwise dynamic planes. When a plane is "removed", it generally doesn't mean a surface has been removed, but rather that the subsystem's understanding of the environment has improved or changed in a way that invalidates that plane.

When a surface is first detected, the subsystem reports it as "added". Subsequent updates to the plane are refinements on this initial plane detection. A plane typically grows as you scan more of the environment.

Some platforms support the concept of planes merging. If a plane is merged into another one, the [BoundedPlane.subsumedById](xref:UnityEngine.XR.ARSubsystems.BoundedPlane.subsumedById) will contain the ID of the plane which subsumed the plane in question. Not all platforms support this. Some might remove one plane and make another plane larger to encompass the first.

## Boundary

A `BoundedPlane` is finite (unlike a [Plane](xref:UnityEngine.Plane), which is infinite). In addition to a position and orientation, which would define an infinite plane, bounded planes have a size (width and height) and can also have boundary points. Boundary points are two-dimensional vertices defined clockwise in plane space (a space relative to the plane's [Pose](xref:UnityEngine.Pose)). The boundary points should define a convex shape.
