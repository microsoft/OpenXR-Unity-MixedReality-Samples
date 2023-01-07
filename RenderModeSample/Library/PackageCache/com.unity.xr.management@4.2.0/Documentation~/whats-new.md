---
uid: xr-plug-in-management-whats-new
---

# What's new in version 4.0.0

Summary of changes in **XR Plug-in Management** package version 4.0.0-pre.4.

The main updates in this release include:

Added
* API to support packages providing their own rendered UI in the Loader Selection pane.
* API for providing notifications per package in the Loader Selection pane.

Updated
* Added sample code to documentation to show how to manage an individual loader manually.
* The **XRManagerSettings.loaders** collection now returns a copy of the collection at runtime. Before this, modifying the list was reflected immediately in the running **XRManagerSettings** instance. This is no longer the case.

Fixed
* Small bug in package resolution that could cause the ui To gray out when returning from Play.
* If there are no loaders set for a target platform, Unity no longer copies `XRGeneralSettings` to the built target.


For a full list of changes and updates in this version, see the **XR Plug-in Management** package changelog.
