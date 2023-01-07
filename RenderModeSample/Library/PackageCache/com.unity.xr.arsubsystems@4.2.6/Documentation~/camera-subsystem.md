---
uid: arsubsystems-camera-subsystem
---
# XR camera subsystem

The camera subsystem manages a hardware camera on the AR device. It provides the following data:

- Camera image (as an "external" texture on the GPU).
- Camera image (as a buffer of bytes available on the CPU).
- Projection matrix, used to set the field of view and other properties of the virtual camera according to the physical one.
- Display matrix, used to orient the camera image correctly.
- Camera intrinsics that describe a mathematical model of the camera, useful for computer vision algorithms.
- Camera conversion utilities for converting the CPU image to RGB and grayscale.
- Light estimation information (color and brightness of the environment).
- Camera focus mode (autofocus or fixed). 

For API details, see the [XRCameraSubsystem](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystem).
