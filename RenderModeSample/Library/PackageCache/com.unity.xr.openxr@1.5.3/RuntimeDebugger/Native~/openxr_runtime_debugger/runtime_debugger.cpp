#include "platform_includes.h"

#include <openxr/openxr_platform_defines.h>

// 32-bit builds don't have type-safe handles so we can't distinguish them
#define XR_TYPE_SAFE_HANDLES (XR_PTR_SIZE == 8)

#if !defined(XR_DEFINE_ATOM)
#if XR_TYPE_SAFE_HANDLES
#define XR_DEFINE_ATOM(object) typedef struct object##_T* object;
#else
#define XR_DEFINE_ATOM(object) typedef uint64_t object;
#endif
#endif

#define XR_NO_PROTOTYPES
#include <openxr/openxr.h>
#include <openxr/openxr_platform.h>

#include "openxr/openxr_reflection_full.h"

#include <cstdio>
#include <cstring>
#include <set>
#include <type_traits>

#include "api_exports.h"

#define CATCH_MISSING_TEMPLATES 0

static void SendString(const char* fieldName, const char* t);

template <typename T>
void SendToCSharp(const char* fieldname, T t)
#if !CATCH_MISSING_TEMPLATES
{
    SendString(fieldname, "<Unknown>");
}
#else
    ;
#endif

template <typename T>
void SendToCSharp(const char* fieldname, T* t)
#if !CATCH_MISSING_TEMPLATES
{
    char buf[32];
    snprintf(buf, 32, "0x%p", t);
    SendString(fieldname, buf);
}
#else
    ;
#endif

// Arrays of base structs
template <typename structType>
bool SendToCSharpBaseStructArray(const char* fieldname, structType t, int lenParam)
{
    return false;
}

// These includes are order-sensitive
// clang-format off
#include "serialize_data.h"
#include "serialize_data_access.h"

#include "serialize_primitives.h"
#include "serialize_enums.h"
#include "serialize_handles.h"
#include "serialize_atoms.h"
#include "serialize_nextptr.h"
#include "serialize_external.h"
#include "serialize_structs.h"
#include "serialize_todo.h"
#include "serialize_nextptr_impl.h"

#include "serialize_funcs_specialization.h"
#include "serialize_funcs.h"
// clang-format on

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetInstanceProcAddr(XrInstance instance, const char* name, PFN_xrVoidFunction* function)
{
    StartFunctionCall("xrGetInstanceProcAddr");
    SendToCSharp("instance", instance);
    SendToCSharp("name", name);
    SendToCSharp("function", "<func>");

    XR_LIST_FUNCS(GEN_FUNC_LOAD)

    EndFunctionCall("xrGetInstanceProcAddr", "UNKNOWN FUNC");

    return orig_xrGetInstanceProcAddr(instance, name, function);
}

extern "C" PFN_xrGetInstanceProcAddr UNITY_INTERFACE_EXPORT XRAPI_PTR HookXrInstanceProcAddr(PFN_xrGetInstanceProcAddr func, uint32_t cacheSize, uint32_t perThreadCacheSize)
{
    ResetLUT();
    s_CacheSize = cacheSize;
    s_PerThreadCacheSize = perThreadCacheSize;
    s_MainDataStore.SetOverflowMode(RingBuf::kOverflowModeTruncate);
    orig_xrGetInstanceProcAddr = func;
    return xrGetInstanceProcAddr;
}
