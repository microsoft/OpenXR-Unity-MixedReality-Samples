---
uid: xr-core-utils-upgrade-guide
---
# Upgrading to XR Core Utilities version 2.0

This package is replacing XR Tools Utilities package which is now deprecated. To upgrade to XR Core Utilities package version 2.0, you need to do the following:

- Change dependency on XR Tools Utilities (`com.unity.xrtools.utils`) package with XR Core Utilities (`com.unity.xr.core-utils`) package.
- Use Unity 2019.4 or newer.

**Change dependency on XR Tools Utilities package with XR Core Utilities package**

Any explicit dependency on XR Tools Utilities package needs to be replaced with a dependency on XR Core Utilities package. Since the asmdefs and namespaces have been renamed as well, any reference to these must be updated.

**Use Unity 2019.4 or newer**

This version of the package requires Unity 2019.4 or newer.
