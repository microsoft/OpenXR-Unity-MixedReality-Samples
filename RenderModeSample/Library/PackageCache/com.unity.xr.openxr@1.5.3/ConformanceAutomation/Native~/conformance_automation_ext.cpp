#include "IUnityInterface.h"
#include "XR/IUnityXRTrace.h"
#include "openxr/openxr.h"
#include "openxr/openxr_reflection.h"
#include <cassert>
#include <cstring>
#include <string>

#include "enums_to_string.h"

#define CHECK_XRCMD(x)             \
    {                              \
        auto ret = x;              \
        assert(ret == XR_SUCCESS); \
    }

typedef XrResult(XRAPI_PTR* PFN_xrSetInputDeviceVelocityUNITY)(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, bool linearValid, XrVector3f linear, bool angularValid, XrVector3f angular);

// OpenXR runtime functions
PFN_xrSetInputDeviceActiveEXT unity_xrSetInputDeviceActiveEXT = nullptr;
PFN_xrSetInputDeviceStateBoolEXT unity_xrSetInputDeviceStateBoolEXT = nullptr;
PFN_xrSetInputDeviceStateFloatEXT unity_xrSetInputDeviceStateFloatEXT = nullptr;
PFN_xrSetInputDeviceStateVector2fEXT unity_xrSetInputDeviceStateVector2fEXT = nullptr;
PFN_xrSetInputDeviceLocationEXT unity_xrSetInputDeviceLocationEXT = nullptr;
PFN_xrSetInputDeviceVelocityUNITY unity_xrSetInputDeviceVelocityUNITY = nullptr;

// Trace for Debug
static IUnityXRTrace* s_Trace = nullptr;

// XR_EXT_conformance_automation functions

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_xrSetInputDeviceActiveEXT(XrSession session, XrPath interactionProfile, XrPath topLevelPath, XrBool32 isActive)
{
    if (nullptr == unity_xrSetInputDeviceActiveEXT)
        return false;

    return XR_SUCCESS == unity_xrSetInputDeviceActiveEXT(session, interactionProfile, topLevelPath, isActive);
}

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_xrSetInputDeviceStateBoolEXT(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, XrBool32 state)
{
    if (nullptr == unity_xrSetInputDeviceStateBoolEXT)
        return false;

    XrResult result = unity_xrSetInputDeviceStateBoolEXT(session, topLevelPath, inputSourcePath, state);
    std::string traceString = "[ConformanceAutomationExt] - script_xrSetInputDeviceStateBoolEXT XrResult is ";

    char resultString[256];
    strcpy(resultString, to_string(result));

    traceString = traceString + resultString;
    traceString = traceString + "\n";
    XR_TRACE_LOG(s_Trace, traceString.c_str());

    return XR_SUCCESS == result;
}

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_xrSetInputDeviceStateFloatEXT(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, float state)
{
    if (nullptr == unity_xrSetInputDeviceStateFloatEXT)
        return false;

    return XR_SUCCESS == unity_xrSetInputDeviceStateFloatEXT(session, topLevelPath, inputSourcePath, state);
}

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_xrSetInputDeviceStateVector2fEXT(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, XrVector2f state)
{
    if (nullptr == unity_xrSetInputDeviceStateVector2fEXT)
        return false;

    return XR_SUCCESS == unity_xrSetInputDeviceStateVector2fEXT(session, topLevelPath, inputSourcePath, state);
}

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_xrSetInputDeviceLocationEXT(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, XrSpace space, XrPosef pose)
{
    if (nullptr == unity_xrSetInputDeviceLocationEXT)
        return false;

    return XR_SUCCESS == unity_xrSetInputDeviceLocationEXT(session, topLevelPath, inputSourcePath, space, pose);
}

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_xrSetInputDeviceVelocityUNITY(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, bool linearValid, XrVector3f linear, bool angularValid, XrVector3f angular)
{
    if (nullptr == unity_xrSetInputDeviceVelocityUNITY)
        return false;

    return XR_SUCCESS == unity_xrSetInputDeviceVelocityUNITY(session, topLevelPath, inputSourcePath, linearValid, linear, angularValid, angular);
}

// Init

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_initialize(PFN_xrGetInstanceProcAddr xrGetInstanceProcAddr, XrInstance instance)
{
    XR_TRACE_LOG(s_Trace, "[ConformanceAutomationExt] - script_initialize starting");

    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetInputDeviceActiveEXT", (PFN_xrVoidFunction*)&unity_xrSetInputDeviceActiveEXT));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetInputDeviceStateBoolEXT", (PFN_xrVoidFunction*)&unity_xrSetInputDeviceStateBoolEXT));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetInputDeviceStateFloatEXT", (PFN_xrVoidFunction*)&unity_xrSetInputDeviceStateFloatEXT));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetInputDeviceStateVector2fEXT", (PFN_xrVoidFunction*)&unity_xrSetInputDeviceStateVector2fEXT));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetInputDeviceLocationEXT", (PFN_xrVoidFunction*)&unity_xrSetInputDeviceLocationEXT));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetInputDeviceVelocityUNITY", (PFN_xrVoidFunction*)&unity_xrSetInputDeviceVelocityUNITY));

    XR_TRACE_LOG(s_Trace, "[ConformanceAutomationExt] - script_initialize complete");
}

// UnityPlugin events

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_Trace = unityInterfaces->Get<IUnityXRTrace>();
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginUnload()
{
}