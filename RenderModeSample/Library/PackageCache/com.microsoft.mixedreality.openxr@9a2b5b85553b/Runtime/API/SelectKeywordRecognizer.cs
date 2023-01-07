// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR || UNITY_STANDALONE || WINDOWS_UWP
using System;
using UnityEngine.Windows.Speech;
using static UnityEngine.Windows.Speech.PhraseRecognizer;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// A keyword recognizer listening to the "select" keyword localized in the system display language of HoloLens 2.
    /// </summary>
    /// <remarks>
    /// <para>This class is only required by HoloLens 2 as the OS filters out the "select" keyword and thus
    /// the Unity <see cref="KeywordRecognizer"/> does not fire events when the word is heard.</para>
    /// <para>The API surface is made mostly identical to the Unity <see cref="KeywordRecognizer"/> for ease of use.</para>
    /// <para>Like the Unity <see cref="KeywordRecognizer"/>, this class is only available under the UNITY_EDITOR || UNITY_STANDALONE || WINDOWS_UWP flags.
    /// We recommend checking for those flags in the code using #if before referencing this class, especially when developing a cross-platform application.</para>
    /// </remarks>
    public sealed class SelectKeywordRecognizer : IDisposable
    {

        /// <summary>
        /// Create a new SelectKeywordRecognizer.
        /// </summary>
        /// <remarks>
        /// <para>Use <see cref="IsSupported"/> to check whether the recognizer is supported by the current platform / Unity version first before calling the constructor.
        /// The constructor does the same check and will throw an exception if not supported.</para>
        /// </remarks>
        public SelectKeywordRecognizer()
        {
            m_provider = new SelectKeywordRecognizerProvider();
        }

        /// <summary>
        /// Check whether the recognizer is supported by the current platform / Unity version
        /// </summary>
        public static bool IsSupported => SelectKeywordRecognizerProvider.IsSupported;

        /// <summary>
        /// Return whether the recognizer is running
        /// </summary>
        public bool IsRunning => m_provider.IsRunning;

        /// <summary>
        /// Event to be fired when the "select" keyword is recognized
        /// </summary>
        public event PhraseRecognizedDelegate OnPhraseRecognized
        {
            add
            {
                m_provider.OnPhraseRecognized += value;
            }
            remove
            {
                m_provider.OnPhraseRecognized -= value;
            }
        }

        /// <summary>
        /// Start the SelectKeywordRecognizer to listen for the select keyword
        /// </summary>
        public void Start() => m_provider.Start();

        /// <summary>
        /// Stop the SelectKeywordRecognizer from listening for the select keyword
        /// </summary>
        public void Stop() => m_provider.Stop();

        /// <summary>
        /// Dispose the resources used by SelectKeywordRecognizer
        /// </summary>
        public void Dispose() => m_provider.Dispose();

        private SelectKeywordRecognizerProvider m_provider;
    }
}
#endif // UNITY_EDITOR || UNITY_STANDALONE || WINDOWS_UWP