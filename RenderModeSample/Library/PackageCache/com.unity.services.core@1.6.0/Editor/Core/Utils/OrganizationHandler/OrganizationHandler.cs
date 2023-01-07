using System;
using UnityEditor;
using System.Reflection;
using UnityEngine;

namespace Unity.Services.Core.Editor.OrganizationHandler
{
    class OrganizationHandler : IOrganizationHandler
    {
        public string Key
        {
            get
            {
#if ENABLE_EDITOR_GAME_SERVICES
                return CloudProjectSettings.organizationKey;
#else
                return GetFallbackOrgKey();
#endif
            }
        }

        internal string GetFallbackOrgKey()
        {
            Type typeConnect = typeof(CloudProjectSettings).Assembly.GetType("UnityEditor.Connect.UnityConnect");
            var connectInstance = typeConnect.GetRuntimeProperty("instance").GetValue(null);
            var getOrganizationForeignKey = typeConnect.GetMethod("GetOrganizationForeignKey");
            string orgKey = "";
            if (getOrganizationForeignKey != null)
            {
                orgKey = (string)getOrganizationForeignKey.Invoke(connectInstance, new object[] {});
            }
            return orgKey;
        }
    }
}
