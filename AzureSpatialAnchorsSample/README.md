# Azure Spatial Anchors Sample

Azure Spatial Anchors is a cross-platform developer service that allows you to create mixed reality experiences with objects that persist their locations across devices over time.
This sample project shows how to use Azure Spatial Anchors on the HoloLens 2 using Unity and OpenXR.

Running this sample project requires the following additional steps:

1. If you don't have an [Azure subscription](https://docs.microsoft.com/en-us/azure/guides/developer/azure-developer-guide#understanding-accounts-subscriptions-and-billing), create a [free account](https://azure.microsoft.com/free/?ref=microsoft.com&utm_source=microsoft.com&utm_medium=docs&utm_campaign=visualstudio) before you begin.
2. Use your Azure subscription to [create a spatial anchors resource](https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-hololens?tabs=azure-portal#create-a-spatial-anchors-resource).
3. [Configure the account information](https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-hololens?tabs=azure-portal#configure-the-account-information) to be used in this project, writing the `Account Key`, `Account ID`, and `Account Domain` into `Assets\AzureSpatialAnchors.SDK\Resources\SpatialAnchorConfig.asset`.
4. Build and run the project.
