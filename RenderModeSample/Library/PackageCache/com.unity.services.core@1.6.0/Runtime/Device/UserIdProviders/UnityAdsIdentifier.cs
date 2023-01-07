namespace Unity.Services.Core.Device
{
    class UnityAdsIdentifier : IUserIdentifierProvider
    {
#if UNITY_ANDROID
        const string k_AndroidSettingsFile = "unityads-installinfo";
#endif
#if UNITY_ANDROID || UNITY_IOS
        const string k_IdfiKey = "unityads-idfi";
#endif

        public string UserId
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return IdentifierForInstallAndroid;
#elif UNITY_IOS && !UNITY_EDITOR
                return IdentifierForInstallIos;
#else
                return null;
#endif
            }
            set
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                IdentifierForInstallAndroid = value;
#elif UNITY_IOS && !UNITY_EDITOR
                IdentifierForInstallIos = value;
#endif
            }
        }

#if UNITY_ANDROID
        static string IdentifierForInstallAndroid
        {
            get => AndroidUtils.SharedPreferencesGetString(k_AndroidSettingsFile, k_IdfiKey);
            set => AndroidUtils.SharedPreferencesPutString(k_AndroidSettingsFile, k_IdfiKey, value);
        }
#endif
#if UNITY_IOS
        static string IdentifierForInstallIos
        {
            get => NSUserDefaults.GetString(k_IdfiKey);
            set => NSUserDefaults.SetString(k_IdfiKey, value);
        }
#endif
    }
}
