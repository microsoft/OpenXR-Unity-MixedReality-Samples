# OpenXR + Unity + Mixed Reality Samples

Welcome!

![OpenXR-Unity-MixedReality-Samples-MainMenu-Cropped](Readme/OpenXR-Unity-MixedReality-Samples-MainMenu-Cropped.jpg)

These sample projects showcase features provided in the [Mixed Reality OpenXR Plugin for Unity](https://aka.ms/openxr-unity).

Features using AR Raycasts and AR Planes require the latest preview OpenXR runtime, which can be enabled using the OpenXR Developer Tools for Windows Mixed Reality. To install this app, search for "OpenXR" in the Microsoft Store app on HoloLens 2.

## Features showcased in the BasicSample project

- **Anchors and anchor persistence** </br>
[AnchorsSample.cs](BasicSample/Assets/ARAnchor/Scripts/AnchorsSample.cs) in the ARAnchor scene demos the usage of ARFoundation to create free-world anchors, and the usage of the [XRAnchorStore](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/unity/spatial-anchors-in-unity?tabs=openxr#using-the-anchorstore) to persist these anchors between sessions.

- **Hand tracking** </br>
  - [FeatureUsageHandJointsManager.cs](BasicSample/Assets/HandTracking/Scripts/FeatureUsageHandJointsManager.cs) in the HandTracking scene demos using Unity Feature Usages to obtain hand joint data.
  - [OpenXRExtensionHandJointsManager.cs](BasicSample/Assets/HandTracking/Scripts/OpenXRExtensionHandJointsManager.cs) in the HandTracking scene demos the usage of the Mixed Reality OpenXR Extension APIs to obtain hand joint data.
  - [HandMesh.cs](BasicSample/Assets/HandTracking/Scripts/HandMesh.cs) in the HandTracking scene demos the usage of hand meshes.

- **Eye tracking** </br>
[FollowEyeGaze.cs](BasicSample/Assets/Interaction/Scripts/FollowEyeGaze.cs) in the Interaction scene demos using Unity Feature Usages to obtain eye tracking data.

- **Locatable camera** </br>
[LocatableCamera.cs](BasicSample/Assets/LocatableCamera/Scripts/LocatableCamera.cs) in the LocatableCamera scene demos the setup and usage of the locatable camera.

- **ARFoundation compatibility** </br>
Scenes [ARAnchor](BasicSample/Assets/ARAnchor), [ARRaycast](BasicSample/Assets/ARRaycast), [ARPlane](BasicSample/Assets/ARPlane), and [ARMesh](BasicSample/Assets/ARMesh) are all implemented using ARFoundation, backed in this project by OpenXR.
  - Find planes using ARPlaneManager
  - Place holograms using ARRaycastManager
  - Display meshes using ARMeshManager

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
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
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
