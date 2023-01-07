# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.5.3] - 2022-09-18
### Fixed
* Fixed `XRInputSubsystem.TryGetBoundaryPoints` returning inaccurate values. If you have guardian/boundary setup in the headset, `TryGetBoundaryPoints` will return a List<Vector3> of size 4 representing the four vertices of the Play Area rectangle, which is centered at the origin and edges corresponding to the X and Z axes of the provided space. Not all systems or spaces may support boundaries.
* Fixed an issue that controllers position not getting updated and stuck to the floor level when Oculus Integration Asset installed in the project.
* Fixed an issue that OpenXR libraries were included in build when OpenXR SDK is not enabled.
* Improved domain reload performance by removing unnecessary checks when entering Playmode.

### Updated
* Updated Input System dependency to 1.4.2.

### Added
* Added support for Android cross-vendor loader.

## [1.5.1] - 2022-08-11
### Fixed
* Fixed compilation errors on Game Core platforms where `ENABLE_VR` is not currently defined. Requires Input System 1.4.0 or newer.
* Fixed an issue that was causing Oculus Android Vulkan builds rendering broken after sleep / awake HMD.
* Fixed Oculus controllers tracking issues when both OpenXR package and Oculus package are installed in the project.
* Fixed an issue in Play Mode OpenXR Runtime selection that `Other` option would be reverted to `System Default` after entering and exiting playmode.
* Fixed a bug in `XR_VARJO_quad_views` support.

### Updated
* Updated Oculus Android manifest with focusAware, supportedDevices and headTracking tags added. Also added a new validation rule to check if Oculus target devices are selected.
* Updated OpenXR Loader to 1.0.23.
* Updated Input System dependency to 1.4.1.

