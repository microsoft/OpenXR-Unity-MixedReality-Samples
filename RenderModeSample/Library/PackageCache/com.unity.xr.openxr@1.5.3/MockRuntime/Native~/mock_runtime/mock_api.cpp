#include "mock.h"

#ifndef TRAMPOLINE
#define TRAMPOLINE 0
#endif

static PFN_xrGetInstanceProcAddr s_GetInstanceProcAddr = nullptr;
static PFN_xrCreateInstance s_xrCreateInstance = nullptr;
static PFN_xrDestroyInstance s_xrDestroyInstance = nullptr;
static XrInstance s_Instance = XR_NULL_HANDLE;

typedef XrResult(XRAPI_PTR* PFN_BeforeFunctionCallback)(const char* name);
typedef void(XRAPI_PTR* PFN_AfterFunctionCallback)(const char* name, XrResult result);

static PFN_BeforeFunctionCallback s_BeforeFunctionCallback = nullptr;
static PFN_AfterFunctionCallback s_AfterFunctionCallback = nullptr;
static bool s_KeepFunctionCallbacks = false;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API MockRuntime_RegisterFunctionCallbacks(PFN_BeforeFunctionCallback before, PFN_AfterFunctionCallback after)
{
    s_BeforeFunctionCallback = before;
    s_AfterFunctionCallback = after;

#if TRAMPOLINE
    if (s_Instance == nullptr || s_GetInstanceProcAddr == nullptr)
        return;
    void (*fptr)(PFN_BeforeFunctionCallback before, PFN_AfterFunctionCallback after) = nullptr;
    s_GetInstanceProcAddr(s_Instance, __func__, (PFN_xrVoidFunction*)&fptr);
    return fptr(before, after);
#endif
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API MockRuntime_SetKeepFunctionCallbacks(bool value)
{
    s_KeepFunctionCallbacks = value;
}

XrResult MockRuntime_BeforeFunction(const char* name)
{
    if (s_BeforeFunctionCallback == nullptr)
        return XR_SUCCESS;

    return s_BeforeFunctionCallback(name);
}

void MockRuntime_AfterFunction(const char* name, XrResult result)
{
    if (s_AfterFunctionCallback == nullptr)
        return;

    s_AfterFunctionCallback(name, result);
}

// Special handling of before / after function callbacks for xrCreateInstance and xrDestroyInstance
extern "C" PFN_xrGetInstanceProcAddr UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API MockRuntime_HookCreateInstance(PFN_xrGetInstanceProcAddr procAddr)
{
    s_GetInstanceProcAddr = procAddr;
    return (PFN_xrGetInstanceProcAddr)[](XrInstance instance, const char* name, PFN_xrVoidFunction* function)->XrResult
    {
        XrResult ret = s_GetInstanceProcAddr(instance, name, function);
        if (strcmp(name, "xrCreateInstance") == 0)
        {
            s_xrCreateInstance = (PFN_xrCreateInstance)*function;
            *function = (PFN_xrVoidFunction)(PFN_xrCreateInstance)[](const XrInstanceCreateInfo* createInfo, XrInstance* instance)->XrResult
            {
                XrResult ret = XR_SUCCESS;
                if (s_BeforeFunctionCallback != nullptr)
                    ret = s_BeforeFunctionCallback("xrCreateInstance");
                if (XR_FAILED(ret))
                    return ret;
                ret = s_xrCreateInstance(createInfo, instance);
                s_Instance = *instance;
                if (s_AfterFunctionCallback != nullptr)
                    s_AfterFunctionCallback("xrCreateInstance", ret);
                MockRuntime_RegisterFunctionCallbacks(s_BeforeFunctionCallback, s_AfterFunctionCallback);
                return ret;
            };
            return ret;
        }
        else if (strcmp(name, "xrDestroyInstance") == 0)
        {
            s_xrDestroyInstance = (PFN_xrDestroyInstance)*function;
            *function = (PFN_xrVoidFunction)(PFN_xrDestroyInstance)[](XrInstance instance)->XrResult
            {
                PFN_BeforeFunctionCallback before = s_BeforeFunctionCallback;
                PFN_AfterFunctionCallback after = s_AfterFunctionCallback;

                if (!s_KeepFunctionCallbacks)
                {
                    MockRuntime_RegisterFunctionCallbacks(nullptr, nullptr);
                }

                XrResult ret = XR_SUCCESS;
                if (before != nullptr)
                    ret = before("xrDestroyInstance");
                if (XR_FAILED(ret))
                    return ret;
                ret = s_xrDestroyInstance(instance);
                s_Instance = XR_NULL_HANDLE;
                if (after != nullptr)
                    after("xrDestroyInstance", ret);
                return ret;
            };
            return ret;
        }
        return ret;
    };
}

#if TRAMPOLINE

#define MOCK_API_TRAMPOLINE(API_RETURN, DEFAULT_VAL, API_NAME, API_ARGS, API_ARG_NAMES) \
    extern "C" API_RETURN UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API API_NAME API_ARGS  \
    {                                                                                   \
        if (s_Instance == XR_NULL_HANDLE || s_GetInstanceProcAddr == nullptr)           \
            return DEFAULT_VAL;                                                         \
        API_RETURN(*fptr)                                                               \
        API_ARGS = nullptr;                                                             \
        s_GetInstanceProcAddr(s_Instance, __func__, (PFN_xrVoidFunction*)&fptr);        \
        return fptr API_ARG_NAMES;                                                      \
    }

#else
#define MOCK_API_TRAMPOLINE(API_RETURN, DEFAULT_VAL, API_NAME, API_ARGS, API_ARG_NAMES) \
    extern "C" API_RETURN UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API API_NAME API_ARGS
#endif

void NO_RETURN()
{
}

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_SetView,
    (XrViewConfigurationType viewConfigurationType, int viewIndex, UnityVector3 position, UnityVector4 orientation, UnityVector4 fov),
    (viewConfigurationType, viewIndex, position, orientation, fov))
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return;

    s_runtime->SetViewPose(viewConfigurationType, viewIndex, {orientation, position}, fov);
}
#endif

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_SetViewState,
    (XrViewConfigurationType viewConfigurationType, XrViewStateFlags stateFlags),
    (viewConfigurationType, stateFlags))
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return;

    s_runtime->SetViewStateFlags(viewConfigurationType, stateFlags);
}
#endif

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_SetReferenceSpace,
    (XrReferenceSpaceType referenceSpace, UnityVector3 position, UnityVector4 orientation, XrSpaceLocationFlags locationFlags),
    (referenceSpace, position, orientation, locationFlags))
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return;

    s_runtime->SetSpace(referenceSpace, {orientation, position}, locationFlags);
}
#endif

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_SetActionSpace,
    (XrAction action, UnityVector3 position, UnityVector4 orientation, XrSpaceLocationFlags locationFlags),
    (action, position, orientation, locationFlags))
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return;

    s_runtime->SetSpace(action, {orientation, position}, locationFlags);
}
#endif

