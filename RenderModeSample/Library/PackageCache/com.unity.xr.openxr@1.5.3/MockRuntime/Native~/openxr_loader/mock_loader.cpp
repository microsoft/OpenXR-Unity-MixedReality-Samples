#ifdef _WIN32
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // !WIN32_LEAN_AND_MEAN
#include <d3d11.h>
#include <d3d12.h>
#include <windows.h>
#endif

#ifdef XR_USE_PLATFORM_ANDROID
#include <jni.h>
#endif

#ifdef XR_USE_GRAPHICS_API_OPENGL_ES
#include <EGL/egl.h>
#endif

#include <vulkan/vulkan.h>

#define XR_NO_PROTOTYPES
#include "XR/IUnityXRTrace.h"
#include "openxr/openxr.h"
#include "openxr/openxr_platform.h"
#include "plugin_load.h"
#include <openxr/loader_interfaces.h>
#include <openxr/openxr_reflection.h>

struct IUnityXRTrace;
extern IUnityXRTrace* s_Trace;

IUnityXRTrace* s_Trace = nullptr;

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetInstanceProcAddr(XrInstance instance, const char* name, PFN_xrVoidFunction* function);

PluginHandle s_PluginHandle = nullptr;
PFN_xrGetInstanceProcAddr s_GetInstanceProcAddr = nullptr;

static bool LoadMockRuntime()
{
    if (nullptr != s_GetInstanceProcAddr)
        return true;

    s_PluginHandle = Plugin_LoadLibrary(L"mock_runtime");
    if (nullptr == s_PluginHandle)
        return false;

    s_GetInstanceProcAddr = (PFN_xrGetInstanceProcAddr)Plugin_GetSymbol(s_PluginHandle, "xrGetInstanceProcAddr");

    return nullptr != s_GetInstanceProcAddr;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetInstanceProcAddr(XrInstance instance, const char* name, PFN_xrVoidFunction* function)
{
    if (!LoadMockRuntime())
        return XR_ERROR_RUNTIME_FAILURE;

    return s_GetInstanceProcAddr(instance, name, function);
}

extern "C" void UNITY_INTERFACE_EXPORT XRAPI_PTR SetXRTrace(IUnityXRTrace* trace)
{
    if (!LoadMockRuntime())
        return;

    typedef void (*PFN_SetXRTrace)(IUnityXRTrace * trace);
    PFN_SetXRTrace set = (PFN_SetXRTrace)Plugin_GetSymbol(s_PluginHandle, "SetXRTrace");
    if (set != nullptr)
        set(trace);
}