### Added
* Added generic Project Validation status in the **Project Settings** window under **XR Plug-in Management** if you have [XR Core Utilities](https://docs.unity3d.com/Packages/com.unity.xr.core-utils@latest) 2.1.0 or later installed. These results include the checks for all XR plug-ins that provide validation rules.
* Added API `OpenXRFeature.SetEnvironmentBlendMode` to set the current XR Environment Blend Mode if it is supported by the active runtime. If not supported, fall back  to the runtime preference.
* Added API `OpenXRFeature.GetEnvironmentBlendMode` to return the current XR Environment Blend Mode.
* Added support for `XR_MSFT_holographic_windown_attachment` extension on UWP so that installing Microsoft Mixed Reality OpenXR Plug-in is no longer required if targeting HoloLens V2 devices. And removed corresponding project validator. 
* Added support for `XR_FB_foveation`, `XR_FB_foveation_configuration`, `XR_FB_swapchain_update_state`, `XR_FB_foveation_vulkan` and `XR_FB_space_warp` extensions.
* Added ability to recover the application after Oculus Link was aborted and re-established. Attempt to restart every 5 seconds after Link disconnected.
* Added validation rule for duplicate settings in OpenXRPackageSettings.asset.

## [1.4.2] - 2022-05-12
### Fixed
* Fixed unnecessary destroying session on pause and resume.

## [1.4.1] - 2022-04-13
### Fixed
* Fixed an issue that would cause failure to load OpenXR loader when non-ascii characters in project path.
* Fixed an editor crash issue when updating OpenXR package version and then enter Playmode.
* Fixed `EyeGaze` functionality not working in the `Controller` sample.
* Fixed Oculus `MenuButton` not being recognized in script.
* Fixed an issue that some OpenXR Editor settings not being serialized properly.
* Fixed `Failed to suggest bindings for interaction profile` console error spamming when a runtime doesn't support a certain interaction profile.

### Updated
* Updated OpenXR Loader to 1.0.20.
* Updated Render Mode naming to Single Pass Instanced / Multiview for Android platform.
* Updated Input System dependency to 1.3.0.
* Updated XR mirror view to be based on the occlusion mesh line loop data obtained from `xrGetVisibilityMaskKHR`.

### Added
* Added runtime failures handling to completely shut down OpenXR when runtime error occurred.
* Added support to dynamically discover runtimes by registry key.
* Added logging for no MainCamera tag detected when depthSubmission mode enabled.
* Added console error logging if entering playmode on unsupported platforms.
* Added support to automatically open OpenXR project validator if any issues detected after package update.
* Added API `OpenXRFeature.GetViewConfigurationTypeForRenderPass`, which returns viewConfigurationType for the given renderPass index.
* Added pre-init support for UWP / WSA platform. Note: OpenXR got unchecked by upgrading to this version (only on UWP), but options chosen under `Features` remained as they were.

## [1.3.1] - 2021-11-17
### Fixed
* Fixed an issue in `OpenXRRestarter` that would prevent a subsequent restart.
* Fixed an issue in `OpenXRRestarter` that would cause a restart to fail depending on where it was initiated in the stack.
* Fixed an issue in UWP that would prevent the main window from being rendered to when using remoting.
* Fixed incorrect negative values on controller linear velocities.
* Fixed a bug in HMD trackingState that would cause tracking state to flip back and forth between two states every frame.

### Updated
* Updated documentation link to 1.3.
* Updated Oculus Android manifest with intent-filter.

## [1.3.0] - 2021-10-20
### Added
* Added API `OpenXRInput.SendHapticImpulse`
* Added API `OpenXRInput.StopHaptics`
* Added API `OpenXRInput.TryGetInputSourceName`
* Added event `OpenXRRuntime.wantsToRestart`
* Added event `OpenXRRuntime.wantsToQuit`
* Added support for `XR_OCULUS_audio_device_guid
` extension.
* Added `Haptic` control to OpenXR Controller Profile layouts that can be bound to an action and used to trigger haptics using the new `OpenXRInput.SendHapticImpulse` API.

### Fixed
* Fixed ARM32 crash when OpenXR API layers were present
  * [ISSUE-1355859](https://issuetracker.unity3d.com/issues/xr-uwp-any-openxr-layer-will-crash-unityopenxr-dot-dll-on-hololens2-when-compiling-with-arm32)
* Fixed issue that would cause console errors if `OpenXRFeature.enable` was changed while  the OpenXR Projects Settings windows was open.
* Fixed potential Android crash when shutting down.
* Fixed potential crash with `XR_MSFT_SECONDARY_VIEW_CONFIGURATION_EXTENSION_NAME`
* Fixed issue with alpha channel on Projection layer causing visual artifacts.
  * [ISSUE-1338699](https://issuetracker.unity3d.com/issues/xr-openxr-a-black-background-is-rendered-in-hmd-when-using-solid-color-camera-clear-flags)
  * [ISSUE-1349271](https://issuetracker.unity3d.com/issues/xr-sdk-openxr-post-processing-postexposure-effect-has-visual-artifacts)
  * [ISSUE-1346455](https://issuetracker.unity3d.com/issues/xr-sdk-openxr-right-eye-contains-artifacts-when-taa-anti-aliasing-is-used-with-multi-pass-rendering-mode)
  * [ISSUE-1336968](https://issuetracker.unity3d.com/issues/xr-openxr-oculus-post-processing-view-in-hmd-is-darker-and-sometimes-flickers-when-changing-depth-of-field-focus-distance)
  * [ISSUE-1338696](https://issuetracker.unity3d.com/issues/xr-sdk-openxr-black-edges-are-rendered-on-the-game-objects-when-fxaa-anti-aliasing-is-used)
* Fixed bug that caused Vulkan support on Oculus to stop working.
* Fixed missing usages on `HoloLensHand` layout.
* Fixed vulkan shared depth buffer on versions `2021.2.0b15` and `2022.1.0a8` or higher.

### Updated
* Improved `OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets` so that no internal methods are required after calling this.
* Updated `Controller` sample to improve the visuals and use the new APIs

## [1.2.8] - 2021-07-29
* Fixed an issue that was causing Oculus Android OpenGL builds to stop working after v31 of the oculus software was installed.
* Fixed a bug that would cause Asset Bundles to fail building in some circumstances when OpenXR was included in the project.
* Fixed a crash that would occur if XR was shut down from within a Feature callback.
* Fixed a bug that was causing duplicate entries in the OpenXR Package Settings file.
* Fixed a bug causing angular velocities on both the HMD and controllers to have the wrong sign when compared to the other Unity XR plugins

## [1.2.3] - 2021-06-17
* Updated OpenXR Loader to 1.0.17 
* Fixed missing haptic output on HTC Vive controller profile
* Fixed bug that caused `ThumbstickTouched` binding on the `ValveIndex` controller profile to not work.
* Changed `feature set` text to `feature group` in the top level XR-Management list
* Fixed issue that would cause the app to not exit when closed by the runtime
* Ensured a deterministic order of features within the OpenXR Settings

## [1.2.2] - 2021-06-01
* Fixed a bug when using SteamVR runtime that would cause the view to stop rendering and input to stop tracking if the main thread stalled for too long.
* Fixed bug with feature sets that could cause the XR Management check box to be out of sync with the checkbox on the OpenXR Settings page.
* Fixed bug with HTC Vive controller profile that caused the `aim` and `grip` poses to be identical.
* Renamed `Feature Sets` to `Feature Groups` in the UI.
* Removed unnecessary build hook for `EyeGaze` that was causing incorrect capabilities to be set on `HoloLens2`.
* Editor runtime override will no longer change the runtime for standalone builds executed using `Build and Run`.

## [1.2.0] - 2021-05-06

* Enabled Android build target for Oculus Quest via the `Oculus Quest Support` feature.
* Removed `experimental` text from OpenXR plugin help icon.
* Removed `Linear` color space restriction for all build targets and graphics apis except OpenGLES.
* Fixed bug with haptics that caused `XRControllerWithRumble.SendImpulse` to not work with `OpenXR`.
* Fixed bug that could cause some interaction profile device layouts to not be registered on startup.
* OpenXR Settings UI reworked to make managing features and interaction profiles easier.
* Added menu item to open Project Validation window (`Window > XR > OpenXR > Project Validation`).
* Project validation window now supports manual fixes for an issue.
* Project validation window now supports optional help links for an issue.
* Added `OpenXRFeature.OnEnableChanged` method to give features a chance to handle their enabled state changing.
* Added `IPackageSettings.GetFeatures` method that returns all features of a given type from all build targets.

## [1.1.1] - 2021-04-06
* Oculus controller profile now exposes both grip and aim poses.
* Fixed issue where OpenXR layouts were not visible in the InputSystem bindings dialog.
* `OpenXRSettings.renderMode` and `OpenXrSettings.depthSubmissionMode` can now be changed via script outside of play mode.
* Fix for managed stripping levels of Medium and High
* Fixed bugs in `XR_KHR_VULKAN_ENABLE2` extension support
* Added support for `XR_VARJO_QUAD_VIEWS` extension 
* Added `XR_COMPOSITION_LAYER_UNPREMULTIPLIED_ALPHA_BIT` and `XR_COMPOSITION_LAYER_BLEND_TEXTURE_SOURCE_ALPHA_BIT` bits to the composition layer flags 
* Added `XrSecondaryViewConfigurationSwapchainCreateInfoMSFT` to to `XrSwapchainCreateInfo` when using a secondary view
* MockRuntime First Person Observer View support
* MockRuntime input support
* MockRuntime vulkan_enable2 support
* MockRuntime d3d11_enable support

## [1.0.2] - 2021-02-04
* Resolve further release verification issues.

## [1.0.1] - 2021-02-03
* Resolve release verification issues.

## [1.0.0] - 2021-01-27
* Runtime Debugger to allow for the inspection of OpenXR calls that occur while OpenXR is actively running.
* XR Plug-In Management dependency update to 4.0.1
* Input System dependency updated to 1.0.2

## [0.1.2-preview.3] - 2021-01-19

* Implemented `XR_KHR_loader_init` and `XR_KHR_loader_init_android`.
* Updated dependency of `com.unity.xr.management` from `4.0.0-pre.2` to `4.0.0-pre.3`.
* Added support for `XR_KHR_vulkan_enable2` alongside `XR_KHR_vulkan_enable`.

## [0.1.2-preview.2] - 2021-01-05

* Fix publishing pipeline.

## [0.1.2-preview.1] - 2020-12-18

* Fixed package errors when Analytics package is absent (case 1300418).
* Fixed tracking origin issue which was causing wrong camera height (case 1300457).
* Fixed issue where large portions of the world were incorrectly culled at certain camera orientations.
* Fixed potential error message when clicking `Fix All` in OpenXR Project Validation window.
* Fixed an issue with sample importing.
* Minor documentation getting started tweaks.
* Minor diagnostic logging tweaks.

## [0.1.1-preview.1] - 2020-12-16

### This is the first release of *OpenXR Plugin \<com.unity.xr.openxr\>*.
