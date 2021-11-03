// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Prints messages logged through the Unity Console onto a text mesh.
    /// </summary>
    public class LoggingPanel : MonoBehaviour
    {
        [SerializeField]
        private TextMesh m_text = null;

        /// <summary>
        /// The maximum number of lines to be stored for displaying on the text mesh.
        /// If this number is exceeded, old messages will be removed.
        /// </summary>
        public int MaxLines = 8;

        private Queue<string> m_loggedLines = new Queue<string>();

        void OnEnable() => Application.logMessageReceived += LogUnityMessage;
        void OnDisable() => Application.logMessageReceived -= LogUnityMessage;

        /// <summary>
        /// Add the string to a TextMesh and optionally log it to the Unity console.
        /// </summary>
        /// <param name="line">The text to log.</param>
        /// <param name="debugLog">Whether to log to the Unity console.</param>
        public void LogUnityMessage(string message, string stackTrace, LogType type)
        {
            while (m_loggedLines.Count >= MaxLines)
            {
                m_loggedLines.Dequeue();
            }
            m_loggedLines.Enqueue(message + "\n");

            if (m_text != null)
            {
                m_text.text = m_loggedLines.Aggregate(new StringBuilder(),
                    (sb, logged) => sb.Append(logged), sb => sb.ToString());
            }
        }
    }
}
