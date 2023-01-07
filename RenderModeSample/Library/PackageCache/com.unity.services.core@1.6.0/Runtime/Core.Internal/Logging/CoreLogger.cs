using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Services.Core.Internal
{
    static class CoreLogger
    {
        internal const string Tag = "[ServicesCore]";
        internal const string VerboseLoggingDefine = "ENABLE_UNITY_SERVICES_CORE_VERBOSE_LOGGING";
        const string k_TelemetryLoggingDefine = "ENABLE_UNITY_SERVICES_CORE_TELEMETRY_LOGGING";

        public static void Log(object message) => Debug.unityLogger.Log(Tag, message);
        public static void LogWarning(object message) => Debug.unityLogger.LogWarning(Tag, message);
        public static void LogError(object message) => Debug.unityLogger.LogError(Tag, message);

        public static void LogException(Exception exception) =>
            Debug.unityLogger.Log(LogType.Exception, Tag, exception);

        [Conditional("UNITY_ASSERTIONS")]
        public static void LogAssertion(object message) => Debug.unityLogger.Log(LogType.Assert, Tag, message);

#if !ENABLE_UNITY_SERVICES_VERBOSE_LOGGING
        [Conditional(VerboseLoggingDefine)]
#endif
        public static void LogVerbose(object message) => Debug.unityLogger.Log(Tag, message);

#if !ENABLE_UNITY_SERVICES_TELEMETRY_LOGGING
        [Conditional(k_TelemetryLoggingDefine)]
#endif
        public static void LogTelemetry(object message) => Debug.unityLogger.Log(Tag, message);
    }
}
