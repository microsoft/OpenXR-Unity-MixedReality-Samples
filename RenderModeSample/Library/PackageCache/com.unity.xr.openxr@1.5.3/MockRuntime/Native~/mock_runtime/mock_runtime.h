#pragma once

typedef XrFlags64 MockRuntimeCreateFlags;
static const MockRuntimeCreateFlags MR_CREATE_DRIVER_EXT = 0x00000001;
static const MockRuntimeCreateFlags MR_CREATE_NULL_GFX_EXT = 0x00000002;
static const MockRuntimeCreateFlags MR_CREATE_CONFORMANCE_AUTOMATION_EXT = 0x00000004;
static const MockRuntimeCreateFlags MR_CREATE_COMPOSITION_LAYER_DEPTH_EXT = 0x00000008;
static const MockRuntimeCreateFlags MR_CREATE_VULKAN_GFX_EXT = 0x00000010;
static const MockRuntimeCreateFlags MR_CREATE_D3D11_GFX_EXT = 0x00000020;
static const MockRuntimeCreateFlags MR_CREATE_VARJO_QUAD_VIEWS_EXT = 0x00000040;
static const MockRuntimeCreateFlags MR_CREATE_MSFT_SECONDARY_VIEW_CONFIGURATION_EXT = 0x00000080;
static const MockRuntimeCreateFlags MR_CREATE_MSFT_FIRST_PERSON_OBSERVER_EXT = 0x00000100;
static const MockRuntimeCreateFlags MR_CREATE_EYE_GAZE_INTERACTION_EXT = 0x00000200;
static const MockRuntimeCreateFlags MR_CREATE_MSFT_HAND_INTERACTION_EXT = 0x00000400;
static const MockRuntimeCreateFlags MR_CREATE_MSFT_THIRD_PERSON_OBSERVER_EXT = 0x00000800;

static const MockRuntimeCreateFlags MR_CREATE_ALL_GFX_EXT = MR_CREATE_VULKAN_GFX_EXT | MR_CREATE_NULL_GFX_EXT | MR_CREATE_D3D11_GFX_EXT;

class MockRuntime
{
public:
    MockRuntime(XrInstance instance, MockRuntimeCreateFlags flags);

    ~MockRuntime();

    XrInstance GetInstance() const
    {
        return instance;
    }

    XrSession GetSession() const
    {
        return session;
    }

    XrSessionState GetSessionState() const
    {
        return currentState;
    }

    bool HasValidSession() const
    {
        return session != XR_NULL_HANDLE;
    }

    bool HasValidInstance() const
    {
        return instance != XR_NULL_HANDLE;
    }

    bool IsSessionState(XrSessionState state) const
    {
        return currentState == state;
    }

    bool IsConformanceAutomationEnabled() const
    {
        return (createFlags & MR_CREATE_CONFORMANCE_AUTOMATION_EXT) != 0;
    }

    XrResult GetNextEvent(XrEventDataBuffer* eventData);

    XrResult CreateSession(const XrSessionCreateInfo* createInfo);
    XrResult BeginSession(const XrSessionBeginInfo* beginInfo);
    XrResult EndSession();
    XrResult DestroySession();

    void SetMockSession(XrInstance instance, XrSession* session);
    void SetSupportingDriverExtension(XrInstance instance, bool usingExtension);

    bool IsStateTransitionValid(XrSessionState newState) const;
    bool ChangeSessionStateFrom(XrSessionState fromState, XrSessionState toState);
    void ChangeSessionState(XrSessionState state);

    bool IsSessionRunning() const
    {
        return isRunning;
    }

    XrResult RequestExitSession();
    bool HasExitBeenRequested() const
    {
        return exitSessionRequested;
    }

    void SetExtentsForReferenceSpace(XrReferenceSpaceType referenceSpace, XrExtent2Df extents);
    XrResult GetReferenceSpaceBoundsRect(XrReferenceSpaceType referenceSpace, XrExtent2Df* extents);

    XrResult CauseInstanceLoss();
    bool IsInstanceLost(XrInstance instance) const
    {
        return instanceIsLost;
    };

    void SetNullGfx(bool nullGfx);
    bool IsNullGfx() const
    {
        return (createFlags & MR_CREATE_NULL_GFX_EXT) != 0;
    }

    bool IsVulkanGfx() const
    {
        return (createFlags & MR_CREATE_VULKAN_GFX_EXT) != 0;
    }

    bool IsD3D11Gfx() const
    {
        return (createFlags & MR_CREATE_D3D11_GFX_EXT) != 0;
    }

    XrResult WaitFrame(const XrFrameWaitInfo* frameWaitInfo, XrFrameState* frameState);

    XrResult LocateSpace(XrSpace space, XrSpace baseSpace, XrTime time, XrSpaceLocation* location);

    void SetSpace(XrReferenceSpaceType referenceSpaceType, XrPosef pose, XrSpaceLocationFlags spaceLocationFlags);
    void SetSpace(XrAction action, XrPosef pose, XrSpaceLocationFlags spaceLocationFlags);
    void SetViewPose(XrViewConfigurationType viewConfigurationType, int viewIndex, XrPosef pose, XrFovf fov);
    void SetViewStateFlags(XrViewConfigurationType viewConfigurationType, XrViewStateFlags viewStateFlags);

