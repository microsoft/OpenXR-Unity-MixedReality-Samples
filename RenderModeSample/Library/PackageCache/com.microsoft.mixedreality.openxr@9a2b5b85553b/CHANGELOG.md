# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.7.0] - Current Release

* Fixed compatibility with the 1.6.0 release of Unity's OpenXR plugin.
* Fixed bugs where Unity application crashes due to unhandled exceptions in MR plugin.
* Fixed a bug that the Unity MR plugin was not properly cleaned up due to unbalanced module ref count.
* Added new API `AppRemoting.StartConnectingToPlayer` for connect mode app remoting.  It replaces the deprecated `AppRemoting.Connect'.
* Added new API `AppRemoting.StartListeningForPlayer` for listen mode app remoting.  It replaces the deprecated `AppRemoting.Listen'.
* Added new API `AppRemoting.StopListening` to stop listening on the remote app for incoming connections.
* Added new API `AppRemoting.IsReadyToStart` to indicate when app remoting is ready to be started.
* Added new APIs for secure mode app remoting connections, e.g. `AppRemoting.SecureRemotingConnectConfiguration` and `AppRemoting.SecureRemotingListenConfiguration`
* Added new events `AppRemoting.Connected`, `AppRemoting.Disconnecting`, and `AppRemoting.ReadyToStart` in addition to existing `AppRemoting.TryGetConnectionState` function.

## [1.6.0] - 2022-11-02

* Depends on version 1.5.3 of Unity's OpenXR plugin.
    * Fixed a bug where Holographic Remoting remote app may fail connection to remoting player
* Update the remoting OpenXR runtime to 2.8.1 release.
    * Added support for XR_MSFT_spatial_anchor_export extension in remoting OpenXR runtime.
    * Added better support for `SpatialGraphNode.FromStaticNodeId` in remoting OpenXR runtime.
