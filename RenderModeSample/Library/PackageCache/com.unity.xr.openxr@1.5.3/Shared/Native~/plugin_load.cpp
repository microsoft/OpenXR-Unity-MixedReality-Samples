#include "plugin_load.h"
#include <cstdlib>
#include <cstring>
#include <string>

#if defined(XR_USE_PLATFORM_WIN32) && !defined(XR_USE_PLATFORM_UWP)

PluginHandle Plugin_LoadLibrary(const wchar_t* libName)
{
    std::wstring lib(libName);
    if ((lib.size() >= 1 && lib[0] == L'.') ||
        lib.find(L'/') == std::string::npos && lib.find(L'\\') == std::string::npos)
    {
        // Look up path of current dll
        wchar_t path[MAX_PATH];
        HMODULE hm = NULL;
        if (GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS |
                    GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                (LPCWSTR)&Plugin_LoadLibrary, &hm) == 0)
        {
            int ret = GetLastError();
            fprintf(stderr, "GetModuleHandle failed, error = %d\n", ret);
            return NULL;
        }
        if (GetModuleFileNameW(hm, path, MAX_PATH) == 0)
        {
            int ret = GetLastError();
            fprintf(stderr, "GetModuleFileNameW failed, error = %d\n", ret);
            return NULL;
        }

        std::wstring basePath(path);
        basePath = basePath.substr(0, basePath.find_last_of(L'\\') + 1);

        lib = basePath + lib + L".dll";
    }

    HMODULE handle = LoadLibraryW(lib.c_str());
    if (handle == NULL)
    {
        int ret = GetLastError();
        fprintf(stderr, "LoadLibraryW failed, error = %d\n", ret);
    }
    return handle;
}

void Plugin_FreeLibrary(PluginHandle handle)
{
    FreeLibrary(handle);
}

PluginFunc Plugin_GetSymbol(PluginHandle handle, const char* symbol)
{
    return GetProcAddress(handle, symbol);
}

#elif defined(XR_USE_PLATFORM_UWP)

PluginHandle Plugin_LoadLibrary(const wchar_t* libName)
{
    if (libName == NULL)
        return NULL;
    HMODULE handle = LoadPackagedLibrary(libName, 0);
    if (handle == NULL)
    {
        int ret = GetLastError();
        fprintf(stderr, "LoadPackagedLibrary failed, error = %d\n", ret);
    }
    return handle;
}

void Plugin_FreeLibrary(PluginHandle handle)
{
    FreeLibrary(handle);
}

PluginFunc Plugin_GetSymbol(PluginHandle handle, const char* symbol)
{
    return GetProcAddress(handle, symbol);
}

#else // Posix

PluginHandle Plugin_LoadLibrary(const wchar_t* libName)
{
    std::wstring lib(libName);
    std::string mbLibName;

    if ((lib.size() >= 1 && lib[0] == L'.') ||
        (lib.find(L'/') == std::string::npos && lib.find(L'\\') == std::string::npos))
    {
        size_t len = std::wcstombs(nullptr, lib.c_str(), lib.size());
        if (len <= 0)
            return NULL;
        mbLibName.resize(len);
        std::wcstombs(&mbLibName[0], lib.c_str(), lib.size());

        Dl_info info;
        if (dladdr((const void*)&Plugin_LoadLibrary, &info) != 0)
        {
            std::string basePath(info.dli_fname);
            basePath = basePath.substr(0, basePath.find_last_of('/') + 1);

#if !defined(XR_USE_PLATFORM_OSX)
            if (mbLibName[0] != '.')
                mbLibName = basePath + "lib" + mbLibName;
            else
#endif
                mbLibName = basePath + mbLibName;
#if defined(XR_USE_PLATFORM_OSX)
            mbLibName += ".dylib";
#else
            mbLibName += ".so";
#endif
        }
    }
    if (mbLibName.size() <= 0)
        return NULL;

    auto ret = dlopen(mbLibName.c_str(), RTLD_LAZY);
    return ret;
}

void Plugin_FreeLibrary(PluginHandle handle)
{
    dlclose(handle);
}

PluginFunc Plugin_GetSymbol(PluginHandle handle, const char* symbol)
{
    return dlsym(handle, symbol);
}
#endif
