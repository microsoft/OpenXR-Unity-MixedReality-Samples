---
uid: arsubsystems-session-subsystem
---
# XR session subsystem

A "session" refers to an instance of AR. While the other AR subsystems provide specific pieces of functionality, like plane detection, the session controls the lifecycle of all AR-related subsystems. If you `Stop` (or fail to `Create`) an `XRSessionSubsystem`, the other AR subsystems might not work.

`Start` starts the session. `Stop` pauses it.

## Determining availability

Some platforms have AR capabilities built into the device's operating system. On others, AR software might be able to be installed on-demand. The question "is AR available on this device?" might require checking a remote server for software availability. Therefore, `XRSessionSubsystem.GetAvailabilityAsync` should be called to determine whether AR is presently available to the app. This method returns a `Promise<SessionAvailability>`, which can be used in a coroutine.

Once availability is determined, the device can be:

* unsupported.
* supported but requiring an update or install.
* supported and ready.

## Installing additional AR software

If `SessionAvailability` is `SessionAvailability.Supported` but not `SessionAvailability.Installed`, you should call `InstallAsync` to install the AR software. This returns another type of `Promise`: `Promise<SessionInstallationStatus>`. If the installation is successful, it's safe to `Start` the subsystem.
