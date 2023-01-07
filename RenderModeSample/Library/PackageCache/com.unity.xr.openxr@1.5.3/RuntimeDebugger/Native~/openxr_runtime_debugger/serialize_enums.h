#pragma once

#define SEND_TO_CSHARP_ENUM_INDIVIDUAL(enumentry, enumvalue) \
    case enumvalue:                                          \
        SendToCSharp(fieldname, #enumentry);                 \
        break;

#define SEND_TO_CSHARP_ENUMS(enumname)                                                                            \
    template <>                                                                                                   \
    void SendToCSharp<>(const char* fieldname, enumname t)                                                        \
    {                                                                                                             \
        switch (t)                                                                                                \
        {                                                                                                         \
            XR_LIST_ENUM_##enumname(SEND_TO_CSHARP_ENUM_INDIVIDUAL) default : SendToCSharp(fieldname, "UNKNOWN"); \
        }                                                                                                         \
    }

XR_LIST_ENUM_TYPES(SEND_TO_CSHARP_ENUMS)

#define SEND_TO_CSHARP_ENUMS_PTR(enumname)                                                                        \
    template <>                                                                                                   \
    void SendToCSharp<>(const char* fieldname, enumname* t)                                                       \
    {                                                                                                             \
        switch (*t)                                                                                               \
        {                                                                                                         \
            XR_LIST_ENUM_##enumname(SEND_TO_CSHARP_ENUM_INDIVIDUAL) default : SendToCSharp(fieldname, "UNKNOWN"); \
        }                                                                                                         \
    }

XR_LIST_ENUM_TYPES(SEND_TO_CSHARP_ENUMS_PTR)

#define XR_ENUM_CASE_STR(name, val) \
    case name:                      \
        return #name;
#define XR_ENUM_STR(enumType)                                                     \
    const char* XrEnumStr(enumType e)                                             \
    {                                                                             \
        switch (e)                                                                \
        {                                                                         \
            XR_LIST_ENUM_##enumType(XR_ENUM_CASE_STR) default : return "Unknown"; \
        }                                                                         \
    }

XR_ENUM_STR(XrResult);