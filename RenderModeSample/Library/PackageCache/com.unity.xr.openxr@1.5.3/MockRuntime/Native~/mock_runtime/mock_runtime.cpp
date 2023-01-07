#include "mock.h"
#include <algorithm>
#include <mutex>

static std::mutex s_EventQueueMutex;
static std::mutex s_ExpectedResultMutex;

MockRuntime::MockRuntime(XrInstance instance, MockRuntimeCreateFlags flags)
{
    this->instance = instance;
    session = XR_NULL_HANDLE;
    nextHandle = 1;
    createFlags = flags;
    currentState = XR_SESSION_STATE_UNKNOWN;

    scriptEventCallback = nullptr;

    isRunning = false;
    exitSessionRequested = false;
    actionSetsAttached = false;

    startTime = std::chrono::high_resolution_clock::now();

    XrViewStateFlags defaultViewStateFlags =
        XR_VIEW_STATE_ORIENTATION_TRACKED_BIT |
        XR_VIEW_STATE_ORIENTATION_VALID_BIT |
        XR_VIEW_STATE_POSITION_TRACKED_BIT |
        XR_VIEW_STATE_POSITION_VALID_BIT;

    XrViewConfigurationView defaultViewConfig{XR_TYPE_VIEW_CONFIGURATION_VIEW};
    defaultViewConfig.recommendedImageRectWidth = 1512;
    defaultViewConfig.maxImageRectWidth = 1512 * 2;
    defaultViewConfig.recommendedImageRectHeight = 1680;
    defaultViewConfig.maxImageRectHeight = 1680 * 2;
    defaultViewConfig.recommendedSwapchainSampleCount = 1;
    defaultViewConfig.maxSwapchainSampleCount = 1;

    // Initialzie stereo view
    MockViewConfiguration stereoViewConfig = {};
    stereoViewConfig.primary = true;
    stereoViewConfig.enabled = true;
    stereoViewConfig.active = true;
    stereoViewConfig.stateFlags = defaultViewStateFlags;
    stereoViewConfig.views = {{defaultViewConfig,
                                  {{0.0f, 0.0f, 0.0f, 1.0f}, {-0.011f, 0.0f, 0.0f}},
                                  {-0.995535672f, 0.811128199f, 0.954059243f, -0.954661012f}},
        {defaultViewConfig,
            {{0.0f, 0.0f, 0.0f, 1.0f}, {0.011f, 0.0f, 0.0f}},
            {-0.812360585f, 0.995566666f, 0.955580175f, -0.953877985f}}};
    viewConfigurations[XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO] = stereoViewConfig;

    // Add quad view poses if the extension is enabled
    if ((createFlags & MR_CREATE_VARJO_QUAD_VIEWS_EXT) != 0)
    {
        auto& stereoView0 = viewConfigurations[XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO].views[0];
        auto& stereoView1 = viewConfigurations[XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO].views[1];
        XrViewConfigurationView quadViewConfigView = stereoView0.configuration;
        quadViewConfigView.recommendedImageRectWidth /= 3;
        quadViewConfigView.maxImageRectWidth /= 3;
        quadViewConfigView.recommendedImageRectHeight /= 3;
        quadViewConfigView.maxImageRectHeight /= 3;

        MockViewConfiguration quadViewConfig = {};
        quadViewConfig.primary = true;
        quadViewConfig.enabled = true;
        quadViewConfig.active = true;
        quadViewConfig.stateFlags = defaultViewStateFlags;
        quadViewConfig.views = {
            stereoView0,
            stereoView1,
            {quadViewConfigView,
                stereoView0.pose,
                {stereoView0.fov.angleLeft / 3.0f,
                    stereoView0.fov.angleRight / 3.0f,
                    stereoView0.fov.angleUp / 3.0f,
                    stereoView0.fov.angleDown / 3.0f}},
            {quadViewConfigView,
                stereoView1.pose,
                {stereoView1.fov.angleLeft / 3.0f,
                    stereoView1.fov.angleRight / 3.0f,
                    stereoView1.fov.angleUp / 3.0f,
                    stereoView1.fov.angleDown / 3.0f}}};
        viewConfigurations[XR_VIEW_CONFIGURATION_TYPE_PRIMARY_QUAD_VARJO] = quadViewConfig;
    }

    // Add microsoft first person observer view if the extension is enabled
    if ((createFlags & (MR_CREATE_MSFT_FIRST_PERSON_OBSERVER_EXT | MR_CREATE_MSFT_SECONDARY_VIEW_CONFIGURATION_EXT)) == (MR_CREATE_MSFT_FIRST_PERSON_OBSERVER_EXT | MR_CREATE_MSFT_SECONDARY_VIEW_CONFIGURATION_EXT))
        MSFTFirstPersonObserver_Init();

    // Add microsoft third person observer view if the extension is enabled
    if ((createFlags & (MR_CREATE_MSFT_THIRD_PERSON_OBSERVER_EXT | MR_CREATE_MSFT_SECONDARY_VIEW_CONFIGURATION_EXT)) == (MR_CREATE_MSFT_THIRD_PERSON_OBSERVER_EXT | MR_CREATE_MSFT_SECONDARY_VIEW_CONFIGURATION_EXT))
        MSFTThirdPersonObserver_Init();

    // Generate the internal strings
    userPaths = {
        {"/user/hand/left", "Left Hand", nullptr},
        {"/user/hand/right", "Right Hand", nullptr},
        {"/user/head", "Head", nullptr},
        {"/user/gamepad", "GamePad", nullptr}};

    // Add the eye gaze user path if enabled
    if ((createFlags & MR_CREATE_EYE_GAZE_INTERACTION_EXT) != 0)
        userPaths.push_back({"/user/eyes_ext", "Eyes", nullptr});

    InitializeInteractionProfiles();

    if (IsConformanceAutomationEnabled())
        ConformanceAutomation_Create();
}

MockRuntime::~MockRuntime()
{
    if (IsConformanceAutomationEnabled())
        ConformanceAutomation_Destroy();
}

XrResult MockRuntime::GetReferenceSpaceBoundsRect(XrReferenceSpaceType referenceSpace, XrExtent2Df* extents)
{
    MockReferenceSpace* mockReferenceSpace = GetMockReferenceSpace(referenceSpace);
    if (nullptr == mockReferenceSpace)
        return XR_SPACE_BOUNDS_UNAVAILABLE;

    if (!mockReferenceSpace->validExtent)
        return XR_SPACE_BOUNDS_UNAVAILABLE;

    *extents = mockReferenceSpace->extent;

    return XR_SUCCESS;
}

bool MockRuntime::ChangeSessionStateFrom(XrSessionState fromState, XrSessionState toState)
{
    if (s_Trace)
        s_Trace->Trace(kXRLogTypeDebug, "  - Transitioning from state %s => %s\n", to_string(fromState), to_string(toState));

    if (!IsSessionState(fromState))
        return false;

    ChangeSessionState(toState);
    return true;
}

void MockRuntime::ChangeSessionState(XrSessionState state)
{
    if (currentState == state)
        return;

    if (s_Trace)
        s_Trace->Trace(kXRLogTypeDebug, "  - Settings state to %s\n", to_string(state));

    currentState = state;

    QueueEvent(XrEventDataSessionStateChanged{
        XR_TYPE_EVENT_DATA_SESSION_STATE_CHANGED,
        nullptr,
        session,
        state});
}

XrTime MockRuntime::GetPredictedTime()
{
    return (XrTime)std::chrono::duration_cast<std::chrono::nanoseconds>(std::chrono::high_resolution_clock::now() - startTime).count();
}

XrResult MockRuntime::WaitFrame(const XrFrameWaitInfo* frameWaitInfo, XrFrameState* frameState)
{
    frameState->predictedDisplayTime = GetPredictedTime();
    frameState->predictedDisplayPeriod = 16666000;
    frameState->shouldRender = (createFlags & MR_CREATE_ALL_GFX_EXT) != 0;

    XrResult result = XR_SUCCESS;
    if ((createFlags & MR_CREATE_MSFT_SECONDARY_VIEW_CONFIGURATION_EXT) != 0)
        result = MSFTSecondaryViewConfiguration_WaitFrame(frameWaitInfo, frameState);

    return result;
}

XrResult MockRuntime::EndFrame(const XrFrameEndInfo* frameEndInfo)
{
    secondaryLayersRendered = 0;

    XrResult result = XR_SUCCESS;
    if ((createFlags & MR_CREATE_MSFT_SECONDARY_VIEW_CONFIGURATION_EXT) != 0)
        result = MSFTSecondaryViewConfiguration_EndFrame(frameEndInfo);

    if (result != XR_SUCCESS)
        return result;

    primaryLayersRendered = frameEndInfo->layerCount;

    QueueEvent(XrEventScriptEventMOCK{XR_TYPE_EVENT_SCRIPT_EVENT_MOCK, nullptr, XR_MOCK_SCRIPT_EVENT_END_FRAME});

    return XR_SUCCESS;
}

