#include "../mock.h"

#define CHECK_EXT()       \
    if (nullptr == s_ext) \
        return XR_ERROR_FUNCTION_UNSUPPORTED;

struct ConformanceAutomation
{
    std::map<XrPath, MockInputState> states;
    std::map<std::pair<XrPath, XrPath>, bool> activeStates;

    MockInputState& GetState(XrPath path, XrActionType actionType)
    {
        auto it = states.find(path);
        if (it != states.end())
            return it->second;

        MockInputState& state = states[path];
        state.type = actionType;
        return state;
    }
};

static ConformanceAutomation* s_ext = nullptr;

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSetInputDeviceActiveEXT(XrSession session, XrPath interactionProfile, XrPath topLevelPath, XrBool32 isActive)
{
    LOG_FUNC();
    CHECK_RUNTIME();
    CHECK_EXT();
    s_ext->activeStates[std::pair<XrPath, XrPath>(interactionProfile, topLevelPath)] = isActive;
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSetInputDeviceStateBoolEXT(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, XrBool32 state)
{
    LOG_FUNC();
    CHECK_EXT();
    s_ext->GetState(inputSourcePath, XR_ACTION_TYPE_BOOLEAN_INPUT).Set(state);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSetInputDeviceStateFloatEXT(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, float state)
{
    LOG_FUNC();
    CHECK_EXT();
    CHECK_SESSION(session);
    s_ext->GetState(inputSourcePath, XR_ACTION_TYPE_FLOAT_INPUT).Set(state);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSetInputDeviceStateVector2fEXT(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, XrVector2f state)
{
    LOG_FUNC();
    CHECK_EXT();
    s_ext->GetState(inputSourcePath, XR_ACTION_TYPE_VECTOR2F_INPUT).Set(state);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSetInputDeviceLocationEXT(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, XrSpace space, XrPosef pose)
{
    LOG_FUNC();
    CHECK_EXT();
    s_ext->GetState(inputSourcePath, XR_ACTION_TYPE_POSE_INPUT).Set(space, pose);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSetInputDeviceVelocityUNITY(XrSession session, XrPath topLevelPath, XrPath inputSourcePath, bool linearValid, XrVector3f linear, bool angularValid, XrVector3f angular)
{
    LOG_FUNC();
    CHECK_EXT();
    s_ext->GetState(inputSourcePath, XR_ACTION_TYPE_POSE_INPUT).SetVelocity(linearValid, linear, angularValid, angular);
    return XR_SUCCESS;
}

void ConformanceAutomation_Create()
{
    // Make sure there is no orphaned extension
    ConformanceAutomation_Destroy();

    s_ext = new ConformanceAutomation();
}

void ConformanceAutomation_Destroy()
{
    delete s_ext;
    s_ext = nullptr;
}

XrResult ConformanceAutomation_GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function)
{
    CHECK_EXT();
    GET_PROC_ADDRESS(xrSetInputDeviceActiveEXT)
    GET_PROC_ADDRESS(xrSetInputDeviceStateBoolEXT)
    GET_PROC_ADDRESS(xrSetInputDeviceStateFloatEXT)
    GET_PROC_ADDRESS(xrSetInputDeviceStateVector2fEXT)
    GET_PROC_ADDRESS(xrSetInputDeviceLocationEXT)
    GET_PROC_ADDRESS(xrSetInputDeviceVelocityUNITY)
    return XR_ERROR_FUNCTION_UNSUPPORTED;
}

XrResult ConformanceAutomation_GetInputState(MockInputState* state)
{
    CHECK_EXT();

    auto it = s_ext->states.find(state->path);
    if (it == s_ext->states.end())
        return XR_ERROR_HANDLE_INVALID;

    state->CopyValue(it->second);

    return XR_SUCCESS;
}

bool ConformanceAutomation_IsActive(XrPath interactionProfilePath, XrPath userPath, bool defaultValue)
{
    if (nullptr == s_ext)
        return false;

    auto active = s_ext->activeStates.find(std::pair<XrPath, XrPath>(interactionProfilePath, userPath));
    if (active == s_ext->activeStates.end())
        active = s_ext->activeStates.find(std::pair<XrPath, XrPath>(XR_NULL_PATH, userPath));

    return (active != s_ext->activeStates.end()) ? active->second : defaultValue;
}