// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class LoggingPanel : MonoBehaviour
    {
        [SerializeField]
        private TextMesh m_text = null;

        public int MaxLines = 8;

        private Queue<string> m_loggedLines = new Queue<string>();

        /// <summary>
        /// Add the string to a TextMesh and optionally log it to the Unity console.
        /// </summary>
        /// <param name="line">The text to log.</param>
        /// <param name="debugLog">Whether to log to the Unity console.</param>
        public void LogText(string line, bool debugLog = true)
        {
            if (debugLog)
            {
                Debug.Log($"[Sample Logging Panel] {line}");
            }

            while (m_loggedLines.Count >= MaxLines)
            {
                m_loggedLines.Dequeue();
            }
            m_loggedLines.Enqueue(line + "\n");

            if (m_text != null)
            {
                m_text.text = m_loggedLines.Aggregate(new StringBuilder(),
                    (sb, logged) => sb.Append(logged), sb => sb.ToString());
            }
        }

        // Pass Unity's logging messages onto our TextMesh as well
        void OnEnable() => Application.logMessageReceived += LogUnityMessage;
        void OnDisable() => Application.logMessageReceived -= LogUnityMessage;
        public void LogUnityMessage(string message, string stackTrace, LogType type)
        {
            if (message.Contains("D3D11") || message.Contains("[Sample Logging Panel]")) return;
            // Since these messages are coming from the Unity log, they're already in the console. No need to log again.
            LogText(message, false);
        }
    }
}
