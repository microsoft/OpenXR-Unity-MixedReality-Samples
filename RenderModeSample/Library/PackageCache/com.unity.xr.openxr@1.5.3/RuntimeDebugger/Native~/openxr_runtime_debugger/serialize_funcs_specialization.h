#pragma once

//XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrLoadControllerModelMSFT(XrSession session, XrControllerModelKeyMSFT modelKey, uint32_t bufferCapacityInput, uint32_t* bufferCountOutput, uint8_t* buffer)
#undef XR_BEFORE_xrLoadControllerModelMSFT
#define XR_BEFORE_xrLoadControllerModelMSFT(funcName)                                                                        \
    {                                                                                                                        \
        StartFunctionCall(funcName);                                                                                         \
        XrResult result = orig_xrLoadControllerModelMSFT(session, modelKey, bufferCapacityInput, bufferCountOutput, buffer); \
        SendToCSharp("session", session);                                                                                    \
        SendToCSharp("modelKey", modelKey);                                                                                  \
        SendToCSharp("bufferCapacityInput", bufferCapacityInput);                                                            \
        SendToCSharp("bufferCountOutput", bufferCountOutput);                                                                \
        SendToCSharp("buffer", "<TODO>");                                                                                    \
        EndFunctionCall("xrLoadControllerModelMSFT", XrEnumStr(result));                                                     \
        return result;                                                                                                       \
    }

//XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrStringToPath(XrInstance instance, const char* pathString, XrPath* path)
#undef XR_AFTER_xrStringToPath
#define XR_AFTER_xrStringToPath(funcName)                                           \
    {                                                                               \
        /* Cache off path -> string for later lookup .. send it off to c# for UI */ \
        if (XR_SUCCEEDED(result))                                                   \
            SendXrPathCreate(funcName, *path, pathString);                          \
    }

//typedef XrResult (XRAPI_PTR *PFN_xrCreateAction)(XrActionSet actionSet, const XrActionCreateInfo* createInfo, XrAction* action);
#undef XR_AFTER_xrCreateAction
#define XR_AFTER_xrCreateAction(funcName)                      \
    {                                                          \
        if (XR_SUCCEEDED(result))                              \
            SendXrActionCreate(funcName, *action, createInfo); \
    }

//typedef XrResult (XRAPI_PTR *PFN_xrCreateActionSet)(XrInstance instance, const XrActionSetCreateInfo* createInfo, XrActionSet* actionSet);
#undef XR_AFTER_xrCreateActionSet
#define XR_AFTER_xrCreateActionSet(funcName)                         \
    {                                                                \
        if (XR_SUCCEEDED(result))                                    \
            SendXrActionSetCreate(funcName, *actionSet, createInfo); \
    }

// typedef XrResult (XRAPI_PTR *PFN_xrCreateActionSpace)(XrSession session, const XrActionSpaceCreateInfo* createInfo, XrSpace* space);
#undef XR_AFTER_xrCreateActionSpace
#define XR_AFTER_xrCreateActionSpace(funcName)                    \
    {                                                             \
        if (XR_SUCCEEDED(result))                                 \
            SendXrActionSpaceCreate(funcName, createInfo, space); \
    }

// typedef XrResult (XRAPI_PTR *PFN_xrCreateReferenceSpace)(XrSession session, const XrReferenceSpaceCreateInfo* createInfo, XrSpace* space);
#undef XR_AFTER_xrCreateReferenceSpace
#define XR_AFTER_xrCreateReferenceSpace(funcName)                    \
    {                                                                \
        if (XR_SUCCEEDED(result))                                    \
            SendXrReferenceSpaceCreate(funcName, createInfo, space); \
    }