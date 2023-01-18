#include <iostream>
#include <string>
#include <exception>
#include <windows.h>


void SetStringRegkey(HKEY hKey, const wchar_t* subKey, const wchar_t* valueName, const wchar_t* value) {
    HKEY hkHive{};
    LSTATUS error = ::RegOpenKeyEx(hKey, subKey, 0, KEY_WRITE | KEY_WOW64_64KEY, &hkHive);
    if (error == ERROR_FILE_NOT_FOUND) {
        error = RegCreateKeyEx(hKey, subKey, 0, nullptr, 0, KEY_WRITE | KEY_WOW64_64KEY, nullptr, &hkHive, nullptr);
        if (error != ERROR_SUCCESS) {
            std::cout << "SetStringRegkey_RegCreateKey" << HRESULT_FROM_WIN32(error);
        }
    }

    if (error != ERROR_SUCCESS) {
        std::cout << "SetStringRegkey_RegOpenKey" << HRESULT_FROM_WIN32(error);
    }

    const wchar_t* string = value == nullptr ? L"\0" : value;
    const uint8_t* bytes = static_cast<const uint8_t*>((void*)string);
    const uint32_t byteSize = (uint32_t)(wcslen(string) + 1) * sizeof(wchar_t);
    error = ::RegSetValueEx(hkHive, valueName, 0, REG_SZ, bytes, byteSize);

    if (error != ERROR_SUCCESS) {
        std::cout << "SetStringRegkey_RegSetValueEx" << HRESULT_FROM_WIN32(error);
    }
}

int main() {

    const HKEY s_detourKey = HKEY_CURRENT_USER;
    const wchar_t* s_detourSubKey = L"SOFTWARE\\Microsoft\\OpenXR\\Remoting\\";
    const wchar_t* s_detourValue = L"DetourRuntime";

    std::cout << "DebugHKCURegistryWrite" << " Calling SetStringRegKey \n";
    SetStringRegkey(s_detourKey, s_detourSubKey, L"ActiveRuntime", s_detourValue);
    std::cout << "DebugHKCURegistryWrite" << " SetStringRegKey Successful \n";
    return 0;
}