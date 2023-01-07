#pragma once

template <class TOUT, class TIN>
TOUT* FindNextPointerType(TIN* typeIn, XrStructureType structureType)
{
    auto* baseStruct = reinterpret_cast<XrBaseOutStructure*>(typeIn);

    // Loop over the next pointers and look for structureType
    while ((baseStruct = static_cast<XrBaseOutStructure*>(baseStruct->next)) != nullptr &&
        baseStruct->type != structureType)
    {
    }

    // If we found it, cast to out type.  If not we'll return nullptr.
    return reinterpret_cast<TOUT*>(baseStruct);
}

template <class TOUT, class TIN>
const TOUT* FindNextPointerType(const TIN* typeIn, XrStructureType structureType)
{
    auto* baseStruct = reinterpret_cast<const XrBaseOutStructure*>(typeIn);

    // Loop over the next pointers and look for structureType
    while ((baseStruct = static_cast<const XrBaseOutStructure*>(baseStruct->next)) != nullptr &&
        baseStruct->type != structureType)
    {
    }

    // If we found it, cast to out type.  If not we'll return nullptr.
    return reinterpret_cast<const TOUT*>(baseStruct);
}