# Valve Index Controller Profile

Enables the OpenXR interaction profile for the Valve Index controler and exposes the `<ValveIndexController>` device layout within the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).  

For more information about the Valve Index interaction profile, see the [OpenXR Specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_htc_vive_controller_profile).


## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
|`/input/system/click`| system | Boolean |
|`/input/system/touch`| systemTouched | Boolean |
|`/input/a/click`| primaryButton | Boolean |
|`/input/a/touch`| primaryTouched | Boolean |
|`/input/b/click`| secondaryButton | Boolean |
|`/input/b/touch`| secondaryTouched | Boolean |
|`/input/squeeze/value`| grip | Float | 
|`/input/squeeze/value`| gripPressed | Boolean (cast from float) |
|`/input/squeeze/force`| gripForce | Float |
|`/input/trigger/click`| triggerPressed | Boolean |
|`/input/trigger/value`| trigger | Float | 
|`/input/trigger/touch`| triggerTouched | Boolean |
|`/input/thumbstick`| thumbstick | Vector2 |
|`/input/thumbstick/click`| thumbstickClicked | Boolean |
|`/input/thumbstick/touch`| thumbstickTouched | Boolean |
|`/input/trackpad`| trackpad | Vector2 | 
|`/input/trackpad/touch`| trackpadTouched | Boolean |
|`/input/trackpad/force`| trackpadForce | Float | 
|`/input/grip/pose` | devicePose | Pose |
|`/input/aim/pose` | pointer | Pose |
| Unity Layout Only  | isTracked | Flag Data |
| Unity Layout Only  | trackingState | Flag Data |
| Unity Layout Only  | devicePosition | Vector3 |
| Unity Layout Only  | deviceRotation | Quaternion |
| Unity Layout Only  | pointerPosition | Vector3 |
| Unity Layout Only  | pointerRotation | Quaternion |