    XrResult GetEndFrameStats(int* primaryLayerCount, int* secondaryLayerCount);
    XrResult EndFrame(const XrFrameEndInfo* frameEndInfo);

    XrResult CreateActionSet(const XrActionSetCreateInfo* createInfo, XrActionSet* actionSet);
    XrResult DestroyActionSet(XrActionSet actionSet);
    XrResult CreateAction(XrActionSet actionSet, const XrActionCreateInfo* createInfo, XrAction* action);
    XrResult DestroyAction(XrAction action);
    XrResult SyncActions(const XrActionsSyncInfo* syncInfo);

    XrResult GetActionStateFloat(const XrActionStateGetInfo* getInfo, XrActionStateFloat* state);
    XrResult GetActionStateBoolean(const XrActionStateGetInfo* getInfo, XrActionStateBoolean* state);
    XrResult GetActionStateVector2f(const XrActionStateGetInfo* getInfo, XrActionStateVector2f* state);
    XrResult GetActionStatePose(const XrActionStateGetInfo* getInfo, XrActionStatePose* state);

    XrResult CreateReferenceSpace(const XrReferenceSpaceCreateInfo* createInfo, XrSpace* space);
    XrResult CreateActionSpace(const XrActionSpaceCreateInfo* createInfo, XrSpace* space);

    void VisibilityMaskChangedKHR(XrViewConfigurationType viewConfigurationType, uint32_t viewIndex);

    XrResult StringToPath(const char* pathString, XrPath* path);
    XrPath StringToPath(const char* pathString);

    std::string PathToString(XrPath) const;
    XrResult PathToString(XrPath path, uint32_t bufferCapacityInput, uint32_t* bufferCountOutput, char* buffer) const;

    // Return the user path portion of the given path handle
    XrPath GetUserPath(XrPath path) const
    {
        return path & 0x00000000FFFFFFFFull;
    }

    // Return the component path portion of the given path handle
    XrPath GetComponentPath(XrPath path) const
    {
        return path & 0xFFFFFFFF00000000ull;
    }

    // Make a new path using the given user path and component path
    XrPath MakePath(XrPath userPath, XrPath componentPath) const;

    // Append the given path
    XrPath AppendPath(XrPath path, const char* append);

    // Returns true if the given path is a valid path handle
    bool IsValidHandle(XrPath path) const;

    XrResult GetCurrentInteractionProfile(XrPath topLevelUserPath, XrInteractionProfileState* interactionProfile);

    XrResult AttachSessionActionSets(const XrSessionActionSetsAttachInfo* attachInfo);

    XrResult SuggestInteractionProfileBindings(const XrInteractionProfileSuggestedBinding* suggestedBindings);

    XrResult GetInputSourceLocalizedName(const XrInputSourceLocalizedNameGetInfo* getInfo, uint32_t bufferCapacityInput, uint32_t* bufferCountOutput, char* buffer);

    XrResult LocateViews(const XrViewLocateInfo* viewLocateInfo, XrViewState* viewState, uint32_t viewCapacityInput, uint32_t* viewCountOutput, XrView* views);

    XrResult EnumerateBoundSourcesForAction(const XrBoundSourcesForActionEnumerateInfo* enumerateInfo, uint32_t sourceCapacityInput, uint32_t* sourceCountOutput, XrPath* sources);

    XrResult EnumerateViewConfigurations(XrSystemId systemId, uint32_t viewConfigurationTypeCapacityInput, uint32_t* viewConfigurationTypeCountOutput, XrViewConfigurationType* viewConfigurationTypes);

    XrResult EnumerateViewConfigurationViews(XrSystemId systemId, XrViewConfigurationType viewConfigurationType, uint32_t viewCapacityInput, uint32_t* viewCountOutput, XrViewConfigurationView* views);

    XrResult EnumerateEnvironmentBlendModes(XrSystemId systemId, XrViewConfigurationType viewConfigurationType, uint32_t environmentBlendModeCapacityInput, uint32_t* environmentBlendModeCountOutput, XrEnvironmentBlendMode* environmentBlendModes);

    XrResult ApplyHapticFeedback(const XrHapticActionInfo* hapticActionInfo, const XrHapticBaseHeader* hapticFeedback);
    XrResult StopHapticFeedback(const XrHapticActionInfo* hapticActionInfo);

    XrResult GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function);

    XrResult ActivateSecondaryView(XrViewConfigurationType viewConfiguration, bool activate);

    XrResult RegisterScriptEventCallback(PFN_ScriptEventCallback callback);

    XrResult GetSystemProperties(XrSystemId systemId, XrSystemProperties* properties);