XrResult MockRuntime::CreateSession(const XrSessionCreateInfo* createInfo)
{
    session = (XrSession)3;

    ChangeSessionState(XR_SESSION_STATE_IDLE);

    // TODO: STATEMANAGEMENT: If users presence is enabled then we need to do this transition at the point
    // where the user is actually known to exist (i.e. presence sensor is triggered.)
    ChangeSessionStateFrom(XR_SESSION_STATE_IDLE, XR_SESSION_STATE_READY);

    return XR_SUCCESS;
}

XrResult MockRuntime::DestroySession()
{
    if (session == 0)
        return XR_ERROR_HANDLE_INVALID;

    isRunning = false;
    exitSessionRequested = false;
    session = XR_NULL_HANDLE;
    actionSetsAttached = false;
    currentState = XR_SESSION_STATE_UNKNOWN;

    return XR_SUCCESS;
}

XrResult MockRuntime::BeginSession(const XrSessionBeginInfo* beginInfo)
{
    isRunning = true;

    ChangeSessionStateFrom(XR_SESSION_STATE_READY, XR_SESSION_STATE_SYNCHRONIZED);
    ChangeSessionStateFrom(XR_SESSION_STATE_SYNCHRONIZED, XR_SESSION_STATE_VISIBLE);
    ChangeSessionStateFrom(XR_SESSION_STATE_VISIBLE, XR_SESSION_STATE_FOCUSED);

    XrResult result = XR_SUCCESS;
    if ((createFlags & MR_CREATE_MSFT_SECONDARY_VIEW_CONFIGURATION_EXT) != 0)
        result = MSFTSecondaryViewConfiguration_BeginSession(beginInfo);
    primaryViewConfiguration = beginInfo->primaryViewConfigurationType;

    return result;
}

XrResult MockRuntime::EndSession()
{
    if (!IsSessionState(XR_SESSION_STATE_STOPPING))
        return XR_ERROR_SESSION_NOT_STOPPING;

    isRunning = false;
    ChangeSessionStateFrom(XR_SESSION_STATE_STOPPING, XR_SESSION_STATE_IDLE);

    if (exitSessionRequested)
    {
        exitSessionRequested = false;
        ChangeSessionStateFrom(XR_SESSION_STATE_IDLE, XR_SESSION_STATE_EXITING);
    }

    return XR_SUCCESS;
}

void MockRuntime::QueueEvent(const XrEventDataBuffer& buffer)
{
    std::lock_guard<std::mutex> lock(s_EventQueueMutex);
    eventQueue.push(buffer);
}

XrEventDataBuffer MockRuntime::GetNextEvent()
{
    std::lock_guard<std::mutex> lock(s_EventQueueMutex);
    if (eventQueue.size() == 0)
        return XrEventDataBuffer{};

    XrEventDataBuffer event = eventQueue.front();
    eventQueue.pop();
    return event;
}

XrResult MockRuntime::GetNextEvent(XrEventDataBuffer* eventData)
{
    if (!eventData)
        return XR_ERROR_HANDLE_INVALID;

    while (true)
    {
        *eventData = GetNextEvent();

        // Handle mock internal events
        switch (eventData->type)
        {
        case XR_TYPE_UNKNOWN:
            return XR_EVENT_UNAVAILABLE;

        case XR_TYPE_EVENT_SCRIPT_EVENT_MOCK:
        {
            XrEventScriptEventMOCK* scriptEvent = (XrEventScriptEventMOCK*)eventData;
            if (nullptr != scriptEventCallback)
                scriptEventCallback(scriptEvent->event, scriptEvent->param);

            continue;
        }
        default:
            break;
        }

        MOCK_TRACE_DEBUG("[GetNextEvent] type=%s\n", to_string(eventData->type));

        return XR_SUCCESS;
    }
}

bool MockRuntime::IsStateTransitionValid(XrSessionState newState) const
{
    if (newState == XR_SESSION_STATE_LOSS_PENDING)
        return true;

    switch (currentState)
    {
    case XR_SESSION_STATE_IDLE:
        return newState == XR_SESSION_STATE_READY || newState == XR_SESSION_STATE_EXITING;
    case XR_SESSION_STATE_READY:
        return newState == XR_SESSION_STATE_SYNCHRONIZED;
    case XR_SESSION_STATE_SYNCHRONIZED:
        return newState == XR_SESSION_STATE_STOPPING || newState == XR_SESSION_STATE_VISIBLE;
    case XR_SESSION_STATE_VISIBLE:
        return newState == XR_SESSION_STATE_SYNCHRONIZED || newState == XR_SESSION_STATE_FOCUSED;
    case XR_SESSION_STATE_FOCUSED:
        return newState == XR_SESSION_STATE_VISIBLE;
    case XR_SESSION_STATE_STOPPING:
        return newState == XR_SESSION_STATE_IDLE;
    case XR_SESSION_STATE_LOSS_PENDING:
        return newState == XR_SESSION_STATE_LOSS_PENDING;
    case XR_SESSION_STATE_EXITING:
        return newState == XR_SESSION_STATE_IDLE;
    default:
        return false;
    }
}

void MockRuntime::SetExtentsForReferenceSpace(XrReferenceSpaceType referenceSpace, XrExtent2Df extents)
{
    MockReferenceSpace* mockReferenceSpace = GetMockReferenceSpace(referenceSpace);
    if (nullptr == mockReferenceSpace)
        return;

    mockReferenceSpace->extent = extents;
    mockReferenceSpace->validExtent = true;

    // queue a reference space changed pending event
    XrEventDataReferenceSpaceChangePending evt = {};
    evt.type = XR_TYPE_EVENT_DATA_REFERENCE_SPACE_CHANGE_PENDING;
    evt.next = nullptr;
    evt.session = session;
    evt.referenceSpaceType = referenceSpace;
    evt.changeTime = 0;
    evt.poseValid = false;
    evt.poseInPreviousSpace = {{0.0f, 0.0f, 0.0f, 1.0f}, {0.0f, 0.0f, 0.0f}};
    QueueEvent(evt);
}

XrResult MockRuntime::CauseInstanceLoss()
{
    instanceIsLost = true;

    auto killTime = std::chrono::system_clock::now() + std::chrono::seconds(5);

    QueueEvent(XrEventDataInstanceLossPending{
        XR_TYPE_EVENT_DATA_INSTANCE_LOSS_PENDING,
        nullptr,
        killTime.time_since_epoch().count()});

    return XR_SUCCESS;
}

void MockRuntime::SetSpace(XrAction action, XrPosef pose, XrSpaceLocationFlags spaceLocationFlags)
{
    // Set all spaces associated with the action
    for (auto& mockSpace : spaces)
    {
        if (mockSpace.action != action)
            continue;

        mockSpace.pose = pose;
        mockSpace.locationFlags = spaceLocationFlags;
    }
}

void MockRuntime::SetSpace(XrReferenceSpaceType referenceType, XrPosef pose, XrSpaceLocationFlags spaceLocationFlags)
{
    // Set all reference spaces that match the reference space type
    for (auto& mockSpace : spaces)
    {
        if (mockSpace.referenceSpaceType == referenceType)
        {
            mockSpace.pose = pose;
            mockSpace.locationFlags = spaceLocationFlags;
        }
    }
}

XrResult MockRuntime::LocateSpace(XrSpace space, XrSpace baseSpace, XrTime time, XrSpaceLocation* location)
{
    MockSpace* mockSpace = GetMockSpace(space);
    if (nullptr == mockSpace)
        return XR_ERROR_HANDLE_INVALID;

    // TODO: relative to the base space?

    location->pose = mockSpace->pose;
    location->locationFlags = mockSpace->locationFlags;

    if (mockSpace->action != XR_NULL_HANDLE)
    {
        MockAction* mockAction = GetMockAction(mockSpace->action);
        if (nullptr == mockAction)
            return XR_ERROR_HANDLE_INVALID;

        for (auto& binding : mockAction->bindings)
        {
            if (mockSpace->subActionPath != XR_NULL_PATH && GetUserPath(binding->path) != mockSpace->subActionPath)
                continue;

            location->pose = binding->GetLocationPose();

            // Optional velocity driven by conformance automation
            XrSpaceVelocity* spaceVelocity = FindNextPointerType<XrSpaceVelocity>(location, XR_TYPE_SPACE_VELOCITY);
            if (nullptr != spaceVelocity)
            {
                spaceVelocity->velocityFlags = 0;
                spaceVelocity->velocityFlags |= (binding->HasLinearVelocity() ? XR_SPACE_VELOCITY_LINEAR_VALID_BIT : 0);
                spaceVelocity->linearVelocity = binding->GetLinearVelocity();
                spaceVelocity->velocityFlags |= (binding->HasAngularVelocity() ? XR_SPACE_VELOCITY_ANGULAR_VALID_BIT : 0);
                spaceVelocity->angularVelocity = binding->GetAngularVelocity();
            }
            break;
        }
    }

    // Eye gaze extension
    if ((createFlags & MR_CREATE_EYE_GAZE_INTERACTION_EXT) != 0)
    {
        XrEyeGazeSampleTimeEXT* eyeGazeSampleTime =
            FindNextPointerType<XrEyeGazeSampleTimeEXT>(location, XR_TYPE_SYSTEM_EYE_GAZE_INTERACTION_PROPERTIES_EXT);

        if (nullptr != eyeGazeSampleTime)
            eyeGazeSampleTime->time = GetPredictedTime();
    }

    return XR_SUCCESS;
}

