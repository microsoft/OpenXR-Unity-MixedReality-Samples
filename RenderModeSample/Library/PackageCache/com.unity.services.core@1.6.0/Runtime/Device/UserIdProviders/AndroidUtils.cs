#if UNITY_ANDROID
using System;
using UnityEngine;

namespace Unity.Services.Core.Device
{
    static class AndroidUtils
    {
        const int k_ContextModePrivate = 0x0000;

        public static AndroidJavaObject GetUnityActivity()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        public static AndroidJavaObject GetSharedPreferences(AndroidJavaObject context, string name, int mode = k_ContextModePrivate)
        {
            return context.Call<AndroidJavaObject>("getSharedPreferences", name, mode);
        }

        public static AndroidJavaObject GetSharedPreferences(string name, int mode = k_ContextModePrivate)
        {
            using (var activity = GetUnityActivity())
            {
                return GetSharedPreferences(activity, name, mode);
            }
        }

        public static string SharedPreferencesGetString(string name, string key, string defValue = "")
        {
            using (var preferences = GetSharedPreferences(name))
            {
                return SharedPreferencesGetString(preferences, key, defValue);
            }
        }

        public static string SharedPreferencesGetString(AndroidJavaObject preferences, string key, string defValue = "")
        {
            if (preferences == null)
                return defValue;
            if (!preferences.Call<bool>("contains", key))
                return defValue;

            try
            {
                return preferences.Call<string>("getString", key, defValue);
            }
            // throws ClassCastException
            catch (Exception)
            {
                return defValue;
            }
        }

        public static void SharedPreferencesPutString(string name, string key, string value)
        {
            using (var preferences = GetSharedPreferences(name))
            {
                SharedPreferencesPutString(preferences, key, value);
            }
        }

        public static void SharedPreferencesPutString(AndroidJavaObject preferences, string key, string value)
        {
            if (preferences == null)
                return;

            var editor = preferences.Call<AndroidJavaObject>("edit");
            editor.Call<AndroidJavaObject>("putString", key, value);
            editor.Call<bool>("commit");
        }
    }
}
#endif
