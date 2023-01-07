// Defines Mock Runtime specific events at the end of the structure type list

const XrStructureType XR_TYPE_EVENT_SCRIPT_EVENT_MOCK = (XrStructureType)(XR_STRUCTURE_TYPE_MAX_ENUM - 1);

// Register a callback that gets called for script events sent by mock
//
typedef enum
{
    XR_MOCK_SCRIPT_EVENT_UNKNOWN,
    XR_MOCK_SCRIPT_EVENT_END_FRAME,
    XR_MOCK_SCRIPT_EVENT_HAPTIC_IMPULSE,
    XR_MOCK_SCRIPT_EVENT_HAPTIC_STOP
} XrMockScriptEvent;

typedef struct XrEventScriptEventMOCK
{
    XrStructureType type;
    const void* XR_MAY_ALIAS next;
    XrMockScriptEvent event;
    uint64_t param;

} XrEventScriptEventMOCK;

typedef void (*PFN_ScriptEventCallback)(XrMockScriptEvent event, uint64_t param);
