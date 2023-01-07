#pragma once

#include <cassert>
#include <deque>
#include <mutex>
#include <stdlib.h>
#include <string>
#include <thread>

enum Command
{
    kStartFunctionCall,
    kStartStruct,

    kFloat,
    kString,
    kInt32,
    kInt64,
    kUInt32,
    kUInt64,

    kEndStruct,
    kEndFunctionCall,

    kCacheNotLargeEnough,

    kLUTDefineTables,
    kLUTEntryUpdateStart,
    kLutEntryUpdateEnd,
    kLUTLookup,

    kEndData = 0xFFFFFFFF
};

enum LUT
{
    kXrPath,
    kXrAction,
    kXrActionSet,
    kXrSpace,

    kLUTPadding = 0xFFFFFFFF,
};

static const char* const kLutNames[] = {
    "XrPaths",
    "XrActions",
    "XrActionSets",
    "XrSpaces",
};

#include "ringbuf.h"

static std::mutex s_DataMutex;

// These get set from c# in HookXrInstanceProcAddr
static uint32_t s_CacheSize = 0;
static uint32_t s_PerThreadCacheSize = 0;
static uint32_t s_LUTCacheSize = 256; // TODO:

// Accessing this must be protected with s_DataMutex.
// Only add into these at kEndFunctionCall to avoid mixing with other threads.
static RingBuf s_MainDataStore = {};

static RingBuf s_LUTDataStore = {};

// Thread local storage of serialized commands.
// On EndFunctionCall they'll be moved into the static storage w/ mutex lock.
thread_local RingBuf s_ThreadLocalDataStore = {};

static void StartFunctionCall(const char* funcName)
{
    if (s_ThreadLocalDataStore.cacheSize != s_PerThreadCacheSize)
    {
        s_ThreadLocalDataStore.Destroy();
        s_ThreadLocalDataStore.Create(s_PerThreadCacheSize, RingBuf::kOverflowModeWrap);
    }

    s_ThreadLocalDataStore.CreateNewBlock();
    s_ThreadLocalDataStore.Write(kStartFunctionCall);
    s_ThreadLocalDataStore.Write(std::this_thread::get_id());
    s_ThreadLocalDataStore.Write(funcName);
}

static void StartStruct(const char* fieldName, const char* structName)
{
    s_ThreadLocalDataStore.Write(kStartStruct);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write(structName);
}

static void SendFloat(const char* fieldName, float t)
{
    s_ThreadLocalDataStore.Write(kFloat);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write(t);
}

static void SendString(const char* fieldName, const char* t)
{
    s_ThreadLocalDataStore.Write(kString);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write(t);
}

static void SendInt32(const char* fieldName, int32_t t)
{
    s_ThreadLocalDataStore.Write(kInt32);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write(t);
}

static void SendInt64(const char* fieldName, int64_t t)
{
    s_ThreadLocalDataStore.Write(kInt64);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write(t);
}

static void SendUInt32(const char* fieldName, uint32_t t)
{
    s_ThreadLocalDataStore.Write(kUInt32);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write(t);
}

static void SendUInt64(const char* fieldName, uint64_t t)
{
    s_ThreadLocalDataStore.Write(kUInt64);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write(t);
}

static void EndStruct()
{
    s_ThreadLocalDataStore.Write(kEndStruct);
}

static void EndFunctionCall(const char* funcName, const char* result)
{
    s_ThreadLocalDataStore.Write(kEndFunctionCall);
    s_ThreadLocalDataStore.Write(result);

    {
        std::lock_guard<std::mutex> guard(s_DataMutex);

        if (s_MainDataStore.cacheSize != s_CacheSize)
        {
            s_MainDataStore.Destroy();
            s_MainDataStore.Create(s_CacheSize, RingBuf::kOverflowModeTruncate);
        }

        if (!s_MainDataStore.MoveFrom(s_ThreadLocalDataStore))
        {
            s_MainDataStore.CreateNewBlock();
            s_MainDataStore.Write(kCacheNotLargeEnough);
            s_MainDataStore.Write(std::this_thread::get_id());
            s_MainDataStore.Write(funcName);
            s_MainDataStore.Write(result);
        }
    }
}

void ResetLUT()
{
    s_LUTDataStore.Destroy();
    s_LUTDataStore.Create(s_LUTCacheSize, RingBuf::kOverflowModeGrowDouble);
    s_LUTDataStore.CreateNewBlock();

    // Setup LUTS
    s_LUTDataStore.Write(kLUTDefineTables);
    uint32_t numLuts = (uint32_t)(sizeof(kLutNames) / sizeof(kLutNames[0]));
    s_LUTDataStore.Write(numLuts);
    for (uint32_t i = 0; i < numLuts; ++i)
    {
        s_LUTDataStore.Write(kLutNames[i]);
    }
}

static void StoreInLUT(const char* funcName)
{
    if (s_LUTDataStore.cacheSize < s_LUTCacheSize)
    {
        ResetLUT();
    }

    {
        std::lock_guard<std::mutex> guard(s_DataMutex); // TODO: probably have a different mutex for LUT data store
        s_LUTDataStore.MoveFrom(s_ThreadLocalDataStore);
    }

    // We hijacked the function that was already started.
    // Start a function call for the real call which happens next.
    StartFunctionCall(funcName);
}

