// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Provides utilities for switching between scenes.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// Request that the MRTK SceneSystem load a scene of a given name.
        /// </summary>
        public void LoadScene(string sceneName) => CoreServices.SceneSystem.LoadContent(sceneName, LoadSceneMode.Single);
    }
}