private:
    struct MockView
    {
        XrViewConfigurationView configuration;
        XrPosef pose;
        XrFovf fov;
    };

    struct MockViewConfiguration
    {
        XrViewStateFlags stateFlags;
        std::vector<MockView> views;
        bool primary;
        bool enabled;
        bool active;
    };

    struct MockAction
    {
        XrAction action;
        XrPath path;
        std::string name;
        std::string localizedName;
        XrActionType type;
        std::vector<MockInputState*> bindings;
        std::vector<XrPath> userPaths;
        bool isDestroyed;
    };

    struct MockActionSet
    {
        XrActionSet actionSet;
        bool attached;
        std::string name;
        std::string localizedName;
        std::vector<MockAction> actions;
        bool isDestroyed;
    };

    struct MockInteractionInputSource
    {
        XrPath path;
        XrActionType actionType;
        const char* localizedName;
    };

    struct MockInteractionProfile
    {
        XrPath path;
        const char* localizedName;
        std::vector<XrPath> userPaths;
    };

    struct MockUserPath
    {
        std::string path;
        std::string localizedName;
        const MockInteractionProfile* profile;
    };

    struct MockSpace
    {
        XrPosef pose;
        bool isDestroyed;
        XrAction action;
        XrPath subActionPath;
        XrSpaceLocationFlags locationFlags;
        XrReferenceSpaceType referenceSpaceType;
    };

    struct MockReferenceSpace
    {
        bool validExtent;
        XrExtent2Df extent;
    };

    uint64_t GetNextHandle()
    {
        return ++nextHandle;
    }

    template <class T>
    T GetNextHandle()
    {
        return (T)GetNextHandle();
    }

    bool IsValidUserPath(XrPath path) const
    {
        return path != XR_NULL_PATH && GetUserPath(path) == path;
    }

    MockActionSet* GetMockActionSet(XrActionSet actionSet);
    MockActionSet* GetMockActionSet(XrAction action);
    MockAction* GetMockAction(XrAction action);
    const MockInteractionProfile* GetMockInteractionProfile(XrPath interactionProfile) const;
    bool IsActionAttached(XrAction action);
    MockInputState* GetMockInputState(const MockInteractionProfile& mockProfile, XrPath path, XrActionType actionType = XR_ACTION_TYPE_MAX_ENUM);
    MockSpace* GetMockSpace(XrSpace space);
    MockViewConfiguration* GetMockViewConfiguration(XrViewConfigurationType viewConfigType);
    MockView* GetMockView(XrViewConfigurationType viewConfigType, size_t viewIndex);
    MockUserPath* GetMockUserPath(XrPath path);
    MockReferenceSpace* GetMockReferenceSpace(XrReferenceSpaceType referenceSpaceType);

    XrResult ValidateName(const char* name) const;
    XrResult ValidatePath(const char* path) const;

    XrTime GetPredictedTime();

    void InitializeInteractionProfiles();

    bool SetActiveInteractionProfile(MockUserPath* mockUserPath, const MockInteractionProfile* mockProfile);
    MockInputState* AddMockInputState(XrPath interactionPath, XrPath path, XrActionType actionType, const char* localizedName);

    //// XR_MSFT_secondary_view_configuration

    XrResult MSFTSecondaryViewConfiguration_BeginSession(const XrSessionBeginInfo* beginInfo);
    XrResult MSFTSecondaryViewConfiguration_WaitFrame(const XrFrameWaitInfo* frameWaitInfo, XrFrameState* frameState);
    XrResult MSFTSecondaryViewConfiguration_EndFrame(const XrFrameEndInfo* frameEndInfo);

    template <typename T>
    void QueueEvent(const T& event)
    {
        QueueEvent((const XrEventDataBuffer&)event);
    }

    void QueueEvent(const XrEventDataBuffer& buffer);

    XrEventDataBuffer GetNextEvent();

    std::vector<XrSecondaryViewConfigurationStateMSFT> secondaryViewConfigurationStates;

    //// XR_MSFT_first_person_observer

    XrResult MSFTFirstPersonObserver_Init();

    //// XR_MSFT_first_person_observer

    XrResult MSFTThirdPersonObserver_Init();

    std::vector<MockInteractionProfile> interactionProfiles;

    MockRuntimeCreateFlags createFlags;
    std::queue<XrEventDataBuffer> eventQueue;
    XrInstance instance;
    XrSession session;
    XrSessionState currentState;
    XrEnvironmentBlendMode blendMode{XR_ENVIRONMENT_BLEND_MODE_OPAQUE};
    bool isRunning;
    bool exitSessionRequested;
    bool actionSetsAttached;
    XrTime lastWaitFrame;
    XrTime invalidTimeThreshold;

    std::map<XrViewConfigurationType, MockViewConfiguration> viewConfigurations;
    XrViewConfigurationType primaryViewConfiguration;

    std::vector<std::string> componentPathStrings;
    std::vector<MockUserPath> userPaths;

    std::chrono::time_point<std::chrono::high_resolution_clock> startTime;

    bool instanceIsLost;
    bool nullGfx;

    int primaryLayersRendered;
    int secondaryLayersRendered;

    uint64_t nextHandle;

    std::vector<MockActionSet> actionSets;
    std::vector<MockInputState> inputStates;
    std::vector<MockSpace> spaces;
    std::map<XrReferenceSpaceType, MockReferenceSpace> referenceSpaces;

    PFN_ScriptEventCallback scriptEventCallback;
};

extern MockRuntime* s_runtime;
