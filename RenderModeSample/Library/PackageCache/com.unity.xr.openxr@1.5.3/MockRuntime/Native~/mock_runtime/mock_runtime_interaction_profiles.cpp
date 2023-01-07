#include "mock.h"

struct MockInputSourcePath
{
    const char* path;
    XrActionType type;
    const char* localizedName;
};

struct MockInteractionProfileDef
{
    const char* localizedName;
    const char* name;
    MockRuntimeCreateFlags requiredFlags;
    std::vector<const char*> userPaths;
    std::vector<MockInputSourcePath> inputSources;
};

static std::vector<MockInteractionProfileDef> s_InteractionProfiles = {
    // Mock Controller
    {
        "Mock Controller",
        "/interaction_profiles/unity/mock_controller",
        0,
        {"/user/hand/left",
            "/user/hand/right"},
        {{"/user/hand/left/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Button"},
            {"/user/hand/left/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/left/input/thumbstick/value", XR_ACTION_TYPE_VECTOR2F_INPUT, "Thumbstick"},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"},
            {"/user/hand/right/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Button"},
            {"/user/hand/right/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/right/input/thumbstick/value", XR_ACTION_TYPE_VECTOR2F_INPUT, "Thumbstick"},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"}}},

    // KHR Simple controller
    {
        "KHR Simple Controller",
        "/interaction_profiles/khr/simple_controller",
        0,
        {"/user/hand/left",
            "/user/hand/right"},
        {{"/user/hand/left/input/select/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Select"},
            {"/user/hand/left/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Menu"},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"},
            {"/user/hand/right/input/select/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Select"},
            {"/user/hand/right/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Menu"},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"}}},

    // Microsoft Mixed Reality Motion Controller Profile
    {
        "Mixed Reality Controller",
        "/interaction_profiles/microsoft/motion_controller",
        0,
        {"/user/hand/left",
            "/user/hand/right"},
        {{"/user/hand/left/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Menu"},
            {"/user/hand/left/input/squeeze/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Squeeze"},
            {"/user/hand/left/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/left/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick X"},
            {"/user/hand/left/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick Y"},
            {"/user/hand/left/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Click"},
            {"/user/hand/left/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT, "Thumbstick"},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/left/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Click"},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"},
            {"/user/hand/right/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Menu"},
            {"/user/hand/right/input/squeeze/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Squeeze"},
            {"/user/hand/right/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/right/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick X"},
            {"/user/hand/right/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick Y"},
            {"/user/hand/right/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Click"},
            {"/user/hand/right/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT, "Thumbstick"},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/right/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Click"},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"}}},

    // Google Daydream Controller Profile
    {
        "Daydream Controller",
        "/interaction_profiles/google/daydream_controller",
        0,
        {"/user/hand/left",
            "/user/hand/right"},
        {{"/user/hand/left/input/select/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Select"},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/left/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Click"},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Pose"},
            {"/user/hand/right/input/select/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Select"},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/right/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Click"},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"}}},

    // HTC Vive Controller Profile
    {
        "HTC Vive Controller",
        "/interaction_profiles/htc/vive_controller",
        0,
        {"/user/hand/left",
            "/user/hand/right"},
        {{"/user/hand/left/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Menu"},
            {"/user/hand/left/input/system/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "System"},
            {"/user/hand/left/input/squeeze/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Squeeze"},
            {"/user/hand/left/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/left/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trigger Click"},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/left/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Click"},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"},
            {"/user/hand/right/input/system/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "System"},
            {"/user/hand/right/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Menu"},
            {"/user/hand/right/input/squeeze/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Squeeze"},
            {"/user/hand/right/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/right/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trigger Click"},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/right/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Click"},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"}}},

    // HTC Vive Pro Profile
    {
        "HTC Vive Pro Controller",
        "/interaction_profiles/htc/vive_pro",
        0,
        {"/user/head"},
        {
            {"/user/head/input/volume_up/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Volume Up"},
            {"/user/head/input/volume_down/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Volume Down"},
            {"/user/head/input/mute_mic/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Mute Mic"},
        }},

    // Microsoft Xbox Controller Profile
    {
        "XBox Controller",
        "/interaction_profiles/microsoft/xbox_controller",
        0,
        {"/user/gamepad"},
        {{"/user/gamepad/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Menu"},
            {"/user/gamepad/input/view/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "View"},
            {"/user/gamepad/input/a/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "A"},
            {"/user/gamepad/input/b/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "B"},
            {"/user/gamepad/input/x/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "X"},
            {"/user/gamepad/input/y/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Y"},
            {"/user/gamepad/input/dpad_down/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "D-pad Down"},
            {"/user/gamepad/input/dpad_right/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "D-pad Right"},
            {"/user/gamepad/input/dpad_up/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "D-pad Up"},
            {"/user/gamepad/input/dpad_left/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "D-pad Left"},
            {"/user/gamepad/input/shoulder_left/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Left Shoulder"},
            {"/user/gamepad/input/shoulder_right/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Right Shoulder"},
            {"/user/gamepad/input/thumbstick_left/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Left Thumbstick Click"},
            {"/user/gamepad/input/thumbstick_right/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Right Thumbstick Click"},
            {"/user/gamepad/input/trigger_left/value", XR_ACTION_TYPE_FLOAT_INPUT, "Left Trigger"},
            {"/user/gamepad/input/trigger_right/value", XR_ACTION_TYPE_FLOAT_INPUT, "Right Trigger"},
            {"/user/gamepad/input/thumbstick_left/x", XR_ACTION_TYPE_FLOAT_INPUT, "Left Thumbstick X"},
            {"/user/gamepad/input/thumbstick_left/y", XR_ACTION_TYPE_FLOAT_INPUT, "Left Thumbstick Y"},
            {"/user/gamepad/input/thumbstick_left", XR_ACTION_TYPE_VECTOR2F_INPUT, "Left Thumbstick"},
            {"/user/gamepad/input/thumbstick_right/x", XR_ACTION_TYPE_FLOAT_INPUT, "Right Thumbstick X"},
            {"/user/gamepad/input/thumbstick_right/y", XR_ACTION_TYPE_FLOAT_INPUT, "Right Thumbstick Y"},
            {"/user/gamepad/input/thumbstick_right", XR_ACTION_TYPE_VECTOR2F_INPUT, "Right Thumbstick"},
            {"/user/gamepad/output/haptic_left", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Left Hatic"},
            {"/user/gamepad/output/haptic_right", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Right Haptic"},
            {"/user/gamepad/output/haptic_left_trigger", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Left Trigger Haptic"},
            {"/user/gamepad/output/haptic_right_trigger", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Right Trigger Haptic"}}},

    // Oculus Go Controller Profile
    {
        "Oculus Go Controller",
        "/interaction_profiles/oculus/go_controller",
        0,
        {"/user/hand/left",
            "/user/hand/right"},
        {
            {"/user/hand/left/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trigger"},
            {"/user/hand/left/input/back/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Back"},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/left/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Click"},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/right/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trigger"},
            {"/user/hand/right/input/back/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Back"},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/right/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Click"},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Pose"},
        }},

    // Oculus Touch Controller Profile
    {
        "Oculus Touch Controller",
        "/interaction_profiles/oculus/touch_controller",
        0,
        {"/user/hand/left",
            "/user/hand/right"},
        {
            {"/user/hand/left/input/x/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "X"},
            {"/user/hand/left/input/x/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "X Touch"},
            {"/user/hand/left/input/y/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Y"},
            {"/user/hand/left/input/y/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Y Touch"},
            {"/user/hand/left/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Menu"},
            {"/user/hand/left/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT, "Grip"},
            {"/user/hand/left/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/left/input/trigger/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Touch"},
            {"/user/hand/left/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick X"},
            {"/user/hand/left/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick Y"},
            {"/user/hand/left/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Click"},
            {"/user/hand/left/input/thumbstick/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Touch"},
            {"/user/hand/left/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT, "Thumbstick"},
            // Rift S and Quest controllers lack thumbrests
            // {"/user/hand/left/input/thumbrest/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"},
            {"/user/hand/right/input/a/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "A"},
            {"/user/hand/right/input/a/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "A Touch"},
            {"/user/hand/right/input/b/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "B"},
            {"/user/hand/right/input/b/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "B Touch"},
            // The system ("Oculus") button is reserved for system applications
            {"/user/hand/right/input/system/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "System"},
            {"/user/hand/right/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT, "Grip"},
            {"/user/hand/right/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/right/input/trigger/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trigger Touch"},
            {"/user/hand/right/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick X"},
            {"/user/hand/right/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick Y"},
            {"/user/hand/right/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Click"},
            {"/user/hand/right/input/thumbstick/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Touch"},
            {"/user/hand/right/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT, "Thumbstick"},
            // Rift S and Quest controllers lack thumbrests
            // {"/user/hand/right/input/thumbrest/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"},
        }},

    // Valve Index Controller Profile
    {
        "Index Controller",
        "/interaction_profiles/valve/index_controller",
        0,
        {"/user/hand/left",
            "/user/hand/right"},
        {
            {"/user/hand/left/input/system/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "System"},
            {"/user/hand/left/input/system/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "System Touch"},
            {"/user/hand/left/input/a/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "A"},
            {"/user/hand/left/input/a/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "A Touch"},
            {"/user/hand/left/input/b/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "B"},
            {"/user/hand/left/input/b/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "B Touch"},
            {"/user/hand/left/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT, "Squeeze"},
            {"/user/hand/left/input/squeeze/force", XR_ACTION_TYPE_FLOAT_INPUT, "Squeeze Force"},
            {"/user/hand/left/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trigger Click"},
            {"/user/hand/left/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/left/input/trigger/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trigger Touch"},
            {"/user/hand/left/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick X"},
            {"/user/hand/left/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick Y"},
            {"/user/hand/left/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Click"},
            {"/user/hand/left/input/thumbstick/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Touch"},
            {"/user/hand/left/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT, "Thumbstick"},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/left/input/trackpad/force", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Force"},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"},
            {"/user/hand/right/input/system/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "System"},
            {"/user/hand/right/input/system/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "System Touch"},
            {"/user/hand/right/input/a/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "A"},
            {"/user/hand/right/input/a/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "A Touch"},
            {"/user/hand/right/input/b/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "B"},
            {"/user/hand/right/input/b/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "B Touch"},
            {"/user/hand/right/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT, "Squeeze"},
            {"/user/hand/right/input/squeeze/force", XR_ACTION_TYPE_FLOAT_INPUT, "Squeeze Force"},
            {"/user/hand/right/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trigger Click"},
            {"/user/hand/right/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT, "Trigger"},
            {"/user/hand/right/input/trigger/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trigger Touch"},
            {"/user/hand/right/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick X"},
            {"/user/hand/right/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT, "Thumbstick Y"},
            {"/user/hand/right/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Click"},
            {"/user/hand/right/input/thumbstick/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Thumbstick Touch"},
            {"/user/hand/right/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT, "Thumbstick"},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad X"},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Y"},
            {"/user/hand/right/input/trackpad/force", XR_ACTION_TYPE_FLOAT_INPUT, "Trackpad Force"},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT, "Trackpad Touch"},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT, "Trackpad"},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT, "Haptic"},
        }},

    // Eye gaze interaction extension
    {
        "Eye Gaze",
        "/interaction_profiles/ext/eye_gaze_interaction",
        MR_CREATE_EYE_GAZE_INTERACTION_EXT,
        {"/user/eyes_ext"},
        {{"/user/eyes_ext/input/gaze_ext/pose", XR_ACTION_TYPE_POSE_INPUT, "Gaze"}}},

    // MSFT Hand Interaction extension
    {
        "Hand",
        "/interaction_profiles/microsoft/hand_interaction",
        MR_CREATE_MSFT_HAND_INTERACTION_EXT,
        {"/user/hand/left", "/user/hand/right"},
        {{"/user/hand/left/input/select/value", XR_ACTION_TYPE_FLOAT_INPUT, "Select"},
            {"/user/hand/left/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT, "Squeeze"},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"},
            {"/user/hand/right/input/select/value", XR_ACTION_TYPE_FLOAT_INPUT, "Select"},
            {"/user/hand/right/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT, "Squeeze"},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT, "Aim"},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT, "Grip"}}},
};

void MockRuntime::InitializeInteractionProfiles()
{
    inputStates.clear();
    interactionProfiles.reserve(s_InteractionProfiles.size());
    for (MockInteractionProfileDef& def : s_InteractionProfiles)
    {
        // Require specific create flags to use this profile?
        if ((def.requiredFlags & createFlags) != def.requiredFlags)
            continue;

        interactionProfiles.emplace_back();
        MockInteractionProfile& mockProfile = interactionProfiles.back();
        mockProfile.path = StringToPath(def.name);
        mockProfile.userPaths.reserve(def.userPaths.size());
        mockProfile.localizedName = def.localizedName;
        for (const char* userPathString : def.userPaths)
        {
            mockProfile.userPaths.push_back(StringToPath(userPathString));
        }

        for (MockInputSourcePath& componentDef : def.inputSources)
        {
            AddMockInputState(mockProfile.path, StringToPath(componentDef.path), componentDef.type, componentDef.localizedName);
        }
    }
}

const MockRuntime::MockInteractionProfile* MockRuntime::GetMockInteractionProfile(XrPath interactionProfile) const
{
    for (const MockInteractionProfile& mockProfile : interactionProfiles)
        if (mockProfile.path == interactionProfile)
            return &mockProfile;

    return nullptr;
}
