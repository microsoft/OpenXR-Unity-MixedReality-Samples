---
uid: xr-core-utils-changelog
---
# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2022-08-22

### Fixed

- Organized alignment for help button on project validation rules window.
- Fixed alignment issues with the error icon.

## [2.1.0-pre.1] - 2022-04-21

### Added

- Add Project Validation for validating packages against package configuration correctness. See the [manual entry for project validation](xref:xr-core-utils-project-validation) for more details.

### Removed

- Removed the **GameObject** &gt; **XR** &gt; **XR Origin** menu item. To create a new XR Origin, users should instead use the menu items provided by [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/index.html#scene-setup) and/or [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest?subfolder=/manual/general-setup.html).

### Fixed

- [NativeArrayUtils.EnsureCapacity](xref:Unity.XR.CoreUtils.NativeArrayUtils.EnsureCapacity*) now checks for unallocated array before disposing it and reallocating for a larger capacity.
- Fixed compilation errors on platforms such as Stadia where the XR module is not available.

## [2.0.0] - 2022-02-16

### Added

- Define constants for documentation to include missing APIs.

### Fixed

- Fixed XR Origin so it appears in the **Component &gt; XR** menu.
- Fixed so test behaviors and [OnDestroyNotifier](xref:Unity.XR.CoreUtils.OnDestroyNotifier) do not show up in the **Component &gt; Add** menu since those are not intended to be used directly by users.

## [2.0.0-pre.6] - 2021-12-10

### Changed

- Stopped firing warnings when users attach the [TrackedPoseDriver](https://docs.unity3d.com/Packages/com.unity.xr.legacyinputhelpers@2.1/api/UnityEngine.SpatialTracking.TrackedPoseDriver.html) from `com.unity.xr.legacyinputhelpers`, but strongly recommend that users attach the [TrackedPoseDriver](https://docs.unity.cn/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.XR.TrackedPoseDriver.html) from `com.unity.inputsystem` to the `camera` property of [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) instead. ([1388617](https://issuetracker.unity3d.com/product/unity/issues/guid/1388617))

## [2.0.0-pre.5] - 2021-11-16

### Changed

- Changed property names so that they adhere to PascalCase.

### Added

- Added `XROrigin` menu item to SceneInspector that creates and configures a basic `XROrigin` in the scene.

### Fixed

- Fixed serialization issue where upgrading to [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) from [XRRig](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@1.0/api/UnityEngine.XR.Interaction.Toolkit.XRRig.html) may cause references to be broken in the component.
- Fixed issue where [XROrigin.CameraInOriginSpacePos](xref:Unity.XR.CoreUtils.XROrigin.CameraInOriginSpacePos) was being miscalculated.
- Fixed issue where the custom Inspector for [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) was not being used.
- Fixed warning message referencing an old property name when a Camera could not be found.
- Fixed a reflection issue with [ScriptableSettingsBase.GetInstanceByType](xref:Unity.XR.CoreUtils.ScriptableSettingsBase.GetInstanceByType(System.Type)) by renaming `EditorScriptableSettings<T>.instance` to `EditorScriptableSettings<T>.Instance`.

## [2.0.0-pre.3] - 2021-11-03

This is the first release of *XR Core Utilities* package.

### Added

- XROrigin is a new XR agnostic setup that handles session space. It will be replacing [ARSessionOrigin](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/api/UnityEngine.XR.ARFoundation.ARSessionOrigin.html?q=ARSessionOrigin) and [XRRig](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@1.0/api/UnityEngine.XR.Interaction.Toolkit.XRRig.html?q=XRRig).
- All the utilities from [XR Tools Utilities](https://docs.unity3d.com/Packages/com.unity.xrtools.utils@latest?preview=1&subfolder=/manual/) (`com.unity.xrtools.utils`) package has been migrated in this package. This includes common utilities used by XR packages like MARS, AR Foundation, and Spatial Framework.

### Changed

- The minimum Unity version for this package is now 2019.4.

### Removed

- Removed `CONTRIBUTING.md` since the package is not accepting external contribution.

### Fixed

- Fixed company name in `LICENSE.md` file from "Unity Technologies ApS" to "Unity Technologies".
