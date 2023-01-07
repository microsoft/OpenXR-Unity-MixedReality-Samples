// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Remoting
{
    internal class AppRemotingCoroutineRunner : MonoBehaviour
    {
        private static AppRemotingCoroutineRunner m_instance;
        protected static AppRemotingCoroutineRunner Instance
        {
            get
            {
                if (m_instance == null)
                {
                    SetupInstance();
                }
                return m_instance;
            }
        }

        private static void SetupInstance()
        {
            GameObject gameObject = new GameObject("AppRemotingCoroutineRunner", typeof(AppRemotingCoroutineRunner))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            DontDestroyOnLoad(gameObject);
            m_instance = gameObject.GetComponent<AppRemotingCoroutineRunner>();
        }

        // Starts a coroutine on this hidden, persistent GameObject, then returns a Coroutine which
        // can be used to observe the progress of the internal routine, if desired. The internal 
        // routine passed into this method will always run to completion.
        internal static Coroutine Start(IEnumerator internalRoutine)
        {
            return Instance.StartCoroutine(internalRoutine);
        }
    }
}