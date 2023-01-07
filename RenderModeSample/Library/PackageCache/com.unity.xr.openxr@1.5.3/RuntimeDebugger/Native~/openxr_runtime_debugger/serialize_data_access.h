#pragma once

#include <sstream>

extern "C" void UNITY_INTERFACE_EXPORT StartDataAccess()
{
    s_DataMutex.lock();
}

extern "C" bool UNITY_INTERFACE_EXPORT GetDataForRead(uint8_t** ptr, uint32_t* size)
{
    s_MainDataStore.SetOverflowMode(RingBuf::kOverflowModeWrap);
    return s_MainDataStore.GetForReadAndClear(ptr, size);
}

extern "C" bool UNITY_INTERFACE_EXPORT GetLUTData(uint8_t** ptr, uint32_t* size, uint32_t offset)
{
    bool ret = s_LUTDataStore.GetForRead(ptr, size);
    if (*size <= offset)
    {
        *ptr = nullptr;
        *size = 0;
        return false;
    }
    *ptr = *ptr + offset;
    *size = *size - offset;
    s_LUTDataStore.DropLastBlock();
    return ret;
}

extern "C" void UNITY_INTERFACE_EXPORT EndDataAccess()
{
    s_MainDataStore.Reset();
    s_DataMutex.unlock();
}
