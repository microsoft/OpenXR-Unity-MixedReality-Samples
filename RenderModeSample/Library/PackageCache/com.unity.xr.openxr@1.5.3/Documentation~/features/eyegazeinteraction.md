# Eye Gaze Interaction

Unity OpenXR provides support for the Eye Tracking Interaction extension specified by Khronos. Use this layout to retrieve the pose data that the extension returns.

At present, this device does not appear in the Unity Input System drop-down menus. To bind, go the gaze position/rotation, and use the following binding paths.

|**Data**|**Binding Path**|
|--------|------------|
|Position|`<EyeGaze>/pose/position`|
|Rotation|`<EyeGaze>/pose/rotation`|

For more information about the Eye Gaze extension, see the [OpenXR Specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_eye_gaze_interaction).

## Available controls

| OpenXR Path | Unity Control Name | Type |
|----|----|----|
| `/input/gaze_ext/pose` | pose | Pose |

