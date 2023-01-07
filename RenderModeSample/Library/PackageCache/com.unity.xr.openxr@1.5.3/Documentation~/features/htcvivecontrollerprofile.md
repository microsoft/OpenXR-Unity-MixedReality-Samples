# HTC Vive Controller Profile

Enables the OpenXR interaction profile for the HTC Vive Controller and exposes the `<ViveController>` device layout within the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).  

For more information about the HTC Vive interaction profile, see the [OpenXR Specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_htc_vive_controller_profile).

## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
|`/input/system/click`| select | Boolean |
|`/input/squeeze/click`| grip | Float ( boolean value cast to float)|
|`/input/squeeze/click`| gripButton | Boolean |
|`/input/menu/click` | menu | Boolean|
|`/input/trigger/value`|trigger|  Float |
|`/input/trigger/click`|triggerPressed| Boolean |
|`/input/trackpad`|trackpad| Vector2 |
|`/input/trackpad/click`|trackpadClicked| Boolean |
|`/input/trackpad/touch`|trackpadTouched| Boolean |
|`/input/grip/pose`| devicePose| Pose |
|`/input/aim/pose`|pointer| Pose |
| Unity Layout Only  | isTracked | Flag Data |
| Unity Layout Only  | trackingState | Flag Data |
| Unity Layout Only  | devicePosition | Vector3 |
| Unity Layout Only  | deviceRotation | Quaternion |





