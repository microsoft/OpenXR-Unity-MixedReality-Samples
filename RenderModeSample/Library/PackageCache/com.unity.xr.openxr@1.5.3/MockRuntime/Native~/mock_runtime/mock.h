#pragma once

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
#include <sstream>
#include <string.h>
#include <string>
#include <sys/system_properties.h>

namespace std
{
template <typename T>
std::string to_string(T value)
{
    std::ostringstream os;
    os << value;
    return os.str();
}
} // namespace std
#endif

#ifdef XR_USE_GRAPHICS_API_OPENGL_ES
#include <EGL/egl.h>
#endif

#include <vulkan/vulkan.h>

struct IUnityXRTrace;
extern IUnityXRTrace* s_Trace;

#define DEBUG_TRACE 1

#define MOCK_TRACE(TYPE, STRING, ...) \
    if (s_Trace != nullptr)           \
    s_Trace->Trace(TYPE, "[Mock] " STRING "\n", ##__VA_ARGS__)
#define MOCK_TRACE_LOG(STRING, ...) MOCK_TRACE(kXRLogTypeLog, STRING, ##__VA_ARGS__)
#define MOCK_TRACE_ERROR(STRING, ...) MOCK_TRACE(kXRLogTypeError, STRING, ##__VA_ARGS__)

#if DEBUG_TRACE
#define MOCK_TRACE_DEBUG(STRING, ...) MOCK_TRACE(kXRLogTypeDebug, STRING, ##__VA_ARGS__)
#else
#define MOCK_TRACE_DEBUG(STRING, ...)
#endif

#define XR_NO_PROTOTYPES
#include "openxr/openxr.h"
#include "openxr/openxr_platform.h"
#include <openxr/openxr_reflection.h>
#include <openxr/xr_msft_third_person_observer_private.h>

#include "XR/IUnityXRTrace.h"

#include <chrono>
#include <map>
#include <queue>
#include <string>
#include <unordered_map>

#include "openxr/openxr_reflection.h"

#include "enums_to_string.h"
#include "openxr_utils.h"

class MockRuntime;
extern MockRuntime* s_runtime;

#define GET_PROC_ADDRESS(funcName)                 \
    if (strcmp(#funcName, name) == 0)              \
    {                                              \
        *function = (PFN_xrVoidFunction)&funcName; \
        return XR_SUCCESS;                         \
    }

#define GET_PROC_ADDRESS_REMAP(funcName, funcProc) \
    if (strcmp(#funcName, name) == 0)              \
    {                                              \
        *function = (PFN_xrVoidFunction)&funcProc; \
        return XR_SUCCESS;                         \
    }

#define CHECK_RUNTIME()       \
    if (s_runtime == nullptr) \
        return XR_ERROR_HANDLE_INVALID;
#define CHECK_INSTANCE(instance)                                      \
    if (s_runtime == nullptr || s_runtime->GetInstance() != instance) \
        return XR_ERROR_HANDLE_INVALID;
#define CHECK_SESSION(session)                                      \
    if (s_runtime == nullptr || s_runtime->GetSession() != session) \
        return XR_ERROR_HANDLE_INVALID;
#define CHECK_SUCCESS(body)       \
    {                             \
        XrResult result = (body); \
        if (result != XR_SUCCESS) \
            return result;        \
    }

#define DEBUG_LOG_EVERY_FUNC_CALL 0

#if DEBUG_LOG_EVERY_FUNC_CALL
#define LOG_FUNC() MOCK_TRACE(kXRLogTypeDebug, __FUNCTION__)
#else
#define LOG_FUNC()
#endif

XrResult GetProcAddrMockAPI(XrInstance instance, const char* name, PFN_xrVoidFunction* function);

XrResult MockRuntime_BeforeFunction(const char* name);
void MockRuntime_AfterFunction(const char* name, XrResult result);

#define MOCK_HOOK_AFTER_NAMED(name, result) MockRuntime_AfterFunction(name, result);
#define MOCK_HOOK_AFTER(result) MOCK_HOOK_AFTER_NAMED(__FUNCTION__, result);

#define MOCK_HOOK_BEFORE_NAMED(name)                        \
    XrResult hookResult = MockRuntime_BeforeFunction(name); \
    if (hookResult != XR_SUCCESS)                           \
    {                                                       \
        MOCK_HOOK_AFTER_NAMED(name, hookResult);            \
        return hookResult;                                  \
    }
#define MOCK_HOOK_BEFORE() MOCK_HOOK_BEFORE_NAMED(__FUNCTION__)

#define MOCK_HOOK_NAMED(name, x)             \
    MOCK_HOOK_BEFORE_NAMED(name)             \
    hookResult = (x);                        \
    MOCK_HOOK_AFTER_NAMED(name, hookResult); \
    return hookResult;

#define MOCK_HOOK(x) MOCK_HOOK_NAMED(__FUNCTION__, (x))

#include "mock_events.h"
#include "mock_extensions.h"
#include "mock_input_state.h"
#include "mock_runtime.h"

struct UnityVector3
{
    float x;
    float y;
    float z;

    UnityVector3()
    {
        x = y = z = 0.0f;
    }

    UnityVector3(const XrVector3f& v)
        : x(v.x)
        , y(v.y)
        , z(-v.z)
    {
    }

    operator XrVector3f() const
    {
        return XrVector3f{x, y, -z};
    }
};

struct UnityVector4
{
    float x;
    float y;
    float z;
    float w;

    UnityVector4()
    {
        x = y = z = 0.0f;
    }

    UnityVector4(const XrFovf& f)
        : x(f.angleLeft)
        , y(f.angleRight)
        , z(f.angleUp)
        , w(f.angleDown)
    {
    }

    UnityVector4(const XrQuaternionf& q)
        : x(-q.x)
        , y(-q.y)
        , z(q.z)
        , w(q.w)
    {
    }

    operator XrFovf() const
    {
        return XrFovf{x, y, z, w};
    }

    operator XrQuaternionf() const
    {
        return XrQuaternionf{-x, -y, z, w};
    }
};