void MockRuntime::SetViewStateFlags(XrViewConfigurationType viewConfigurationType, XrViewStateFlags viewStateFlags)
{
    MockViewConfiguration* mockViewConfiguration = GetMockViewConfiguration(viewConfigurationType);
    if (nullptr == mockViewConfiguration)
        return;

    mockViewConfiguration->stateFlags = viewStateFlags;
}

void MockRuntime::SetViewPose(XrViewConfigurationType viewConfigurationType, int viewIndex, XrPosef pose, XrFovf fov)
{
    MockView* mockView = GetMockView(viewConfigurationType, viewIndex);
    if (nullptr == mockView)
        return;

    mockView->pose = pose;
    mockView->fov = fov;
}

XrResult MockRuntime::LocateViews(const XrViewLocateInfo* viewLocateInfo, XrViewState* viewState, uint32_t viewCapacityInput, uint32_t* viewCountOutput, XrView* views)
{
    if (nullptr == viewLocateInfo)
        return XR_ERROR_VALIDATION_FAILURE;

    if (viewCapacityInput == 0)
        return XR_SUCCESS;

    // OpenXR 1.0: The runtime must return error XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED if the given viewConfigurationType is not one of the supported type reported by xrEnumerateViewConfigurations.
    MockViewConfiguration* mockViewConfiguration = GetMockViewConfiguration(viewLocateInfo->viewConfigurationType);

    // Emulate varjo runtime failure where xrBeginSession's primary view configuration influences viewCountOutput.
    if (viewLocateInfo->viewConfigurationType == XR_VIEW_CONFIGURATION_TYPE_PRIMARY_QUAD_VARJO && primaryViewConfiguration != XR_VIEW_CONFIGURATION_TYPE_PRIMARY_QUAD_VARJO)
        mockViewConfiguration = GetMockViewConfiguration(primaryViewConfiguration);

    if (nullptr == mockViewConfiguration)
        return XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED;

    if (!mockViewConfiguration->enabled)
        return XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED;

    *viewCountOutput = (uint32_t)mockViewConfiguration->views.size();

    if (viewCapacityInput == 0)
        return XR_SUCCESS;

    if (viewCapacityInput < (uint32_t)mockViewConfiguration->views.size())
        return XR_ERROR_VALIDATION_FAILURE;

    viewState->viewStateFlags = mockViewConfiguration->stateFlags;

    // If the view is not active then remove the tracked bits
    if (!mockViewConfiguration->active)
        viewState->viewStateFlags &= ~(XR_VIEW_STATE_ORIENTATION_TRACKED_BIT | XR_VIEW_STATE_POSITION_TRACKED_BIT);

    for (uint32_t i = 0; i < mockViewConfiguration->views.size(); i++)
    {
        const MockView& mockView = mockViewConfiguration->views[i];
        views[i].pose = mockView.pose;
        views[i].fov = mockView.fov;
    }

    return XR_SUCCESS;
}

XrResult MockRuntime::GetEndFrameStats(int* primaryLayersRendered, int* secondaryLayersRendered)
{
    *primaryLayersRendered = this->primaryLayersRendered;
    *secondaryLayersRendered = this->secondaryLayersRendered;
    return XR_SUCCESS;
}

void MockRuntime::VisibilityMaskChangedKHR(XrViewConfigurationType viewConfigurationType, uint32_t viewIndex)
{
    XrEventDataVisibilityMaskChangedKHR evt = {};
    evt.type = XR_TYPE_EVENT_DATA_VISIBILITY_MASK_CHANGED_KHR;
    evt.next = nullptr;
    evt.session = session;
    evt.viewConfigurationType = viewConfigurationType;
    evt.viewIndex = viewIndex;
    QueueEvent(evt);
}

XrResult MockRuntime::ValidateName(const char* name) const
{
    const char* nonperiod = name;
    for (const char* c = name; *c; c++)
    {
        if ((*c >= '0' && *c <= '9') || (*c >= 'a' && *c <= 'z') || *c == '-' || *c == '_')
        {
            nonperiod = c;
            continue;
        }
        else if (*c == '.')
            continue;

        return XR_ERROR_PATH_FORMAT_INVALID;
    }

    return XR_SUCCESS;
}

XrResult MockRuntime::ValidatePath(const char* path) const
{
    // Path name strings must start with a single forward slash character.
    if (path[0] != '/')
        return XR_ERROR_PATH_FORMAT_INVALID;

    const char* c = path + 1;
    const char* p = path;
    const char* nonperiod = path;
    const char* slash = path;
    for (; *c; c++, p++)
    {
        // Path name strings must not contain two or more adjacent forward slash characters
        // Path name strings must not contain two forward slash characters that are separated by only period characters.
        if (*c == '/' && nonperiod == slash)
            return XR_ERROR_PATH_FORMAT_INVALID;

        // non-period valid characters
        if ((*c >= '0' && *c <= '9') || (*c >= 'a' && *c <= 'z') || *c == '-' || *c == '_')
        {
            nonperiod = c;
            continue;
        }
        else if (*c == '.')
        {
            continue;
        }
        else if (*c == '/')
        {
            slash = c;
            nonperiod = c;
            continue;
        }

        // Path name strings must be constructed entirely from characters on the following list (a-z|0-9|-|_|.|/)
        return XR_ERROR_PATH_FORMAT_INVALID;
    }

    // Path name strings must not end with a forward slash character or a dot.
    if (*p == '/')
        return XR_ERROR_PATH_FORMAT_INVALID;

    // OpenXR 1.0: Path name strings must not contain only period characters following the final forward slash character in the string.
    if (nonperiod == slash)
        return XR_ERROR_PATH_FORMAT_INVALID;

    // The maximum string length for a path name string, including the terminating \0 character, is defined by
    if (c - p + 1 > XR_MAX_PATH_LENGTH)
        return XR_ERROR_PATH_FORMAT_INVALID;

    return XR_SUCCESS;
}

XrResult MockRuntime::CreateActionSet(const XrActionSetCreateInfo* createInfo, XrActionSet* actionSet)
{
    // OpenXR 1.0: If actionSetName or localizedActionSetName are empty strings, the runtime must return XR_ERROR_NAME_INVALID or XR_ERROR_LOCALIZED_NAME_INVALID
    if (createInfo->actionSetName[0] == 0)
        return XR_ERROR_NAME_INVALID;
    if (createInfo->localizedActionSetName[0] == 0)
        return XR_ERROR_LOCALIZED_NAME_INVALID;

    // OpenXR 1.0: actionSetName must be a null-terminated UTF-8 string whose length is less than or equal to XR_MAX_ACTION_SET_NAME_SIZE
    if (strlen(createInfo->actionSetName) >= XR_MAX_ACTION_SET_NAME_SIZE)
        return XR_ERROR_NAME_INVALID;

    // OpenXR 1.0: localizedActionSetName must be a null-terminated UTF-8 string whose length is less than or equal to XR_MAX_LOCALIZED_ACTION_SET_NAME_SIZE
    if (strlen(createInfo->localizedActionSetName) >= XR_MAX_LOCALIZED_ACTION_SET_NAME_SIZE)
        return XR_ERROR_LOCALIZED_NAME_INVALID;

    // OpenXR 1.0: If actionSetName contains characters which are not allowed in a single level of a well-formed path string, the runtime must return XR_ERROR_PATH_FORMAT_INVALID
    CHECK_SUCCESS(ValidateName(createInfo->actionSetName));

    // OpenXR 1.0: If actionSetName or localizedActionSetName are duplicates of the corresponding field for any existing action set in the specified instance, the runtime must return XR_ERROR_NAME_DUPLICATED or XR_ERROR_LOCALIZED_NAME_DUPLICATED respectively
    for (auto it = actionSets.begin(); it != actionSets.end(); it++)
    {
        auto& existing = *it;
        if (existing.name.compare(createInfo->actionSetName) == 0)
            return XR_ERROR_NAME_DUPLICATED;

        if (existing.localizedName.compare(createInfo->localizedActionSetName) == 0)
            return XR_ERROR_LOCALIZED_NAME_DUPLICATED;
    }

    if (actionSets.size() >= 0xFFFE)
        return XR_ERROR_LIMIT_REACHED;

    // Add a new action set.
    actionSets.emplace_back();
    auto& added = actionSets.back();
    added.actionSet = (XrActionSet)actionSets.size();
    added.isDestroyed = false;
    added.attached = false;
    added.name = createInfo->actionSetName;
    added.localizedName = createInfo->localizedActionSetName;

    // Return the action set
    *actionSet = added.actionSet;

    return XR_SUCCESS;
}

