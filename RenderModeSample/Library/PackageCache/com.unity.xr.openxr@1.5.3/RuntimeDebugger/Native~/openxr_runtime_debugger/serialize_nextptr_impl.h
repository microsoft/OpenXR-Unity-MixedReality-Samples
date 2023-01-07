#pragma once

#define SEND_NEXT_PTR(structname, structtype)                      \
    case structtype:                                               \
        SendToCSharp("next", reinterpret_cast<structname*>(next)); \
        break;

template <>
void SendToCSharp<>(const char* fieldname, XrBaseOutStructure* t)
{
    auto* next = t;
    do
    {
        switch (next->type)
        {
            XR_LIST_STRUCTURE_TYPES(SEND_NEXT_PTR)
        default:
            SendToCSharp("next", next->type);
            continue;
        }
    } while ((next = static_cast<XrBaseOutStructure*>(next->next)) != nullptr);
}

#define SEND_NEXT_PTR_CONST(structname, structtype)                      \
    case structtype:                                                     \
        SendToCSharp("next", reinterpret_cast<structname const*>(next)); \
        break;

template <>
void SendToCSharp<>(const char* fieldname, XrBaseInStructure const* t)
{
    auto* next = t;
    do
    {
        switch (next->type)
        {
            XR_LIST_STRUCTURE_TYPES(SEND_NEXT_PTR_CONST)
        default:
            SendToCSharp("next", next->type);
            continue;
        }
    } while ((next = static_cast<XrBaseInStructure const*>(next->next)) != nullptr);
}
