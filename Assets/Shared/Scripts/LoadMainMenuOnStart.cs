// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// MRTK will always load the "Manager Scene" SamplesBase as the first scene on app start. In this case, the main menu scene is switched to as the entry point for the app.
    /// </summary>

    public class LoadMainMenuOnStart : MonoBehaviour
    {
        private static bool IsFirstLoad = true;
        private void Start()
        {
            if (IsFirstLoad)
            {
                IsFirstLoad = false;
                CoreServices.SceneSystem.LoadContent("MainMenu", LoadSceneMode.Single);
            }
        }
    }
}