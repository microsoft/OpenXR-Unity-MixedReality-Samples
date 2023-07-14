// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    /// <summary>
    /// Prints messages logged through the Unity Console onto a text mesh.
    /// </summary>
    public class DisplayUnityLogs : MonoBehaviour, ITextProvider
    {
        /// <summary>
        /// The maximum number of lines to be stored for displaying on the text mesh.
        /// If this number is exceeded, old messages will be removed.
        /// </summary>
        public int MaxLines = 20;

        private object m_lock = new object();
        private static string m_defaultString = "Unity Logs ...";
        private string m_logs = m_defaultString;
        private Queue<string> m_lines = new Queue<string>();

        void OnEnable() => Application.logMessageReceived += LogUnityMessage;
        void OnDisable() => Application.logMessageReceived -= LogUnityMessage;

        /// <summary>
        /// Add the string to a TextMesh and optionally log it to the Unity console.
        /// </summary>
        /// <param name="line">The text to log.</param>
        /// <param name="debugLog">Whether to log to the Unity console.</param>
        public void LogUnityMessage(string message, string stackTrace, LogType type)
        {
            lock (m_lock)
            {
                while (m_lines.Count >= MaxLines)
                {
                    m_lines.Dequeue();
                }
                m_lines.Enqueue(message + "\n");

                m_logs = m_lines.Aggregate(new StringBuilder(),
                    (sb, logged) => sb.Append(logged), sb => sb.ToString());

                if (string.IsNullOrWhiteSpace(m_logs))
                {
                    m_logs = m_defaultString;
                }
            }
        }

        string ITextProvider.UpdateText()
        {
            lock (m_lock)
            {
                return m_logs;
            }
        }
    }
}
