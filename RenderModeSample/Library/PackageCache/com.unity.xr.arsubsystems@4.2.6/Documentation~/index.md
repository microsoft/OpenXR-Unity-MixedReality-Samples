---
uid: arsubsystems-manual
---
# About AR Subsystems

A [subsystem](xref:UnityEngine.Subsystem) is a platform-agnostic interface for surfacing different types of functionality and data. The AR-related subsystems are defined in this package and use the namespace `UnityEngine.XR.ARSubsystems`. This package only provides the interface for various subsystems. Implementations for these subsystems (called "providers") are typically provided as separate packages or plug-ins. These are called "provider implementations".

This package provides interfaces for the following subsystems:

- [Session](session-subsystem.md)
- [Anchors](anchor-subsystem.md)
- [Raycasting](raycasting-subsystem.md)
- [Camera](camera-subsystem.md)
- [Plane Detection](plane-subsystem.md)
- [Depth](depth-subsystem.md)
- [Image Tracking](image-tracking.md)
- [Face Tracking](face-tracking.md)
- [Environment Probes](environment-probe-subsystem.md)
- [Object Tracking](object-tracking.md)
- [Body Tracking](xref:UnityEngine.XR.ARSubsystems.XRHumanBodySubsystem)
- [Occlusion](occlusion-subsystem.md)
- [Meshes](mesh-subsystem.md)

## Installing AR Subsystems

AR Subsystems is a dependency of platform-specific packages which use one or more of the subsystems defined within it. To use AR Subsystems, you must also install at least one of these platform-specific AR packages:

- ARCore XR Plug-in
- ARKit XR Plug-in
- Magic Leap XR Plug-in
- Windows XR Plug-in

## Using AR Subsystems

All subsystems have the same lifecycle: they can be created, started, stopped, and destroyed. Each subsystem has a corresponding `SubsystemDescriptor`, which describes the capabilities of a particular provider. Use the `SubsystemManager` to enumerate the available subsystems of a particular type. When you have a valid subsystem descriptor, you can `Create()` the subsystem. This is the only way to construct a valid subsystem.

### Example: picking a plane subsystem

This example iterates through all the `XRPlaneSubsystemDescriptor`s to look for one which supports a particular feature, then creates it. You can only have one subsystem per platform.

```csharp
XRPlaneSubsystem CreatePlaneSubsystem()
{
    // Get all available plane subsystems
    var descriptors = new List<XRPlaneSubsystemDescriptor>();
    SubsystemManager.GetSubsystemDescriptors(descriptors);

    // Find one that supports boundary vertices
    foreach (var descriptor in descriptors)
    {
        if (descriptor.supportsBoundaryVertices)
        {
            // Create this plane subsystem
            return descriptor.Create();
        }
    }

    return null;
}
```

When created, you can `Start` and `Stop` the subsystem. The exact behavior of `Start` and `Stop` varies by subsystem, but generally corresponds to "start doing work" and "stop doing work". A subsystem can be started and stopped multiple times. To completely destroy the subsystem instance, call `Destroy` on the subsystem. It is not valid to access a subsystem after it has been destroyed.

```csharp
var planeSubsystem = CreatePlaneSubsystem();
if (planeSubsystem != null)
{
    // Start plane detection
    planeSubsystem.Start();
}

// Some time later...
if (planeSubsystem != null)
{
    // Stop plane detection. This doesn't discard already detected planes.
    planeSubsystem.Stop();
}

// When shutting down the AR portion of the app:
if (planeSubsystem != null)
{
    planeSubsystem.Destroy();
    planeSubsystem = null;
}
```

Refer to the subsystem-specific documentation list above for more details about each subsystem this package provides.

## Implementing an AR subsystem

If you are implementing one of the AR Subsystems in this package (for example, you are a hardware manufacturer for a new AR device), you need to implement a concrete instance of the relevant abstract base class this package provides. Those types are typically named `XR<feature>Subsystem`.

Each subsystem has a nested class called `IProvider`. This is the primary interface you must implement for each subsystem you plan to support.

### Tracking subsystems

A "tracking" subsystem is any subsystem that detects and tracks something in the physical environment (for example, plane tracking and image tracking). The thing that the tracking subsystem tracks is called a "trackable". For example, the plane subsystem detects planes, so a plane is a trackable.

Each tracking subsystem requires you to implement a method called `GetChanges`. This method retrieves data about the trackables it manages. Each trackable can be uniquely identified by a `TrackableId`, a 128-bit GUID (Globally Unique Identifier). A trackable can be added, updated, or removed. It's an error to update or remove a trackable that hasn't been added yet. Likewise, a trackable can't be removed without having been added, nor updated if it hasn't been added or was already removed.

`GetChanges` should report all added, updated, and removed trackables since the previous call to `GetChanges`. You should expect `GetChanges` to be called once per frame.

## Technical details

### Requirements

This version of AR Subsystems is compatible with the following versions of the Unity Editor:

* 2020.3
* 2021.1
* 2021.2
