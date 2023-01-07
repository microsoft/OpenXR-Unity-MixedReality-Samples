#pragma once

// Handles are defined as uin64_t on 32-bit builds.  They already have a template defined, so we need to exclude this block from 32-bit builds.
// (and remember that _WIN32 is defined on 64-bit windows builds)
#if XR_TYPE_SAFE_HANDLES

typedef struct XrActionSpecialize_t* XrActionSpecialize;
#define XrAction XrActionSpecialize
typedef struct XrActionSetSpecialize_t* XrActionSetSpecialize;
#define XrActionSet XrActionSetSpecialize
typedef struct XrSpaceSpecialize_t* XrSpaceSpecialize;
#define XrSpace XrSpaceSpecialize

#define SEND_TO_CSHARP_HANDLES(handlename)                   \
    template <>                                              \
    void SendToCSharp<>(const char* fieldname, handlename t) \
    {                                                        \
        SendToCSharp(fieldname, (uint64_t)t);                \
    }

XR_LIST_HANDLES(SEND_TO_CSHARP_HANDLES)

#define SEND_TO_CSHARP_HANDLES_PTRS(handlename)               \
    template <>                                               \
    void SendToCSharp<>(const char* fieldname, handlename* t) \
    {                                                         \
        if (t != nullptr)                                     \
            SendToCSharp(fieldname, (uint64_t)*t);            \
        else                                                  \
            SendToCSharp(fieldname, (uint64_t)0);             \
    }

XR_LIST_HANDLES(SEND_TO_CSHARP_HANDLES_PTRS)

#undef XrAction
#undef XrActionSet
#undef XrSpace

#endif
