---
uid: arfoundation-mesh-manager
---
# AR mesh manager

Some platforms provide a meshing feature that generates a mesh based on scanned real-world geometry. The mesh manager enables and configures this functionality on supported platforms.

## Requirements

See documentation for your platform-specific package to confirm whether meshing is supported.

Additionally, not all devices provide support for all the features in the mesh manager. Some properties in the mesh manager are ignored on certain platforms. The platform-specific package documentation details which meshing features are available for each platform.

## Using meshing in a scene

To use meshing with AR Foundation, you need to add the [ARMeshManager](xref:UnityEngine.XR.ARFoundation.ARMeshManager) component to your scene.

![ARFoundation ARMeshManager component](images/arfoundation-mesh-manager.png)

### Mesh Prefab

You need to set the [meshPrefab](xref:UnityEngine.XR.ARFoundation.ARMeshManager.meshPrefab) to a Prefab that is instantiated for each scanned mesh. The [meshPrefab](xref:UnityEngine.XR.ARFoundation.ARMeshManager.meshPrefab) must contain at least a [MeshFilter](xref:UnityEngine.MeshFilter) component.

If you want to render the scanned meshes, you need to add a [MeshRenderer](xref:UnityEngine.MeshRenderer) component and a [Material](xref:UnityEngine.Material) component to the [meshPrefab](xref:UnityEngine.XR.ARFoundation.ARMeshManager.meshPrefab)'s GameObject.

If you want to include virtual content that interacts physically with the real-world scanned meshes, you must add a [MeshCollider](xref:UnityEngine.MeshCollider) component to the [meshPrefab](xref:UnityEngine.XR.ARFoundation.ARMeshManager.meshPrefab)'s GameObject.

This image demonstrates a mesh Prefab configured with the required [MeshFilter](xref:UnityEngine.MeshFilter) component, an optional [MeshCollider](xref:UnityEngine.MeshCollider) component to allow for physics interactions, and optional [MeshRenderer](xref:UnityEngine.MeshRenderer) and [Material](xref:UnityEngine.Material) components to render the mesh.

![Mesh prefab example](images/arfoundation-mesh-prefab.png)

### Density

The [density](xref:UnityEngine.XR.ARFoundation.ARMeshManager.density) property, in the range 0 to 1, specifies the amount of tessellation to perform on the generated mesh. A value of 0 results in the least amount tessellation, whereas a value of 1 produces the most tessellation.

Not all platforms support this feature.

### Normals

When the device is constructing the mesh geometry, it might calculate the vertex normals for the mesh. If you don't need the mesh vertex normals, disable [normals](xref:UnityEngine.XR.ARFoundation.ARMeshManager.normals) to save on memory and CPU time.

Not all platforms support this feature.

### Tangents

When the device is constructing the mesh geometry, it might calculate the vertex tangents for the mesh. If you don't need the mesh vertex tangents, disable [tangents](xref:UnityEngine.XR.ARFoundation.ARMeshManager.tangents) to save on memory and CPU time.

Not all platforms support this feature.

### Texture coordinates

When the device is constructing the mesh geometry, it might calculate the vertex texture coordinates for the mesh. If you don't need the mesh vertex texture coordinates, disable [textureCoordinates](xref:UnityEngine.XR.ARFoundation.ARMeshManager.textureCoordinates) to save on memory and CPU time.

Not all platforms support this feature.

### Colors

When the device is constructing the mesh geometry, it might calculate the vertex colors for the mesh. If you don't need the mesh vertex colors, disable [colors](xref:UnityEngine.XR.ARFoundation.ARMeshManager.colors) to save on memory and CPU time.

Not all platforms support this feature.

### Concurrent queue size

To avoid blocking the main thread, the tasks of converting the device mesh into a Unity mesh and creating the physics collision mesh (if the [meshPrefab](xref:UnityEngine.XR.ARFoundation.ARMeshManager.meshPrefab)'s GameObject contains a [MeshCollider](xref:UnityEngine.MeshCollider) component) are moved into a job queue processed on a background thread. [concurrentQueueSize](xref:UnityEngine.XR.ARFoundation.ARMeshManager.concurrentQueueSize) specifies the number of meshes to be processed concurrently.
