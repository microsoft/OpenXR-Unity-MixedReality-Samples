---
uid: arfoundation-tracked-object-manager
---
# AR tracked object manager

The tracked object manager is a type of [trackable manager](trackable-managers.md).

![AR tracked object manager](images/ar-tracked-object-manager.png "AR tracked object manager")

The tracked object manager creates `GameObject`s for each detected object in the environment. Before an object can be detected, the manager must be instructed to look for a set of reference objects compiled into a reference image library. It only detects objects in this library.

## Reference library

For instructions on how to create a reference object library, see documentation on the [Tracked Object Subsystem](http://docs.unity3d.com/Packages/com.unity.xr.arsubsystems@4.2/manual/object-tracking.html).

The reference object library can be set at runtime, but as long as the tracked object manager component is enabled, the reference object library must be non-null.

The reference object library is an instance of the `ScriptableObject` `XRReferenceImageLibrary`. This object contains mostly Editor data. The actual library data (containing the object data) is provider-specific. Refer to your provider's documentation for details.

## Creating a manager at runtime

When you add a component to an active `GameObject` at runtime, Unity immediately invokes its `OnEnable` method. However, the `ARTrackedObjectManager` requires a non-null reference object library. If the reference object library is null when the `ARTrackedObjectManager` is enabled, it automatically disables itself.

To add an `ARTrackedObjectManager` at runtime, set its reference object library and then re-enable it:

```csharp
var manager = gameObject.AddComponent<ARTrackedObjectManager>();
manager.referenceLibrary = myLibrary;
manager.enabled = true;
```

## Tracked object Prefab

This Prefab is instantiated whenever an object from the reference object library is detected. The manager ensures the instantiated `GameObject` includes an `ARTrackedObject` component. You can get the reference object that was used to detect the `ARTrackedObject` with the `ARTrackedObject.referenceObject` property.