XrResult MockRuntime::DestroyActionSet(XrActionSet actionSet)
{
    // Scan the action sets for a match
    for (auto it = actionSets.begin(); it != actionSets.end(); it++)
    {
        auto& mockActionSet = *it;
        if (mockActionSet.actionSet == actionSet)
        {
            actionSets.erase(it);

            // TODO: The implementation must not free underlying resources for the action set while there are other valid handles that refer to those resources. The implementation may release resources for an action set when all of the action spaces for actions in that action set have been destroyed. See Action Spaces Lifetime for details.
            return XR_SUCCESS;
        }
    }

    return XR_ERROR_HANDLE_INVALID;
}

MockRuntime::MockReferenceSpace* MockRuntime::GetMockReferenceSpace(XrReferenceSpaceType referenceSpaceType)
{
    auto it = referenceSpaces.find(referenceSpaceType);
    if (it == referenceSpaces.end())
        return nullptr;

    return &it->second;
}

MockRuntime::MockViewConfiguration* MockRuntime::GetMockViewConfiguration(XrViewConfigurationType viewConfigType)
{
    auto it = viewConfigurations.find(viewConfigType);
    if (it == viewConfigurations.end())
        return nullptr;

    return &it->second;
}

MockRuntime::MockView* MockRuntime::GetMockView(XrViewConfigurationType viewConfigType, size_t viewIndex)
{
    MockViewConfiguration* mockViewConfiguration = GetMockViewConfiguration(viewConfigType);
    if (nullptr == mockViewConfiguration)
        return nullptr;

    if (viewIndex >= mockViewConfiguration->views.size())
        return nullptr;

    return &mockViewConfiguration->views[viewIndex];
}

MockRuntime::MockActionSet* MockRuntime::GetMockActionSet(XrActionSet actionSet)
{
    size_t actionSetIndex = ((uint64_t)actionSet) - 1;
    if (actionSetIndex >= actionSets.size())
        return nullptr;

    MockActionSet* mockActionSet = &actionSets[actionSetIndex];
    if (mockActionSet->actionSet != actionSet)
        return nullptr;

    return mockActionSet;
}

