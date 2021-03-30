// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class ParentToMainCamera : MonoBehaviour
    {
        void Awake()
        {
            transform.SetParent(Camera.main.transform, worldPositionStays: false);
        }
    }
}