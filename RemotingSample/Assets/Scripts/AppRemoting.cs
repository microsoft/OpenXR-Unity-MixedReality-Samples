// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.RemotingSample
{
    /// <summary>
    /// Helper script for automatically connecting an OpenXR app to a specific remote device.
    /// </summary>
    public class AppRemoting : MonoBehaviour
    {
        [SerializeField, Tooltip("A text field to input the IP address of the remote device, such as a HoloLens.")]
        private UnityEngine.UI.InputField textInput = null;

        [SerializeField, Tooltip("A text field to display log information when an incorrect address is provided.")]
        private UnityEngine.UI.Text outputText = null;

        [SerializeField, Tooltip("The configuration information for connecting to the remote.")]
        private Remoting.RemotingConfiguration remotingConfiguration = new Remoting.RemotingConfiguration { RemotePort = 8265, MaxBitrateKbps = 20000 };

        [SerializeField, Tooltip("The configuration information for listening to remote connection.")]
        private Remoting.RemotingListenConfiguration remotingListenConfiguration = new Remoting.RemotingListenConfiguration { ListenInterface = "0.0.0.0", HandshakeListenPort = 8265, TransportListenPort = 8266, MaxBitrateKbps = 20000 };

        private static readonly List<XRDisplaySubsystem> XRDisplaySubsystems = new List<XRDisplaySubsystem>();
        private Remoting.ConnectionState m_connectionState = Remoting.ConnectionState.Disconnected;
        private Remoting.DisconnectReason m_disconnectReason = Remoting.DisconnectReason.None;
        private bool m_listenMode = false;

        private void Awake()
        {
            // This is intended for app remoting and shouldn't run in the editor
            if (Application.isEditor)
            {
                gameObject.SetActive(false);
                return;
            }

            SubsystemManager.GetInstances(XRDisplaySubsystems);
            foreach (XRDisplaySubsystem xrDisplaySubsystem in XRDisplaySubsystems)
            {
                // If a running XR display is found, assume an XR headset is attached.
                // In this case, don't display the UI, since the app has already launched
                // into an XR experience and it's too late to connect remoting.
                if (xrDisplaySubsystem.running)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }
        }

        private void Update()
        {
            var ip = textInput.text;
            var connectPort = remotingConfiguration.RemotePort;
            var listenPort = remotingListenConfiguration.TransportListenPort;
            string connectMessage, listenMessage;

            if (Remoting.AppRemoting.TryGetConnectionState(
                out Remoting.ConnectionState connectionState,
                out Remoting.DisconnectReason disconnectReason))
            {
                if (m_connectionState != connectionState || disconnectReason != m_disconnectReason)
                {
                    m_connectionState = connectionState;
                    m_disconnectReason = disconnectReason;
                    connectMessage = string.IsNullOrWhiteSpace(ip)
                            ? $"No IP address was provided to {nameof(Remoting.AppRemoting)}."
                                : m_connectionState == Remoting.ConnectionState.Connected
                                    ? $"Connected to {ip}:{connectPort}."
                                    : m_connectionState == Remoting.ConnectionState.Connecting
                                        ? $"Connecting to {ip}:{connectPort}..."
                                        : $"Disconnected to {ip}:{connectPort}. Reason is {m_disconnectReason}";
                    listenMessage = m_connectionState == Remoting.ConnectionState.Connected
                                    ? $"Connected on {listenPort}."
                                    : m_connectionState == Remoting.ConnectionState.Connecting
                                        ? $"Listening to incoming connection on {listenPort}..."
                                        : $"Disconnected on {listenPort}. Reason is {m_disconnectReason}";
                    if (!m_listenMode)
                    {
                        if (outputText.text != connectMessage)
                        {
                            outputText.text = connectMessage;
                        }
                    } else
                    {
                        if (outputText.text != listenMessage)
                        {
                            outputText.text = listenMessage;
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Connects to
        ///     1. the IP address parameter, if one is passed in
        ///     2. the serialized input field's text, if no IP address is passed in and the input field exists
        ///     3. the remote host name in the remoting configuration, if no input field exists
        /// </summary>
        /// <param name="address">The (optional) address to connect to.</param>
        public void ConnectToRemote(string address = null)
        {
            m_listenMode = false;
            if (!string.IsNullOrWhiteSpace(address))
            {
                remotingConfiguration.RemoteHostName = address;
            }
            else if (textInput != null)
            {
                remotingConfiguration.RemoteHostName = textInput.text;
            }

            StartCoroutine(Remoting.AppRemoting.Connect(remotingConfiguration));
        }

        public void ListenToRemote()
        {
            m_listenMode = true; 
            StartCoroutine(Remoting.AppRemoting.Listen(remotingListenConfiguration));
        }

        /// <summary>
        /// Disconnects from the remote session.
        /// </summary>
        public void DisconnectFromRemote()
        {
            Remoting.AppRemoting.Disconnect();
            if (outputText != null)
            {
                outputText.text = "Disconnected";
            }
        }
    }
}
