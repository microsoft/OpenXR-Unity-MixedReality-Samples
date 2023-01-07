#include "mock.h"
#include <openxr/loader_interfaces.h>

IUnityXRTrace* s_Trace = nullptr;
MockRuntime* s_runtime = nullptr;

uint64_t s_nextInstanceId = 11; // Start at 11 because 10 is a special test case

#define XR_UNITY_mock_test_SPEC_VERSION 123
#define XR_UNITY_MOCK_TEST_EXTENSION_NAME "XR_UNITY_mock_test"

#define XR_UNITY_null_gfx_SPEC_VERSION 1
#define XR_UNITY_NULL_GFX_EXTENSION_NAME "XR_UNITY_null_gfx"

#define XR_UNITY_android_present_SPEC_VERSION 1
#define XR_UNITY_ANDROID_PRESENT_EXTENSION_NAME "XR_UNITY_android_present"

#define ENUM_TO_STR(name, val)                                 \
    case val:                                                  \
        strncpy(buffer, #name, XR_MAX_RESULT_STRING_SIZE - 1); \
        break;

// clang-format off
    static XrExtensionProperties s_Extensions[] = {
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_UNITY_MOCK_TEST_EXTENSION_NAME,
        XR_UNITY_mock_test_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_UNITY_NULL_GFX_EXTENSION_NAME,
        XR_UNITY_null_gfx_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_UNITY_ANDROID_PRESENT_EXTENSION_NAME,
        XR_UNITY_android_present_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_KHR_VISIBILITY_MASK_EXTENSION_NAME,
        XR_KHR_visibility_mask_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_EXT_CONFORMANCE_AUTOMATION_EXTENSION_NAME,
        XR_EXT_conformance_automation_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_KHR_COMPOSITION_LAYER_DEPTH_EXTENSION_NAME,
        XR_KHR_composition_layer_depth_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_VARJO_QUAD_VIEWS_EXTENSION_NAME,
        XR_VARJO_quad_views_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_MSFT_SECONDARY_VIEW_CONFIGURATION_EXTENSION_NAME,
        XR_MSFT_secondary_view_configuration_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_EXT_EYE_GAZE_INTERACTION_EXTENSION_NAME,
        XR_EXT_eye_gaze_interaction_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_MSFT_HAND_INTERACTION_EXTENSION_NAME,
        XR_MSFT_hand_interaction_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_MSFT_FIRST_PERSON_OBSERVER_EXTENSION_NAME,
        XR_MSFT_first_person_observer_SPEC_VERSION
    },
    {
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_MSFT_THIRD_PERSON_OBSERVER_PRIVATE_EXTENSION_NAME,
        XR_MSFT_third_person_observer_private_SPEC_VERSION
    }
#if defined(XR_USE_GRAPHICS_API_VULKAN)    
    ,{
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_KHR_VULKAN_ENABLE2_EXTENSION_NAME,
        XR_KHR_vulkan_enable2_SPEC_VERSION        
    }
#endif    

#if defined(XR_USE_PLATFORM_WIN32)
    ,{
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_KHR_WIN32_CONVERT_PERFORMANCE_COUNTER_TIME_EXTENSION_NAME,
        XR_KHR_win32_convert_performance_counter_time_SPEC_VERSION        
    }
#endif

#if defined(XR_USE_GRAPHICS_API_D3D11)
    ,{
        XR_TYPE_EXTENSION_PROPERTIES,
        nullptr,
        XR_KHR_D3D11_ENABLE_EXTENSION_NAME,
        XR_KHR_D3D11_enable_SPEC_VERSION
    }
#endif
};
// clang-format on

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEnumerateApiLayerProperties(uint32_t propertyCapacityInput, uint32_t* propertyCountOutput, XrApiLayerProperties* properties)
{
    LOG_FUNC();
    *propertyCountOutput = 0;
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEnumerateInstanceExtensionProperties(const char* layerName, uint32_t propertyCapacityInput, uint32_t* propertyCountOutput, XrExtensionProperties* properties)
{
    LOG_FUNC();

    *propertyCountOutput = sizeof(s_Extensions) / sizeof(XrExtensionProperties);

    if (propertyCapacityInput == 0)
        return XR_SUCCESS;
    if (propertyCapacityInput < *propertyCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    uint32_t count = 0;
    while (count < *propertyCountOutput)
    {
        properties[count] = s_Extensions[count];
        ++count;
    }

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateInstance(const XrInstanceCreateInfo* createInfo, XrInstance* instance)
{
    LOG_FUNC();

    // Destroy any existing runtime if there is one
    if (s_runtime != nullptr)
    {
        delete s_runtime;
        s_runtime = nullptr;
    }

    *instance = 0;

    MockRuntimeCreateFlags flags = 0;

    for (uint32_t i = 0; i < createInfo->enabledExtensionCount; ++i)
    {
        const char* extension = createInfo->enabledExtensionNames[i];
        if (strncmp(XR_UNITY_MOCK_TEST_EXTENSION_NAME, extension, sizeof(XR_UNITY_MOCK_TEST_EXTENSION_NAME)) == 0)
        {
            *instance = (XrInstance)10;
            continue;
        }

        if ((flags & MR_CREATE_ALL_GFX_EXT) == 0)
        {
#if defined(XR_USE_GRAPHICS_API_VULKAN)
            if (strncmp(XR_KHR_VULKAN_ENABLE2_EXTENSION_NAME, extension, sizeof(XR_KHR_VULKAN_ENABLE2_EXTENSION_NAME)) == 0)
            {
                flags |= MR_CREATE_VULKAN_GFX_EXT;
                continue;
            }
#endif
#if defined(XR_USE_GRAPHICS_API_D3D11)
            if (strncmp(XR_KHR_D3D11_ENABLE_EXTENSION_NAME, extension, sizeof(XR_KHR_D3D11_ENABLE_EXTENSION_NAME)) == 0)
            {
                flags |= MR_CREATE_D3D11_GFX_EXT;
                continue;
            }
#endif
            if (strncmp(XR_UNITY_NULL_GFX_EXTENSION_NAME, extension, sizeof(XR_UNITY_NULL_GFX_EXTENSION_NAME)) == 0)
            {
                flags |= MR_CREATE_NULL_GFX_EXT;
                continue;
            }
        }

        // Conformance Automation
        if (strncmp(XR_EXT_CONFORMANCE_AUTOMATION_EXTENSION_NAME, extension, sizeof(XR_EXT_CONFORMANCE_AUTOMATION_EXTENSION_NAME)) == 0)
        {
            flags |= MR_CREATE_CONFORMANCE_AUTOMATION_EXT;
            continue;
        }

        if (strncmp(XR_KHR_COMPOSITION_LAYER_DEPTH_EXTENSION_NAME, extension, sizeof(XR_KHR_COMPOSITION_LAYER_DEPTH_EXTENSION_NAME)) == 0)
        {
            flags |= MR_CREATE_COMPOSITION_LAYER_DEPTH_EXT;
            continue;
        }

        if (strncmp(XR_VARJO_QUAD_VIEWS_EXTENSION_NAME, extension, sizeof(XR_VARJO_QUAD_VIEWS_EXTENSION_NAME)) == 0)
        {
            flags |= MR_CREATE_VARJO_QUAD_VIEWS_EXT;
            continue;
        }

        if (strncmp(XR_MSFT_SECONDARY_VIEW_CONFIGURATION_EXTENSION_NAME, extension, sizeof(XR_MSFT_SECONDARY_VIEW_CONFIGURATION_EXTENSION_NAME)) == 0)
        {
            flags |= MR_CREATE_MSFT_SECONDARY_VIEW_CONFIGURATION_EXT;
            continue;
        }

        if (strncmp(XR_MSFT_FIRST_PERSON_OBSERVER_EXTENSION_NAME, extension, sizeof(XR_MSFT_FIRST_PERSON_OBSERVER_EXTENSION_NAME)) == 0)
        {
            flags |= MR_CREATE_MSFT_FIRST_PERSON_OBSERVER_EXT;
            continue;
        }

        if (strncmp(XR_MSFT_THIRD_PERSON_OBSERVER_PRIVATE_EXTENSION_NAME, extension, sizeof(XR_MSFT_THIRD_PERSON_OBSERVER_PRIVATE_EXTENSION_NAME)) == 0)
        {
            flags |= MR_CREATE_MSFT_THIRD_PERSON_OBSERVER_EXT;
            continue;
        }

        if (strncmp(XR_EXT_EYE_GAZE_INTERACTION_EXTENSION_NAME, extension, sizeof(XR_EXT_EYE_GAZE_INTERACTION_EXTENSION_NAME)) == 0)
        {
            flags |= MR_CREATE_EYE_GAZE_INTERACTION_EXT;
            continue;
        }

        if (strncmp(XR_MSFT_HAND_INTERACTION_EXTENSION_NAME, extension, sizeof(XR_MSFT_HAND_INTERACTION_EXTENSION_NAME)) == 0)
        {
            flags |= MR_CREATE_MSFT_HAND_INTERACTION_EXT;
            continue;
        }
    }

    if ((flags & MR_CREATE_ALL_GFX_EXT) == 0)
        flags |= MR_CREATE_NULL_GFX_EXT;

    // Assign an instance id if one was not given
    if (*instance == 0)
        *instance = (XrInstance)(s_nextInstanceId++);

    s_runtime = new MockRuntime(*instance, flags);

    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrDestroyInstance(XrInstance instance)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);

    MOCK_HOOK_BEFORE();

    delete s_runtime;
    s_runtime = nullptr;

    MOCK_HOOK_AFTER(XR_SUCCESS);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetInstanceProperties(XrInstance instance, XrInstanceProperties* instanceProperties)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);

    instanceProperties->runtimeVersion = XR_MAKE_VERSION(0, 0, 2);
    strncpy(instanceProperties->runtimeName, "Unity Mock Runtime", XR_MAX_RUNTIME_NAME_SIZE);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrPollEvent(XrInstance instance, XrEventDataBuffer* eventData)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK(s_runtime->GetNextEvent(eventData));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrResultToString(XrInstance instance, XrResult value, char buffer[XR_MAX_RESULT_STRING_SIZE])
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);

    switch (value)
    {
        XR_LIST_ENUM_XrResult(ENUM_TO_STR) default : strncpy(buffer, ((value < 0 ? "XR_UNKNOWN_FAILURE_" : "XR_UNKNOWN_SUCCESS_") + std::to_string(value)).c_str(), XR_MAX_RESULT_STRING_SIZE - 1);
        break;
    }

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrStructureTypeToString(XrInstance instance, XrStructureType value, char buffer[XR_MAX_STRUCTURE_NAME_SIZE])
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);

    switch (value)
    {
        XR_LIST_ENUM_XrStructureType(ENUM_TO_STR) default : strncpy(buffer, ("XR_UNKNOWN_STRUCTURE_TYPE_" + std::to_string(value)).c_str(), XR_MAX_RESULT_STRING_SIZE - 1);
        break;
    }

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetSystem(XrInstance instance, const XrSystemGetInfo* getInfo, XrSystemId* systemId)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    *systemId = (XrSystemId)2;
    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetSystemProperties(XrInstance instance, XrSystemId systemId, XrSystemProperties* properties)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK(s_runtime->GetSystemProperties(systemId, properties));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEnumerateEnvironmentBlendModes(XrInstance instance, XrSystemId systemId, XrViewConfigurationType viewConfigurationType, uint32_t environmentBlendModeCapacityInput, uint32_t* environmentBlendModeCountOutput, XrEnvironmentBlendMode* environmentBlendModes)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK(s_runtime->EnumerateEnvironmentBlendModes(systemId, viewConfigurationType, environmentBlendModeCapacityInput, environmentBlendModeCountOutput, environmentBlendModes));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateSession(XrInstance instance, const XrSessionCreateInfo* createInfo, XrSession* session)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK_BEFORE();

    XrResult result = s_runtime->CreateSession(createInfo);
    if (result == XR_SUCCESS)
        *session = s_runtime->GetSession();

    MOCK_HOOK_AFTER(result);

    return result;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrDestroySession(XrSession session)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->DestroySession());
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEnumerateReferenceSpaces(XrSession session, uint32_t spaceCapacityInput, uint32_t* spaceCountOutput, XrReferenceSpaceType* spaces)
{
    LOG_FUNC();
    CHECK_SESSION(session);

    if (!spaceCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    *spaceCountOutput = 4;

    if (spaceCapacityInput == 0)
        return XR_SUCCESS;

    if (spaceCapacityInput < *spaceCountOutput || !spaces)
        return XR_ERROR_VALIDATION_FAILURE;

    spaces[0] = XR_REFERENCE_SPACE_TYPE_VIEW;
    spaces[1] = XR_REFERENCE_SPACE_TYPE_LOCAL;
    spaces[2] = XR_REFERENCE_SPACE_TYPE_STAGE;
    spaces[3] = XR_REFERENCE_SPACE_TYPE_UNBOUNDED_MSFT;

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateReferenceSpace(XrSession session, const XrReferenceSpaceCreateInfo* createInfo, XrSpace* space)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->CreateReferenceSpace(createInfo, space));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetReferenceSpaceBoundsRect(XrSession session, XrReferenceSpaceType referenceSpaceType, XrExtent2Df* bounds)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->GetReferenceSpaceBoundsRect(referenceSpaceType, bounds));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateActionSpace(XrSession session, const XrActionSpaceCreateInfo* createInfo, XrSpace* space)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->CreateActionSpace(createInfo, space));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrLocateSpace(XrSpace space, XrSpace baseSpace, XrTime time, XrSpaceLocation* location)
{
    LOG_FUNC();
    CHECK_RUNTIME();
    MOCK_HOOK(s_runtime->LocateSpace(space, baseSpace, time, location));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrDestroySpace(XrSpace space)
{
    LOG_FUNC();
    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEnumerateViewConfigurations(XrInstance instance, XrSystemId systemId, uint32_t viewConfigurationTypeCapacityInput, uint32_t* viewConfigurationTypeCountOutput, XrViewConfigurationType* viewConfigurationTypes)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK(s_runtime->EnumerateViewConfigurations(systemId, viewConfigurationTypeCapacityInput, viewConfigurationTypeCountOutput, viewConfigurationTypes));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetViewConfigurationProperties(XrInstance instance, XrSystemId systemId, XrViewConfigurationType viewConfigurationType, XrViewConfigurationProperties* configurationProperties)
{
    LOG_FUNC();
    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEnumerateViewConfigurationViews(XrInstance instance, XrSystemId systemId, XrViewConfigurationType viewConfigurationType, uint32_t viewCapacityInput, uint32_t* viewCountOutput, XrViewConfigurationView* views)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK(s_runtime->EnumerateViewConfigurationViews(systemId, viewConfigurationType, viewCapacityInput, viewCountOutput, views));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEnumerateSwapchainFormats(XrSession session, uint32_t formatCapacityInput, uint32_t* formatCountOutput, int64_t* formats)
{
    LOG_FUNC();

    *formatCountOutput = 1;

    if (formatCapacityInput == 0)
        return XR_SUCCESS;
    if (formatCapacityInput < *formatCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    formats[0] = 0;

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateSwapchain(XrSession session, const XrSwapchainCreateInfo* createInfo, XrSwapchain* swapchain)
{
    LOG_FUNC();

    static uint64_t uniqueSwapchainHandle = 0;

    *swapchain = (XrSwapchain)++uniqueSwapchainHandle;

    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrDestroySwapchain(XrSwapchain swapchain)
{
    LOG_FUNC();
    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEnumerateSwapchainImages(XrSwapchain swapchain, uint32_t imageCapacityInput, uint32_t* imageCountOutput, XrSwapchainImageBaseHeader* images)
{
    LOG_FUNC();

    *imageCountOutput = 1;

    if (imageCapacityInput == 0)
        return XR_SUCCESS;
    if (imageCapacityInput < *imageCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrAcquireSwapchainImage(XrSwapchain swapchain, const XrSwapchainImageAcquireInfo* acquireInfo, uint32_t* index)
{
    LOG_FUNC();

    *index = 0;

    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrWaitSwapchainImage(XrSwapchain swapchain, const XrSwapchainImageWaitInfo* waitInfo)
{
    LOG_FUNC();
    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrReleaseSwapchainImage(XrSwapchain swapchain, const XrSwapchainImageReleaseInfo* releaseInfo)
{
    LOG_FUNC();
    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrBeginSession(XrSession session, const XrSessionBeginInfo* beginInfo)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->BeginSession(beginInfo));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEndSession(XrSession session)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    //CHECK_EXPECTED_RESULT(XR_SUCCESS, XR_ERROR_SESSION_NOT_STOPPING);

    MOCK_HOOK(s_runtime->EndSession());
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrRequestExitSession(XrSession session)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->RequestExitSession());
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrWaitFrame(XrSession session, const XrFrameWaitInfo* frameWaitInfo, XrFrameState* frameState)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->WaitFrame(frameWaitInfo, frameState));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrBeginFrame(XrSession session, const XrFrameBeginInfo* frameBeginInfo)
{
    LOG_FUNC();
    MOCK_HOOK(XR_SUCCESS);
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEndFrame(XrSession session, const XrFrameEndInfo* frameEndInfo)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->EndFrame(frameEndInfo));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrLocateViews(XrSession session, const XrViewLocateInfo* viewLocateInfo, XrViewState* viewState, uint32_t viewCapacityInput, uint32_t* viewCountOutput, XrView* views)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->LocateViews(viewLocateInfo, viewState, viewCapacityInput, viewCountOutput, views));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrStringToPath(XrInstance instance, const char* pathString, XrPath* path)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK(s_runtime->StringToPath(pathString, path));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrPathToString(XrInstance instance, XrPath path, uint32_t bufferCapacityInput, uint32_t* bufferCountOutput, char* buffer)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK(s_runtime->PathToString(path, bufferCapacityInput, bufferCountOutput, buffer));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateActionSet(XrInstance instance, const XrActionSetCreateInfo* createInfo, XrActionSet* actionSet)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK(s_runtime->CreateActionSet(createInfo, actionSet));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrDestroyActionSet(XrActionSet actionSet)
{
    LOG_FUNC();
    CHECK_RUNTIME();
    MOCK_HOOK(s_runtime->DestroyActionSet(actionSet));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateAction(XrActionSet actionSet, const XrActionCreateInfo* createInfo, XrAction* action)
{
    LOG_FUNC();
    CHECK_RUNTIME();
    MOCK_HOOK(s_runtime->CreateAction(actionSet, createInfo, action));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrDestroyAction(XrAction action)
{
    LOG_FUNC();
    CHECK_RUNTIME();
    MOCK_HOOK(s_runtime->DestroyAction(action));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSuggestInteractionProfileBindings(XrInstance instance, const XrInteractionProfileSuggestedBinding* suggestedBindings)
{
    LOG_FUNC();
    CHECK_INSTANCE(instance);
    MOCK_HOOK(s_runtime->SuggestInteractionProfileBindings(suggestedBindings));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrAttachSessionActionSets(XrSession session, const XrSessionActionSetsAttachInfo* attachInfo)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->AttachSessionActionSets(attachInfo));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetCurrentInteractionProfile(XrSession session, XrPath topLevelUserPath, XrInteractionProfileState* interactionProfile)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->GetCurrentInteractionProfile(topLevelUserPath, interactionProfile));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetActionStateBoolean(XrSession session, const XrActionStateGetInfo* getInfo, XrActionStateBoolean* state)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->GetActionStateBoolean(getInfo, state));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetActionStateFloat(XrSession session, const XrActionStateGetInfo* getInfo, XrActionStateFloat* state)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->GetActionStateFloat(getInfo, state));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetActionStateVector2f(XrSession session, const XrActionStateGetInfo* getInfo, XrActionStateVector2f* state)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->GetActionStateVector2f(getInfo, state));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetActionStatePose(XrSession session, const XrActionStateGetInfo* getInfo, XrActionStatePose* state)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->GetActionStatePose(getInfo, state));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSyncActions(XrSession session, const XrActionsSyncInfo* syncInfo)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->SyncActions(syncInfo));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEnumerateBoundSourcesForAction(XrSession session, const XrBoundSourcesForActionEnumerateInfo* enumerateInfo, uint32_t sourceCapacityInput, uint32_t* sourceCountOutput, XrPath* sources)
{
    LOG_FUNC();
    CHECK_SESSION(session)
    MOCK_HOOK(s_runtime->EnumerateBoundSourcesForAction(enumerateInfo, sourceCapacityInput, sourceCountOutput, sources));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetInputSourceLocalizedName(XrSession session, const XrInputSourceLocalizedNameGetInfo* getInfo, uint32_t bufferCapacityInput, uint32_t* bufferCountOutput, char* buffer)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->GetInputSourceLocalizedName(getInfo, bufferCapacityInput, bufferCountOutput, buffer));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrApplyHapticFeedback(XrSession session, const XrHapticActionInfo* hapticActionInfo, const XrHapticBaseHeader* hapticFeedback)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->ApplyHapticFeedback(hapticActionInfo, hapticFeedback));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrStopHapticFeedback(XrSession session, const XrHapticActionInfo* hapticActionInfo)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    MOCK_HOOK(s_runtime->StopHapticFeedback(hapticActionInfo));
}

extern uint32_t s_VisibilityMaskVerticesSizes[2][3];
extern uint32_t s_VisibilityMaskIndicesSizes[2][3];
extern XrVector2f s_VisibilityMaskVertices[2][3][99];
extern uint32_t s_VisibilityMaskIndices[2][3][99];

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetVisibilityMaskKHR(XrSession session, XrViewConfigurationType viewConfigurationType, uint32_t viewIndex, XrVisibilityMaskTypeKHR visibilityMaskType, XrVisibilityMaskKHR* visibilityMask)
{
    const uint32_t visiblityMaskTypeLookup = visibilityMaskType - 1;
    visibilityMask->vertexCountOutput = s_VisibilityMaskVerticesSizes[viewIndex][visiblityMaskTypeLookup];
    visibilityMask->indexCountOutput = s_VisibilityMaskIndicesSizes[viewIndex][visiblityMaskTypeLookup];

    if (visibilityMask->vertexCapacityInput == 0 || visibilityMask->indexCapacityInput == 0)
        return XR_SUCCESS;
    if (visibilityMask->vertexCapacityInput < visibilityMask->vertexCountOutput ||
        visibilityMask->indexCapacityInput < visibilityMask->indexCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    memcpy(visibilityMask->vertices, &s_VisibilityMaskVertices[viewIndex][visiblityMaskTypeLookup], sizeof(XrVector2f) * visibilityMask->vertexCountOutput);
    memcpy(visibilityMask->indices, &s_VisibilityMaskIndices[viewIndex][visiblityMaskTypeLookup], sizeof(uint32_t) * visibilityMask->indexCountOutput);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetInstanceProcAddr(XrInstance instance, const char* name, PFN_xrVoidFunction* function)
{
    LOG_FUNC();

    if (s_runtime != nullptr && XR_SUCCESS == s_runtime->GetInstanceProcAddr(name, function))
        return XR_SUCCESS;

    GET_PROC_ADDRESS(xrEnumerateApiLayerProperties)
    GET_PROC_ADDRESS(xrEnumerateInstanceExtensionProperties)
    GET_PROC_ADDRESS(xrCreateInstance)
    GET_PROC_ADDRESS(xrDestroyInstance)
    GET_PROC_ADDRESS(xrGetInstanceProperties)
    GET_PROC_ADDRESS(xrPollEvent)
    GET_PROC_ADDRESS(xrResultToString)
    GET_PROC_ADDRESS(xrStructureTypeToString)
    GET_PROC_ADDRESS(xrGetSystem)
    GET_PROC_ADDRESS(xrGetSystemProperties)
    GET_PROC_ADDRESS(xrEnumerateEnvironmentBlendModes)
    GET_PROC_ADDRESS(xrCreateSession)
    GET_PROC_ADDRESS(xrDestroySession)
    GET_PROC_ADDRESS(xrEnumerateReferenceSpaces)
    GET_PROC_ADDRESS(xrCreateReferenceSpace)
    GET_PROC_ADDRESS(xrGetReferenceSpaceBoundsRect)
    GET_PROC_ADDRESS(xrCreateActionSpace)
    GET_PROC_ADDRESS(xrLocateSpace)
    GET_PROC_ADDRESS(xrDestroySpace)
    GET_PROC_ADDRESS(xrEnumerateViewConfigurations)
    GET_PROC_ADDRESS(xrGetViewConfigurationProperties)
    GET_PROC_ADDRESS(xrEnumerateViewConfigurationViews)
    GET_PROC_ADDRESS(xrEnumerateSwapchainFormats)
    GET_PROC_ADDRESS(xrCreateSwapchain)
    GET_PROC_ADDRESS(xrDestroySwapchain)
    GET_PROC_ADDRESS(xrEnumerateSwapchainImages)
    GET_PROC_ADDRESS(xrAcquireSwapchainImage)
    GET_PROC_ADDRESS(xrWaitSwapchainImage)
    GET_PROC_ADDRESS(xrReleaseSwapchainImage)
    GET_PROC_ADDRESS(xrBeginSession)
    GET_PROC_ADDRESS(xrEndSession)
    GET_PROC_ADDRESS(xrRequestExitSession)
    GET_PROC_ADDRESS(xrWaitFrame)
    GET_PROC_ADDRESS(xrBeginFrame)
    GET_PROC_ADDRESS(xrEndFrame)
    GET_PROC_ADDRESS(xrLocateViews)
    GET_PROC_ADDRESS(xrStringToPath)
    GET_PROC_ADDRESS(xrPathToString)
    GET_PROC_ADDRESS(xrCreateActionSet)
    GET_PROC_ADDRESS(xrDestroyActionSet)
    GET_PROC_ADDRESS(xrCreateAction)
    GET_PROC_ADDRESS(xrDestroyAction)
    GET_PROC_ADDRESS(xrSuggestInteractionProfileBindings)
    GET_PROC_ADDRESS(xrAttachSessionActionSets)
    GET_PROC_ADDRESS(xrGetCurrentInteractionProfile)
    GET_PROC_ADDRESS(xrGetActionStateBoolean)
    GET_PROC_ADDRESS(xrGetActionStateFloat)
    GET_PROC_ADDRESS(xrGetActionStateVector2f)
    GET_PROC_ADDRESS(xrGetActionStatePose)
    GET_PROC_ADDRESS(xrSyncActions)
    GET_PROC_ADDRESS(xrEnumerateBoundSourcesForAction)
    GET_PROC_ADDRESS(xrGetInputSourceLocalizedName)
    GET_PROC_ADDRESS(xrApplyHapticFeedback)
    GET_PROC_ADDRESS(xrStopHapticFeedback)
    GET_PROC_ADDRESS(xrGetVisibilityMaskKHR)

    if (XR_SUCCEEDED(GetProcAddrMockAPI(instance, name, function)))
        return XR_SUCCESS;

    return XR_ERROR_FUNCTION_UNSUPPORTED;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetXRTrace(IUnityXRTrace* trace)
{
    s_Trace = trace;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrNegotiateLoaderRuntimeInterface(const XrNegotiateLoaderInfo* loaderInfo, XrNegotiateRuntimeRequest* runtimeRequest)
{
    runtimeRequest->getInstanceProcAddr = xrGetInstanceProcAddr;
    runtimeRequest->runtimeApiVersion = XR_CURRENT_API_VERSION;
    runtimeRequest->runtimeInterfaceVersion = 1;
    return XR_SUCCESS;
}
