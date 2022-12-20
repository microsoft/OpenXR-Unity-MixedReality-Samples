// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Remoting;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Helper script for automatically connecting an OpenXR app to a specific remote device.
    /// </summary>
    public class AppRemotingSample : MonoBehaviour
    {
        [SerializeField, Tooltip("The UI to be displayed in a 2D app window, when the remote session hasn't yet been established.")]
        private GameObject flatUI = null;

        [SerializeField, Tooltip("The UI to be displayed in a 2D app window for play mode scenario, when the remote session hasn't yet been established.")]
        private GameObject collapsedFlatUI = null;

        [SerializeField, Tooltip("A text field to input the IP address of the remote device, such as a HoloLens.")]
        private UnityEngine.UI.InputField textInput = null;

        [SerializeField, Tooltip("A text field to display log information when an incorrect address is provided.")]
        private UnityEngine.UI.Text outputText = null;

        [SerializeField, Tooltip("The UI Button in the 2D app window to start connecting to the player.")]
        private Button connectButton = null;

        [SerializeField, Tooltip("The UI Button in the 2D app window to start listening for the player.")]
        private Button listenButton = null;

        [SerializeField, Tooltip("The UI Button in the 2D app window to stop listening for the player.")]
        private Button stopListeningButton = null;

        [SerializeField, Tooltip("The configuration information for the remote connection.")]
        private RemotingConnectConfiguration remotingConnectConfiguration = new RemotingConnectConfiguration { RemotePort = 8265, MaxBitrateKbps = 20000 };

        [SerializeField, Tooltip("The configuration information for listening to remote connection.")]
        private Remoting.RemotingListenConfiguration remotingListenConfiguration = new Remoting.RemotingListenConfiguration { ListenInterface = "0.0.0.0", HandshakeListenPort = 8265, TransportListenPort = 8266, MaxBitrateKbps = 20000 };

        private bool m_connected = false;
        private DisconnectReason m_disconnectReason = DisconnectReason.None;
        private bool m_remotingInProgress = false;
        private AppRemotingMode m_appRemotingMode = AppRemotingMode.none;
        private bool m_expandUIByDefault = true;

        private void Awake()
        {
            // The app remoting flat UI menu is collapsed in editor. User can click on it to expand the menu and enter the details for app remoting.
            // The purpose of collapsible UI is for MRTK in-editor simulation.
            if (Application.isEditor)
            {
                m_expandUIByDefault = false;
            }

            // If the app already has an app remoting connection established (e.g. some other script connected remoting before us), it is noted here.
            if (Remoting.AppRemoting.TryGetConnectionState(out ConnectionState connectionState, out DisconnectReason _))
            {
                m_connected = connectionState == ConnectionState.Connected;
            }

            List<XRDisplaySubsystem> xrDisplaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(xrDisplaySubsystems);
            foreach (XRDisplaySubsystem xrDisplaySubsystem in xrDisplaySubsystems)
            {
                // If a running XR display is found, assume an XR headset is attached.
                // In this case, don't display the UI, since the app has already launched
                // into an XR experience and it's too late to connect remoting.
                if (xrDisplaySubsystem.running && !Application.isEditor)
                {
                    if (!m_connected)
                    {
                        // Disable this remoting scenario
                        SetObjectActive(this.gameObject, false);
                    }
                    else
                    {
                        HideConnectionUI();
                    }
                    return;
                }
            }

            ShowConnectionUI();

            Remoting.AppRemoting.Connected += OnConnected;
            Remoting.AppRemoting.Disconnecting += OnDisconnecting;
            Remoting.AppRemoting.ReadyToStart += OnReadyToStart;
        }

        private void OnDestroy()
        {
            Remoting.AppRemoting.Connected -= OnConnected;
            Remoting.AppRemoting.Disconnecting -= OnDisconnecting;
            Remoting.AppRemoting.ReadyToStart -= OnReadyToStart;
        }

        private int m_updateTextCount = 0;
        private const int m_updateTextCountMax = 30;
        private void Update()
        {
            if (m_updateTextCount++ < m_updateTextCountMax)
            {
                return; // Only need to update text after a while.
            }

            m_updateTextCount = 0;
            var ip = textInput.text;
            var hostIp = GetLocalIPAddress();
            var connectPort = remotingConnectConfiguration.RemotePort;
            var listenPort = remotingListenConfiguration.TransportListenPort;

            string commonMessage = "Welcome to App Remoting! Provide an IP address & click Connect or click Start Listening";

            string connectMessage = m_connected
                            ? $"Connected to {ip}:{connectPort}."
                                : !m_remotingInProgress
                                    ? $"Disconnected from {ip}:{connectPort}. Reason is {m_disconnectReason}"
                                    : $"Connecting to {ip}:{connectPort}...";

            string listenMessage = m_connected
                            ? $"Connected on {hostIp}."
                            : !m_remotingInProgress
                                ? $"Stopped listening on {hostIp}:{listenPort}"
                                : $"Listening to incoming connection on {hostIp}";

            string text = "Status unknown...";
            switch (m_appRemotingMode)
            {
                case AppRemotingMode.none:
                    text = commonMessage;
                    break;
                case AppRemotingMode.connect:
                    text = connectMessage;
                    break;
                case AppRemotingMode.listen:
                    text = listenMessage;
                    break;
            }

            if (outputText.text != text)
            {
                outputText.text = text;
            }
        }


        //////////////////////////////////// Button Events ////////////////////////////////////

        // Connects to
        //     1. the IP address parameter, if one is passed in
        //     2. the serialized input field's text, if no IP address is passed in and the input field exists
        public void OnConnectButtonPressed()
        {
            if (m_remotingInProgress)
            {
                Debug.LogWarning("Current session is still in progress, try to connect again after completion");
                return;
            }

            if (textInput != null)
            {
                remotingConnectConfiguration.RemoteHostName = textInput.text;
            }

            if (string.IsNullOrWhiteSpace(remotingConnectConfiguration.RemoteHostName))
            {
                Debug.LogWarning($"No IP address was provided to {nameof(AppRemoting)}. Returning without connecting.");
                return;
            }

            m_connected = false;
            m_remotingInProgress = true;
            m_appRemotingMode = AppRemotingMode.connect;

            DisableButtons();
            Remoting.AppRemoting.StartConnectingToPlayer(remotingConnectConfiguration);
        }

        // Listens to the incoming connections as specified in the <see cref="Remoting.RemotingListenConfiguration"/>.
        public void OnStartListeningButtonPressed()
        {
            if (m_remotingInProgress)
            {
                Debug.LogWarning("Current session is still in progress, try to listen again after completion");
                return;
            }
            m_connected = false;
            m_remotingInProgress = true;
            m_appRemotingMode = AppRemotingMode.listen;
            DisableButtons();
            Remoting.AppRemoting.StartListeningForPlayer(remotingListenConfiguration);
        }

        // Disconnects from the remote session.
        public void OnDisconnectButtonPressed()
        {
            Remoting.AppRemoting.Disconnect();
        }

        // Stops listening for remote connections.        
        public void OnStopListeningButtonPressed()
        {
            Remoting.AppRemoting.StopListening();
        }

        public void OnOpenRemotingUIButtonPressed()
        {
            SetObjectActive(flatUI, true);
            SetObjectActive(collapsedFlatUI, false);
        }

        public void OnCloseRemotingUIButtonPressed()
        {
            SetObjectActive(flatUI, false);
            SetObjectActive(collapsedFlatUI, true);
        }


        //////////////////////////////////// Remoting Events ////////////////////////////////////

        private void OnConnected()
        {
            m_connected = true;
            HideConnectionUI();
        }

        private void OnDisconnecting(DisconnectReason disconnectReason)
        {
            m_disconnectReason = disconnectReason;
            m_connected = false;
            ShowConnectionUI();
        }

        private void OnReadyToStart()
        {
            m_remotingInProgress = false;
            EnableButtons();
        }


        //////////////////////////////////// UI State Management ////////////////////////////////////

        private void EnableButtons()
        {
            connectButton.interactable = true;
            listenButton.interactable = true;
            stopListeningButton.interactable = true;
        }

        private void DisableButtons()
        {
            if (m_appRemotingMode == AppRemotingMode.connect)
            {
                stopListeningButton.interactable = false;
            }
            connectButton.interactable = false;
            listenButton.interactable = false;
        }

        private void ShowConnectionUI()
        {
            if (m_expandUIByDefault || flatUI.activeSelf)
            {
                SetObjectActive(flatUI, true);
                SetObjectActive(collapsedFlatUI, false);
            }
            else
            {
                SetObjectActive(flatUI, false);
                SetObjectActive(collapsedFlatUI, true);
            }
        }

        private void HideConnectionUI()
        {
            SetObjectActive(flatUI, false);
            SetObjectActive(collapsedFlatUI, false);
        }

        //////////////////////////////////// Misc. Helpers ////////////////////////////////////

        private string GetLocalIPAddress()
        {
            UnicastIPAddressInformation mostSuitableIp = null;

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var network in networkInterfaces)
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                    continue;

                var properties = network.GetIPProperties();

                if (properties.GatewayAddresses.Count == 0)
                    continue;

                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    if (!address.IsDnsEligible)
                    {
                        if (mostSuitableIp == null)
                            mostSuitableIp = address;
                        continue;
                    }

                    // The best IP is the IP got from DHCP server
                    if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                    {
                        if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                            mostSuitableIp = address;
                        continue;
                    }

                    return address.Address.ToString();
                }
            }

            return mostSuitableIp != null
                ? mostSuitableIp.Address.ToString()
                : "";
        }

        private void SetObjectActive(GameObject @object, bool active)
        {
            if (@object != null && @object.activeSelf != active)
            {
                @object.SetActive(active);
            }
        }
    }

    public enum AppRemotingMode
    {
        none = 0,
        connect = 1,
        listen = 2
    }
}
