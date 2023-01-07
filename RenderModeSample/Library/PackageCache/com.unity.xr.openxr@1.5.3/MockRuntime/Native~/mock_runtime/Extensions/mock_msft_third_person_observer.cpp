#include "../mock.h"

XrResult MockRuntime::MSFTThirdPersonObserver_Init()
{
    MockViewConfiguration thirdPersonConfig = {};
    thirdPersonConfig.primary = false;
    thirdPersonConfig.enabled = false;
    thirdPersonConfig.active = false;
    thirdPersonConfig.stateFlags = XR_VIEW_STATE_ORIENTATION_TRACKED_BIT |
        XR_VIEW_STATE_ORIENTATION_VALID_BIT |
        XR_VIEW_STATE_POSITION_TRACKED_BIT |
        XR_VIEW_STATE_POSITION_VALID_BIT;
    thirdPersonConfig.views = {
        {viewConfigurations[XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO].views[0].configuration,
            {{0.0f, 0.0f, 0.0f, 1.0f}, {0.0f, 2.0f, 0.0f}},
            {-0.995535672f, 0.995566666f, 0.954059243f, -0.954661012f}}};

    viewConfigurations[XR_VIEW_CONFIGURATION_TYPE_SECONDARY_MONO_THIRD_PERSON_OBSERVER_MSFT] = thirdPersonConfig;

    return XR_SUCCESS;
}