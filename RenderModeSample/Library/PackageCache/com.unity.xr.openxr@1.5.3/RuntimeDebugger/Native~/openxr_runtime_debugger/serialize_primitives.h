#pragma once

template <>
void SendToCSharp<>(const char* fieldname, int32_t t)
{
    SendInt32(fieldname, t);
}

template <>
void SendToCSharp<>(const char* fieldname, int32_t* t)
{
    if (t != nullptr)
        SendInt32(fieldname, *t);
    else
        SendString(fieldname, "nullptr");
}

template <>
void SendToCSharp<>(const char* fieldname, int64_t t)
{
    SendInt64(fieldname, t);
}

template <>
void SendToCSharp<>(const char* fieldname, uint32_t t)
{
    SendUInt32(fieldname, t);
}

template <>
void SendToCSharp<>(const char* fieldname, uint32_t* t)
{
    if (t != nullptr)
        SendUInt32(fieldname, *t);
    else
        SendString(fieldname, "nullptr");
}

template <>
void SendToCSharp<>(const char* fieldname, uint64_t t)
{
    SendUInt64(fieldname, t);
}

template <>
void SendToCSharp<>(const char* fieldname, float t)
{
    SendFloat(fieldname, t);
}

template <>
void SendToCSharp<>(const char* fieldname, float* t)
{
    if (t != nullptr)
        SendFloat(fieldname, *t);
    else
        SendString(fieldname, "nullptr");
}

template <>
void SendToCSharp<>(const char* fieldname, char* t)
{
    if (t != nullptr)
        SendString(fieldname, t);
    else
        SendString(fieldname, "nullptr");
}

template <>
void SendToCSharp<>(const char* fieldname, char const* t)
{
    if (t != nullptr)
        SendString(fieldname, t);
    else
        SendString(fieldname, "nullptr");
}

#if XR_TYPE_SAFE_HANDLES
template <>
void SendToCSharp<>(const char* fieldname, XrPath t)
{
    SendXrPath(fieldname, t);
}

template <>
void SendToCSharp<>(const char* fieldname, XrPath* t)
{
    SendXrPath(fieldname, *t);
}

template <>
void SendToCSharp<>(const char* fieldname, XrAction t)
{
    SendXrAction(fieldname, t);
}

template <>
void SendToCSharp<>(const char* fieldname, XrAction* t)
{
    SendXrAction(fieldname, *t);
}

template <>
void SendToCSharp<>(const char* fieldname, XrActionSet t)
{
    SendXrActionSet(fieldname, t);
}

template <>
void SendToCSharp<>(const char* fieldname, XrActionSet* t)
{
    SendXrActionSet(fieldname, *t);
}

template <>
void SendToCSharp<>(const char* fieldname, XrSpace t)
{
    SendXrSpace(fieldname, t);
}

template <>
void SendToCSharp<>(const char* fieldname, XrSpace* t)
{
    SendXrSpace(fieldname, *t);
}
#endif