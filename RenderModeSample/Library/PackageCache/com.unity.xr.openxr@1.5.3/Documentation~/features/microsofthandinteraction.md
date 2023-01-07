### Microsoft Hand Interaction

Unity OpenXR provides support for the Hololens 2 Hand interaction profile. This layout inherits from `<XRController>` so bindings that use XR Controller and are available on this device (for example, `<XRController>/devicePosition`) will bind correctly.

This interaction profile does not provide hand mesh or hand rig data. These will be added in the future.

For more information about the Microsoft Hand Interaction extension, see the [OpenXR Specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_MSFT_hand_interaction).

## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
|`/input/select/click`| select | Boolean |
|`/input/squeeze/value` | squeeze | Float |
|`/input/grip/pose` | devicePose | Pose |
|`/input/aim/pose` | pointer | Pose |
| Unity Layout Only  | isTracked | Flag Data |
| Unity Layout Only  | trackingState | Flag Data |
| Unity Layout Only  | devicePosition | Vector3 |
| Unity Layout Only  | deviceRotation | Quaternion |
| Unity Layout Only  | pointerPosition | Vector3 |
| Unity Layout Only  | pointerRotation | Quaternion |