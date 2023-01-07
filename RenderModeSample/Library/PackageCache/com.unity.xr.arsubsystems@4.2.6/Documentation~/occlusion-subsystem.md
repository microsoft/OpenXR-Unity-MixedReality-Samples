---
uid: arsubsystems-occlusion-subsystem
---
# Occlusion subsystem

Provider packages for platforms that expose per-frame images representing depth or stencil images can implement the [XROcclsionSubsystem](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystem). Incorporating these depth images into the rendering process are often the best approach for achieving realistic-looking blending of augmented and real-world content by making sure that nearby physical objects occlude virtual content that is located behind them in the shared AR space.

The types of depth images supported are:
- **Environment Depth**: distance from the device to any part of the environment in the camera field of view.
- **Human Depth**: distance from the device to any part of a human recognized within the camera field of view.
- **Human Stencil**: value designating, for each pixel, whether that pixel contains a recognized human.

The subsystem interface allows for:
- Enabling, disabling, and quality configuration for the various supported depth images.
- Querying the availability of each type of depth image.
- Access to the depth image data.
