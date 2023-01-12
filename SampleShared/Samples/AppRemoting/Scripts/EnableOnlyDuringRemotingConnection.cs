// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Remoting;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    // Enables this GameObject during remoting scenarios and disables it otherwise.
    // This GameObject must be enabled by default in the scene, to ensure the Awake() method is called.
    // If the app wants to display UI only while connected, such as a "Disconnect" button, this script can be used.
    public class EnableOnlyDuringRemotingConnection : MonoBehaviour
    {
        void Awake()
        {
            bool succeeded = Remoting.AppRemoting.TryGetConnectionState(out ConnectionState connectionState, out DisconnectReason _);
            if (!succeeded || connectionState == ConnectionState.Disconnected)
            {
                this.gameObject.SetActive(false);
            }

            // Uses AppRemoting events to observe changes in state even while the GameObject is inactive.
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
