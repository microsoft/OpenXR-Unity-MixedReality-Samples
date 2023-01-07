---
uid: arfoundation-upgrade-guide
---
# Upgrading to AR Foundation version 4.2

To upgrade to AR Foundation package version 4.2, you need to do the following:

- Use Unity 2020.3 or newer.
- Be aware of behavioral changes to the ARSession.

**Use Unity 2020.3 or newer**

This version of the package requires Unity 2020.3 or newer.

**Be aware of behavioral changes to the ARSession**

The [ARSession](xref:UnityEngine.XR.ARFoundation.ARSession) optionally sets the application's `vSyncCount` and `targetFrameRate` (see [matchFrameRateRequested](xref:UnityEngine.XR.ARFoundation.ARSession.matchFrameRateRequested)). In earlier versions, the original values were not restored when the ARSession was disabled. The behavior has changed so that the original values are restored when the ARSession disabled if the frame rate was set while the ARSession was enabled.
