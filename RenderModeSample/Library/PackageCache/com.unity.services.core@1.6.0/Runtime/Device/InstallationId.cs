using System;
using Unity.Services.Core.Device.Internal;
using UnityEngine;

namespace Unity.Services.Core.Device
{
    class InstallationId : IInstallationId
    {
        const string k_UnityInstallationIdKey = "UnityInstallationId";

        internal string Identifier;

        internal IUserIdentifierProvider UnityAdsIdentifierProvider;
        internal IUserIdentifierProvider UnityAnalyticsIdentifierProvider;

        public InstallationId()
        {
            UnityAdsIdentifierProvider = new UnityAdsIdentifier();
            UnityAnalyticsIdentifierProvider = new UnityAnalyticsIdentifier();
        }

        public string GetOrCreateIdentifier()
        {
            if (string.IsNullOrEmpty(Identifier))
                CreateIdentifier();

            return Identifier;
        }

        public void CreateIdentifier()
        {
            Identifier = ReadIdentifierFromFile();
            if (!string.IsNullOrEmpty(Identifier))
                return;

            var analyticsId = UnityAnalyticsIdentifierProvider.UserId;
            var adsId = UnityAdsIdentifierProvider.UserId;

            if (!string.IsNullOrEmpty(analyticsId))
            {
                Identifier = analyticsId;
            }
            else if (!string.IsNullOrEmpty(adsId))
            {
                Identifier = adsId;
            }
            else
            {
                Identifier = GenerateGuid();
            }

            WriteIdentifierToFile(Identifier);

            if (string.IsNullOrEmpty(analyticsId))
            {
                UnityAnalyticsIdentifierProvider.UserId = Identifier;
            }

            if (string.IsNullOrEmpty(adsId))
            {
                UnityAdsIdentifierProvider.UserId = Identifier;
            }
        }

        static string ReadIdentifierFromFile()
        {
            return PlayerPrefs.GetString(k_UnityInstallationIdKey);
        }

        static void WriteIdentifierToFile(string identifier)
        {
            PlayerPrefs.SetString(k_UnityInstallationIdKey, identifier);
            PlayerPrefs.Save();
        }

        static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
