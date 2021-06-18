---
page_type: sample
name: OpenXR Mixed Reality samples for Unity
description: These sample projects showcase how to build Unity applications for HoloLens 2 or Mixed Reality headsets using the Mixed Reality OpenXR plugin.
languages:
- csharp
products:
- windows-mixed-reality
- hololens
---

# OpenXR + Unity + Mixed Reality Samples

## Welcome!

![OpenXR-Unity-MixedReality-Samples-MainMenu](Readme/OpenXR-Unity-MixedReality-Samples-MainMenu.jpg)

These sample projects showcase how to build Unity applications
for HoloLens 2 or Mixed Reality headsets using the Mixed Reality OpenXR plugin.
For more details on installing related tools and setting up a Unity project,
please reference [the plugin documentation on https://docs.microsoft.com/](https://aka.ms/openxr-unity).

> NOTE: This repository uses [Git Large File Storage](https://git-lfs.github.com/) to store large files,
> such as Unity packages and images. Please install the latest git-lfs before cloning this repo.

## Recommended tool versions

It's recommended to run these samples on HoloLens 2 using the following versions:

- Latest Visual Studio 2019
- Latest Unity 2020.3 LTS, recommended 2020.3.8f1 or newer
- Latest Unity OpenXR plugin, recommended 1.2.0 or newer
- Latest Mixed Reality OpenXR Plugin, recommended 1.0.0 or newer
- Latest MRTK-Unity, recommended 2.7.0 or newer
- Latest Windows Mixed Reality Runtime, recommended 106 or newer

### Sample for anchors and anchor persistence

[AnchorsSample.cs](BasicSample/Assets/ARAnchor/Scripts/AnchorsSample.cs) in the ARAnchor scene
demos the usage of ARFoundation to create free-world anchors,
and the usage of the [XRAnchorStore](https://docs.microsoft.com/windows/mixed-reality/develop/unity/spatial-anchors-in-unity?tabs=openxr#using-the-anchorstore) to persist these anchors between sessions.

### Sample for hand tracking

- [FeatureUsageHandJointsManager.cs](BasicSample/Assets/HandTracking/Scripts/FeatureUsageHandJointsManager.cs)
  in the HandTracking scene demos using Unity Feature Usages to obtain hand joint data.
- [OpenXRExtensionHandJointsManager.cs](BasicSample/Assets/HandTracking/Scripts/OpenXRExtensionHandJointsManager.cs)
  in the HandTracking scene demos the usage of the Mixed Reality OpenXR Extension APIs to obtain hand joint data.
- [HandMesh.cs](BasicSample/Assets/HandTracking/Scripts/HandMesh.cs)
  in the HandTracking scene demos the usage of hand meshes.

### Sample for eye tracking

[FollowEyeGaze.cs](BasicSample/Assets/Interaction/Scripts/FollowEyeGaze.cs) in the Interaction scene demos using Unity Feature Usages to obtain eye tracking data.

### Sample for locatable camera

[LocatableCamera.cs](BasicSample/Assets/LocatableCamera/Scripts/LocatableCamera.cs) in the LocatableCamera scene demos the setup and usage of the locatable camera.

### Sample for ARFoundation compatibility

Scenes [ARAnchor](BasicSample/Assets/ARAnchor), [ARRaycast](BasicSample/Assets/ARRaycast), [ARPlane](BasicSample/Assets/ARPlane),
and [ARMesh](BasicSample/Assets/ARMesh) are all implemented using ARFoundation, backed in this project by OpenXR plugin on HoloLens 2.

- Find planes using [ARPlaneManager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.0/api/UnityEngine.XR.ARFoundation.ARPlaneManager.html)
- Place holograms using [ARRaycastManager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.0/api/UnityEngine.XR.ARFoundation.ARRaycastManager.html).
- Display meshes using [ARMeshManager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.0/api/UnityEngine.XR.ARFoundation.ARMeshManager.html)

### Sample for Azure Spatial Anchors

[SpatialAnchorsSample.cs](AzureSpatialAnchorsSample/Assets/Scripts/SpatialAnchorsSample.cs) in the [Azure Spatial Anchors sample project](AzureSpatialAnchorsSample) demos saving and locating spatial anchors. For more information on how to set up the Azure Spatial Anchors project, see the [readme](AzureSpatialAnchorsSample) in the project's folder.

## How to file issues and get help

This project uses GitHub Issues to track bugs and feature requests.
For help and questions about using this project, please use GitHub Issues in this project.
Please search the existing issues before filing new issues to avoid duplicates.
For new issues, file your bug or feature request as a new Issue.

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
