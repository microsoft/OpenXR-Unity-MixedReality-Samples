---
uid: arfoundation-point-cloud-manager
---
# AR point cloud manager

The point cloud manager is a type of [trackable manager](trackable-managers.md).

![AR Point Cloud Manager](images/ar-point-cloud-manager.png "AR Point Cloud Manager")

The point cloud manager creates point clouds, which are sets of feature points. A feature point is a specific point in the point cloud which the device uses to determine its location in the world. Feature points are typically notable features in the environment that the device can track between frames, such as a knot in a wooden table.

A point cloud is a set of feature points that can change from frame to frame. Some platforms only produce one point cloud, while others organize their feature points into different point clouds in different areas of space.

A point cloud is considered a trackable, while individual feature points are not. However, feature points can be uniquely identified between frames as they have unique identifiers.

## Feature point properties

Each feature point has a position, and optionally an identifier and confidence value. These are stored as parallel arrays of [Vector3](xref:UnityEngine.Vector3), `ulong`, and `float`, respectively.

### Position

Each feature point has a 3D position, reported in session space. You can access positions via [`ARPointCloud.positions`](xref:UnityEngine.XR.ARFoundation.ARPointCloud.positions).

### Identifier

Each feature point can have a unique identifier, represented as a `ulong`. You can access identifiers via [`ARPointCloud.identifiers`](xref:UnityEngine.XR.ARFoundation.ARPointCloud.identifiers).

This array is parallel to `positions`. This feature varies by provider. Check the `ARPointCloudManager.descriptor`.

### Confidence value

Feature points can also have confidence values, represented as `float`s in the 0..1 range. You can access confidence values via [`ARPointCloud.confidenceValues`](xref:UnityEngine.XR.ARFoundation.ARPointCloud.confidenceValues).

This array is parallel to `positions`. This feature varies by provider. Check the `SubsystemDescriptor` (`ARPointCloud.descriptor`).
