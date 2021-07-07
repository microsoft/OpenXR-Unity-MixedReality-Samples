// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.OpenXR.BasicSample
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