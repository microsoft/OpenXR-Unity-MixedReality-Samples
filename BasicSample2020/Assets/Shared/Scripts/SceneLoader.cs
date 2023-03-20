// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.SceneManagement;

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
