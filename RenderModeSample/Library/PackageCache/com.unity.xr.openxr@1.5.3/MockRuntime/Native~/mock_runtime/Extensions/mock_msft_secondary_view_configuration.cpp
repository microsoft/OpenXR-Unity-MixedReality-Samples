#include "../mock.h"

XrResult MockRuntime::MSFTSecondaryViewConfiguration_BeginSession(const XrSessionBeginInfo* beginInfo)
{
    // Check for secondary view configuration
    const XrSecondaryViewConfigurationSessionBeginInfoMSFT* secondaryViewConfiguration =
        FindNextPointerType<XrSecondaryViewConfigurationSessionBeginInfoMSFT>(beginInfo, XR_TYPE_SECONDARY_VIEW_CONFIGURATION_SESSION_BEGIN_INFO_MSFT);

    if (nullptr == secondaryViewConfiguration)
        return XR_SUCCESS;

    if (secondaryViewConfiguration->viewConfigurationCount == 0)
        return XR_ERROR_VALIDATION_FAILURE;

    if (secondaryViewConfiguration->enabledViewConfigurationTypes == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    secondaryViewConfigurationStates.reserve(secondaryViewConfiguration->viewConfigurationCount);
    for (uint32_t i = 0; i < secondaryViewConfiguration->viewConfigurationCount; i++)
    {
        MockViewConfiguration* viewConfiguration = GetMockViewConfiguration(secondaryViewConfiguration->enabledViewConfigurationTypes[i]);
        if (nullptr == viewConfiguration)
            return XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED;

        if (viewConfiguration->primary || viewConfiguration->enabled)
            return XR_ERROR_VALIDATION_FAILURE;

        viewConfiguration->enabled = true;

        XrSecondaryViewConfigurationStateMSFT viewState = {XR_TYPE_SECONDARY_VIEW_CONFIGURATION_STATE_MSFT};
        viewState.viewConfigurationType = secondaryViewConfiguration->enabledViewConfigurationTypes[i];
        viewState.active = viewConfiguration->active;
        secondaryViewConfigurationStates.push_back(viewState);
    }

    return XR_SUCCESS;
}

XrResult MockRuntime::MSFTSecondaryViewConfiguration_WaitFrame(const XrFrameWaitInfo* frameWaitInfo, XrFrameState* frameState)
{
    // Check for secondary view configuration
    XrSecondaryViewConfigurationFrameStateMSFT* secondaryFrameState =
        FindNextPointerType<XrSecondaryViewConfigurationFrameStateMSFT>(frameState, XR_TYPE_SECONDARY_VIEW_CONFIGURATION_FRAME_STATE_MSFT);

    if (secondaryFrameState == nullptr)
        return XR_SUCCESS;

    if (secondaryFrameState->viewConfigurationCount == 0 || secondaryFrameState->viewConfigurationStates == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    if (secondaryFrameState->viewConfigurationCount != (uint32_t)secondaryViewConfigurationStates.size())
        return XR_ERROR_VALIDATION_FAILURE;

    for (auto& viewConfiguration : secondaryViewConfigurationStates)
        viewConfiguration.active = GetMockViewConfiguration(viewConfiguration.viewConfigurationType)->active;

    memcpy(secondaryFrameState->viewConfigurationStates, secondaryViewConfigurationStates.data(), secondaryViewConfigurationStates.size() * sizeof(XrSecondaryViewConfigurationStateMSFT));

    return XR_SUCCESS;
}

XrResult MockRuntime::MSFTSecondaryViewConfiguration_EndFrame(const XrFrameEndInfo* frameEndInfo)
{
    const XrSecondaryViewConfigurationFrameEndInfoMSFT* secondaryFrameEndInfo =
        FindNextPointerType<XrSecondaryViewConfigurationFrameEndInfoMSFT>(frameEndInfo, XR_TYPE_SECONDARY_VIEW_CONFIGURATION_FRAME_END_INFO_MSFT);

    if (secondaryFrameEndInfo == nullptr)
        return XR_SUCCESS;

    if (secondaryFrameEndInfo->viewConfigurationCount == 0 || secondaryFrameEndInfo->viewConfigurationLayersInfo == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    for (uint32_t i = 0; i < secondaryFrameEndInfo->viewConfigurationCount; i++)
    {
        auto& info = secondaryFrameEndInfo->viewConfigurationLayersInfo[0];
        MockViewConfiguration* viewConfiguration = GetMockViewConfiguration(info.viewConfigurationType);
        if (viewConfiguration->primary)
            return XR_ERROR_LAYER_INVALID;
        if (!viewConfiguration->enabled)
            return XR_ERROR_SECONDARY_VIEW_CONFIGURATION_TYPE_NOT_ENABLED_MSFT;
        if (!viewConfiguration->active)
            continue;

        secondaryLayersRendered += info.layerCount;
    }

    return XR_SUCCESS;
}

XrResult MockRuntime::ActivateSecondaryView(XrViewConfigurationType viewConfigurationType, bool active)
{
    MockViewConfiguration* viewConfiguration = GetMockViewConfiguration(viewConfigurationType);
    if (nullptr == viewConfiguration || (IsSessionRunning() && !viewConfiguration->enabled))
        return XR_ERROR_SECONDARY_VIEW_CONFIGURATION_TYPE_NOT_ENABLED_MSFT;

    if (viewConfiguration->primary)
        return XR_ERROR_VALIDATION_FAILURE;

    viewConfiguration->active = active;

    return XR_SUCCESS;
}
