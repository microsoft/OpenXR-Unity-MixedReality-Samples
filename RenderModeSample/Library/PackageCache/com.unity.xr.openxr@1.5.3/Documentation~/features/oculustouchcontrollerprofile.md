# Oculus Touch Controller Profile

Enables the OpenXR interaction profile for Oculus Touch controllers and exposes the `<OculusTouchController>` device layout within the [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/).  

For more information about the Oculus Touch interaction profile, see the [OpenXR Specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_oculus_touch_controller_profile).

## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
|`/input/thumbstick`| thumbstick | Vector2 |
|`/input/squeeze/value`| grip | Float |
|`/input/squeeze/value`| gripPressed | Float ( float cast to boolean) |
|`/input/menu/click`| menu (Left Hand Only)| Boolean | 
|`/input/system/click`| menu (Right Hand Only)| Boolean | 
|`/input/a/click`| primaryButton (Right Hand Only) | Boolean | 
|`/input/a/touch`| primaryTouched (Right Hand Only) | Boolean | 
|`/input/b/click`| secondaryButton (Right Hand Only) | Boolean | 
|`/input/b/touch`| secondaryTouched (Right Hand Only) | Boolean | 
|`/input/x/click`| primaryButton (Left Hand Only) | Boolean | 
|`/input/x/touch`| primaryTouched (Left Hand Only) | Boolean | 
|`/input/y/click`| secondaryButton (Left Hand Only) | Boolean | 
|`/input/y/touch`| secondaryTouched (Left Hand Only) | Boolean | 
|`/input/trigger/value`| trigger | Float |
|`/input/trigger/value`| triggerPressed | Boolean (float cast to boolean) |
|`/input/trigger/touch`| triggerTouched| Boolean (float cast to boolean) |
|`/input/thumbstick/click`| thumbstickClicked | Boolean |
|`/input/thumbstick/touch`| thumbstickTouched | Boolean |
|`/input/grip/pose` | devicePose | Pose |
|`/input/aim/pose` | pointer | Pose |
| Unity Layout Only  | isTracked | Flag Data |
| Unity Layout Only  | trackingState | Flag Data |
| Unity Layout Only  | devicePosition | Vector3 |
| Unity Layout Only  | deviceRotation | Quaternion |

