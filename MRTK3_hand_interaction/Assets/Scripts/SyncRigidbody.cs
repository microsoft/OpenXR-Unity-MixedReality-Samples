// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos
{
    internal class SyncRigidbody : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        private Rigidbody body;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            body.MovePosition(target.position);
        }
    }
}
