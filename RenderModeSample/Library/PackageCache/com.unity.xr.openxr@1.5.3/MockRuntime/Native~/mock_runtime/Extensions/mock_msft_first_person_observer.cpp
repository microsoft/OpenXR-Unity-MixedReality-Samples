#include "../mock.h"

XrResult MockRuntime::MSFTFirstPersonObserver_Init()
{
    MockViewConfiguration firstPersonConfig = {};
    firstPersonConfig.primary = false;
    firstPersonConfig.enabled = false;
    firstPersonConfig.active = false;
    firstPersonConfig.stateFlags = XR_VIEW_STATE_ORIENTATION_TRACKED_BIT |
        XR_VIEW_STATE_ORIENTATION_VALID_BIT |
        XR_VIEW_STATE_POSITION_TRACKED_BIT |
        XR_VIEW_STATE_POSITION_VALID_BIT;
    firstPersonConfig.views = {
        {viewConfigurations[XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO].views[0].configuration,
            {{0.0f, 0.0f, 0.0f, 1.0f}, {0.0f, 2.0f, 0.0f}},
            {-0.995535672f, 0.995566666f, 0.954059243f, -0.954661012f}}};

    viewConfigurations[XR_VIEW_CONFIGURATION_TYPE_SECONDARY_MONO_FIRST_PERSON_OBSERVER_MSFT] = firstPersonConfig;

    return XR_SUCCESS;
}