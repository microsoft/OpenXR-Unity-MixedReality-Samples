#pragma once

//
// Platform Headers
//
#ifdef XR_USE_PLATFORM_WIN32
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // !WIN32_LEAN_AND_MEAN

#ifndef NOMINMAX
#define NOMINMAX
#endif // !NOMINMAX

#include <windows.h>
#endif

#if defined(XR_USE_PLATFORM_WIN32) && !defined(XR_USE_PLATFORM_UWP)
#include "errhandlingapi.h"
#include "libloaderapi.h"

typedef HMODULE PluginHandle;
typedef FARPROC PluginFunc;

#elif defined(XR_USE_PLATFORM_UWP)

#include "errhandlingapi.h"

typedef HMODULE PluginHandle;
typedef FARPROC PluginFunc;

#else // Posix

#include <dlfcn.h>

typedef void* PluginHandle;
typedef void* PluginFunc;

#endif

PluginHandle Plugin_LoadLibrary(const wchar_t* libName);
void Plugin_FreeLibrary(PluginHandle handle);
PluginFunc Plugin_GetSymbol(PluginHandle handle, const char* symbol);
