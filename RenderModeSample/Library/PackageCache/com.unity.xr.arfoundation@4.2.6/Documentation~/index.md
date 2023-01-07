---
uid: arfoundation-manual
---
# About AR Foundation

AR Foundation allows you to work with augmented reality platforms in a multi-platform way within Unity. This package presents an interface for Unity developers to use, but doesn't implement any AR features itself. To use AR Foundation on a target device, you also need separate packages for the target platforms officially supported by Unity:

* [ARCore XR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.arcore@4.2/manual/index.html) on Android
* [ARKit XR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.arkit@4.2/manual/index.html) on iOS
* [Magic Leap XR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.magicleap@6.0/manual/index.html) on Magic Leap
* [Windows XR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.windowsmr@4.0/manual/index.html) on HoloLens

AR Foundation is a set of `MonoBehaviour`s and APIs for dealing with devices that support the following concepts:

- Device tracking: track the device's position and orientation in physical space.
- Plane detection: detect horizontal and vertical surfaces.
- Point clouds, also known as feature points.
- Anchor: an arbitrary position and orientation that the device tracks.
- Light estimation: estimates for average color temperature and brightness in physical space.
- Environment probe: a means for generating a cube map to represent a particular area of the physical environment.
- Face tracking: detect and track human faces.
- 2D image tracking: detect and track 2D images.
- 3D object tracking: detect 3D objects.
- Meshing: generate triangle meshes that correspond to the physical space.
- Body tracking: 2D and 3D representations of humans recognized in physical space.
- Colaborative participants: track the position and orientation of other devices in a shared AR experience.
- Human segmentation: determines a stencil texture and depth map of humans detected in the camera image.
- Raycast: queries physical surroundings for detected planes and feature points.
- Pass-through video: optimized rendering of mobile camera image onto touch screen as the background for AR content.
- Session management: manipulation of the platform-level configuration automatically when AR Features are enable or disabled.
- Occlusion: allows for occlusion of virtual content by detected environmental depth (environment occlusion) or by detected human depth (human occlusion).

## Platform Support

AR Foundation does not implement any AR features itself but, instead, defines a multi-platform API that allows you to work with functionality common to multiple platforms.

### Feature Support Per Platform

You can refer to this table to understand which parts of AR Foundation are relevant on specific platforms:

|                                |ARCore|ARKit|Magic Leap|HoloLens|
|--------------------------------|:----:|:---:|:--------:|:------:|
|Device tracking                 |  ✓   |  ✓  |    ✓     |   ✓    |
|Plane tracking                  |  ✓   |  ✓  |    ✓     |        |
|Point clouds                    |  ✓   |  ✓  |          |        |
|Anchors                         |  ✓   |  ✓  |    ✓     |   ✓    |
|Light estimation                |  ✓   |  ✓  |          |        |
|Environment probes              |  ✓   |  ✓  |          |        |
|Face tracking                   |  ✓   |  ✓  |          |        |
|2D Image tracking               |  ✓   |  ✓  |    ✓     |        |
|3D Object tracking              |      |  ✓  |          |        |
|Meshing                         |      |  ✓  |    ✓     |   ✓    |
|2D & 3D body tracking           |      |  ✓  |          |        |
|Collaborative participants      |      |  ✓  |          |        |
|Human segmentation              |      |  ✓  |          |        |
|Raycast                         |  ✓   |  ✓  |    ✓     |        |
|Pass-through video              |  ✓   |  ✓  |          |        |
|Session management              |  ✓   |  ✓  |    ✓     |   ✓    |
|Occlusion                       |  ✓   |  ✓  |          |        |