MOCK_API_TRAMPOLINE(XrSessionState, XR_SESSION_STATE_UNKNOWN, MockRuntime_GetSessionState,
    (),
    ())
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return XR_SESSION_STATE_UNKNOWN;

    return s_runtime->GetSessionState();
}
#endif

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_RequestExitSession,
    (),
    ())
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return;

    s_runtime->RequestExitSession();
}
#endif

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_CauseInstanceLoss,
    (),
    ())
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return;

    s_runtime->CauseInstanceLoss();
}
#endif

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_SetReferenceSpaceBounds,
    (XrReferenceSpaceType referenceSpaceType, XrExtent2Df bounds),
    (referenceSpaceType, bounds))
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return;

    s_runtime->SetExtentsForReferenceSpace(referenceSpaceType, bounds);
}
#endif

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_GetEndFrameStats,
    (int* primaryLayerCount, int* secondaryLayerCount),
    (primaryLayerCount, secondaryLayerCount))
#if !TRAMPOLINE
{
    *primaryLayerCount = 0;
    *secondaryLayerCount = 0;

    if (nullptr == s_runtime)
        return;

    s_runtime->GetEndFrameStats(primaryLayerCount, secondaryLayerCount);
}
#endif

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_ActivateSecondaryView,
    (XrViewConfigurationType viewConfigurationType, bool activate),
    (viewConfigurationType, activate))
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return;

    s_runtime->ActivateSecondaryView(viewConfigurationType, activate);
}
#endif

MOCK_API_TRAMPOLINE(void, NO_RETURN(), MockRuntime_RegisterScriptEventCallback,
    (PFN_ScriptEventCallback callback),
    (callback))
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return;

    s_runtime->RegisterScriptEventCallback(callback);
}
#endif

MOCK_API_TRAMPOLINE(bool, false, MockRuntime_TransitionToState,
    (XrSessionState requestedState, bool forceTransition),
    (requestedState, forceTransition))
#if !TRAMPOLINE
{
    if (nullptr == s_runtime)
        return false;

    if (!forceTransition && !s_runtime->IsStateTransitionValid(requestedState))
    {
        MOCK_TRACE_ERROR("Failed to request state. Was transition valid: %s with force %s",
            s_runtime->IsStateTransitionValid(requestedState) ? "TRUE" : "FALSE",
            forceTransition ? "TRUE" : "FALSE");
        return false;
    }

    s_runtime->ChangeSessionState(requestedState);

    return true;
}
#endif

#if !TRAMPOLINE
XrResult GetProcAddrMockAPI(XrInstance instance, const char* name, PFN_xrVoidFunction* function)
{
    GET_PROC_ADDRESS(MockRuntime_SetView)
    GET_PROC_ADDRESS(MockRuntime_SetViewState)
    GET_PROC_ADDRESS(MockRuntime_SetReferenceSpace)
    GET_PROC_ADDRESS(MockRuntime_SetActionSpace)
    GET_PROC_ADDRESS(MockRuntime_GetSessionState)
    GET_PROC_ADDRESS(MockRuntime_RequestExitSession)
    GET_PROC_ADDRESS(MockRuntime_CauseInstanceLoss)
    GET_PROC_ADDRESS(MockRuntime_SetReferenceSpaceBounds)
    GET_PROC_ADDRESS(MockRuntime_GetEndFrameStats)
    GET_PROC_ADDRESS(MockRuntime_ActivateSecondaryView)
    GET_PROC_ADDRESS(MockRuntime_RegisterScriptEventCallback)
    GET_PROC_ADDRESS(MockRuntime_RegisterFunctionCallbacks)
    GET_PROC_ADDRESS(MockRuntime_TransitionToState)

    return XR_ERROR_FUNCTION_UNSUPPORTED;
}
#endif
