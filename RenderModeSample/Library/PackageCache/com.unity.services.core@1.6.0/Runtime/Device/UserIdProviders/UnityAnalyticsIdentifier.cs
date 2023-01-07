using System;
using UnityEngine;

namespace Unity.Services.Core.Device
{
    class UnityAnalyticsIdentifier : IUserIdentifierProvider
    {
        const string k_PlayerUserIdKey = "unity.cloud_userid";

        public string UserId
        {
            get => PlayerPrefs.GetString(k_PlayerUserIdKey);
            set
            {
                try
                {
                    PlayerPrefs.SetString(k_PlayerUserIdKey, value);
                    PlayerPrefs.Save();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
