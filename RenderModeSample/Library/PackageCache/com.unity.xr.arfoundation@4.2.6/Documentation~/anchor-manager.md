---
uid: arfoundation-anchor-manager
---
# AR anchor manager

The anchor manager is a type of [trackable manager](trackable-managers.md).

![AR anchor manager](images/ar-anchor-manager.png "AR anchor manager")

The anchor manager creates `GameObject`s for each anchor. An anchor is a particular point in space that you want the device to track. The device typically performs additional work to update the position and orientation of the anchor throughout its lifetime. Because anchors are generally resource-intensive objects, you should use them sparingly.

## Anchor Prefab

While the [ARAnchorManager](xref:UnityEngine.XR.ARFoundation.ARAnchorManager) has an ["Anchor Prefab"](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.anchorPrefab) field, this is not intended for content. When an anchor is created in some way other than `AddComponent`, such as loading an [ARWorldMap](https://docs.unity3d.com/Packages/com.unity.xr.arkit@4.2/api/UnityEngine.XR.ARKit.ARWorldMap.html) that contained anchors, AR Foundation creates a new [GameObject](xref:GameObjects) to represent it.

If "Anchor Prefab" is `null`, AR Foundation creates a GameObject with an `ARAnchor` component on it. However, if you want the anchor to also include additional components, you can provide a Prefab for AR Foundation to instantiate for the anchor. In other words, the Prefab field extends the default behavior of anchors, but it is not the recommended way to place content in the world.

## Adding and removing anchors

To add a new anchor, add the [ARAnchor](xref:UnityEngine.XR.ARFoundation.ARAnchor) component to any GameObject using [AddComponent&lt;ARAnchor&gt;](xref:UnityEngine.GameObject.AddComponent). Anchors can also be created indirectly, for example by loading an [ARWorldMap](https://docs.unity3d.com/Packages/com.unity.xr.arkit@4.2/api/UnityEngine.XR.ARKit.ARWorldMap.html) which includes saved anchors.

You should not change the [transform](xref:UnityEngine.Transform) of an anchor; its transform is updated automatically by ARFoundation.

When you add an anchor, it might take a frame or two before the anchor manager's [anchorsChanged](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.anchorsChanged) event reports it as added. During the time between being added and being reported as added, the anchor is in a "pending" state. You can query for this with the [ARAnchor.pending](xref:UnityEngine.XR.ARFoundation.ARTrackable`2.pending) property.

To remove an anchor, [Destroy](xref:UnityEngine.Object.Destroy(UnityEngine.Object)) the `ARAnchor` component (or its GameObject).

> [!NOTE]
> When you `Destroy` an `ARAnchor`, you will not receive an `anchorsChanged` event for it.

## Anchoring content

A typical use case for anchors is to place virtual content in the physical world.

**Examples**:

To make existing content an anchor:

[!code-cs[anchor_existing_content](../Tests/CodeSamples/AnchorSamples.cs#anchor_existing_content)]

To instantiate a prefab as an anchor:

[!code-cs[anchor_prefab_content](../Tests/CodeSamples/AnchorSamples.cs#anchor_prefab_content)]

## Attaching anchors

You can also create anchors that are attached to a plane. The [AttachAnchor method](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.AttachAnchor(UnityEngine.XR.ARFoundation.ARPlane,UnityEngine.Pose)) does this:

```csharp
public ARAnchor AttachAnchor(ARPlane plane, Pose pose);
```

Attaching an anchor to a plane affects the anchor update semantics. This type of anchor only changes its position along the normal of the plane to which it is attached, thus maintaining a constant distance from the plane.

A typical use case for attaching an anchor to a plane is to place virtual content on the plane. Unity recommends creating the anchor with `AttachAnchor`, and then parenting your content to the anchor.

