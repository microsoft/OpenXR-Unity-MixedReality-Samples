using System;
using System.Collections;

using UnityEngine;
using UnityEngine.XR.Management;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.OpenXR
{
    internal class OpenXRRestarter : MonoBehaviour
    {
        internal Action onAfterRestart;
        internal Action onAfterShutdown;
        internal Action onQuit;
        internal Action onAfterCoroutine;
        internal Action onAfterSuccessfulRestart;

        static OpenXRRestarter()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode)
                    m_pauseAndRestartAttempts = 0;
            };
#endif
            TimeBetweenRestartAttempts = 5.0f;
        }


        public void ResetCallbacks ()
        {
            onAfterRestart = null;
            onAfterSuccessfulRestart = null;
            onAfterShutdown = null;
            onAfterCoroutine = null;
            onQuit = null;
            m_pauseAndRestartAttempts = 0;
        }

        /// <summary>
        /// True if the restarter is currently running
        /// </summary>
        public bool isRunning => m_Coroutine != null;

        private static OpenXRRestarter s_Instance = null;

        private Coroutine m_Coroutine;

        private Coroutine m_pauseAndRestartCoroutine;

        private static int m_pauseAndRestartAttempts = 0;

        public static float TimeBetweenRestartAttempts
        {
            get;
            set;
        }

        public static int PauseAndRestartAttempts
        {
            get
            {
                return m_pauseAndRestartAttempts;
            }
        }

        public static OpenXRRestarter Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var go = GameObject.Find("~oxrestarter");
                    if (go == null)
                    {
                        go = new GameObject("~oxrestarter");
                        go.hideFlags = HideFlags.HideAndDontSave;
                        go.AddComponent<OpenXRRestarter>();
                    }
                    s_Instance = go.GetComponent<OpenXRRestarter>();
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// Shutdown the the OpenXR loader and optionally quit the application
        /// </summary>
        public void Shutdown ()
        {
            if (OpenXRLoader.Instance == null)
                return;

            if (m_Coroutine != null)
            {
                Debug.LogError("Only one shutdown or restart can be executed at a time");
                return;
            }

            m_Coroutine = StartCoroutine(RestartCoroutine(false));
        }

        /// <summary>
        /// Restart the OpenXR loader
        /// </summary>
        public void ShutdownAndRestart ()
        {
            if (OpenXRLoader.Instance == null)
                return;

            if (m_Coroutine != null)
            {
                Debug.LogError("Only one shutdown or restart can be executed at a time");
                return;
            }

            m_Coroutine = StartCoroutine(RestartCoroutine(true));
        }

        /// <summary>
        /// Pause and then restart.
        /// If the restart triggers another restart, the pause adds some delay between restarts.
        /// </summary>
        public void PauseAndRestart()
        {
            if (OpenXRLoader.Instance == null)
                return;

            if (m_pauseAndRestartCoroutine != null)
            {
                Debug.LogError("Only one pause then shutdown/restart can be executed at a time");
                return;
            }

            Debug.Log("Please make sure the device is connected. Will try to restart xr periodically.");
            m_pauseAndRestartCoroutine = StartCoroutine(PauseAndRestartCoroutine(TimeBetweenRestartAttempts));
        }

        public IEnumerator PauseAndRestartCoroutine(float pauseTimeInSeconds)
        {
            try
            {
                yield return new WaitForSeconds(pauseTimeInSeconds);
                m_pauseAndRestartAttempts += 1;
                if (m_Coroutine == null)
                {
                    m_Coroutine = StartCoroutine(RestartCoroutine(true));
                }
                else
                {
                    Debug.LogError(String.Format("Restart/Shutdown already in progress so skipping this attempt."));
                }
            }
            finally
            {
                m_pauseAndRestartCoroutine = null;
                onAfterCoroutine?.Invoke();
            }
        }

        private IEnumerator RestartCoroutine (bool shouldRestart)
        {
            try
            {
                yield return null;

                // Always shutdown the loader
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
                yield return null;

                onAfterShutdown?.Invoke();

                // Restart?
                if (shouldRestart && OpenXRRuntime.ShouldRestart())
                {
                    yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

                    XRGeneralSettings.Instance.Manager.StartSubsystems();

                    if (XRGeneralSettings.Instance.Manager.activeLoader == null)
                    {
                        Debug.LogError("Failure to restart OpenXRLoader after shutdown.");
                    }
                    else
                    {
                        Debug.Log("OpenXRLoader restart successful.");
                        m_pauseAndRestartAttempts = 0;
                        onAfterSuccessfulRestart?.Invoke();
                    }

                    onAfterRestart?.Invoke();
                }
                // Quit?
                else if (OpenXRRuntime.ShouldQuit())
                {
                    onQuit?.Invoke();
#if !UNITY_INCLUDE_TESTS
#if UNITY_EDITOR
                    if (EditorApplication.isPlaying || EditorApplication.isPaused)
                    {
                        EditorApplication.ExitPlaymode();
                    }
#else
                    Application.Quit();
#endif
#endif
                }
            }
            finally
            {
                m_Coroutine = null;
                onAfterCoroutine?.Invoke();
            }
        }
    }
}
