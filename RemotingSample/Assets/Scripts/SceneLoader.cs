// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// Provides utilities for switching between scenes.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// Request that the SceneManager load a scene of a given name.
        /// </summary>
        public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
