#pragma once

#define BASE_TO_TYPE(typeName, typeType)                      \
    case typeType:                                            \
    {                                                         \
        typeName* realType = reinterpret_cast<typeName*>(&t); \
        SendToCSharp(fieldname, realType);                    \
    }                                                         \
    break;

#define SEND_TO_CSHARP_BASE_STRUCT(structType)                    \
    template <>                                                   \
    void SendToCSharp<>(const char* fieldname, structType t)      \
    {                                                             \
        switch (t.type)                                           \
        {                                                         \
            XR_LIST_BASE_STRUCT_TYPES_##structType(BASE_TO_TYPE); \
        case XR_TYPE_UNKNOWN:                                     \
        default:                                                  \
            SendToCSharp(fieldname, "<Unknown>");                 \
            break;                                                \
        }                                                         \
    }

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_BASE_STRUCT)

#define BASE_TO_TYPE_PTR(typeName, typeType)                 \
    case typeType:                                           \
    {                                                        \
        typeName* realType = reinterpret_cast<typeName*>(t); \
        SendToCSharp(fieldname, realType);                   \
    }                                                        \
    break;

#define SEND_TO_CSHARP_BASE_STRUCT_PTR(structType)                    \
    template <>                                                       \
    void SendToCSharp<>(const char* fieldname, structType* t)         \
    {                                                                 \
        switch (t->type)                                              \
        {                                                             \
            XR_LIST_BASE_STRUCT_TYPES_##structType(BASE_TO_TYPE_PTR); \
        case XR_TYPE_UNKNOWN:                                         \
        default:                                                      \
            SendToCSharp(fieldname, t->type);                         \
            break;                                                    \
        }                                                             \
    }

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_BASE_STRUCT_PTR)

#define BASE_TO_TYPE_CONST_PTR(typeName, typeType)                       \
    case typeType:                                                       \
    {                                                                    \
        typeName const* realType = reinterpret_cast<typeName const*>(t); \
        SendToCSharp(fieldname, realType);                               \
    }                                                                    \
    break;

#define SEND_TO_CSHARP_BASE_STRUCT_CONST_PTR(structType)                    \
    template <>                                                             \
    void SendToCSharp<>(const char* fieldname, structType const* t)         \
    {                                                                       \
        switch (t->type)                                                    \
        {                                                                   \
            XR_LIST_BASE_STRUCT_TYPES_##structType(BASE_TO_TYPE_CONST_PTR); \
        case XR_TYPE_UNKNOWN:                                               \
        default:                                                            \
            SendToCSharp(fieldname, "<Unknown>");                           \
            break;                                                          \
        }                                                                   \
    }

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_BASE_STRUCT_CONST_PTR)

// If we serializing a base struct array such as XrSwapchainImageBaseHeader,
// we can only loop over the array of the real type.

#define BASE_TO_TYPE_ARRAY(typeName, typeType)                    \
    case typeType:                                                \
    {                                                             \
        typeName* realTypeArray = reinterpret_cast<typeName*>(t); \
        for (int i = 0; i < lenParam; ++i)                        \
            SendToCSharp(fieldname, realTypeArray[i]);            \
    }                                                             \
    break;

#define SEND_TO_CSHARP_BASE_STRUCT_ARRAY(structType)                                                  \
    template <>                                                                                       \
    bool SendToCSharpBaseStructArray<structType*>(const char* fieldname, structType* t, int lenParam) \
    {                                                                                                 \
        if (t == nullptr)                                                                             \
        {                                                                                             \
            SendToCSharp(fieldname, "nullptr");                                                       \
            return true;                                                                              \
        }                                                                                             \
                                                                                                      \
        switch (t[0].type)                                                                            \
        {                                                                                             \
            XR_LIST_BASE_STRUCT_TYPES_##structType(BASE_TO_TYPE_ARRAY);                               \
        case XR_TYPE_UNKNOWN:                                                                         \
        default:                                                                                      \
            SendToCSharp(fieldname, "<Unknown>");                                                     \
            break;                                                                                    \
        }                                                                                             \
        return true;                                                                                  \
    }

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_BASE_STRUCT_ARRAY)

#define BASE_TO_TYPE_ARRAY_PTR(typeName, typeType)                  \
    case typeType:                                                  \
    {                                                               \
        typeName** realTypeArray = reinterpret_cast<typeName**>(t); \
        SendToCSharp(fieldname, realTypeArray[i]);                  \
    }                                                               \
    break;

#define SEND_TO_CSHARP_BASE_STRUCT_ARRAY_PTR(structType)                                                \
    template <>                                                                                         \
    bool SendToCSharpBaseStructArray<structType**>(const char* fieldname, structType** t, int lenParam) \
    {                                                                                                   \
        if (t == nullptr || t[0] == nullptr)                                                            \
        {                                                                                               \
            SendToCSharp(fieldname, "nullptr");                                                         \
            return true;                                                                                \
        }                                                                                               \
                                                                                                        \
        for (int i = 0; i < lenParam; ++i)                                                              \
        {                                                                                               \
            switch (t[i]->type)                                                                         \
            {                                                                                           \
                XR_LIST_BASE_STRUCT_TYPES_##structType(BASE_TO_TYPE_ARRAY_PTR);                         \
            case XR_TYPE_UNKNOWN:                                                                       \
            default:                                                                                    \
                SendToCSharp(fieldname, "<Unknown>");                                                   \
                break;                                                                                  \
            }                                                                                           \
        }                                                                                               \
        return true;                                                                                    \
    }

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_BASE_STRUCT_ARRAY_PTR)

#define BASE_TO_TYPE_ARRAY_CONST_PTR(typeName, typeType)                                    \
    case typeType:                                                                          \
    {                                                                                       \
        typeName const* const* realTypeArray = reinterpret_cast<typeName const* const*>(t); \
        SendToCSharp(fieldname, realTypeArray[i]);                                          \
    }                                                                                       \
    break;

#define SEND_TO_CSHARP_BASE_STRUCT_ARRAY_CONST_PTR(structType)                                                                  \
    template <>                                                                                                                 \
    bool SendToCSharpBaseStructArray<structType const* const*>(const char* fieldname, structType const* const* t, int lenParam) \
    {                                                                                                                           \
        if (t == nullptr || t[0] == nullptr)                                                                                    \
        {                                                                                                                       \
            SendToCSharp(fieldname, "nullptr");                                                                                 \
            return true;                                                                                                        \
        }                                                                                                                       \
                                                                                                                                \
        for (int i = 0; i < lenParam; ++i)                                                                                      \
        {                                                                                                                       \
            switch (t[i]->type)                                                                                                 \
            {                                                                                                                   \
                XR_LIST_BASE_STRUCT_TYPES_##structType(BASE_TO_TYPE_ARRAY_CONST_PTR);                                           \
            case XR_TYPE_UNKNOWN:                                                                                               \
            default:                                                                                                            \
                SendToCSharp(fieldname, "<Unknown>");                                                                           \
                break;                                                                                                          \
            }                                                                                                                   \
        }                                                                                                                       \
        return true;                                                                                                            \
    }

XR_LIST_BASE_STRUCTS(SEND_TO_CSHARP_BASE_STRUCT_ARRAY_CONST_PTR)
