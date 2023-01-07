---
uid: arsubsystems-object-tracking-subsystem
---
# XR object tracking subsystem

The object tracking subsystem attempts to detect three-dimensional objects in the environment that have previously been scanned and stored in a reference objects library.

## Terminology

|**Term**|**Description**|
|--------|---------------|
|**Reference object**|A reference object is a previously scanned object. The object tracking subsystem attempts to find instances of the object and report on their poses.|
|**Reference object library**|A set of reference objects. When you start an object tracking subsystem, you must first provide it with a library of reference objects so it knows what to search for.|
|**Provider**|The object tracking subsystem is an interface which is implemented in other packages. Each implementation is called a "provider". For example, you might have a different provider package for each AR platform.|

## Creating a reference object library

You create reference object libraries in the Unity Editor, then fill it with reference objects. Each reference object requires a provider-specific representation for each provider package you have in your project.

From Unity's main menu, go to **Assets** &gt; **Create** &gt; **XR** &gt; **Reference Object Library**.

This creates a new asset in your project. To create reference objects, select this asset, then select **Add Entry**:

![A reference object library](images/reference-object-library-inspector.png "A reference object library")

Reference objects have a **Name**, followed by a list of provider-specific entries. In the example above, the object only has one entry for ARKit.

You need to populate the reference object entries with provider-specific assets. For instructions on how to do this, refer to the provider's documentation.

## Using the library at runtime

To use the library at runtime, set it on the subsystem. For example:

```csharp
XRReferenceObjectLibrary myLibrary = ...
XRObjectTrackingSubsystem subsystem = ...

subsystem.library = myLibrary;
subsystem.Start();
```

> [!NOTE]
> You must set `imageLibrary` to a non-null reference before starting the subsystem.

Query for changes to tracked objects with `XRImageTrackingSubsystem.GetChanges`. This returns all changes to tracked objects (added, updated, and removed) since the last call to this method.
