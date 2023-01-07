---
uid: xr-core-utils-manual
---
# About XR Core Utilities

The XR Core Utilities package contains a variety of classes and extension methods which are commonly used and shared between multiple packages. Some particularly useful utilities include:

- Geometry Utilities
  - Point of closest approach
  - Closest edge of a polygon
  - Check if point inside of polygon
  - Compute 2D convex hull
- Collection pool
- Math Utilities
  - Bounds extensions
  - Quaternion extensions
- Editor GUI
  - Flags Property Drawer
- Scriptable Settings (serialized project settings)
- Conditional Compilation Utility
- [XROrigin](xref:xr-core-utils-xr-origin)
- [Project validation](xref:xr-core-utils-project-validation)

# Installing XR Core Utilities

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

# Using XR Core Utilities

This contains entirely C# code. Refer to the API documentation for details.

# Troubleshooting the Input System
There is an issue with missing dependencies in version 1.0.2 of `com.unity.inputsystem`. This issue appears in versions 2019.4, 2020.3 and 2021.1 of the Unity Editor. Upgrading to version 1.1.1 of the input system package fixes these missing dependencies.

# Technical details

## Requirements
This version of the XR Core Utilities package is compatible with the following versions of the Unity Editor:
 - 2019.4+

## Document revision history
|Date|Reason|
|---|---|
|October 18, 2021|First Official version of package.|
