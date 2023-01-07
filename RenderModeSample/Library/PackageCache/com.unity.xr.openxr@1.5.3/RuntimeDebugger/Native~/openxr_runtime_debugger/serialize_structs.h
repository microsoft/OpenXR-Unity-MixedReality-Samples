#pragma once

#define SEND_TO_CSHARP_BASE_STRUCT_DECL(structType) \
    template <>                                     \
    bool SendToCSharpBaseStructArray<structType*>(const char* fieldname, structType* t, int lenParam);

#define SEND_TO_CSHARP_BASE_STRUCT_PTR_DECL(structType) \
    template <>                                         \
    bool SendToCSharpBaseStructArray<structType**>(const char* fieldname, structType** t, int lenParam);

#define SEND_TO_CSHARP_BASE_STRUCT_CONST_PTR_DECL(structType) \
    template <>                                               \
    bool SendToCSharpBaseStructArray<structType const* const*>(const char* fieldname, structType const* const* t, int lenParam);

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_BASE_STRUCT_DECL)

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_BASE_STRUCT_PTR_DECL)

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_BASE_STRUCT_CONST_PTR_DECL)

#define SEND_TO_CSHARP_STRUCT_DECL(structType, ...) \
    template <>                                     \
    void SendToCSharp<>(const char* fieldname, structType t);

#define SEND_TO_CSHARP_STRUCT_PTR_DECL(structType, ...) \
    template <>                                         \
    void SendToCSharp<>(const char* fieldname, structType* t);

#define SEND_TO_CSHARP_STRUCT_CONST_PTR(structType, ...) \
    template <>                                          \
    void SendToCSharp<>(const char* fieldname, structType const* t);

// Fwd declare base structs
XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_STRUCT_DECL)

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_STRUCT_PTR_DECL)

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_STRUCT_CONST_PTR)

// Fwd declare basic structs
XR_LIST_BASIC_STRUCTS(SEND_TO_CSHARP_STRUCT_DECL)

XR_LIST_BASIC_STRUCTS(SEND_TO_CSHARP_STRUCT_PTR_DECL)

XR_LIST_BASIC_STRUCTS(SEND_TO_CSHARP_STRUCT_CONST_PTR)

// Fwd declare full structs
XR_LIST_STRUCTURE_TYPES(SEND_TO_CSHARP_STRUCT_DECL)

XR_LIST_STRUCTURE_TYPES(SEND_TO_CSHARP_STRUCT_PTR_DECL)

XR_LIST_STRUCTURE_TYPES(SEND_TO_CSHARP_STRUCT_CONST_PTR)

#define SEND_TO_CSHARP_INDIVIDUAL_FIELDS(structname) \
    SendToCSharp(#structname, t.structname);

#define SEND_TO_CSHARP_INDIVIDUAL_FIELDS_PTR(structname) \
    SendToCSharp(#structname, t->structname);

#define SEND_TO_CSHARP_ARRAYS(structname, structlen)                          \
    if (!SendToCSharpBaseStructArray(#structname, t.structname, t.structlen)) \
    {                                                                         \
        for (uint32_t i = 0; i < t.structlen; ++i)                            \
            SendToCSharp(#structname, t.structname[i]);                       \
    }

#define SEND_TO_CSHARP_ARRAYS_PTR(structname, structlen)                        \
    if (!SendToCSharpBaseStructArray(#structname, t->structname, t->structlen)) \
    {                                                                           \
        for (uint32_t i = 0; i < t->structlen; ++i)                             \
            SendToCSharp(#structname, t->structname[i]);                        \
    }

#define SEND_TO_CSHARP_STRUCTS(structname, ...)                        \
    template <>                                                        \
    void SendToCSharp<>(const char* fieldname, structname t)           \
    {                                                                  \
        StartStruct(fieldname, #structname);                           \
        XR_LIST_STRUCT_##structname(SEND_TO_CSHARP_INDIVIDUAL_FIELDS); \
        XR_LIST_STRUCT_ARRAYS_##structname(SEND_TO_CSHARP_ARRAYS);     \
        EndStruct();                                                   \
    }

#define SEND_TO_CSHARP_STRUCTS_PTRS(structname, ...)                       \
    template <>                                                            \
    void SendToCSharp<>(const char* fieldname, structname* t)              \
    {                                                                      \
        StartStruct(fieldname, #structname);                               \
        XR_LIST_STRUCT_##structname(SEND_TO_CSHARP_INDIVIDUAL_FIELDS_PTR); \
        XR_LIST_STRUCT_ARRAYS_##structname(SEND_TO_CSHARP_ARRAYS_PTR);     \
        EndStruct();                                                       \
    }

#define SEND_TO_CSHARP_STRUCTS_CONST_PTRS(structname, ...)                 \
    template <>                                                            \
    void SendToCSharp<>(const char* fieldname, const structname* t)        \
    {                                                                      \
        StartStruct(fieldname, #structname);                               \
        XR_LIST_STRUCT_##structname(SEND_TO_CSHARP_INDIVIDUAL_FIELDS_PTR); \
        XR_LIST_STRUCT_ARRAYS_##structname(SEND_TO_CSHARP_ARRAYS_PTR);     \
        EndStruct();                                                       \
    }

// Basic Structs
XR_LIST_BASIC_STRUCTS(SEND_TO_CSHARP_STRUCTS)

XR_LIST_BASIC_STRUCTS(SEND_TO_CSHARP_STRUCTS_PTRS)

XR_LIST_BASIC_STRUCTS(SEND_TO_CSHARP_STRUCTS_CONST_PTRS)

// Full Structs
XR_LIST_STRUCTURE_TYPES(SEND_TO_CSHARP_STRUCTS)

XR_LIST_STRUCTURE_TYPES(SEND_TO_CSHARP_STRUCTS_PTRS)

XR_LIST_STRUCTURE_TYPES(SEND_TO_CSHARP_STRUCTS_CONST_PTRS)

#include "serialize_structs_base.h"