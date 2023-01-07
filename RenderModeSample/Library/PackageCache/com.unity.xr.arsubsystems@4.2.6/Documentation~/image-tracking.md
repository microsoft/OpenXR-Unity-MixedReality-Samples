---
uid: arsubsystems-image-tracking-subsystem
---
# XR image tracking subsystem

The image tracking subsystem attempts to detect two-dimensional images in the environment that match images stored in a reference images library.

## Terminology

|**Term**|**Description**|
|--------|---------------|
|**Reference image**|A reference image is an image that the `XRImageTrackingSubsystem` attempts to find in the real world. The subsystem associates detected images with the reference image used to detect them. Each detected image has a pose in the world.|
|**Reference image library**|A set of reference images. When you start an image tracking subsystem, you must first provide it with a library of reference images so it knows what to search for.|
|**Provider**|The image tracking subsystem is an interface which is implemented in other packages. Each implementation is called a "provider". For example, you might have a different provider package for each AR platform.|

## Creating a reference image library

You create reference image libraries in the Unity Editor. From Unity's main menu, go to **Assets** &gt; **Create** &gt; **XR** &gt; **Reference Image Library**.

This creates a new reference image library asset in your project. To add images, select this Asset, then click **Add Image** and complete the fields that appear:

![Editing a reference image library](images/create-reference-image-library.gif "Editing a reference image library")

You can specify the following information:

|**Field**|**Description**|
|------|-------|
|**Name**|A string name that is available at runtime. This name isn't used by the subsystem, but it can be useful to identify which reference image has been detected. There is no check for duplicate name conflicts.|
|**Specify Size**|If enabled, you can specify the physical size you expect the image to be in the real world. This field is mandatory for some providers and optional for others. For more information, see your provider's documentation. If you specify this field, the dimensions must be greater than zero. Editing one dimension (for example, width) causes the other dimension (height) to change automatically based on the image's aspect ratio.|
|**Keep Texture at Runtime**|If enabled, `XRReferenceImage.texture` contains a reference to the source texture. This can be useful if you need access to the source texture at runtime. By default, this is unchecked to reduce the built Player size. When unchecked, `XRReferenceImage.texture` is null.|

## Using the library at runtime

To use the library at runtime, set it on the subsystem. For example:

```csharp
XRReferenceImageLibrary myLibrary = ...
XRImageTrackingSubsystem subsystem = ...

subsystem.imageLibrary = myLibrary;
subsystem.Start();
```

> [!NOTE]
> You must set `imageLibrary` to a non-null reference before starting the subsystem.

Query for changes to tracked images with `XRImageTrackingSubsystem.GetChanges`. This returns all changes to tracked images (added, updated, and removed) since the last call to this method.

## Using reference image libraries with asset bundles

Prior to AR Subsystems 4.2, reference image libraries had to be built into the Player; that is, the `XRReferenceImageLibrary` served only as a means to look up data that was expected to be packaged in the app. This meant that you could not, for example, download a new reference image library to an already released app. As of AR Subsystems 4.2, platform-specifc data can be attached to the `XRReferenceImageLibrary` asset when building a Player or [asset bundle](xref:AssetBundlesIntro). This means that you can include an `XRReferenceImageLibrary` in an asset bundle and use it in an app that was not built with that reference image library present.
