#include "../mock.h"

#if defined(XR_USE_PLATFORM_WIN32)

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrConvertWin32PerformanceCounterToTimeKHR(XrInstance instance, const LARGE_INTEGER* performanceCounter, XrTime* time)
{
    LOG_FUNC();
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrConvertTimeToWin32PerformanceCounterKHR(XrInstance instance, XrTime time, LARGE_INTEGER* performanceCounter)
{
    LOG_FUNC();
    return XR_SUCCESS;
}

XrResult MockWin32ConvertPerformanceCounterTime_GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function)
{
    GET_PROC_ADDRESS(xrConvertWin32PerformanceCounterToTimeKHR)
    GET_PROC_ADDRESS(xrConvertTimeToWin32PerformanceCounterKHR)
    return XR_ERROR_FUNCTION_UNSUPPORTED;
}

#endif
