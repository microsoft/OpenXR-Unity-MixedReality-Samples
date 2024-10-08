---
page_type: sample
name: Azure Spatial Anchors Sample
description: This sample project shows how to use Azure Spatial Anchors on the HoloLens 2 using Unity and OpenXR.
languages:
- csharp
products:
- windows-mixed-reality
- hololens
---

# Deprecation
 
This sample scene is being deprecated and will no longer be updated. Please note that Azure Spatial Anchors (ASA) will be retired on November 20, 2024, read the [Azure Spatial Anchors Retirement accounment](https://azure.microsoft.com/en-us/updates/azure-spatial-anchors-retirement/) for more details.

## Azure Spatial Anchors Sample

![License](https://img.shields.io/badge/license-MIT-green.svg)

Supported Unity versions | Built with XR configuration
:-----------------: | :----------------: |
Unity 2020 or higher | Mixed Reality OpenXR Plugin 1.4.1 |

Azure Spatial Anchors is a cross-platform developer service that allows you to create mixed reality experiences with objects that persist their locations across devices over time.
This sample project shows how to use Azure Spatial Anchors on the HoloLens 2 using Unity and OpenXR.

![Photo of Azure Spatial Anchors Sample Scene](../Readme/OpenXR-Unity-ASASample-Screenshot.jpg)

## Contents

| File/folder | Description |
|-------------|-------------|
| `Assets` | Unity assets, scenes, prefabs, and scripts. |
| `Packages` | Project manifest and packages list. |
| `ProjectSettings` | Unity asset setting files. |
| `UserSettings` | Generated user settings from Unity. |
| `.gitignore` | Define what to ignore at commit time. |
| `README.md` | This README file. |

## Running the app

1. If you don't have an [Azure subscription](https://docs.microsoft.com/en-us/azure/guides/developer/azure-developer-guide#understanding-accounts-subscriptions-and-billing), create a [free account](https://azure.microsoft.com/free/?ref=microsoft.com&utm_source=microsoft.com&utm_medium=docs&utm_campaign=visualstudio) before you begin.
2. Use your Azure subscription to [create a spatial anchors resource](https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-hololens?tabs=azure-portal#create-a-spatial-anchors-resource).
3. [Configure the account information](https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-hololens?tabs=azure-portal#configure-the-account-information) to be used in this project, writing the `Account Key`, `Account ID`, and `Account Domain` into `Assets\AzureSpatialAnchors.SDK\Resources\SpatialAnchorConfig.asset`.
4. Build and run the project.
