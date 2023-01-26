// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos
{
    internal class SyncPose : MonoBehaviour
    {
        private Rigidbody body;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        public void Sync(Transform otherTransform)
        {
            transform.SetPositionAndRotation(otherTransform.position, otherTransform.rotation);

            if (otherTransform.TryGetComponent(out Rigidbody otherBody))
            {
                otherBody.velocity = Vector3.zero;
                otherBody.angularVelocity = Vector3.zero;
            }

            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }
}
