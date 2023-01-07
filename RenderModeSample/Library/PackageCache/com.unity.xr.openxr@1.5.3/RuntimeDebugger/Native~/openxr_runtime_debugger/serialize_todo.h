#pragma once

template <>
void SendToCSharp<>(const char* fieldname, XrEventDataBuffer* t)
{
    XrEventDataBaseHeader* evt = reinterpret_cast<XrEventDataBaseHeader*>(t);
    SendToCSharp("varying", evt);
}

template <>
void SendToCSharp<>(const char* fieldname, XrDebugUtilsMessengerCreateInfoEXT const* t)
{
    SendToCSharp(fieldname, "<TODO: XrDebugUtilsMessengerCreateInfoEXT>");
}

template <>
void SendToCSharp<>(const char* fieldname, XrSpatialGraphNodeSpaceCreateInfoMSFT const* t)
{
    SendToCSharp(fieldname, "<TODO: XrSpatialGraphNodeSpaceCreateInfoMSFT>");
}

template <>
void SendToCSharp<>(const char* fieldname, XrInteractionProfileAnalogThresholdVALVE* t)
{
    SendToCSharp(fieldname, "<TODO: XrInteractionProfileAnalogThresholdVALVE>");
}
