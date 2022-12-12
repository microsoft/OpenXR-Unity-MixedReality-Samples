// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using Microsoft.MixedReality.OpenXR.Remoting;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    // Enables this GameObject during remoting scenarios and disables it otherwise.
    // Uses AppRemoting events to observe changes in state even while the GameObject is inactive.
    public class EnableOnlyDuringRemoting : MonoBehaviour
    {
        void Awake()
        {
            bool succeeded = Remoting.AppRemoting.TryGetConnectionState(out ConnectionState connectionState, out DisconnectReason _);
            if (!succeeded || connectionState == ConnectionState.Disconnected)
            {
                this.gameObject.SetActive(false);
            }

            Remoting.AppRemoting.Connected += OnRemotingConnected;
            Remoting.AppRemoting.Disconnecting += OnRemotingDisconnecting;
        }

        void OnDestroy()
        {
            Remoting.AppRemoting.Connected -= OnRemotingConnected;
            Remoting.AppRemoting.Disconnecting -= OnRemotingDisconnecting;
        }

        void OnRemotingConnected() => this.gameObject.SetActive(true);
        void OnRemotingDisconnecting(DisconnectReason reason) => this.gameObject.SetActive(false);
    }
}
