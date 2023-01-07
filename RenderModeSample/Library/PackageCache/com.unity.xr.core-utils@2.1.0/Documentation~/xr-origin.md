---
uid: xr-core-utils-xr-origin
---
# XR Origin

The XR Origin represents the center of worldspace in an XR scene.

![XR Origin](images/xr-origin.png "XR Origin")

The purpose of the XR Origin is to transform objects and trackable features to their final position, orientation, and scale in the Unity scene. It specifies an Origin, a Camera Floor Offset Object, and a Camera.

## Setting up the XR Origin

The [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) class is a shared dependency between two packages: [AR Foundation (com.unity.xr.arfoundation)](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest/index.html) and [XR Interaction Toolkit (com.unity.xr.interaction.toolkit)](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest/index.html). AR Foundation and XR Interaction Toolkit are independent packages, but can also be used together to build XR applications. Each package provides different XR Origin configuration options based on the types of hardware each package supports.

## How to Setup for mobile AR

To create a pre-configured [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) for mobile AR:
- Install [AR Foundation (com.unity.xr.arfoundation)](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest/index.html).
- Enable the [provider plug-in(s)](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/index.html#provider-plug-in-setup) for your platform targets in the [XR Plug-in Management](https://docs.unity3d.com/Manual/configuring-project-for-xr.html) window. 
- Right-click anywhere in the Hierarchy window and select **XR** &gt; **XR Origin (Mobile AR)**.

Right-click in the Hierarchy window       |  Select XR Origin (Mobile AR)
:-------------------------:|:-------------------------:
![Select XR](images/xr-origin-xr.png)  |  ![Select XR Origin (Mobile AR)](images/xr-origin-mobile.png)

## How to Setup for VR HMD

To create a pre-configured [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) for VR with controllers:
- Install [XR Interaction Toolkit (com.unity.xr.interaction.toolkit)](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest/index.html).
- Right-click anywhere in the Hierarchy window and select **XR** &gt; **XR Origin (VR)**.

> [!NOTE]
> Components in the XR Interaction Toolkit (XRI) are Action-based by default, and make use of the [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest?subfolder=/manual/Installation.html). See the [XRI Samples](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest?subfolder=/manual/samples.html) for example Input Actions you can use to map inputs from your controllers.
> If your project depends on the Legacy Input System, a legacy Device-based XR Origin is available by right-clicking in the Hierarchy window and selecting **XR** &gt; **Device-based** &gt; **XR Origin (Device-based)**. See [Action-based vs Device-based behaviors](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest?subfolder=/manual/general-setup.html) in the XR Interaction Toolkit manual for more information.

## How to setup for all-purpose AR

To create a pre-configured [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) for AR which supports both HMD and mobile platforms:
- Install [AR Foundation (com.unity.xr.arfoundation)](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest/index.html).
- Enable the [provider plug-in(s)](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/index.html#provider-plug-in-setup) for your platform targets in the [XR Plug-in Management](https://docs.unity3d.com/Manual/configuring-project-for-xr.html) window.
- Install [XR Interaction Toolkit (com.unity.xr.interaction.toolkit)](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest/index.html).
- Right-click anywhere in the Hierarchy window and select **XR** &gt; **XR Origin (AR)**.
- For further configuration guidance of the XR Origin GameObject and its components, see the [XR Interaction Toolkit manual](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest?subfolder=/manual/general-setup.html).