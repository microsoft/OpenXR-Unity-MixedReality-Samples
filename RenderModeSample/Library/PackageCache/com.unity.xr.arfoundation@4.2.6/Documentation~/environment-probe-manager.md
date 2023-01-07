---
uid: arfoundation-environment-probe-manager
---
# AR environment probe manager

The environment probe manager is a type of [trackable manager](trackable-managers.md).

![AR environment probe manager](images/ar-environment-probe-manager.png "AR Environment Probe Manager")

Environment probes are a technique for capturing real-world imagery from the device’s camera and organizing that information into an environment texture, such as a cube map, that contains the view in all directions from a certain point in the scene. Rendering 3D objects using this environment texture allows for real-world imagery to be reflected in the rendered objects, which creates realistic reflections and lighting of virtual objects as influenced by the real-world views.

For more details, see the [Environment Probe Subsystem](xref:arsubsystems-environment-probe-subsystem) documentation.

Environment probes can be placed manually, automatically, or using both methods.

## Manual placement

> [!NOTE]
> Support for manual placement and removal of environment probes depends on the underlying AR framework's capabilities. Check the [subsystem's descriptor](xref:UnityEngine.XR.ARSubsystems.XREnvironmentProbeSubsystemDescriptor) before attempting to add or destroy an environment probe.

To create an environment probe, add an `AREnvironmentProbe` component to a GameObject using [AddComponent](xref:UnityEngine.GameObject.AddComponent(System.Type)). Like [anchors](anchor-manager.md), the `AREnvironmentProbe` might be in a "pending" state for a few frames.

To remove an environment probe, [Destroy](xref:UnityEngine.Object.Destroy(UnityEngine.Object)) it as you would any component or GameObject.

## Automatic placement

With automatic environment probe placement, the device automatically selects suitable locations for environment probes and creates them.

Environment probes can be created in any orientation. However, Unity's reflection probes, which consume the environment probe data, only support axis-aligned orientations. This means the orientation you specify (or your application selected automatically) might not be fully respected.

## Texture filter mode

This corresponds to the [UnityEngine.FilterMode](https://docs.unity3d.com/ScriptReference/FilterMode.html) for the cubemap that the environment probe generates.

## Debug Prefab

This Prefab is instantiated for each manually or automatically placed environment probe. This is not required, but Unity provides it for debugging purposes.
