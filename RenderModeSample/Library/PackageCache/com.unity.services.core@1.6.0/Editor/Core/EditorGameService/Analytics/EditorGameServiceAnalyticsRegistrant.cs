#if !UNITY_2022_1_OR_NEWER
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.Analytics;

namespace Unity.Services.Core.Editor
{
    [InitializeOnLoad]
    class EditorGameServiceAnalyticsRegistrant: IEditorAnalyticsEventRegistrant
    {
        static EditorGameServiceAnalyticsRegistrant()
        {
            RegisterEvent();
        }

        static void RegisterEvent()
        {
            var registrant = new EditorGameServiceAnalyticsRegistrant();
            _ = registrant.RegisterEditorAnalyticsEventAsync();
        }

        /// <summary>
        /// Registers the "editorgameserviceeditor" editor analytics event.
        /// Uses a pessimistic approach, will not register the event if we can't ensure the validity of the data.
        /// </summary>
        /// <returns>AnalyticsResult</returns>
        public async Task<AnalyticsResult> RegisterEditorAnalyticsEventAsync()
        {
            if(await WaitForEditorAnalyticsEnablement())
            {
                var editorAnalytics = GetEditorAnalyticsByReflection();
                var registerEventWithLimitMethod = GetEditorAnalyticsPrivateRegisterEventWithLimitMethod(editorAnalytics);
                return RegisterEditorGameServiceEvent(editorAnalytics, registerEventWithLimitMethod);
            }
            return AnalyticsResult.AnalyticsDisabled;
        }

        internal static Type GetEditorAnalyticsByReflection()
        {
            return Type.GetType("UnityEditor.EditorAnalytics,UnityEditor.dll");
        }

        internal static MethodInfo GetEditorAnalyticsPrivateRegisterEventWithLimitMethod(Type editorAnalytics)
        {
            if (editorAnalytics != null)
            {
                var methods = editorAnalytics.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                return methods.FirstOrDefault(info => {
                        var methodParams = info.GetParameters();
                        return methodParams.Length == 7
                            && ParameterIsOfExpectedNameAndType(methodParams[0], "eventName", typeof(string))
                            && ParameterIsOfExpectedNameAndType(methodParams[1], "maxEventPerHour", typeof(int))
                            && ParameterIsOfExpectedNameAndType(methodParams[2], "maxItems", typeof(int))
                            && ParameterIsOfExpectedNameAndType(methodParams[3], "vendorKey", typeof(string))
                            && ParameterIsOfExpectedNameAndType(methodParams[4], "ver", typeof(int))
                            && ParameterIsOfExpectedNameAndType(methodParams[5], "prefix", typeof(string))
                            && ParameterIsOfExpectedNameAndType(methodParams[6], "assembly", typeof(Assembly));
                });
            }
            return null;
        }

        static bool ParameterIsOfExpectedNameAndType(ParameterInfo parameterInfo, string parameterExpectedName, Type parameterExpectedType)
        {
            return parameterInfo.Name == parameterExpectedName && parameterInfo.ParameterType == parameterExpectedType;
        }

        static AnalyticsResult RegisterEditorGameServiceEvent(Type editorAnalytics, MethodInfo registerEventWithLimitMethod)
        {
            if (editorAnalytics != null && registerEventWithLimitMethod != null)
            {
                const string eventName = "editorgameserviceeditor";
                const int maxEventsPerHour = 100;
                const int maxItems = 10;
                const string vendorKey = "unity.services.core.editor";
                const int version = 1;
                const string prefix = "";

                var paramsArray = new object[]
                    {eventName, maxEventsPerHour, maxItems, vendorKey, version, prefix, editorAnalytics.Assembly};
                return (AnalyticsResult)registerEventWithLimitMethod.Invoke(null, paramsArray);
            }
            return AnalyticsResult.AnalyticsDisabled;
        }

        static async Task<bool> WaitForEditorAnalyticsEnablement()
        {
            var attempts = 0;
            while (!EditorAnalytics.enabled)
            {
                await Task.Delay(100);
                attempts++;
                if (attempts > 100)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
#endif
