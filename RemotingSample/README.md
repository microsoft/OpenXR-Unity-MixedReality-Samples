---
page_type: sample
name: Remoting Sample
description: This sample project provides a single scene example where ARFoundation managers are used in remoting (late XR initialization) scenarios on the HoloLens 2 using Unity and OpenXR.
languages:
- csharp
products:
- windows-mixed-reality
- hololens
---

# Remoting Sample

![License](https://img.shields.io/badge/license-MIT-green.svg)

Supported Unity versions | Built with XR configuration
:-----------------: | :----------------: |
Unity 2020 or higher | Mixed Reality OpenXR Plugin 1.4.1 |

When the app runs in Holographic Application Remoting or Holographic Playmode Remoting modes, late XR initialization is used. ARFoundation trackable managers will not connect to XR subsystems if the trackable managers are active in the Unity scene
before XR initialization. The application must disable then reenable these trackable managers after XR initialization, or wait to add active trackable managers to the scene until after XR initialization. With a single scene in the application,
one will definitely run into this problem as ARFoundation managers are active before XR initialization. With multiple scenes in the application, one may not hit this case as the XR initialization happens late but before getting into a scene where ARFoundation managers are active.
Please refer to [HandleLateXRInitialization.cs](Assets/Scripts/HandleLateXRInitialization.cs) for more information on how this works.

![Photo of Remoting Sample Scene](../Readme/OpenXR-Unity-RemotingSample-Screenshot.jpg)

## Contents

| File/folder | Description |
|-------------|-------------|
| `Assets` | Unity assets, scenes, prefabs, and scripts. |
| `Packages` | Project manifest and packages list. |
| `ProjectSettings` | Unity asset setting files. |
| `UserSettings` | Generated user settings from Unity. |
| `.gitignore` | Define what to ignore at commit time. |
| `README.md` | This README file. | 
