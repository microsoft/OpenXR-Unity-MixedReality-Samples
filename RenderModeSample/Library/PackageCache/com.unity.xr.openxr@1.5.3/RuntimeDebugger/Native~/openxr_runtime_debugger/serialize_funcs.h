#pragma once

// Begin Declaration of original functions
static PFN_xrGetInstanceProcAddr orig_xrGetInstanceProcAddr;

#define GEN_ORIG_FUNC_PTRS(f, ...) \
    static PFN_##f orig_##f;

XR_LIST_FUNCS(GEN_ORIG_FUNC_PTRS)
// End Declaration of original functions

#define GEN_PARAMS(...) \
    __VA_ARGS__

#define SEND_PARAM_TO_CSHARP(param) \
    SendToCSharp(#param, param);

#define SEND_ARRAY_TO_CSHARP(param, lenParam)                  \
    if (!SendToCSharpBaseStructArray(#param, param, lenParam)) \
    {                                                          \
        for (uint32_t i = 0; i < lenParam; ++i)                \
            SendToCSharp(#param, param[i]);                    \
    }

#define GEN_FUNCS(f, ...)                                                     \
    extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR f(__VA_ARGS__)       \
    {                                                                         \
        XR_BEFORE_##f(#f);                                                    \
        StartFunctionCall(#f);                                                \
        XrResult result = orig_##f(XR_LIST_FUNC_PARAM_NAMES_##f(GEN_PARAMS)); \
        XR_AFTER_##f(#f);                                                     \
        uint32_t lastCount = 0;                                               \
        XR_LIST_FUNC_##f(SEND_PARAM_TO_CSHARP);                               \
        XR_LIST_FUNC_ARRAYS_##f(SEND_ARRAY_TO_CSHARP);                        \
        EndFunctionCall(#f, XrEnumStr(result));                               \
        return result;                                                        \
    }

XR_LIST_FUNCS(GEN_FUNCS)

#define GEN_FUNC_LOAD(f, ...)                                                                  \
    if (strcmp(#f, name) == 0)                                                                 \
    {                                                                                          \
        auto ret = orig_xrGetInstanceProcAddr(instance, name, (PFN_xrVoidFunction*)&orig_##f); \
        if (ret == XR_SUCCESS)                                                                 \
            *function = (PFN_xrVoidFunction)&f;                                                \
        EndFunctionCall(#f, XrEnumStr(ret));                                                   \
        return ret;                                                                            \
    }