XrResult MockRuntime::CreateAction(XrActionSet actionSet, const XrActionCreateInfo* createInfo, XrAction* action)
{
    MockActionSet* mockActionSet = GetMockActionSet(actionSet);
    if (nullptr == mockActionSet)
        return XR_ERROR_HANDLE_INVALID;

    // OpenXR 1.0: If actionSet has been included in a call to xrAttachSessionActionSets, the implementation must return XR_ERROR_ACTIONSETS_ALREADY_ATTACHED
    if (mockActionSet->attached)
        return XR_ERROR_ACTIONSETS_ALREADY_ATTACHED;

    // OpenXR 1.0: If actionName or localizedActionName are empty strings, the runtime must return XR_ERROR_NAME_INVALID or XR_ERROR_LOCALIZED_NAME_INVALID respectively.
    if (createInfo->actionName[0] == 0)
        return XR_ERROR_NAME_INVALID;
    if (createInfo->localizedActionName[0] == 0)
        return XR_ERROR_LOCALIZED_NAME_INVALID;

    // OpenXR 1.0: If actionSetName contains characters which are not allowed in a single level of a well-formed path string, the runtime must return XR_ERROR_PATH_FORMAT_INVALID
    CHECK_SUCCESS(ValidateName(createInfo->actionName));

    // OpenXR 1.0f: actionName must be a null-terminated UTF-8 string whose length is less than or equal to XR_MAX_ACTION_NAME_SIZE
    if (strlen(createInfo->actionName) >= XR_MAX_ACTION_NAME_SIZE)
        return XR_ERROR_NAME_INVALID;

    // OpenXR 1.0f: localizedActionName must be a null-terminated UTF-8 string whose length is less than or equal to XR_MAX_LOCALIZED_ACTION_NAME_SIZE
    if (strlen(createInfo->localizedActionName) >= XR_MAX_LOCALIZED_ACTION_NAME_SIZE)
        return XR_ERROR_LOCALIZED_NAME_INVALID;

    // OpenXR 1.0: If actionName or localizedActionName are duplicates of the corresponding field for any existing action in the specified action set, the runtime must return XR_ERROR_NAME_DUPLICATED or XR_ERROR_LOCALIZED_NAME_DUPLICATED respectively
    MockAction* mockAction = nullptr;
    for (auto& existingMockAction : mockActionSet->actions)
    {
        if (existingMockAction.isDestroyed)
        {
            if (existingMockAction.name == createInfo->actionName)
                mockAction = &existingMockAction;
            continue;
        }

        if (existingMockAction.name == createInfo->actionName)
            return XR_ERROR_NAME_DUPLICATED;

        if (!existingMockAction.isDestroyed && existingMockAction.localizedName == createInfo->localizedActionName)
            return XR_ERROR_LOCALIZED_NAME_DUPLICATED;
    }

    // OpenXR 1.0: actionType must be a valid XrActionType value
    switch (createInfo->actionType)
    {
    case XR_ACTION_TYPE_BOOLEAN_INPUT:
    case XR_ACTION_TYPE_FLOAT_INPUT:
    case XR_ACTION_TYPE_VECTOR2F_INPUT:
    case XR_ACTION_TYPE_POSE_INPUT:
    case XR_ACTION_TYPE_VIBRATION_OUTPUT:
        break;

    default:
        return XR_ERROR_VALIDATION_FAILURE;
    }

    if (mockActionSet->actions.size() >= 0xFFFE)
        return XR_ERROR_LIMIT_REACHED;

    if (createInfo->countSubactionPaths > 0 && createInfo->subactionPaths == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    // Create a new action if we arent reusing a destroyed action
    if (nullptr == mockAction)
    {
        mockActionSet->actions.emplace_back();
        mockAction = &mockActionSet->actions.back();
    }

    // Create a new mock action
    mockAction->action = (XrAction)((uint64_t)actionSet + (mockActionSet->actions.size() << 32));
    mockAction->name = createInfo->actionName;
    mockAction->localizedName = createInfo->localizedActionName;
    mockAction->type = createInfo->actionType;
    mockAction->isDestroyed = false;

    for (uint32_t i = 0; i < createInfo->countSubactionPaths; i++)
    {
        auto subactionPath = createInfo->subactionPaths[i];
        if (!IsValidUserPath(subactionPath))
            return XR_ERROR_PATH_UNSUPPORTED;

        // Do not allow duplicate sub action paths
        if (std::find(mockAction->userPaths.begin(), mockAction->userPaths.end(), subactionPath) != mockAction->userPaths.end())
            return XR_ERROR_PATH_UNSUPPORTED;

        mockAction->userPaths.push_back(subactionPath);
    }

    *action = mockAction->action;

    return XR_SUCCESS;
}

XrResult MockRuntime::DestroyAction(XrAction action)
{
    MockAction* mockAction = GetMockAction(action);
    if (nullptr == mockAction)
        return XR_ERROR_HANDLE_INVALID;

    mockAction->isDestroyed = true;
    return XR_SUCCESS;
}

XrResult MockRuntime::RequestExitSession()
{
    if (!IsSessionRunning())
        return XR_ERROR_SESSION_NOT_RUNNING;

    exitSessionRequested = true;

    ChangeSessionStateFrom(XR_SESSION_STATE_FOCUSED, XR_SESSION_STATE_VISIBLE);
    ChangeSessionStateFrom(XR_SESSION_STATE_VISIBLE, XR_SESSION_STATE_SYNCHRONIZED);
    ChangeSessionStateFrom(XR_SESSION_STATE_SYNCHRONIZED, XR_SESSION_STATE_STOPPING);

    return XR_SUCCESS;
}

XrPath MockRuntime::StringToPath(const char* pathString)
{
    XrPath path = XR_NULL_PATH;
    if (XR_SUCCESS != StringToPath(pathString, &path))
        return XR_NULL_PATH;
    return path;
}

XrResult MockRuntime::StringToPath(const char* pathString, XrPath* path)
{
    CHECK_SUCCESS(ValidatePath(pathString));

    // Ensure the string is not too long
    if (strlen(pathString) >= XR_MAX_PATH_LENGTH)
    {
        *path = XR_NULL_PATH;
        return XR_ERROR_PATH_FORMAT_INVALID;
    }

    // If the path contains a user path then separate the user path from the component path
    XrPath result = XR_NULL_PATH;
    for (size_t i = 0; i < userPaths.size(); i++)
    {
        auto& userPathString = userPaths[i].path;
        if (strncmp(pathString, userPathString.c_str(), userPathString.length()) == 0)
        {
            result |= (XrPath)(i + 1);
            pathString += userPathString.length();
            break;
        }
    }

    // Search the component paths
    if (pathString[0] == '/')
    {
        for (size_t i = 0llu; i < componentPathStrings.size(); i++)
        {
            if (strcmp(pathString, componentPathStrings[i].c_str()) == 0)
            {
                result |= (XrPath)((i + 1) << 32);
                *path = result;
                return XR_SUCCESS;
            }
        }

        componentPathStrings.push_back(pathString);
        result |= (XrPath)(componentPathStrings.size() << 32);
    }

    *path = result;
    return XR_SUCCESS;
}

std::string MockRuntime::PathToString(XrPath path) const
{
    size_t userPath = (size_t)(path & 0xFFFFFFFF) - 1;
    size_t componentPath = (size_t)(((uint64_t)path) >> 32) - 1;

    // Both user path and component path are there.
    if (userPath < userPaths.size() && componentPath < componentPathStrings.size())
        return userPaths[userPath].path + componentPathStrings[componentPath];

    if (userPath < userPaths.size())
        return userPaths[userPath].path;

    if (componentPath < componentPathStrings.size())
        return componentPathStrings[componentPath];

    return "";
}

XrResult MockRuntime::PathToString(XrPath path, uint32_t bufferCapacityInput, uint32_t* bufferCountOutput, char* buffer) const
{
    std::string pathString = PathToString(path);
    if (pathString.length() == 0)
    {
        *bufferCountOutput = 0;
        return XR_ERROR_PATH_INVALID;
    }

    if (buffer == nullptr)
    {
        *bufferCountOutput = (uint32_t)(pathString.length() + 1);
        return XR_SUCCESS;
    }

    if (pathString.length() + 1 > bufferCapacityInput)
    {
        *bufferCountOutput = 0;
        return XR_ERROR_SIZE_INSUFFICIENT;
    }

    uint32_t strSize = (uint32_t)pathString.length();
    memcpy(buffer, pathString.c_str(), pathString.length());

    buffer[strSize] = '\0';
    *bufferCountOutput = strSize + 1;

    return XR_SUCCESS;
}

bool MockRuntime::IsValidHandle(XrPath path) const
{
    if (XR_NULL_PATH == path)
        return false;

    size_t userPath = (size_t)(path & 0xFFFFFFFF);
    size_t componentPath = (size_t)(path >> 32);
    return userPath <= userPaths.size() && componentPath <= componentPathStrings.size();
}

XrPath MockRuntime::AppendPath(XrPath path, const char* append)
{
    std::string current = PathToString(path);
    if (current.length() == 0)
        return XR_NULL_PATH;

    return StringToPath((current + append).c_str());
}

XrPath MockRuntime::MakePath(XrPath userPath, XrPath componentPath) const
{
    if (GetUserPath(userPath) != userPath)
        return XR_NULL_PATH;

    if (GetComponentPath(componentPath) != componentPath)
        return XR_NULL_PATH;

    return userPath | componentPath;
}

XrResult MockRuntime::SuggestInteractionProfileBindings(const XrInteractionProfileSuggestedBinding* suggestedBindings)
{
    // OpenXR 1.0: suggestedBindings must be a pointer to a valid XrInteractionProfileSuggestedBinding structure
    if (nullptr == suggestedBindings || suggestedBindings->type != XR_TYPE_INTERACTION_PROFILE_SUGGESTED_BINDING || suggestedBindings->countSuggestedBindings == 0)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0 The countSuggestedBindings parameter must be greater than 0
    if (suggestedBindings->countSuggestedBindings == 0)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: suggestedBindings must be a pointer to an array of countSuggestedBindings valid XrActionSuggestedBinding structures
    if (suggestedBindings->suggestedBindings == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    if (actionSetsAttached)
        return XR_ERROR_ACTIONSETS_ALREADY_ATTACHED;

    const MockInteractionProfile* mockProfile = GetMockInteractionProfile(suggestedBindings->interactionProfile);
    if (nullptr == mockProfile)
        return XR_ERROR_PATH_UNSUPPORTED;

    for (uint32_t i = 0; i < suggestedBindings->countSuggestedBindings; i++)
    {
        auto& suggestedBinding = suggestedBindings->suggestedBindings[i];

        // Ensure the action is valid
        MockAction* mockAction = GetMockAction(suggestedBinding.action);
        if (nullptr == mockAction)
            return XR_ERROR_HANDLE_INVALID;

        // Top level user path must be specified
        XrPath bindingUserPath = GetUserPath(suggestedBinding.binding);
        if (GetUserPath(suggestedBinding.binding) == XR_NULL_PATH)
            return XR_ERROR_PATH_UNSUPPORTED;

        MockUserPath* mockUserPath = GetMockUserPath(bindingUserPath);
        if (nullptr == mockUserPath)
            return XR_ERROR_PATH_UNSUPPORTED;

        if (mockUserPath->profile == nullptr)
            if (!SetActiveInteractionProfile(mockUserPath, mockProfile))
                return XR_ERROR_PATH_UNSUPPORTED;

        // Try to bind directly to a known input source
        MockInputState* inputState = GetMockInputState(*mockProfile, suggestedBinding.binding, mockAction->type);
        if (nullptr == inputState)
        {
            MOCK_TRACE_ERROR("[SuggestInteractionProfileBindings] %s:%s%s: XR_ERROR_PATH_UNSUPPORTED", mockAction->name.c_str(), PathToString(mockProfile->path).c_str(), PathToString(suggestedBinding.binding).c_str());
            return XR_ERROR_PATH_UNSUPPORTED;
        }

        mockAction->bindings.push_back(inputState);
    }

    return XR_SUCCESS;
}

MockInputState* MockRuntime::AddMockInputState(XrPath interactionPath, XrPath path, XrActionType actionType, const char* localizedName)
{
    inputStates.emplace_back();
    MockInputState& mockInputState = inputStates.back();
    mockInputState.interactionProfile = interactionPath;
    mockInputState.path = path;
    mockInputState.type = actionType;
    mockInputState.localizedName = localizedName;
    mockInputState.Reset();
    return &mockInputState;
}

bool MockRuntime::SetActiveInteractionProfile(MockUserPath* mockUserPath, const MockInteractionProfile* mockProfile)
{
    if (nullptr == mockUserPath || nullptr == mockProfile)
        return false;

    mockUserPath->profile = mockProfile;

    QueueEvent(XrEventDataInteractionProfileChanged{XR_TYPE_EVENT_DATA_INTERACTION_PROFILE_CHANGED, nullptr, session});

    return true;
}

XrResult MockRuntime::AttachSessionActionSets(const XrSessionActionSetsAttachInfo* attachInfo)
{
    // OpenXR 1.0: attachInfo must be a pointer to a valid XrSessionActionSetsAttachInfo structure
    if (attachInfo == nullptr || attachInfo->type != XR_TYPE_SESSION_ACTION_SETS_ATTACH_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: actionSets must be a pointer to an array of countActionSets valid XrActionSet handles
    if (attachInfo->actionSets == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: The countActionSets parameter must be greater than 0
    if (attachInfo->countActionSets == 0)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: The runtime must return XR_ERROR_ACTIONSETS_ALREADY_ATTACHED if xrAttachSessionActionSets is called more than once for a given session
    if (actionSetsAttached)
        return XR_ERROR_ACTIONSETS_ALREADY_ATTACHED;

    for (uint32_t i = 0; i < attachInfo->countActionSets; i++)
    {
        auto actionSet = GetMockActionSet(attachInfo->actionSets[i]);

        // OpenXR 1.0: actionSets must be a pointer to an array of countActionSets valid XrActionSet handles
        if (nullptr == actionSet)
            return XR_ERROR_HANDLE_INVALID;

        actionSet->attached = true;
    }

    actionSetsAttached = true;

#if 0
    // Print all bindings
    for (auto& as : actionSets)
        for (auto& a : as.actions)
        {
            int bindingIndex = 0;
            for (auto& b : a.bindings)
                MOCK_TRACE_LOG("[Binding] %s.%s(%d) -> %s%s", as.name.c_str(), a.name.c_str(), bindingIndex++, PathToString(b->interactionProfile).c_str(), PathToString(b->path).c_str());
        }
#endif

    return XR_SUCCESS;
}

XrResult MockRuntime::GetCurrentInteractionProfile(XrPath topLevelUserPath, XrInteractionProfileState* interactionProfile)
{
    // OpenXR 1.0: If xrAttachSessionActionSets has not yet been called for the session, the runtime must return XR_ERROR_ACTIONSET_NOT_ATTACHED
    if (!actionSetsAttached)
        return XR_ERROR_ACTIONSET_NOT_ATTACHED;

    // OpenXR 1.0: If topLevelUserPath is not one of the device input subpaths described in section /user paths, the runtime must return XR_ERROR_PATH_UNSUPPORTED.
    MockUserPath* mockUserPath = GetMockUserPath(topLevelUserPath);
    if (nullptr == mockUserPath)
        return XR_ERROR_PATH_UNSUPPORTED;

    interactionProfile->interactionProfile = mockUserPath->profile != nullptr ? mockUserPath->profile->path : XR_NULL_PATH;

    return XR_SUCCESS;
}

MockRuntime::MockUserPath* MockRuntime::GetMockUserPath(XrPath path)
{
    XrPath userPath = GetUserPath(path);
    if (userPath == XR_NULL_PATH)
        return nullptr;

    return &userPaths[userPath - 1];
}

MockRuntime::MockActionSet* MockRuntime::GetMockActionSet(XrAction action)
{
    size_t actionSetIndex = (size_t)((uint64_t)action & 0xFFFF) - 1;
    if (actionSetIndex >= actionSets.size())
        return nullptr;

    return &actionSets[actionSetIndex];
}

MockRuntime::MockAction* MockRuntime::GetMockAction(XrAction action)
{
    MockActionSet* mockActionSet = GetMockActionSet(action);
    if (nullptr == mockActionSet)
        return nullptr;

    size_t actionIndex = (size_t)(((uint64_t)action) >> 32) - 1;
    if (actionIndex >= mockActionSet->actions.size())
        return nullptr;

    MockAction* mockAction = &mockActionSet->actions[actionIndex];
    if (mockAction->action != action)
        return nullptr;

    return mockAction;
}

MockInputState* MockRuntime::GetMockInputState(const MockInteractionProfile& mockProfile, XrPath path, XrActionType actionType)
{
    for (auto& mockInputState : inputStates)
    {
        if (mockInputState.interactionProfile == mockProfile.path && mockInputState.path == path)
            return &mockInputState;
    }

    // Nothing was found, could be a parent path, lets check
    switch (actionType)
    {
    case XR_ACTION_TYPE_BOOLEAN_INPUT:
    {
        MockInputState* mockInputState = GetMockInputState(mockProfile, AppendPath(path, "/value"));
        if (nullptr != mockInputState)
            return mockInputState;

        return GetMockInputState(mockProfile, AppendPath(path, "/click"));
    }

    case XR_ACTION_TYPE_FLOAT_INPUT:
    {
        return GetMockInputState(mockProfile, AppendPath(path, "/value"));
    }

    default:
        break;
    }

    return nullptr;
}

XrResult MockRuntime::SyncActions(const XrActionsSyncInfo* syncInfo)
{
    // OpenXR 1.0: syncInfo must be a pointer to a valid XrActionsSyncInfo structure
    if (syncInfo == nullptr || syncInfo->type != XR_TYPE_ACTIONS_SYNC_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: If countActiveActionSets is not 0, activeActionSets must be a pointer to an array of countActiveActionSets valid XrActiveActionSet structures
    if (syncInfo->countActiveActionSets > 0 && syncInfo->activeActionSets == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    for (size_t i = 0; i < syncInfo->countActiveActionSets; i++)
    {
        MockActionSet* mockActionSet = GetMockActionSet(syncInfo->activeActionSets[i].actionSet);
        if (nullptr == mockActionSet)
            return XR_ERROR_HANDLE_INVALID;

        // OpenXR 1.0: If any action sets not attached to this session are passed to xrSyncActions it must return XR_ERROR_ACTIONSET_NOT_ATTACHED
        if (!mockActionSet->attached)
            return XR_ERROR_ACTIONSET_NOT_ATTACHED;

        // Update the all input sources for this action set from conformance automation if eneabled
        if (IsConformanceAutomationEnabled())
        {
            for (auto& mockAction : mockActionSet->actions)
            {
                for (auto& binding : mockAction.bindings)
                {
                    // If a specific sub action path is given then ignore bindings that dont match that path
                    if (syncInfo->activeActionSets[i].subactionPath != XR_NULL_PATH && syncInfo->activeActionSets[i].subactionPath != GetUserPath(binding->path))
                        continue;

                    ConformanceAutomation_GetInputState(binding);
                }
            }
        }
    }

    // OpenXR 1.0: If session is not focused, the runtime must return XR_SESSION_NOT_FOCUSED, and all action states in the session must be inactive
    if (currentState != XR_SESSION_STATE_FOCUSED)
        return XR_SESSION_NOT_FOCUSED;

    return XR_SUCCESS;
}

bool MockRuntime::IsActionAttached(XrAction action)
{
    if (!actionSetsAttached)
        return false;

    MockActionSet* mockActionSet = GetMockActionSet(action);
    if (nullptr == mockActionSet)
        return false;

    return mockActionSet->attached;
}

XrResult MockRuntime::GetActionStateFloat(const XrActionStateGetInfo* getInfo, XrActionStateFloat* state)
{
    // OpenXR 1.0: getInfo must be a pointer to a valid XrActionStateGetInfo structure
    if (nullptr == getInfo || getInfo->type != XR_TYPE_ACTION_STATE_GET_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: state must be a pointer to an XrActionStateFloat structure
    if (nullptr == state || state->type != XR_TYPE_ACTION_STATE_FLOAT)
        return XR_ERROR_VALIDATION_FAILURE;

    MockAction* mockAction = GetMockAction(getInfo->action);
    if (nullptr == mockAction)
        return XR_ERROR_HANDLE_INVALID;

    if (!IsActionAttached(mockAction->action))
        return XR_ERROR_ACTIONSET_NOT_ATTACHED;

    float value = 0.0f;

    for (auto& binding : mockAction->bindings)
    {
        // If a subpath is given the binding subpath must match
        if (getInfo->subactionPath != XR_NULL_PATH && GetUserPath(binding->path) != getInfo->subactionPath)
            continue;

        // Must match the action type
        if (!binding->IsCompatibleType(XR_ACTION_TYPE_FLOAT_INPUT))
            return XR_ERROR_ACTION_TYPE_MISMATCH;

        // OpenXR 1.0: The current state must be the state of the input with the largest absolute value
        float bindingValue = binding->GetFloat();
        if (std::abs(bindingValue) > std::abs(value))
            value = bindingValue;
    }

    state->currentState = value;
    state->lastChangeTime = GetPredictedTime();
    return XR_SUCCESS;
}

XrResult MockRuntime::GetActionStateBoolean(const XrActionStateGetInfo* getInfo, XrActionStateBoolean* state)
{
    if (nullptr == getInfo || getInfo->type != XR_TYPE_ACTION_STATE_GET_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    MockAction* mockAction = GetMockAction(getInfo->action);
    if (nullptr == mockAction)
        return XR_ERROR_HANDLE_INVALID;

    if (!IsActionAttached(mockAction->action))
        return XR_ERROR_ACTIONSET_NOT_ATTACHED;

    bool value = false;

    for (auto& binding : mockAction->bindings)
    {
        if (getInfo->subactionPath == XR_NULL_PATH || GetUserPath(binding->path) == getInfo->subactionPath)
        {
            if (!binding->IsCompatibleType(XR_ACTION_TYPE_BOOLEAN_INPUT))
                return XR_ERROR_ACTION_TYPE_MISMATCH;

            value |= (bool)binding->GetBoolean();
        }
    }

    state->currentState = (XrBool32)value;
    state->lastChangeTime = GetPredictedTime();
    return XR_SUCCESS;
}

XrResult MockRuntime::GetActionStateVector2f(const XrActionStateGetInfo* getInfo, XrActionStateVector2f* state)
{
    if (nullptr == getInfo || getInfo->type != XR_TYPE_ACTION_STATE_GET_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    MockAction* mockAction = GetMockAction(getInfo->action);
    if (nullptr == mockAction)
        return XR_ERROR_HANDLE_INVALID;

    if (!IsActionAttached(mockAction->action))
        return XR_ERROR_ACTIONSET_NOT_ATTACHED;

    XrVector2f value = {};
    float valueLength = 0.0f;

    for (auto& binding : mockAction->bindings)
    {
        if (getInfo->subactionPath == XR_NULL_PATH || GetUserPath(binding->path) == getInfo->subactionPath)
        {
            if (!binding->IsCompatibleType(XR_ACTION_TYPE_VECTOR2F_INPUT))
                return XR_ERROR_ACTION_TYPE_MISMATCH;

            XrVector2f bindingValue = binding->GetVector2();
            float bindingValueLength = bindingValue.x * bindingValue.x + bindingValue.y * bindingValue.y;
            if (bindingValueLength > valueLength)
            {
                valueLength = bindingValueLength;
                value = bindingValue;
            }
        }
    }

    state->currentState = value;
    state->lastChangeTime = GetPredictedTime();
    return XR_SUCCESS;
}

XrResult MockRuntime::GetActionStatePose(const XrActionStateGetInfo* getInfo, XrActionStatePose* state)
{
    if (nullptr == getInfo || getInfo->type != XR_TYPE_ACTION_STATE_GET_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    MockAction* mockAction = GetMockAction(getInfo->action);
    if (nullptr == mockAction)
        return XR_ERROR_HANDLE_INVALID;

    if (!IsActionAttached(mockAction->action))
        return XR_ERROR_ACTIONSET_NOT_ATTACHED;

    state->isActive = !mockAction->bindings.empty();

    // Conformance automation can disable a user path so if enabled
    // make sure any given subpaths are checked for being deactivated.
    if (IsConformanceAutomationEnabled() && getInfo->subactionPath != XR_NULL_PATH)
    {
        MockUserPath* mockUserPath = GetMockUserPath(getInfo->subactionPath);
        if (nullptr != mockUserPath)
            state->isActive = ConformanceAutomation_IsActive(XR_NULL_PATH, getInfo->subactionPath, state->isActive);
    }

    return XR_SUCCESS;
}

XrResult MockRuntime::CreateReferenceSpace(const XrReferenceSpaceCreateInfo* createInfo, XrSpace* space)
{
    // OpenXR 1.0: type must be XR_TYPE_REFERENCE_SPACE_CREATE_INFO
    if (createInfo == nullptr || createInfo->type != XR_TYPE_REFERENCE_SPACE_CREATE_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    switch (createInfo->referenceSpaceType)
    {
    case XR_REFERENCE_SPACE_TYPE_LOCAL:
    case XR_REFERENCE_SPACE_TYPE_STAGE:
    case XR_REFERENCE_SPACE_TYPE_VIEW:
    case XR_REFERENCE_SPACE_TYPE_UNBOUNDED_MSFT:
        break;

    default:
        return XR_ERROR_REFERENCE_SPACE_UNSUPPORTED;
    }

    // Create a mock reference space if needed
    MockReferenceSpace* mockReferenceSpace = GetMockReferenceSpace(createInfo->referenceSpaceType);
    if (nullptr == mockReferenceSpace)
    {
        mockReferenceSpace = &referenceSpaces[createInfo->referenceSpaceType];
        mockReferenceSpace->validExtent = false;
        mockReferenceSpace->extent = {0, 0};
    }

    // Add the sapce and create the handle
    spaces.emplace_back();
    MockSpace& mockSpace = spaces.back();
    mockSpace.pose = createInfo->poseInReferenceSpace;
    mockSpace.locationFlags =
        XR_SPACE_LOCATION_ORIENTATION_VALID_BIT |
        XR_SPACE_LOCATION_POSITION_VALID_BIT |
        XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT |
        XR_SPACE_LOCATION_POSITION_TRACKED_BIT;
    mockSpace.isDestroyed = false;
    mockSpace.action = XR_NULL_HANDLE;
    mockSpace.referenceSpaceType = createInfo->referenceSpaceType;
    mockSpace.subActionPath = XR_NULL_PATH;

    *space = (XrSpace)spaces.size();

    return XR_SUCCESS;
}

XrResult MockRuntime::CreateActionSpace(const XrActionSpaceCreateInfo* createInfo, XrSpace* space)
{
    // OpenXR 1.0: type must be XR_TYPE_ACTION_SPACE_CREATE_INFO
    if (createInfo == nullptr || createInfo->type != XR_TYPE_ACTION_SPACE_CREATE_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: space must be a pointer to an XrSpace handle
    if (nullptr == space)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: action must be a valid XrAction handle
    MockAction* mockAction = GetMockAction(createInfo->action);
    if (nullptr == mockAction)
        return XR_ERROR_HANDLE_INVALID;

    // OpenXR 1.0: The runtime must return XR_ERROR_ACTION_TYPE_MISMATCH if the action provided in action is not of type XR_ACTION_TYPE_POSE_INPUT
    if (mockAction->type != XR_ACTION_TYPE_POSE_INPUT)
        return XR_ERROR_ACTION_TYPE_MISMATCH;

    // OpenXR 1.0: If subactionPath is a valid path not specified when the action was created the runtime must return XR_ERROR_PATH_UNSUPPORTED
    if (createInfo->subactionPath != XR_NULL_PATH)
    {
        if (std::find(mockAction->userPaths.begin(), mockAction->userPaths.end(), createInfo->subactionPath) == mockAction->userPaths.end())
            return XR_ERROR_PATH_UNSUPPORTED;
    }

    // Add the space and create the handle
    spaces.emplace_back();
    MockSpace& mockSpace = spaces.back();
    mockSpace.pose = createInfo->poseInActionSpace;
    mockSpace.isDestroyed = false;
    mockSpace.action = mockAction->action;
    mockSpace.subActionPath = createInfo->subactionPath;
    mockSpace.referenceSpaceType = XR_REFERENCE_SPACE_TYPE_MAX_ENUM;
    mockSpace.locationFlags =
        XR_SPACE_LOCATION_ORIENTATION_VALID_BIT |
        XR_SPACE_LOCATION_POSITION_VALID_BIT |
        XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT |
        XR_SPACE_LOCATION_POSITION_TRACKED_BIT;

    *space = (XrSpace)spaces.size();

    return XR_SUCCESS;
}

XrResult MockRuntime::GetInputSourceLocalizedName(const XrInputSourceLocalizedNameGetInfo* getInfo, uint32_t bufferCapacityInput, uint32_t* bufferCountOutput, char* buffer)
{
    if (nullptr == getInfo || getInfo->type != XR_TYPE_INPUT_SOURCE_LOCALIZED_NAME_GET_INFO || getInfo->whichComponents == 0 || nullptr == bufferCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    if (bufferCapacityInput > 0 && buffer == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    if (!actionSetsAttached)
        return XR_ERROR_ACTIONSET_NOT_ATTACHED;

    std::string localizedName;

    // Interaction profile
    MockUserPath* mockUserPath = GetMockUserPath(getInfo->sourcePath);
    if (nullptr == mockUserPath)
        return XR_ERROR_PATH_UNSUPPORTED;

    if (mockUserPath->profile != nullptr && (getInfo->whichComponents & XR_INPUT_SOURCE_LOCALIZED_NAME_INTERACTION_PROFILE_BIT) == XR_INPUT_SOURCE_LOCALIZED_NAME_INTERACTION_PROFILE_BIT)
    {
        localizedName = mockUserPath->profile->localizedName;
    }

    // Top level user path
    if ((getInfo->whichComponents & XR_INPUT_SOURCE_LOCALIZED_NAME_USER_PATH_BIT) == XR_INPUT_SOURCE_LOCALIZED_NAME_USER_PATH_BIT)
    {
        if (localizedName.size() != 0)
            localizedName += " ";

        localizedName += mockUserPath->localizedName;
    }

    // Component
    XrPath componentPath = GetComponentPath(getInfo->sourcePath);
    if (mockUserPath->profile != nullptr && componentPath != XR_NULL_PATH && (getInfo->whichComponents & XR_INPUT_SOURCE_LOCALIZED_NAME_COMPONENT_BIT) == XR_INPUT_SOURCE_LOCALIZED_NAME_COMPONENT_BIT)
    {
        MockInputState* mockInputState = GetMockInputState(*mockUserPath->profile, getInfo->sourcePath);
        if (mockInputState != nullptr && mockInputState->localizedName != nullptr)
        {
            if (localizedName.size() != 0)
                localizedName += " ";

            localizedName += mockInputState->localizedName;
        }
    }

    *bufferCountOutput = (uint32_t)(localizedName.size() + 1);

    if (bufferCapacityInput == 0)
        return XR_SUCCESS;

    strcpy(buffer, localizedName.c_str());

    return XR_SUCCESS;
}

XrResult MockRuntime::EnumerateBoundSourcesForAction(const XrBoundSourcesForActionEnumerateInfo* enumerateInfo, uint32_t sourceCapacityInput, uint32_t* sourceCountOutput, XrPath* sources)
{
    // OpenXR 1.0: enumerateInfo must be a pointer to a valid XrBoundSourcesForActionEnumerateInfo structure
    if (enumerateInfo == nullptr || enumerateInfo->type != XR_TYPE_BOUND_SOURCES_FOR_ACTION_ENUMERATE_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: If sourceCapacityInput is not 0, sources must be a pointer to an array of sourceCapacityInput XrPath values
    if (sourceCapacityInput > 0 && sources == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: sourceCountOutput must be a pointer to a uint32_t value
    if (sourceCountOutput == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: action must be a valid XrAction handle
    MockAction* mockAction = GetMockAction(enumerateInfo->action);
    if (nullptr == mockAction)
        return XR_ERROR_HANDLE_INVALID;

    // OpenXR 1.0: must return XR_ERROR_ACTIONSET_NOT_ATTACHED if passed an action in an action set never attached to the session with xrAttachSessionActionSets.
    if (!actionSetsAttached || !GetMockActionSet(mockAction->action)->attached)
        return XR_ERROR_ACTIONSET_NOT_ATTACHED;

    *sourceCountOutput = 0;
    for (auto& mockInputState : mockAction->bindings)
    {
        if (sources != nullptr && *sourceCountOutput >= sourceCapacityInput)
            return XR_ERROR_SIZE_INSUFFICIENT;

        (*sourceCountOutput)++;

        if (sources != nullptr)
            *(sources++) = mockInputState->path;
    }

    return XR_SUCCESS;
}

XrResult MockRuntime::ApplyHapticFeedback(const XrHapticActionInfo* hapticActionInfo, const XrHapticBaseHeader* hapticFeedback)
{
    // OpenXR 1.0: hapticActionInfo must be a pointer to a valid XrHapticActionInfo structure
    if (nullptr == hapticActionInfo || hapticActionInfo->type != XR_TYPE_HAPTIC_ACTION_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: hapticFeedback must be a pointer to a valid XrHapticBaseHeader-based structure
    if (hapticFeedback == nullptr || hapticFeedback->type != XR_TYPE_HAPTIC_VIBRATION)
        return XR_ERROR_VALIDATION_FAILURE;

    if (!IsActionAttached(hapticActionInfo->action))
        return XR_ERROR_ACTIONSET_NOT_ATTACHED;

    MockAction* mockAction = GetMockAction(hapticActionInfo->action);
    if (nullptr == mockAction)
        return XR_ERROR_HANDLE_INVALID;

    QueueEvent(XrEventScriptEventMOCK{XR_TYPE_EVENT_SCRIPT_EVENT_MOCK, nullptr, XR_MOCK_SCRIPT_EVENT_HAPTIC_IMPULSE, (uint64_t)hapticActionInfo->action});

    return XR_SUCCESS;
}

XrResult MockRuntime::StopHapticFeedback(const XrHapticActionInfo* hapticActionInfo)
{
    // OpenXR 1.0: hapticActionInfo must be a pointer to a valid XrHapticActionInfo structure
    if (nullptr == hapticActionInfo || hapticActionInfo->type != XR_TYPE_HAPTIC_ACTION_INFO)
        return XR_ERROR_VALIDATION_FAILURE;

    if (!IsActionAttached(hapticActionInfo->action))
        return XR_ERROR_ACTIONSET_NOT_ATTACHED;

    MockAction* mockAction = GetMockAction(hapticActionInfo->action);
    if (nullptr == mockAction)
        return XR_ERROR_HANDLE_INVALID;

    QueueEvent(XrEventScriptEventMOCK{XR_TYPE_EVENT_SCRIPT_EVENT_MOCK, nullptr, XR_MOCK_SCRIPT_EVENT_HAPTIC_STOP, (uint64_t)hapticActionInfo->action});

    return XR_SUCCESS;
}

MockRuntime::MockSpace* MockRuntime::GetMockSpace(XrSpace space)
{
    if (space == 0 || ((size_t)space) > spaces.size())
        return nullptr;

    return &spaces[((size_t)space) - 1];
}

XrResult MockRuntime::EnumerateViewConfigurations(XrSystemId systemId, uint32_t viewConfigurationTypeCapacityInput, uint32_t* viewConfigurationTypeCountOutput, XrViewConfigurationType* viewConfigurationTypes)
{
    if (viewConfigurationTypeCountOutput == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    *viewConfigurationTypeCountOutput = (uint32_t)viewConfigurations.size();

    if (viewConfigurationTypeCapacityInput == 0)
        return XR_SUCCESS;

    if (viewConfigurationTypes == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    if (viewConfigurationTypeCapacityInput < viewConfigurations.size())
        return XR_ERROR_SIZE_INSUFFICIENT;

    int idx = 0;
    for (auto view : viewConfigurations)
    {
        viewConfigurationTypes[idx++] = view.first;
    }

    return XR_SUCCESS;
}

XrResult MockRuntime::EnumerateViewConfigurationViews(XrSystemId systemId, XrViewConfigurationType viewConfigurationType, uint32_t viewCapacityInput, uint32_t* viewCountOutput, XrViewConfigurationView* viewsOutput)
{
    if (nullptr == viewCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    // OpenXR 1.0: The runtime must return error XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED if the given viewConfigurationType is not one of the supported type reported by xrEnumerateViewConfigurations.
    MockViewConfiguration* mockViewConfig = GetMockViewConfiguration(viewConfigurationType);
    if (nullptr == mockViewConfig)
        return XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED;

    *viewCountOutput = (uint32_t)mockViewConfig->views.size();

    // Just requesting the number of views
    if (viewCapacityInput == 0)
        return XR_SUCCESS;

    if (viewsOutput == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    for (size_t i = 0; i < mockViewConfig->views.size(); i++)
    {
        viewsOutput[i] = mockViewConfig->views[i].configuration;
    }

    return XR_SUCCESS;
}

XrResult MockRuntime::EnumerateEnvironmentBlendModes(XrSystemId systemId, XrViewConfigurationType viewConfigurationType, uint32_t environmentBlendModeCapacityInput, uint32_t* environmentBlendModeCountOutput, XrEnvironmentBlendMode* environmentBlendModes)
{
    MockViewConfiguration* mockViewConfig = GetMockViewConfiguration(viewConfigurationType);
    if (nullptr == mockViewConfig)
        return XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED;

    *environmentBlendModeCountOutput = 2;

    if (environmentBlendModeCapacityInput == 0)
        return XR_SUCCESS;

    if (environmentBlendModeCapacityInput < *environmentBlendModeCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;
    //Mock runtime supporting multiple environment blend modes.
    environmentBlendModes[0] = XR_ENVIRONMENT_BLEND_MODE_OPAQUE;
    environmentBlendModes[1] = XR_ENVIRONMENT_BLEND_MODE_ADDITIVE;
    return XR_SUCCESS;
}

XrResult MockRuntime::GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function)
{
#if defined(XR_USE_GRAPHICS_API_VULKAN)
    if (IsVulkanGfx() && XR_SUCCESS == MockVulkan_GetInstanceProcAddr(name, function))
        return XR_SUCCESS;
#endif

#if defined(XR_USE_GRAPHICS_API_D3D11)
    if (IsD3D11Gfx() && XR_SUCCESS == MockD3D11_GetInstanceProcAddr(name, function))
        return XR_SUCCESS;
#endif

#if defined(_WIN32)
    if (XR_SUCCESS == MockWin32ConvertPerformanceCounterTime_GetInstanceProcAddr(name, function))
        return XR_SUCCESS;
#endif

    if (IsConformanceAutomationEnabled() && XR_SUCCESS == ConformanceAutomation_GetInstanceProcAddr(name, function))
        return XR_SUCCESS;

    return XR_ERROR_FUNCTION_UNSUPPORTED;
}

XrResult MockRuntime::RegisterScriptEventCallback(PFN_ScriptEventCallback callback)
{
    scriptEventCallback = callback;
    return XR_SUCCESS;
}

XrResult MockRuntime::GetSystemProperties(XrSystemId systemId, XrSystemProperties* properties)
{
    properties->vendorId = 0xFEFE;
    properties->systemId = (XrSystemId)2;
    properties->graphicsProperties.maxLayerCount = XR_MIN_COMPOSITION_LAYERS_SUPPORTED;

    // Eye gaze extension
    if ((createFlags & MR_CREATE_EYE_GAZE_INTERACTION_EXT) != 0)
    {
        XrSystemEyeGazeInteractionPropertiesEXT* secondaryViewConfiguration =
            FindNextPointerType<XrSystemEyeGazeInteractionPropertiesEXT>(properties, XR_TYPE_SYSTEM_EYE_GAZE_INTERACTION_PROPERTIES_EXT);

        if (nullptr != secondaryViewConfiguration)
            secondaryViewConfiguration->supportsEyeGazeInteraction = true;
    }

    return XR_SUCCESS;
}
