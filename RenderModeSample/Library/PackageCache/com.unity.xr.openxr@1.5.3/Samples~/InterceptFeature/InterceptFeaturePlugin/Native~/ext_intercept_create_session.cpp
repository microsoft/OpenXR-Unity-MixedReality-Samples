#include "IUnityInterface.h"
#include "XR/IUnityXRTrace.h"
#include "openxr/openxr.h"
#include <cstring>
#include <string>

typedef void (*SendMessageToScript_pfn)(const char* message);

static IUnityXRTrace* s_Trace = nullptr;
static PFN_xrGetInstanceProcAddr s_xrGetInstanceProcAddr = nullptr;
static PFN_xrCreateSession s_xrCreateSession = nullptr;
static std::string s_message = "not set from c#";
static SendMessageToScript_pfn s_callback = nullptr;

static XrResult XRAPI_PTR intercepted_xrCreateSession(XrInstance instance, const XrSessionCreateInfo* createInfo, XrSession* session)
{
    XR_TRACE_LOG(s_Trace, "Intercepted xrCreateSession, message: %s\n", s_message.c_str());
    if (s_callback != nullptr)
        s_callback(s_message.c_str());

    return s_xrCreateSession(instance, createInfo, session);
}

static XrResult XRAPI_PTR intercept_xrCreateSession_xrGetInstanceProcAddr(XrInstance instance, const char* name, PFN_xrVoidFunction* function)
{
    XrResult result = s_xrGetInstanceProcAddr(instance, name, function);
    if (strcmp("xrCreateSession", name) == 0)
    {
        s_xrCreateSession = (PFN_xrCreateSession)*function;
        *function = (PFN_xrVoidFunction)intercepted_xrCreateSession;
    }
    return result;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API script_set_callback(SendMessageToScript_pfn callback)
{
    s_callback = callback;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API script_set_message(char* message)
{
    s_message = message;
}

extern "C" PFN_xrGetInstanceProcAddr UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_intercept_xrCreateSession_xrGetInstanceProcAddr(PFN_xrGetInstanceProcAddr old)
{
    s_xrGetInstanceProcAddr = old;
    return intercept_xrCreateSession_xrGetInstanceProcAddr;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_Trace = unityInterfaces->Get<IUnityXRTrace>();
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginUnload()
{
}
