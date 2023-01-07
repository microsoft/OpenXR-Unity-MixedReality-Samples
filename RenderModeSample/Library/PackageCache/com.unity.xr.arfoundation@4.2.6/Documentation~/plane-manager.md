---
uid: arfoundation-plane-manager
---
# AR plane manager

The plane manager is a type of [trackable manager](trackable-managers.md).

![AR Plane Manager](images/ar-plane-manager.png "AR Plane Manager")

The plane manager creates GameObjects for each detected plane in the environment. A plane is a flat surface represented by a pose, dimensions, and boundary points. The boundary points are convex.

Examples of features in the environment that can be detected as planes are horizontal tables, floors, countertops, and vertical walls.

You can specify a detection mode, which can be horizontal, vertical, or both. Some platforms require extra work to perform vertical plane detection, so if you only need horizontal planes, you should disable vertical plane detection.

![AR Plane Manager](images/ar-plane-manager-detection-mode.png "AR Plane Manager")

## Responding to planes

Planes can be added, updated, and removed. Once per frame, the AR plane manager can invoke the `planesChanged` event with `List`s of planes that have been added, updated, and removed since the last frame.

When a plane is detected, the AR Plane Manager instantiates the Plane Prefab to represent the plane. The Plane Prefab can stay `null`, but the plane manager ensures the instantiated GameObject has an `ARPlane` component on it. The `ARPlane` component only contains data about the detected plane.

When a plane is updated, it's likely that its boundary vertices have also changed. To respond to this event, subscribe to the plane's `ARPlane.boundaryChanged` event. This event only fires if at least one boundary vertex has changed by at least the **Vertex Changed Threshold**, or if the total number of vertices changes.

## Visualizing planes

To visualize planes, you need to create a Prefab or GameObject which includes a component that subscribes to `ARPlane`'s `boundaryChanged` event. `ARFoundation` provides an `ARPlaneMeshVisualizer`. This component generates a `Mesh` from the boundary vertices and assigns it to a `MeshCollider`, `MeshFilter`, and `LineRenderer`, if present.

To create a new GameObject which you can then use to create your Prefab, right-click in your Scene view and select **GameObject** &gt; **XR** &gt; **AR Default Plane** from the context menu that appears.

![Creating a new AR Default Plane GameObject](images/ar_default_plane.png "AR Default Plane")

After you create the GameObject, assign it to the `ARPlaneManager`'s `Plane Prefab` field. You can use it directly or create a Prefab by dragging the GameObject into your Assets folder. The default plane looks like this:

![Default AR plane](images/ar-default-plane.png "AR Default Plane")

It is recommended to save the `AR Default Plane` as a Prefab first, delete the `AR Default Plane` GameObject, and then use that in the Orefab field, because leaving the plane in your scene leaves a zero scale plane artifact in the scene.

![Default AR Plane prefab](images/ar_default_plane_as_prefab.png "AR Default Plane Prefab")

The AR Foundation package includes these components for ease of use, but you can create your own visualizers (or other logic) as you want.

## Disabling planes

As long as the AR Plane Manager is enabled, it continues to create, update, and remove planes. To stop rendering existing planes, deactivate their GameObjects like this:

```csharp
foreach (var plane in planeManager.trackables)
{
    plane.gameObject.SetActive(false);
}
```

You shouldn't `Destroy` an `ARPlane` while the plane manager is still managing it.
