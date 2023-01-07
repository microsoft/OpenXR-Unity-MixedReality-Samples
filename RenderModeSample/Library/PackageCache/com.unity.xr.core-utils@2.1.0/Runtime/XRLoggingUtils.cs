using System;
using System.Linq;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utility methods for logging. These methods will not log anything if tests are being run via command line
    /// </summary>
    public static class XRLoggingUtils
    {
        static readonly bool k_DontLogAnything;

        static XRLoggingUtils()
        {
            k_DontLogAnything = Environment.GetCommandLineArgs().Contains("-runTests");
        }

        /// <summary>
        /// Debug.Log, but will not print anything if tests are being run
        /// </summary>
        /// <param name="message">Log message for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void Log(string message, UnityEngine.Object context = null)
        {
            if(!k_DontLogAnything)
                Debug.Log(message, context);
        }

        /// <summary>
        /// Debug.LogWarning, but will not print anything if tests are being run
        /// </summary>
        /// <param name="message">Warning message for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogWarning(string message, UnityEngine.Object context = null)
        {
            if(!k_DontLogAnything)
                Debug.LogWarning(message, context);
        }

        /// <summary>
        /// Debug.LogError, but will not print anything if tests are being run
        /// </summary>
        /// <param name="message">Error message for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogError(string message, UnityEngine.Object context = null)
        {
            if(!k_DontLogAnything)
                Debug.LogError(message, context);
        }

        /// <summary>
        /// Debug.LogException, but will not print anything if tests are being run
        /// </summary>
        /// <param name="exception">Runtime Exception.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogException(Exception exception, UnityEngine.Object context = null)
        {
            if(!k_DontLogAnything)
                Debug.LogException(exception, context);
        }
    }
}
