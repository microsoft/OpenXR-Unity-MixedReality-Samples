---
uid: openxr-input
---

# Input in Unity OpenXR

This page details how to use and configure OpenXR input within unity. 

For information on how to configure Unity to use OpenXR input, see the [Getting Started](#getting-started) section of this document.

## Overview

Initially, Unity will provide a controller-based approach to interfacing with OpenXR. This will allow existing games and applications that are using Unity's [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/) or the [Feature API](https://docs.unity3d.com/Manual/xr_input.html) to continue to use their existing input mechanisms.

The Unity OpenXR package provides a set of controller layouts for various devices that you can bind to your actions when using the Unity Input System. For more information, see the [Interaction profile features](./index.md#interaction-profile-features) section.

To use OpenXR Input, you must select the correct interaction profiles features to send to OpenXR. To learn more about OpenXR features in Unity, see the [Interaction profile features](./index.md#interaction-profile-features) page.

Future versions of the Unity OpenXR Package will provide further integration with the OpenXR Input system. For smooth upgrading, Unity recommends that you use the device layouts included with the OpenXR package. These have the '(OpenXR)' suffix in the Unity Input System binding window. 

Unity will provide documentation on these features when they become available. 

Interaction profiles manifest themselves as device layouts in the Unity [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/). 

## Getting Started

### Run the sample

The Open XR package contains a sample named `Controller` that will help you get started using input in OpenXR.  To install the `Controller` sample, follow these steps:

1. Open the **Package Manager** window (menu: **Window &gt; Package Manager**).
2. Select the OpenXR package in the list.
3. Expand the **Samples** list on the right.
4. Click the **Import** button next to the `Controller` sample.

This adds a `Samples` folder to your project with a Scene named `ControllerSample` that you can run.

### Locking input to the game window

Versions V1.0.0 to V1.1.0 of the Unity Input System only route data to or from XR devices to the Unity Editor while the Editor is in the **Game** view.  To work around this issue, use the [Unity OpenXR Project Validator](xref:openxr-project-config#project-validation) or follow these steps:

* Access the Input System Debugger window (menu: **Window &gt; Analysis &gt; Input Debugger**).
* In the **Options** section, enable the **Lock Input to the Game Window** option.

Unity recommends that you enable the **Lock Input to the Game Window** option from either the [Unity OpenXR Project Validator](xref:openxr-project-config#project-validation) or the Input System Debugger window

![lock-input-to-game-view](images/lock-input-to-game-view.png)

### Recommendations

To set up input in your project, follow these recommendations:

* Bind to the OpenXR layouts wherever possible.
* Use specific control bindings over usages.
* Avoid generic "any controller" bindings if possible (for example, bindings to `<XRController>`).
* Use action references and avoid inline action definitions.

OpenXR Requires that all bindings be attached only once at application startup. Unity recommends the use of Input Action Assets, and Input Action References to Actions within those assets so that Unity can present those bindings to OpenXR at applications startup.

## Using OpenXR input with Unity

Using OpenXR with Unity is the same as configuring any other input device using the Unity Input System:

1. Decide on what actions and action maps you want to use to describe your gameplay, experience or menu operations
2. Create an `Input Action` Asset, or use the one included with the [Sample](#run-the-sample).
3. Add the actions and action maps you defined in step 1 in the `Input Action` Asset you decided to use in step 2. 
4. Create bindings for each action.

    When using OpenXR, you must either create a "Generic" binding, or use a binding to a device that Unity's OpenXR implementation specifically supports. For a list of these specific devices, see the [Interaction bindings](#interaction-bindings) section. 

5. Save your `Input Action` Asset.
6. Ensure your actions and action maps are enabled at runtime.

    The [Sample](#run-the-sample) contains a helper script called `Action Asset Enabler` which enables every action within an `Input Action` Asset. If you want to enable or disable specific actions and action maps, you can manage this process yourself. 

7. Write code that reads data from your actions. 

    For more information, see the [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/) package documentation, or consult the [Sample](#run-the-sample) to see how it reads input from various actions. 

8. Enable the set of Interaction Features that your application uses.

    If you want to receive input from OpenXR, the Interaction Features you enable must contain the devices you've created bindings with. For example, a binding to `<WMRSpatialController>{leftHand}/trigger` requires the Microsoft Motion Controller feature to be enabled in order for that binding to receive input. For more information on Interaction Features, see the [Interaction profile features](./index.md#interaction-profile-features) section.

9. Run your application!

You can use the Unity Input System Debugger (menu: **Window &gt; Analysis &gt; Input Debugger**) to troubleshoot any problems with input and actions.
The Input System Debugger can be found under **Window &gt; Analysis &gt; Input Debugger**

## Detailed information

### Unity Input System

Unity requires the use of the Input System package when using OpenXR. Unity automatically installs this package when you install Unity OpenXR Support. For more information, see the Input System package [documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Installation.html).

###  Interaction Profile Features

Each Interaction Profile Feature contains both the device layout for creating bindings in the Unity Input System and a set of bindings that we send to OpenXR. The OpenXR Runtime will determine which bindings to use based on the set of Interaction Profiles that we send to it. 

Unity Recommends that Developers select only the Interaction Profiles that they are able to test their experience with.

Selecting an Interaction Profile from the features menu will add that device to the bindable devices in the Unity Input System. They will be selectable from under the **XR Controller** section of the binding options. 

![XR Controller Menu](images/xr-controller-input-menu.png)

See [Set the interaction profile](xref:openxr-project-config#interaction-profile) for instructions on setting a profile.

### Mapping between OpenXR paths and Unity bindings

The OpenXR specification details a number of `Interaction Profiles` that you can use to suggest bindings to the OpenXR runtime. Unity uses its own existing XRSDK naming scheme to identify controls and devices and map OpenXR action data to them. 

The table below outlines the common mappings between OpenXR paths and Unity XRSDK Control names.
Which controls are available on which devices is covered in the specific device documentation.

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
|`/input/system/click`| system | Boolean |
|`/input/system/touch`| systemTouched | Boolean |
|`/input/select/click`| select | Boolean |
|`/input/menu/click`| menu | Boolean |
|`/input/squeeze/value` | grip | Float |
|`/input/squeeze/click` | gripPressed | Boolean | 
|`/input/squeeze/force` | gripForce | Boolean |
|`/input/trigger/value` | trigger | Float |
|`/input/trigger/squeeze` | triggerPressed | Boolean |
|`/input/trigger/touch` | triggerTouched | Boolean |
|`/input/thumbstick`| joystick | Vector2 |
|`/input/thumbstick/touch`| joystickTouched | Vector2 |
|`/input/thumbstick/clicked`| joystickClicked | Vector2 |
|`/input/trackpad`| touchpad | Vector2 |
|`/input/trackpad/touch`| touchpadTouched | Boolean | 
|`/input/trackpad/clicked` | touchpadClicked | Boolean |
|`/input/a/click` | primaryButton | Boolean |
|`/input/a/touch` | primaryTouched | Boolean |
|`/input/b/click` | secondaryButton | Boolean |
|`/input/b/touch` | secondaryTouched | Boolean |
|`/input/x/click` | primaryButton | Boolean |
|`/input/x/touch` | primaryTouched | Boolean |
|`/input/y/click` | secondaryButton | Boolean |
|`/input/y/touch` | secondaryTouched | Boolean |

the Unity control `touchpad` and `trackpad` are used interchangeably, as are `joystick` and `thumbstick`.

### Pose data

Unity expresses Pose data as individual elements (for example, position, rotation, velocity, and so on). OpenXR expresses poses as a group of data. Unity has introduced a new type to the Input System called a `Pose` that is used to represent OpenXR poses. The available poses and their OpenXR paths are listed below:

|Pose Mapping| |
|----|----|
|`/input/grip/pose`| devicePose |
|`/input/aim/pose` | pointerPose |

For backwards compatibility, the existing individual controls will continue to be supported when using OpenXR. The mapping between OpenXR pose data and Unity Input System pose data is found below.

|Pose | Pose Element| Binding| Type|
|---|---|---|---|
|`/input/grip/pose`| position | devicePosition | Vector3|
|`/input/grip/pose`| orientation | deviceRotation | Quaternion|
|`/input/aim/pose`| position | pointerPosition | Vector3|
|`/input/aim/pose`| orientation | pointerRotation | Quaternion|

### HMD bindings

To read HMD data from OpenXR, use the existing HMD bindings available in the Unity Input System. Unity recommends binding the `centerEye` action of the `XR HMD` device for HMD tracking. The following image shows the use of `centerEye` bindings with the `Tracked Pose Driver`. 

![hmd-config-tpd](images/hmd-config-tpd.png)


OpenXR HMD Data contains the following elements. 
- Center Eye
- Device
- Left Eye
- Right Eye

All of the elements expose the following controls:
- position
- rotation
- velocity
- angularVelocity

These are exposed in the Unity Input System through the following bindings. These bindings can be found under the XR HMD menu option when binding actions within the Input System.

- `<XRHMD>/centerEyePosition`
- `<XRHMD>/centerEyeRotation`
- `<XRHMD>/devicePosition`
- `<XRHMD>/deviceRotation`
- `<XRHMD>/leftEyePosition`
- `<XRHMD>/leftEyeRotation`
- `<XRHMD>/rightEyePosition`
- `<XRHMD>/rightEyePosition`

When using OpenXR the `centerEye` and `device` values are identical.

The HMD position reported by Unity when using OpenXR is calculated from the currently selected Tracking Origin space within OpenXR. 

The Unity `Device Tracking Origin` is mapped to `Local Space`.
The Unity `Floor Tracking Origin` is mapped to `Stage Space`.

By default, Unity attempts to attach the `Stage Space` where possible. To help manage the different tracking origins, use the `XR Origin` from the XR Interaction Package, or the `Camera Offset` component from the Legacy Input Helpers package. 

### Interaction bindings

If you use OpenXR input with controllers or interactions such as eye gaze, Unity recommends that you use bindings from the Device Layouts available with the Unity OpenXR package. The Unity OpenXR package provides the following Layouts via features:

|Device|Layout|Feature|
|-----|--------|----|
|Generic XR controller|`<XRController>`|n/a|
|Generic XR controller w/ rumble support|`<XRControllerWithRumble>`|n/a|
|Windows Mixed Reality controller|`<WMRSpatialController>`|[MicrosoftMotionControllerProfile](./features/microsoftmotioncontrollerprofile.md)|
|Oculus Touch (Quest,Rift)|`<OculusTouchController>`|[OculusTouchControllerProfile](./features/oculustouchcontrollerprofile.md)|
|HTC Vive controller|`<ViveController>`|[HTC Vive Controller Profile](./features/htcvivecontrollerprofile.md)|
|Valve Index controller|`<ValveIndexController>`|[ValveIndexControllerProfile](./features/valveindexcontrollerprofile.md)|
|Khronos Simple Controller|`<KHRSimpleController>`|[KHRSimpleControllerProfile](./features/khrsimplecontrollerprofile.md)|
|Eye Gaze Interaction|`<EyeGaze>`|[EyeGazeInteraction](./features/eyegazeinteraction.md)|
|Microsoft Hand Interaction|`<HololensHand>`|[MicrosoftHandInteraction](./features/microsofthandinteraction.md)|

## Haptics

OpenXR Controllers that support haptics contain a Haptic control that you can bind to an action in the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest). Set the action type to Haptic when binding an InputAction to a haptic. To send a haptic impulse, call the [OpenXRInput.SendHapticImpulse](xref:UnityEngine.XR.OpenXR.Input.OpenXRInput.SendHapticImpulse*) method and specify the InputAction that you bound to the Haptic control. You can cancel the haptic impulse by calling the [OpenXRInput.StopHaptics](UnityEngine.XR.OpenXR.Input.OpenXRInput.StopHaptics*) method.

## Debugging

For more information on debugging OpenXR input, see the [Input System Debugging](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Debugging.html) documentation.


## Future plans

Looking ahead, we will work towards allowing Unity users to leverage more functionality of OpenXR's input stack, allowing the runtime to bind Unity Actions to OpenXR Actions. This will allow OpenXR Runtimes to perform much more complex binding scenarios than currently possible.
