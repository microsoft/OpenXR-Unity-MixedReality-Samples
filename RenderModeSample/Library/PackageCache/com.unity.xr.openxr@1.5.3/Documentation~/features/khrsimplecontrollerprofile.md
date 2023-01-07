# Khronos Simple Controller Profile

Enables the OpenXR interaction profile for the Khronos Simple Controller and exposes the `<SimpleController>` device layout within the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).

For more information about the Khronos Simple Controller interaction profile, see the [OpenXR Specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_khronos_simple_controller_profile).

## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
|`/input/select/click`| select | Boolean |
|`/input/menu/click` | menu | Boolean |
|`/input/grip/pose` | devicePose | Pose |
|`/input/aim/pose` | pointer | Pose |
| Unity Layout Only  | isTracked | Flag Data |
| Unity Layout Only  | trackingState | Flag Data |
| Unity Layout Only  | devicePosition | Vector3 |
| Unity Layout Only  | deviceRotation | Quaternion |
| Unity Layout Only  | pointerPosition | Vector3 |
| Unity Layout Only  | pointerRotation | Quaternion |