# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.6.0] - 2022-10-31

### Changed

- Services configuration file isn't temporarily added to the StreamingAssets folder during builds on Unity 2021.3 and up.
- Services initialization without a linked project id will fail (throw `UnityProjectNotLinkedException`).

### Fixed

- Persisting telemetry no longer logs errors, unless `ENABLE_UNITY_SERVICES_CORE_TELEMETRY_LOGGING` is enabled as scripting define. Diagnostics are sent when persisting telemetry produces an error.

## [1.5.2] - 2022-10-17

### Fixed

- Core will no longer fail initialization when it fails to find a stripped service package.

## [1.5.1] - 2022-10-06

### Added

- `UnityServices.ExternalUserId` which can be used to pass a user identifier from a third party provider to Unity Gaming Services

### Fixed

- Services Core failing to find all `IInitializablePackage` if an unity package implements it multiple times.

## [1.5.0] - 2022-10-03

### Added

- All `IInitializablePackage` initialization time is now measured by Services Core.

### Changed

- `CoreRegistryInitializer` now throws a `ServicesInitializationException` instead of an explicit `NullReferenceException` when the dependency tree is null.
- ActionScheduler updated to be thread safe

### Fixed

- A case where null configuration values were causing serialization issues
- Issue with stripping when authentication APIs are not used.

## [1.4.3] - 2022-07-28

### Fixed

- Added diagnostic message length limit and telemetry count limit, so telemetry payload will not be rejected by telemetry service.

### Changed

- Telemetry logs now use the `ENABLE_UNITY_SERVICES_CORE_TELEMETRY_LOGGING` define instead
  of `ENABLE_UNITY_SERVICES_CORE_VERBOSE_LOGGING`.

## [1.4.2] - 2022-06-16

### Changed

- Log error instead of warning when core initialize with no cloud project id

### Added

- Log a JSON containing the common configuration shared among all services for debugging purposes when
  using `ENABLE_UNITY_SERVICES_CORE_VERBOSE_LOGGING` as a scripting define.
- Log warning when building a project with core package included and without linking the project in project settings.

### Fixed

- NullReferenceException while telemetry was provided an empty file, fixed by introducing a null check
- DirectoryNotFoundException happened inconsistently on Switch, fixed by resolving racing condition issue

## [1.4.2-pre.2] - 2022-05-27

### Fixed

- NSUserDefaults handling null values

## [1.4.1] - 2022-05-20

### Added

- Log warning when core initialize with no cloud project id
- Add a message in "Link your unity project" popup to inform the user has to sign-up

## [1.4.0] - 2022-04-29

### Added

- Add Vivox public interfaces: `IVivox`, `IVivoxTokenProviderInternal`, to enable interactions with the Vivox service.

## [1.3.2] - 2022-04-14

### Fixed

- Crash on Switch when initializing telemetry persistence. Now telemetry won't persist anything on Switch.
- NullReferenceException while linking the project
- Issue with user roles and service flags

## [1.3.1] - 2022-03-29

### Changed

- Newtonsoft package dependency update to 3.0.2.

## [1.3.0] - 2022-03-21

### Added

- Add QoS public interface: `IQosResults` and return type `QosResult`, to provide QoS functionality to other packages

### Fixed

- Code stripping when core package is not used
- Retrying to initialize all services after a first attempt failed.

## [1.2.0] - 2022-02-23

### Added

- Add Wire public interfaces: `IWire`, `IChannel`, `IChannelTokenProvider`, and their dependencies, to enable
  interactions with the Wire service.
- The `IUnityThreadUtils` component to simplify working with the Unity thread.

### Changed

- Newtonsoft dependency to use the latest major Newtonsoft version, 13.0.1.

## [1.1.0-pre.69] - 2022-02-17

### Added

- Add `IEnvironmentId` component to provide the environment ID from the Access Token to other packages
- `OrganizationProvider` & `IOrganizationHandler` to enable package developers to access Organization Key.

## [1.1.0-pre.41] - 2021-12-08

### Added

- `IDiagnosticsFactory` component & `IDiagnostics` to enable package developers to send diagnostics for their package.
- Add `AnalyticsOptionsExtensions` with `SetAnalyticsUserId(string identifier)` to set a custom analytics user id.
- `IMetricsFactory` component & `IMetrics` to enable package developers to send metrics for their package.

### Fixed

- Calling `UnityServices.InitializeAsync(null)` throwing a null reference exception.

## [1.1.0-pre.11] - 2021-10-25

### Added

- Getter methods for `ConfigurationBuilder`.

### Fixed

- Fix layout for Project Bind Redirect Popup for Light theme

## [1.1.0-pre.10] - 2021-10-08

### Added

- `IActionScheduler` component to schedule actions at runtime.
- `ICloudProjectId` component to access cloudProjectId.

### Removed

- Removed the Service Activation Popup

### Fixed

- Fix define check bug on Android and WebGL

## [1.1.0-pre.9] - 2021-09-24

### Added

- New common error codes: `ApiMissing`, `RequestRejected`, `NotFound`, `InvalidRequest`.
- Link project pop-up dialog

### Fixed

- Core registry throwing exceptions when domain reloads are disabled

## [1.1.0-pre.8] - 2021-08-06

### Added

- Added base exception type for other Operate SDKs to derive from. Consistent error handling experience.

## [1.1.0-pre.7] - 2021-08-06

### Added

- `UnityServices` class at runtime. It is the entry point to initialize unity services with `InitializeAsync()`
  or `InitializeAsync(InitializationOptions)`.
- `InitializationOptions` to enable services initialization customization through code.
- `IInstallationId` component to access the Unity Installation Identifier.
- `IEnvironments` component to get the environment currently used by services.
- `SetEnvironmentName` initialization option to set the environment services should use.
- MiniJson.
- `IProjectConfiguration` component to access services settings at runtime.
- `IConfigurationProvider` to provide configuration values that need to be available at runtime.

## [1.0.1] - 2021-06-28

### Added

- DevEx integration into the editor.
- Service Activation popup.

### This is the first release of *com.unity.services.core*.
