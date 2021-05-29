// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.SceneManagement;
using MrtkCoreServices = Microsoft.MixedReality.Toolkit.CoreServices;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// Provides utilities for switching between scenes.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// Request that the MRTK SceneSystem load a scene of a given name.
        /// </summary>
        public void LoadScene(string sceneName) => MrtkCoreServices.SceneSystem.LoadContent(sceneName, LoadSceneMode.Single);
    }
}
