#if UNITY_IOS
using System;
using System.Runtime.InteropServices;

namespace Unity.Services.Core.Device
{
    class NSUserDefaults
    {
        public static string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            return UserDefaultsGetString(key);
        }

        public static void SetString(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            UserDefaultsSetString(key, value);
        }

        [DllImport("__Internal", EntryPoint = "UOCPUserDefaultsGetString")]
        static extern string UserDefaultsGetString(string key);

        [DllImport("__Internal", EntryPoint = "UOCPUserDefaultsSetString")]
        static extern void UserDefaultsSetString(string key, string value);
    }
}
#endif
