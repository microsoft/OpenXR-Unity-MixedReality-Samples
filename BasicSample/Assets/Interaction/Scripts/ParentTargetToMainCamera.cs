// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class ParentTargetToMainCamera : MonoBehaviour
    {
        public GameObject target;

        void Awake() => target.transform.SetParent(Camera.main.transform, worldPositionStays: false);
        void OnDestroy() => Destroy(target);
    }
}