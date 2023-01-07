using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS
using UnityEngine.Analytics;
#endif //UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS

[assembly:InternalsVisibleTo("Unity.XR.Management.Editor")]
namespace UnityEngine.XR.Management
{
    internal static class XRManagementAnalytics
    {
        private const int kMaxEventsPerHour = 1000;
        private const int kMaxNumberOfElements = 1000;
        private const string kVendorKey = "unity.xrmanagement";
        private const string kEventBuild = "xrmanagment_build";

#if ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS
        private static bool s_Initialized = false;
#endif //ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS

        [Serializable]
        private struct BuildEvent
        {
            public string buildGuid;
            public string buildTarget;
            public string buildTargetGroup;
            public string[] assigned_loaders;
        }

        private static bool Initialize()
        {
#if ENABLE_TEST_SUPPORT || !ENABLE_CLOUD_SERVICES_ANALYTICS || !UNITY_ANALYTICS
            return false;
#else

#if UNITY_EDITOR
            if (!EditorAnalytics.enabled)
                return false;

            if(AnalyticsResult.Ok != EditorAnalytics.RegisterEventWithLimit(kEventBuild, kMaxEventsPerHour, kMaxNumberOfElements, kVendorKey))
                return false;
            s_Initialized = true;
#endif //UNITY_EDITOR
            return s_Initialized;
#endif //ENABLE_TEST_SUPPORT || !ENABLE_CLOUD_SERVICES_ANALYTICS || !UNITY_ANALYTICS

        }

#if UNITY_EDITOR
        public static void SendBuildEvent(GUID guid, BuildTarget buildTarget, BuildTargetGroup buildTargetGroup, IEnumerable<XRLoader> loaders)
        {

#if UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS

            if (!s_Initialized && !Initialize())
            {
                return;
            }

            List<string> loaderTypeNames = new List<string>();
            foreach (var loader in loaders)
            {
                loaderTypeNames.Add(loader.GetType().Name);
            }

            var data = new BuildEvent
            {
                buildGuid = guid.ToString(),
                buildTarget = buildTarget.ToString(),
                buildTargetGroup = buildTargetGroup.ToString(),
                assigned_loaders = loaderTypeNames.ToArray(),
            };

            EditorAnalytics.SendEventWithLimit(kEventBuild, data);

#endif //UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS

        }
#endif //UNITY_EDITOR
    }
}