* Added new dependency to com.unity.xr.core-utils package 
    * Changed project settings recommendation for HoloLens 2 to use [Unity's project validation system](https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.1/manual/project-validation.html)
* Added new API `AnchorConverter.CreateFromOpenXRHandle` for creating ARAnchor from OpenXR handle.
* Added new API `ViewConfiguration.StereoSeparationAdjustment` for adjusting the stereo separation on HoloLens 2.
* Supports running Holographic Remoting remote app in elevated process.
* Fixed a bug where remoting app build may fail if building into a non-standard exe name or building a Standalone build with "Create Visual Studio Solution" enabled.
* Fixed a bug in validator which incorrectly recommend disabling "Run in Background" settings.  Enabling this setting can workaround a Unity bug so that Unity app can continue rendering when the app lost keyboard focus.
* Fixed a bug where the Mixed Reality OpenXR Plugin DLL wasn't being included in the build when specific features (Hand Tracking and Mixed Reality Features) weren't checked.

## [1.5.1] - 2022-09-15

* Fixed a bug where apps may be deadlocked and stop rendering due to a race condition when using ARMeshManager to acquire meshes.

## [1.5.0] - 2022-08-31

* Added new API `AppRemoting.TryLocateUserReferenceSpace` to locate the [XR_REMOTING_REFERENCE_SPACE_TYPE_USER_MSFT reference space](https://docs.microsoft.com/windows/mixed-reality/develop/native/holographic-remoting-coordinate-system-synchronization-openxr) in Unity's scene origin space in the remote app.
* The [ControllerModel](https://docs.microsoft.com/dotnet/api/microsoft.mixedreality.openxr.controllermodel) API now also supports loading Quest controller models.
* Fixed a bug where MeshProvider.AcquireMesh might crash in a rare race condition.
* Added warning message for the user to know that app remoting failure was due to the app running in elevated mode.
* Added new [`HandTracker.MotionRange`](https://docs.microsoft.com/dotnet/api/microsoft.mixedreality.openxr.handtracker.motionrange) API to support [hand joints motion range](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_hand_joints_motion_range).
* Fixed a bug where some new anchors were not tracked before their first update.
* Added a validator to remove "Run In Background" project setting for HoloLens 2 apps.
* Added a [SelectKeywordRecognizer](https://docs.microsoft.com/dotnet/api/microsoft.mixedreality.openxr.selectkeywordrecognizer) to allow developers to get notification when the "select" keyword is said on HoloLens 2.
* Added a new property [`ControllerModel.IsSupported`](https://docs.microsoft.com/dotnet/api/microsoft.mixedreality.openxr.controllermodel.issupported) to ControllerModel class.
* Renamed the API `AnchorProvider.FromPerceptionSpatialAnchor` to `AnchorProvider.CreateFromPerceptionSpatialAnchor` and the old method is now deprecated.
* Added support to [`CommonUsages.trackingState`](https://docs.unity3d.com/ScriptReference/XR.CommonUsages-trackingState.html) for hand tracking input device.
* Added support for Unity's [KeywordRecognizer](https://docs.unity3d.com/ScriptReference/Windows.Speech.KeywordRecognizer.html) over Holographic Remoting for Play Mode.
* Added new [`ControllerModelArticulator`](https://docs.microsoft.com/dotnet/api/microsoft.mixedreality.openxr.controllermodelarticulator) API for rendering controller model parts articulation.
* Fixed a potential null reference exception when calling the app remoting APIs on unsupported platforms.
* Removed dependency on Unity.Subsystem.Registration assembly.

## [1.4.4] - 2022-08-08

* Fixed a bug caused by some runtimes reporting an active hand tracker while returning invalid hand joint poses.

* Fixed a bug caused by some runtimes reporting an active hand tracker while returning invalid hand joint poses.

## [1.4.3] - 2022-07-21

* Newly added ARAnchors will now report at least one update through the ARAnchorManager anchorsChanged event.

## [1.4.2] - 2022-07-06

* Fixed a bug where the anchors are located incorrectly after user clear all holograms in user's settings page.
* Fixed a bug where rendering framerate might be dropped due to hand and controller tracking cost.
* Fixed a bug where ARPlanes might be incorrectly reported as updated or removed before they are reported as added.
* Added feature validation to warn developer about missing internet capabilities in appxmanifest for remoting apps.

## [1.4.1] - 2022-06-07

* Depends on version 1.4.2 of Unity's OpenXR plugin.
    * Fixed unnecessary destroying session on pause and resume.
    * Fixed a bug where ARAnchor doesn't relocate properly after suspend and resume on HL2.
    * Fixed an editor crash issue when updating OpenXR package version and then enter Playmode.
* Fixed a bug where AR Subsystems would clear trackables on subsystem stop/restart. Trackables will now only be cleared in this way on subsystem destroy/recreate.
* Fixed a bug where meshes provided through the XRMeshSubsystem could have the wrong 'updated' value.
* Fixed a bug where occlusion-optimized meshes were being computed at the wrong cadence.
* Fixed a bug where the app or Unity editor could crash when an OpenXR extension was not available.
* Fixed a bug where the XRAnchorSubsystem would not report anchors as removed when a remoting session was ended.
* Fixed a bug where the XRAnchorStore could not be loaded after a remoting session had disconnected and reconnected.
* Fixed a bug where loading an anchor from the XRAnchorStore multiple times would result in multiple ARAnchors.
* Upgraded the OpenXR remoting runtime to 2.8.0 release.

## [1.4.0] - 2022-04-05

* Added SpatialGraphNode.FromDynamicNodeId() function to support interop with PV camera tracking.
* Added SpatialGraphNode.TryLocate(long qpcTime) function to support locating space at a historical time.
* Deprecated the "IDisposable" usage of GestureRecognizer, and replaced with "Destroy" function.
* Deprecated the MeshComputeSettings.MeshType property.
* Deprecated the XRAnchorStore.LoadAsync in favor of extension methods LoadAnchorStoreAsync.
* Fixed the ViewConfigurationType enum values to match OpenXR standard.
* Added a setting for application to choose MRC rendering between extra render pass with better hologram alignment using first person observer, or less render pass with better performance but compromise on hologram alignment.
* Support PlayMode remoting when the Unity project turned off "initialize XR at start up" setting, typically for Holographic remoting app.
    * Note: ARFoundation trackable managers will not connect to XR subsystems if the trackable managers are active in the Unity scene before XR initialization. The application must disable then reenable these trackable managers after XR initialization, or wait to add active trackable managers to the scene until after XR initialization.
* Improved the performance by reducing the update events of ARAnchor and ARPlane when there's no location updates from the runtime.
* Upgraded the OpenXR remoting runtime to 2.7.5 release

## [1.3.1] - 2022-02-16

* Fixed a bug where Unity editor sometimes crashes after upgrading the MR OpenXR plugin package.

## [1.3.0] - 2022-02-09

* Fixed a bug where input system sometimes reports identity rotation for controller pose when the Hand Tracking feature was enabled.
* Enabled the "Hand Tracking feature" when it's used together with Unity's Oculus Quest feature.
* Fixed a crash on app resume when using plane finding.

## [1.2.1] - 2021-12-03

* Depends on version 1.3.1 of Unity's OpenXR plugin.
    * Fixed a bug where UWP remoting app won't render desktop view after XR session is started.
    * Fixed a bug where a restart of XR session prevent future restart to happen.
    * Fixed incorrect negative values on controller linear velocities.
* Fixed a bug that prevent UWP app to resume after suspend to background.

## [1.2.0] - 2021-11-18

* Depends on version 1.3.0 of Unity's OpenXR plugin.
    * Supports better HoloLens hand interaction action binding
    * Fixed a crash during app suspend/resume when taking MRC video
* Depends on version 4.2.0 of XR management package.
* Fixed a bug where sometimes the the project settings assets are not created before being used.
* Added Microsoft.MixedReality.OpenXR.Remoting.AppRemoting.Listen function to support listen mode for a Holographic Remoting remote app.
* Added new enum value HandshakePermissionDenied to enum type RemotingDisconnectReason.
* Fixed a bug where after a failed remoting connection the XR session automatically restarted and repeat the failure.
* When hand tracking becomes untracked or out of view, the corresponding InputDevice for hand joints will remain valid and report `isTracked = false`, instead of invalidating the InputDevice.

## [1.1.2] - 2021-10-27

* Fixed a bug where Unity Editor sometimes cannot quit after an unsuccessful connection to Holographic Remoting player in Play Mode.
* Update the OpenXR remoting runtime to 2.7.1 release.

## [1.1.1] - 2021-10-15

* Fixed a bug where projects would fail to build if the project also referenced DotNetWinRT package.

## [1.1.0] - 2021-10-07

* Added new APIs for spatial anchor transfer batch: Microsoft.MixedReality.OpenXR.XRAnchorTransferBatch
* Supports the XRMeshSubsystem through OpenXR scene understanding extensions.
* Supports OpenXR remoting runtime 2.7, with Spatial Anchor Store and Surface mapping in Holographic Remoting apps.
* Removed direct package dependency to ARSubsystem package.  It's now implicit through ARFoundation package.
* Fixed a bug where Unity's UI froze briefly when Holographic Remoting failed to connect to remote player app.
* Fixed a bug where persisted anchor may lead to error saying "An item with the same key has already been added."
* Supports the ratified "XR_MSFT_scene_understanding" extension instead of "_preview3" version.
* Fixed a bug where projects would fail to build with Windows XR Plugin installed and app remoting enabled.

## [1.0.3] - 2021-09-07

* Supports the OpenXR spatial anchor persistence MSFT extension.
* Fixed a bug where Editor Remoting settings are present in PackageSettings instead of UserSettings.
* Fixed a bug where some anchors could fail to be persisted after clearing the XRAnchorStore.
* Fixed a bug where extra anchors were created when switching between Unity scenes.

## [1.0.2] - 2021-08-05

* Depends on Unity's 1.2.8 OpenXR plugin.
* Fixed a bug where ARAnchors were occasionally not removed properly.
* Fixed a bug where invalid ARAnchor changes were occasionally reported after restarting Holographic Remoting for Play Mode.
* Fixed a bug where view configurations were not properly reported when using Holographic Remoting for Play Mode.
* Added more specific settings validation with more precise messages when using Holographic Remoting for Play Mode.
* Added validation for "Initialize XR on Startup" setting when using Holographic Remoting for Play Mode.

## [1.0.1] - 2021-07-13

* Depends on Unity's 1.2.3 OpenXR plugin.
* Updated Holographic Remoting runtime to 2.6.0
* Removed the "Holographic Remoting for Play Mode" feature group from Unity settings UX and kept the feature independent.
* Fixed a bug where build process cannot find the app.cpp when building a XAML type unity project.

## [1.0.0] - 2021-06-18

* Fixed a bug where a the XRAnchorSubsystem was always started on app start regardless ARAnchorManager's present.
* Fixed a bug where the reprojection mode didn't work properly.

## [1.0.0-preview.2] - 2021-06-14

* Depends on Unity's 1.2.2 OpenXR plugin.
* Changed Holographic Remoting features in to individual feature groups.
* Fixed a bug where "Apply HoloLens 2 project settings" changes project color space.  This is no longer needed after Unity OpenXR 1.2.0 plugin.
* Fixed a bug where a input device get connected without disconnect after application suspended and resumed.
* Added support for detecting plugin and current tracking states via ARSession.
* Fixed a bug where the "AR Default Plane" ARFoundation prefab wouldn't be visible.

## [1.0.0-preview.1] - 2021-06-02

* Supports OpenXR scene understanding MSFT extensions instead of preview extensions.
* Plane detection on HoloLens 2 no longer requires preview versions of the Mixed Reality OpenXR runtimes.

## [0.9.5] - 2021-05-21

* Depends on Unity's 1.2.0 OpenXR Plugin
* Adapted to the new feature UI (in OpenXR Plugin 1.2.0) for configuration.
* Fixed a bug where the locatable camera provider wasn't properly unregistering.
* Cleaned up some extra usages of `[Preserve]`.
* Update "HP Reverb G2 Controller (OpenXR)" name in the input system UI.

## [0.9.4] - 2021-05-20

* Depends on Unity's 1.2.0 OpenXR Plugin.
* Added new C# API to get motion controller glTF model.
* Added new C# API to get enabled view configurations and set reprojection settings.
* Added new C# API to set additional settings for computing meshes with XRMeshSubsystem.
* Added new C# API to configure and subscribe to gesture recognition events.
* Added Windows->XR->Editor Remoting settings dialog.
* Added ARM support for HoloLens UWP applications.

## [0.9.3] - 2021-04-29

* Fixed a bug where Holographic remoting connection is not reliable
* Fixed a bug where the VR rendering performance is sub-optimum after upgrade to Unity's 1.1.1 OpenXR plugin.

## [0.9.2] - 2021-04-21

* Plane detection on HoloLens 2 in plugin version 0.9.1 will work with version 105 of the Mixed Reality OpenXR preview runtime.
* Plane detection on HoloLens 2 in plugin version 0.9.2 will work with version 106 of the Mixed Reality OpenXR preview runtime.
* Removed some unused callbacks from InputProvider to prevent calls like XRInputSubsystem.GetTrackingOriginMode (which aren't managed by our input system) from returning success with misleading values.
* Split out deprecated version of XRAnchorStore into its own file to prevent Unity console warning.

## [0.9.1] - 2021-04-20

* Depends on Unity's 1.1.1 OpenXR Plugin.
* Added support for [Holographic Remoting application](https://aka.ms/openxr-unity-app-remoting) for UWP platform.
* Fix UnityException where XRAnchorStore was trying to get a settings instance outside the main thread.

## [0.9.0] - 2021-03-29

* Added support for spatial mapping via XRMeshSubsystem and ARMeshManager.
* Added new C# API to get OpenXR handles to support other Unity packages consumes OpenXR extensions.
* Added new C# API to interop with Windows.Perception APIs to support other Unity packages consuming Perception WinRT APIs.
* Removed interaction profiles from required features in Windows Mixed Reality feature set, so developers can choose the motion controllers they tested with.
* Added Holographic editor remoting feature validator to help users to setup editor remoting properly.
* Fixed a bug where Unity editor crashes when exiting Holographic editor remoting mode after connection failure.
* Fixed a bug where unpremultipled alpha textures leads to sub-optimum performance on HoloLens 2.
* Fixed a bug where hand tracking was not located correctly when the scene origin was at floor level.
* Fixed a bug where hand mesh tracking disappear after leaving and loading a new scene.
* Fixed a bug where locatable camera provider didn't properly clean up.
* Revised the namespace of XRAnchorStore API into Microsoft.MixedReality.OpenXR.

## [0.2.0] - 2021-03-24

* Depends on Unity's 1.0.3 OpenXR Plugin.
* Removed deprecated preview APIs.
* Supports new API "EyeLevelSceneOrigin" for easily setup eye level experience for HoloLens 2.
* Supports plane detection using the ARPlaneSubsystem on HoloLens 2.
* Supports single raycasts for planes using the ARRaycastSubsystem on HoloLens 2.
* Supports new HandMeshTracker API for hand mesh tracking inputs on HoloLens 2.
* Fixed a bug where ARAnchor is not properly reporting tracking state.

## [0.1.5] - 2021-03-15

* Fixed a bug where using an HP Reverb G2 controller lead to errors in the Unity plugin.
* Fixed a bug that the Unity's "Input Debugger" window is blank when using Mixed Reality plugin.

## [0.1.4] - 2021-03-02

* Depends on Unity's 1.0.2 OpenXR Plugin.
* Fixed a bug where SpatialGraphNode's TryLocateSpace's FrameTime parameter was ignored.
* Fixed a bug where hand tracking could occasionally cause a crash.

## [0.1.3] - 2021-02-11

* Adds support for [desktop app holographic remoting](https://aka.ms/openxr-unity-app-remoting).
* Adds support for "SpatialGraphNode" API that bridges to other Mixed Reality tracking libraries, such as QR code tracking.
* Promote "FrameTime" concept from Preview API to supported API.
* Fixed a bug where eye tracking device capability is duplicated in manifest file.
* Fixed a bug where the plugin doesn't compile in Unity 2021.1+.

## [0.1.2] - 2021-01-08

* Depends on Unity's 0.1.2-preview.2
* Fixed unnecessary error message in XRAnchorStore before XR plugin is initialized.
* Fixed a bug where HandTracker's `TryLocateHandJoints` method might throw a `DllNotFoundException` if the DLL wasn't properly loaded. It now returns `false` instead.

## [0.1.1] - 2020-12-18

* Fixed a bug where non-existent sources were being reported disconnected on shutdown, possibly causing errors.
* Fixed a bug that the menu button on HP Reverb G2 didn't bind correctly.
* Changed the SetSceneOrigin script to focus on overriding eye level experience instead.
* Fixed a bug that returns incorrect room boundary on Mixed Reality headset.
* Fixed a bug where sample scene anchor scenarios didn't work with ARFoundation before 4.1.1.

## [0.1.0] - 2020-12-16

### Initial release

This is initial release of *Mixed Reality OpenXR Plugin \<com.microsoft.mixedreality.openxr\>*.

* Supports both UWP applications for HoloLens 2 and Win32 VR applications for Windows Mixed Reality headsets.
* Optimizes UWP package and CoreWindow interaction for HoloLens 2 applications.
* Supports motion controller and hand interactions, including the new HP Reverb G2 controller.
* Supports articulated hand tracking using 26 joints and joint radius inputs.
* Supports eye gaze interaction on HoloLens 2.
* Supports locating PV camera on HoloLens 2.
* Supports mixed reality capture using 3rd eye rendering through PV camera.
* Supports "Play" to HoloLens 2 using Holographic Remoting app, allow developers to debug scripts without build and deploy to the device.
* Compatible with MRTK Unity 2.5.2 through MRTK OpenXR adapter package.
