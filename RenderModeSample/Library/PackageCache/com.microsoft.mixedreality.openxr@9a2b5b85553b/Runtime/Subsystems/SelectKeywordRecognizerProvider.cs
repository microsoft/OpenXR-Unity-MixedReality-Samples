// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if UNITY_EDITOR || UNITY_STANDALONE || WINDOWS_UWP
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Windows.Speech;
using static UnityEngine.Windows.Speech.PhraseRecognizer;
using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.System.UserProfile;
using Windows.UI.Input.Spatial;
#endif // WINDOWS_UWP

namespace Microsoft.MixedReality.OpenXR
{
    internal class SelectKeywordRecognizerProvider
    {
        internal SelectKeywordRecognizerProvider()
        {
            if (!IsSupported)
            {
                if (!IsPlatformSupported)
                {
                    throw new NotSupportedException($"{nameof(SelectKeywordRecognizer)} is only supported when running the application on HoloLens 2. " +
                        $"Please use {nameof(IsSupported)} to check for support before calling the constructor.");
                }
                if (!IsSystemLanguageSupported)
                {
                    throw new NotSupportedException($"{nameof(SelectKeywordRecognizer)} is not supported by the current system language. " +
                        $"Please use {nameof(IsSupported)} to check for support before calling the constructor.");
                }
                if (!IsUnityVersionSupported)
                {
                    throw new NotSupportedException($"{nameof(SelectKeywordRecognizer)} is not supported by the current Unity version. " +
                        $"This is not expected and please file a bug with the Unity version number {Application.unityVersion} for us to investigate.");
                }
            }
        }

        internal static bool IsSupported => IsPlatformSupported && IsUnityVersionSupported && IsSystemLanguageSupported;

        internal bool IsRunning { get; private set; } = false;

#pragma warning disable CS0067 // Turn the "never used" warning off, as this is needed by SelectKeywordRecognizer
        internal event PhraseRecognizedDelegate OnPhraseRecognized;
#pragma warning restore CS0067

        internal void Start()
        {
#if WINDOWS_UWP
            if (IsRunning)
            {
                Debug.LogWarning($"{nameof(SelectKeywordRecognizer)} is already running when Start() is called.");
                return;
            }
            SpatialInteractionManager.SourcePressed += SpatialInteractionManager_SourcePressed;
            IsRunning = true;
#endif // WINDOWS_UWP
        }

        internal void Stop()
        {
#if WINDOWS_UWP
            if (IsRunning)
            {
                SpatialInteractionManager.SourcePressed -= SpatialInteractionManager_SourcePressed;
                IsRunning = false;
            }
            else
            {
                Debug.LogWarning($"{nameof(SelectKeywordRecognizer)} is not running when Stop() is called.");
                return;
            }
#endif // WINDOWS_UWP
        }

        internal void Dispose()
        {
#if WINDOWS_UWP
            if (IsRunning)
            {
                Stop();
            }
            m_spatialInteractionManager = null;
#endif // WINDOWS_UWP
        }

        private static bool IsPlatformSupported
        {
            get
            {
                if (!m_isPlatformSupported.HasValue)
                {
                    m_isPlatformSupported = CheckPlatformSupport();
                }
                return m_isPlatformSupported.Value;
            }
        }

        private static bool IsUnityVersionSupported => m_recogEventArgsConstructorInfo != null;

        private static bool IsSystemLanguageSupported => m_localizedSelectKeyword != null;

        private static bool CheckPlatformSupport()
        {
#if WINDOWS_UWP
            return NativeLib.IsSelectKeywordFiltered();
#else
            return false;
#endif // WINDOWS_UWP
        }

        private static PhraseRecognizedEventArgs GeneratePhraseRecognizedEventArgs()
        {
            return (PhraseRecognizedEventArgs)m_recogEventArgsConstructorInfo.Invoke(
                new object[] { m_localizedSelectKeyword, ConfidenceLevel.High, null, DateTime.Now, new TimeSpan(m_selectKeywordDurationInTicks) });
        }

        private static ConstructorInfo GetPhraseRecognizedEventArgsConstructorInfo()
        {
            // Find the internal constructor using m_recogEventArgsConstructorArgTypes, the array storing the argument types of the constructor
            return typeof(PhraseRecognizedEventArgs).GetConstructor(
                bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance, types: m_recogEventArgsConstructorArgTypes, binder: null, modifiers: null);
        }

        private static string GetLocalizedKeyword()
        {
            if (m_currentSystemLanguage == null)
            {
                return null;
            }

            // Due to the way system language is retrieved, m_currentSystemLanguage does not have a consistent format.
            // For example, for Japanese it's "ja", for Chinese Simplified it's "zh-Hans-CN", for italian it's "it-IT".
            // For almost all speech-supported language, all available flavors of the languages are speech-supported.
            // The only exception is Chinese, where "zh-Hans-CN" is speech-supported but "zh-Hant-TW" is not. This if check handles this special case.
            if (m_currentSystemLanguage == "zh-Hant-TW")
            {
                return null;
            }

            // Only check for the first two letters as m_currentSystemLanguage has inconsistent formats for different locales
            if (m_localizedSelectKeywordLookup.TryGetValue(m_currentSystemLanguage.Substring(0, 2), out string localizedKeyword))
            {
                return localizedKeyword;
            }
            return null;
        }

#if WINDOWS_UWP
        private void SpatialInteractionManager_SourcePressed(SpatialInteractionManager sender, SpatialInteractionSourceEventArgs args)
        {
            if (args.State.Source.Kind == SpatialInteractionSourceKind.Voice)
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    PhraseRecognizedEventArgs eventArgs = GeneratePhraseRecognizedEventArgs();
                    OnPhraseRecognized.Invoke(eventArgs);
                }, false);
            }
        }

        private static SpatialInteractionManager m_spatialInteractionManager = null;

        private static SpatialInteractionManager SpatialInteractionManager
        {
            get
            {
                if (m_spatialInteractionManager == null)
                {
                    UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                    {
                        m_spatialInteractionManager = SpatialInteractionManager.GetForCurrentView();
                    }, true);
                }

                return m_spatialInteractionManager;
            }
        }
#endif // WINDOWS_UWP

        // The typical duration of pronouncing the "select" keyword in ticks (100 nanoseconds). 1 second.
        private const long m_selectKeywordDurationInTicks = 10000000;

        private static readonly Type[] m_recogEventArgsConstructorArgTypes = new Type[] {
            typeof(string), typeof(ConfidenceLevel), typeof(SemanticMeaning[]), typeof(DateTime), typeof(TimeSpan) };

        private static readonly Dictionary<string, string> m_localizedSelectKeywordLookup = new Dictionary<string, string>
        {
            {"en", "select"},
            {"ja", "選択"},
            {"es", "seleccionar"},
            {"zh", "选择"},
            {"de", "auswählen"},
            {"fr", "sélectionner"},
            {"it", "seleziona"},
        };

        private static readonly string m_currentSystemLanguage =
#if WINDOWS_UWP
            GlobalizationPreferences.Languages[0];
#else
            null;
#endif // WINDOWS_UWP

        private static readonly string m_localizedSelectKeyword = GetLocalizedKeyword();

        private static readonly ConstructorInfo m_recogEventArgsConstructorInfo = GetPhraseRecognizedEventArgsConstructorInfo();

        private static bool? m_isPlatformSupported = null;
    }
}
#endif // UNITY_EDITOR || UNITY_STANDALONE || WINDOWS_UWP