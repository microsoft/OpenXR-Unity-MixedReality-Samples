---
uid: xr-plug-in-management-upgrade-guide
---

# Upgrade Guide to 4.0.0

Runtime behavior of **XRManagerSettings.loaders** property has changed to return a shallow copy of the list of loaders. This list can be operated on without affecting the original currently held list. However, the list itself can be set to a mutated list as long as it follows the following guidelines:

* It only reorders the elements.
* It only adds loaders that are of the same type as a loader that was present and registered prior to initial startup.
* It only removes loaders.

While this was always the intended runtime behavior for this collection, it was not actually enforced before this release.
