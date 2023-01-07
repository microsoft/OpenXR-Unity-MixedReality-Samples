# Intercept Feature Sample

Demonstrates intercepting an OpenXR method using an OpenXR Feature.

The OpenXR API allows any of its functions to be intercepted by providing a custom [xrGetInstanceProcAddr](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#xrGetInstanceProcAddr) method.  Within Unity this can be done by implementing a custom `OpenXRFeature` and overriding the `HookGetInstanceProcAddr` to provide an alternative version of the `xrGetInstanceProcAddr` method.  Once in place this method can return alternative versions of any of the OpenXR methods.  In this sample this mechanism is used to _hook_ the [xrCreateSession](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#xrCreateSession) method such that each time `xrCreateSession` is called it will invoke a C# callback with a message string, which is then displayed on screen.

See the [Unity OpenXR Documentation](https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest) for more information on developing a custom feature.

See the [OpenXR API Specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html) for more information on OpenXR.