**Note:** To use ARCore cloud anchors, download and install Google's [ARCore Extensions for Unity's AR Foundation](https://developers.google.com/ar/develop/unity-arf).

### Supported Platform Packages
The following platform packages and later implement the AR Foundation features indicated above:

|Package Name            |Version             |
|:---                    |:---                |
|ARCore XR Plug-in       |   4.2              |
|ARKit XR Plug-in        |   4.2              |
|ARKit Face Tracking     |   4.2              |
|Magic Leap XR Plug-in   |   6.0              |
|Windows XR Plug-in      |   5.0              |

## Subsystems

AR Foundation is built on subsystems. A **subsystem** is a platform-agnostic interface for surfacing different types of information. The AR-related subsystems are defined in the [`AR Subsystems`](https://docs.unity3d.com/Packages/com.unity.xr.arsubsystems@4.2/index.html) package and use the namespace `UnityEngine.XR.ARSubsystems`. You occasionally need to interact with the types in the AR Subsystems package.

Each subsystem handles specific functionality. For example, `XRPlaneSubsystem` provides the plane detection interface.

### Providers

A **provider** is a concrete implementation of a subsystem. For example, the `ARCore XR Plugin` package contains the ARCore implementation for many of the AR subsystems.

Because different providers have varying support for specific features, each subsystem also has a descriptor that indicates which specific subsystem features it supports. For example, the `XRPlaneSubsystemDescriptor` contains properties indicating whether it supports horizontal or vertical plane detection.

Each individual provider determines how to implement each subsystem. In general, they wrap that platform's native SDK (for example, ARKit on iOS and ARCore on Android).

# Installing AR Foundation

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

Subsystems are implemented in other packages. To use AR Foundation, you must also install at least one of these platform-specific AR packages from the Package Manager window (menu: **Window** &gt; **Package Manager**):

 - ARKit XR Plug-in
 - ARCore XR Plug-in
 - Magic Leap XR Plug-in
 - Windows XR Plug-in

# Glossary

| **Term** | **Description** |
|-|-|
| **Tracking** | The AR device's ability to determine its relative position and orientation in the physical world. If the environment is too dark, for example, the device might "lose tracking", which means it can no longer accurately report its position. |
| **Trackable** | A real-world feature, such as a planar surface, that the AR device tracks and/or detects. |
| **Feature Point** | A specific point in a point cloud. An AR device uses the device’s camera and image analysis to track specific points in the world, and uses these points to build a map of its environment. These are usually high-frequency elements, such as a knot in a wood-grain surface.|
| **Session** | An AR instance. |
| **Session Space** | The coordinate system relative to the beginning of the AR session. For example, session space (0, 0, 0) refers to the position at which the AR session was created. An AR device typically reports trackables and tracking information relative to its session origin.|

# Using AR Foundation

## Samples

For examples, see the [ARFoundation Samples](https://github.com/Unity-Technologies/arfoundation-samples) GitHub repository.

## Provider plug-in setup

Provider plug-ins must be enabled before AR Foundation can use them. You can enable specific plug-in providers for each target platform from the [XR Plug-in Management](https://docs.unity3d.com/2020.2/Documentation/Manual/configuring-project-for-xr.html) window.

![XR Plug-in Management](images/enable-arcore-plugin.png "XR Plug-in Management")

## Scene setup

A basic AR scene hierarchy looks like this:

![Scene graph](images/simple_scene_graph.png "Scene graph")

To create these scenes automatically, right-click in the scene Hierarchy window, and select **XR** &gt; **AR Session** or **XR** &gt; **AR Session Origin** from the context menu.

![Context menu](images/gameobject_context_menu.png "Context menu")

The required components are explained in more detail below.

### ARSession

An AR scene should include an `ARSession` component. The AR Session controls the lifecycle of an AR experience by enabling or disabling AR on the target platform. The `ARSession` can be on any `GameObject`.

![ARSession component](images/ar-session.png "ARSession component")

When you disable the `ARSession`, the system no longer tracks features in its environment, but if you enable it at a later time, the system attempts to recover and maintain previously-detected features.

If you enable the **Attempt Update** option, the device tries to install AR software if possible. Support for this feature is platform-dependent.

> [!NOTE]
> An AR session is a global construct. An `ARSession` component manages this global session, so multiple `ARSession` components will all try to manage the same global session.

#### Checking for device support

Some platforms might support a limited subset of devices. On these platforms, your application needs to be able to detect support for AR Foundation so it can provide an alternative experience when AR is not supported.

The `ARSession` component has a static coroutine that you can use to determine whether AR is supported at runtime:

```csharp
public class MyComponent {
    [SerializeField] ARSession m_Session;

    IEnumerator Start() {
        if ((ARSession.state == ARSessionState.None) ||
            (ARSession.state == ARSessionState.CheckingAvailability))
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            // Start some fallback experience for unsupported devices
        }
        else
        {
            // Start the AR session
            m_Session.enabled = true;
        }
    }
}
```

#### Session state

To determine the current state of the session (for example, whether the device is supported, if AR software is being installed, and whether the session is working), use `ARSession.state`. You can also subscribe to an event when the session state changes: `ARSession.stateChanged`.

|`ARSessionState`|**Description**|
|-|-|
|`None`|The AR System has not been initialized and availability is unknown.|
|`Unsupported`|The current device doesn't support AR.|
|`CheckingAvailability`|The system is checking the availability of AR on the current device.|
|`NeedsInstall`|The current device supports AR, but AR support requires additional software to be installed.|
|`Installing`|AR software is being installed.|
|`Ready`|AR is supported and ready.|
|`SessionInitialized`|An AR session is initializing (that is, starting up). This usually means AR is working, but hasn't gathered enough information about the environment.|
|`SessionTracking`|An AR session is running and is tracking (that is, the device is able to determine its position and orientation in the world).|

### AR Session Origin

![AR session origin](images/ar-session-origin.png "AR Session Origin")

The purpose of the `ARSessionOrigin` is to transform trackable features, such as planar surfaces and feature points, into their final position, orientation, and scale in the Unity scene. Because AR devices provide their data in "session space", which is an unscaled space relative to the beginning of the AR session, the `ARSessionOrigin` performs the appropriate transformation into Unity space.

This concept is similar to the difference between "model" or "local" space and world space when working with other Assets in Unity. For instance, if you import a house asset from a DCC tool, the door's position is relative to the modeler's origin. This is commonly called "model space" or "local space". When Unity instantiates it, it also has a world space that's relative to Unity's origin.

Likewise, trackables that an AR device produces, such as planes, are provided in "session space", relative to the device's coordinate system. When instantiated in Unity as `GameObject`s, they also have a world space. In order to instantiate them in the correct place, AR Foundation needs to know where the session origin should be in the Unity scene.

`ARSessionOrigin` also allows you to scale virtual content and apply an offset to the AR Camera. If you're scaling or offsetting the `ARSessionOrigin`, then its AR Camera should be a child of the `ARSessionOrigin`. Because the AR Camera is session-driven, this setup allows the AR Camera and detected trackables to move together.

#### Scale

To apply scale to the `ARSessionOrigin`, set its `transform`'s scale. This has the effect of scaling all the data coming from the device, including the AR Camera's position and any detected trackables. Larger values make AR content appear smaller. For example, a scale of 10 would make your content appear 10 times smaller, while 0.1 would make your content appear 10 times larger.

### AR Pose Driver

The `AR Pose Driver` drives the local position and orientation of the parent GameObject according to the device's tracking information.  The most common use case for this would be attaching the `ARPoseDriver` to the AR Camera to drive the camera's position and orientation in an AR scene.

![AR Pose Driver](images/ar-pose-driver.png "AR Pose Driver")

#### Legacy Input Helpers and the Tracked Pose Driver component

The `ARPoseDriver` provides a similar functionality to the `TrackedPoseDriver` from the `com.unity.xr.legacyinputhelpers` package and was implemented to remove the dependency on that package. Projects are able to use either the `ARPoseDriver` component or the `TrackedPoseDriver` component to drive a GameObjects transform. It is not recommended to use both as the behaviour is undefined. `Use Relative Transform` option is unavailable for the `ARPoseDriver` because it introduces additional unnecessary transformations.

![Tracked Pose Driver](images/tracked-pose-driver.png "Tracked Pose Driver")

### AR Camera manager

The `ARCameraManager` enables features for the AR Camera, including the management of the device camera textures and the properties that set the light estimation modes.

![AR Camera Manager](images/ar-camera-manager.png "AR Camera Manager")

| **Setting** | **Function** |
|-|-|
| **Auto Focus** | Enables or disables the hardware camera's automatic focus mode. When disabled, the focus is fixed and doesn't change automatically. _Note:_ Availability of *Auto Focus* depends on camera hardware so this preference might be ignored at runtime. |
| **Light Estimation** | Estimates lighting properties of the environment. There are five options: <ul><li><b>Ambient Intensity:</b> Estimates the overall average brightness</li><li><b>Ambient Color:</b> Estimates the overall average color</li><li><b>Ambient Spherical Harmonics:</b> Estimates the [spherical harmonics](https://en.wikipedia.org/wiki/Spherical_harmonic_lighting) describing the scene. Spherical harmonics are used to produce more realistic lighting calculations.</li><li><b>Main Light Direction:</b> Estimates the direction of the primary light source. The direction points away from the light (so that it matches the light's direction).</li><li><b>Main Light Intensity:</b> Estimates the brightness of the primary light source.</li></ul>While you can request any of these simultaneously, support for each of these varies greatly among devices. Some platforms may not be able to be simultaneously provide all options, or it may depend on other features (for example, camera facing direction). |
| **Facing Direction** | Controls which camera is used for pass through video. This can be World or User. On handheld mobile devices like phones and tablets, World refers to the rear camera and User refers to the front-facing (that is, "selfie") camera. |

### AR Camera background

If you want the video feed from the device camera to show up as the rendered background of the scene at runtime, you need to add an `ARCameraBackground` component to a Camera. Otherwise, the background at runtime comes from the `Camera.clearFlags` setting. The `ARCameraBackground` component subscribes to AR Camera events and renders the AR Camera texture to the screen (that is, the background texture from the device camera must be rendered for each frame). This is not required, but common for AR apps.

![AR Camera Background](images/ar-camera-background.png "AR Camera Background")

The `Custom Material` property is optional, and typically you don't need to set it. The platform-specific packages that Unity provides, such as ARCore and ARKit, contain their own shaders for background rendering.

If `Use Custom Material` is `true`, the `ARCameraBackground` uses the `Material` you specify for background rendering.

If you have exactly one `ARSessionOrigin`, you only need to add the `ARCameraBackground` to that Camera. If you have multiple `ARSessionOrigin`s (for example, to selectively render different content at different scales), you should use separate Cameras for each `ARSessionOrigin` and a separate, single AR Camera for the `ARCameraBackground`.

#### Configuring ARCameraBackground with the Universal Render Pipeline (URP)

See [this additional documentation to configure an AR Foundation project with a URP](ar-camera-background-with-scriptable-render-pipeline.md).

#### Automatic occlusion

Some devices offer depth information about the real world. For instance, with a feature known as person occlusion, iOS devices with the A12 Bionic chip (and newer) provide depth information for humans detected in the AR Camera frame. Newer Android phones and iOS devices equipped with a LiDAR scanner can provide an environment depth image where each pixel contains a depth estimate between the device and physical surroundings.

Adding the `AROcclusionManager` component to the Camera with the `ARCameraBackground` component automatically enables the background rendering pass to incorporate any available depth information when rendering the depth buffer. This allows for rendered geometry to be occluded by detected geometry from the real world. For example, in the case of iOS devices that support person occlusion, detected humans occlude rendered content that exists behind them.

#### Copying the Camera Texture to a Render Texture when accessing the camera image on the GPU

Camera Textures are likely [external Textures](https://docs.unity3d.com/ScriptReference/Texture2D.CreateExternalTexture.html) and might not last beyond a frame boundary. It can be useful to copy the Camera image to a [Render Texture](https://docs.unity3d.com/Manual/class-RenderTexture.html) to persist it or process it further. The following code sets up a [command buffer](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.html) that will [clear the render target](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.ClearRenderTarget.html) and then perform a GPU copy or ["blit"](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.Blit.html) to a Render Texture of your choice immediately:

```csharp
var commandBuffer = new CommandBuffer();
commandBuffer.name = "AR Camera Background Blit Pass";
var texture = !m_ArCameraBackground.material.HasProperty("_MainTex") ? null : m_ArCameraBackground.material.GetTexture("_MainTex");
Graphics.SetRenderTarget(renderTexture.colorBuffer, renderTexture.depthBuffer);
commandBuffer.ClearRenderTarget(true, false, Color.clear);
commandBuffer.Blit(texture, BuiltinRenderTextureType.CurrentActive, m_ArCameraBackground.material);
Graphics.ExecuteCommandBuffer(commandBuffer);
```

Note: [`Graphics.SetRenderTarget`](https://docs.unity3d.com/ScriptReference/Graphics.SetRenderTarget.html) will overwrite the current render target after executing the command buffer.

### Accessing the Camera Image on the CPU

See documentation on [camera images](cpu-camera-image.md).

### AR input manager

This component is required to enable world tracking. Without it, the Tracked Pose Driver can't acquire a pose for the device.

This component can be anywhere in your Scene, but you shouldn't have more than one.

![AR Input Manager](images/ar-input-manager.png "AR Input Manager")

### Trackable managers

See documentation on [trackable managers](trackable-managers.md).

### Visualizing trackables

Trackable components don't do anything on their own; they contain data associated with each trackable. There are many ways to visualize trackables, so AR Foundation includes some visualizers that you can use for debugging or as a starting point to create a visualizer suitable for your application.

## Ray casting

See [ARRaycastManager](raycast-manager.md).

## Meshing

See [ARMeshManager](mesh-manager.md).

# Technical details

## Requirements

This version of AR Foundation is compatible with the following versions of the Unity Editor:

* 2020.3
* 2021.1
* 2021.2
