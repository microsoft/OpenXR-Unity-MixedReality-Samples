// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class ParentTargetToMainCamera : MonoBehaviour
    {
        public GameObject target;

        void Awake() => target.transform.SetParent(Camera.main.transform, worldPositionStays: false);
        void OnDestroy() => Destroy(target);
    }
}