static void SendXrPathCreate(const char* funcName, XrPath path, const char* string)
{
    s_ThreadLocalDataStore.Reset();
    s_ThreadLocalDataStore.CreateNewBlock();
    s_ThreadLocalDataStore.Write(kLUTEntryUpdateStart);
    s_ThreadLocalDataStore.Write(kXrPath);
    s_ThreadLocalDataStore.Write((uint64_t)path);
    s_ThreadLocalDataStore.Write(string);
    StartStruct("", "");
    EndStruct();

    StoreInLUT(funcName);
}

static void SendXrPath(const char* fieldName, XrPath t)
{
    s_ThreadLocalDataStore.Write(kLUTLookup);
    s_ThreadLocalDataStore.Write(kXrPath);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write((uint64_t)t);
}

template <>
void SendToCSharp<>(const char*, const XrActionCreateInfo*);
static void SendXrActionCreate(const char* funcName, XrAction action, const XrActionCreateInfo* createInfo)
{
    s_ThreadLocalDataStore.Reset();
    s_ThreadLocalDataStore.CreateNewBlock();
    s_ThreadLocalDataStore.Write(kLUTEntryUpdateStart);
    s_ThreadLocalDataStore.Write(kXrAction);
    s_ThreadLocalDataStore.Write((uint64_t)action);
    s_ThreadLocalDataStore.Write(createInfo->actionName);
    SendToCSharp("", createInfo);

    StoreInLUT(funcName);
}

static void SendXrAction(const char* fieldName, XrAction t)
{
    s_ThreadLocalDataStore.Write(kLUTLookup);
    s_ThreadLocalDataStore.Write(kXrAction);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write((uint64_t)t);
}

template <>
void SendToCSharp<>(const char*, const XrActionSetCreateInfo*);
static void SendXrActionSetCreate(const char* funcName, XrActionSet actionSet, const XrActionSetCreateInfo* createInfo)
{
    s_ThreadLocalDataStore.Reset();
    s_ThreadLocalDataStore.CreateNewBlock();
    s_ThreadLocalDataStore.Write(kLUTEntryUpdateStart);
    s_ThreadLocalDataStore.Write(kXrActionSet);
    s_ThreadLocalDataStore.Write((uint64_t)actionSet);
    s_ThreadLocalDataStore.Write(createInfo->actionSetName);
    SendToCSharp("", createInfo);

    StoreInLUT(funcName);
}

static void SendXrActionSet(const char* fieldName, XrActionSet t)
{
    s_ThreadLocalDataStore.Write(kLUTLookup);
    s_ThreadLocalDataStore.Write(kXrActionSet);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write((uint64_t)t);
}

template <>
void SendToCSharp<>(const char*, const XrActionSpaceCreateInfo*);
static void SendXrActionSpaceCreate(const char* funcName, const XrActionSpaceCreateInfo* createInfo, XrSpace* space)
{
    s_ThreadLocalDataStore.Reset();
    s_ThreadLocalDataStore.CreateNewBlock();
    s_ThreadLocalDataStore.Write(kLUTEntryUpdateStart);
    s_ThreadLocalDataStore.Write(kXrSpace);
    s_ThreadLocalDataStore.Write((uint64_t)*space);
    s_ThreadLocalDataStore.Write("Action Space");
    SendToCSharp("", createInfo);

    StoreInLUT(funcName);
}

#define GENERATE_REFERENCE_SPACE_STRING(enumentry, enumvalue) \
    case enumvalue:                                           \
        return #enumentry;                                    \
        break;

static const char* GetReferenceSpaceString(XrReferenceSpaceType t)
{
    switch (t)
    {
        XR_LIST_ENUM_XrReferenceSpaceType(GENERATE_REFERENCE_SPACE_STRING) default : return "Reference Space Unknown";
    }
}

template <>
void SendToCSharp<>(const char*, const XrReferenceSpaceCreateInfo*);
static void SendXrReferenceSpaceCreate(const char* funcName, const XrReferenceSpaceCreateInfo* createInfo, XrSpace* space)
{
    s_ThreadLocalDataStore.Reset();
    s_ThreadLocalDataStore.CreateNewBlock();
    s_ThreadLocalDataStore.Write(kLUTEntryUpdateStart);
    s_ThreadLocalDataStore.Write(kXrSpace);
    s_ThreadLocalDataStore.Write((uint64_t)*space);
    s_ThreadLocalDataStore.Write(GetReferenceSpaceString(createInfo->referenceSpaceType));
    SendToCSharp("", createInfo);

    StoreInLUT(funcName);
}

static void SendXrSpace(const char* fieldName, XrSpace t)
{
    s_ThreadLocalDataStore.Write(kLUTLookup);
    s_ThreadLocalDataStore.Write(kXrSpace);
    s_ThreadLocalDataStore.Write(fieldName);
    s_ThreadLocalDataStore.Write((uint64_t)t);
}
