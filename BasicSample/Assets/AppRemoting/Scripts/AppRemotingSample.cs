// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Helper script for automatically connecting an OpenXR app to a specific remote device.
    /// </summary>
    public class AppRemotingSample : MonoBehaviour
    {
        [SerializeField, Tooltip("The UI to be displayed in a 2D app window, when the remote session hasn't yet been established.")]
        private GameObject flatUI = null;

        [SerializeField, Tooltip("The UI to be displayed in the 3D app session, when remoting and XR have both been established.")]
        private GameObject immersiveUI = null;

        [SerializeField, Tooltip("A text field to input the IP address of the remote device, such as a HoloLens.")]
        private UnityEngine.UI.InputField textInput = null;

        [SerializeField, Tooltip("A text field to display log information when an incorrect address is provided.")]
        private UnityEngine.UI.Text outputText = null;

        [SerializeField, Tooltip("The configuration information for the remote connection.")]
        private Remoting.RemotingConfiguration remotingConfiguration = new Remoting.RemotingConfiguration { RemotePort = 8265, MaxBitrateKbps = 20000 };

        [SerializeField, Tooltip("The configuration information for listening to remote connection.")]
        private Remoting.RemotingListenConfiguration remotingListenConfiguration = new Remoting.RemotingListenConfiguration { ListenInterface = "0.0.0.0", HandshakeListenPort = 8265, TransportListenPort = 8266, MaxBitrateKbps = 20000 };

        private static readonly List<XRDisplaySubsystem> XRDisplaySubsystems = new List<XRDisplaySubsystem>();
        private Remoting.ConnectionState m_connectionState = Remoting.ConnectionState.Disconnected;
        private Remoting.DisconnectReason m_disconnectReason = Remoting.DisconnectReason.None;
        private AppRemotingMode m_appRemotingMode = AppRemotingMode.none;
        private bool m_disconnectedOnListenMode = false;

        private void Awake()
        {
            // This is intended for app remoting and shouldn't run in the editor
            if (Application.isEditor)
            {
                DisableConnection2DUI();
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
                    var connectionValid = Remoting.AppRemoting.TryGetConnectionState(out Remoting.ConnectionState connectionState, out Remoting.DisconnectReason disconnectReason);
                    if (!connectionValid || connectionState == Remoting.ConnectionState.Disconnected)
                    {
                        DisableConnection2DUI();
                    }
                    else
                    {
                        HideConnection2DUI();
                    }
                    
                    return;
                }
            }

            ShowConnection2DUI();
        }

        private void Update()
        {
            var ip = textInput.text;
            var hostIp = GetLocalIPAddress();
            var connectPort = remotingConfiguration.RemotePort;
            var listenPort = remotingListenConfiguration.TransportListenPort;
            var connectionStateValid = Remoting.AppRemoting.TryGetConnectionState(out Remoting.ConnectionState connectionState, out Remoting.DisconnectReason disconnectReason);

            if (connectionStateValid)
            {
                if (m_connectionState != connectionState || disconnectReason != m_disconnectReason)
                {
                    m_connectionState = connectionState;
                    m_disconnectReason = disconnectReason;

                    if(m_appRemotingMode == AppRemotingMode.connect)
                    {
                        Debug.Log($"Connection state changed : {ip}:{connectPort}, {connectionState}, {m_disconnectReason}");
                    }
                    else if (m_appRemotingMode == AppRemotingMode.listen)
                    {
                        Debug.Log($"Connection state changed : {hostIp}:{listenPort}, {connectionState}, {m_disconnectReason}");
                    }
                    

                    switch (m_connectionState)
                    {
                        case Remoting.ConnectionState.Connected:
                            HideConnection2DUI();
                            break;
                        case Remoting.ConnectionState.Connecting:
                            ShowConnection2DUI();
                            break;
                        case Remoting.ConnectionState.Disconnected:
                            ShowConnection2DUI();
                            break;
                    }
                }
            }
            else
            {
                m_connectionState = Remoting.ConnectionState.Disconnected;
            }

            string commonMessage = "Welcome to App Remoting! Provide Ip address & click Connect or click Listen";

            string connectMessage = string.IsNullOrWhiteSpace(ip)
                    ? $"No IP address was provided to {nameof(Remoting.AppRemoting)}."
                        : m_connectionState == Remoting.ConnectionState.Connected
                            ? $"Connected to {ip}:{connectPort}."
                            : m_connectionState == Remoting.ConnectionState.Connecting
                                ? $"Connecting to {ip}:{connectPort}..."
                                : $"Disconnected to {ip}:{connectPort}. Reason is {m_disconnectReason}";
            string listenMessage = m_connectionState == Remoting.ConnectionState.Connected
                            ? $"Connected on {hostIp}."
                            : m_connectionState == Remoting.ConnectionState.Disconnected && m_disconnectedOnListenMode
                                ? $"Disconnected on {hostIp}:{listenPort}. Reason is {m_disconnectReason}"
                                : $"Listening to incoming connection on {hostIp}";

            switch(m_appRemotingMode)
            {
                case AppRemotingMode.none:
                    outputText.text = commonMessage;
                    break;
                case AppRemotingMode.connect:
                    outputText.text = connectMessage;
                    break;
                case AppRemotingMode.listen:
                    outputText.text = listenMessage;
                    break;
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
            m_appRemotingMode = AppRemotingMode.connect;
            if (!string.IsNullOrWhiteSpace(address))
            {
                remotingConfiguration.RemoteHostName = address;
            }
            else if (textInput != null)
            {
                remotingConfiguration.RemoteHostName = textInput.text;
            }

            if (string.IsNullOrWhiteSpace(remotingConfiguration.RemoteHostName))
            {
                Debug.LogWarning($"No IP address was provided to {nameof(Remoting.AppRemoting)}. Returning without connecting.");
                return;
            }

            StartCoroutine(Remoting.AppRemoting.Connect(remotingConfiguration));
        }

        /// <summary>
        /// Listens to the incoming connections as specified in the remotinglistenconfiguration
        /// </summary>
        public void ListenToRemote()
        {
            m_appRemotingMode = AppRemotingMode.listen;
            StartCoroutine(Remoting.AppRemoting.Listen(remotingListenConfiguration));
        }

        /// <summary>
        /// Disconnects from the remote session.
        /// </summary>
        public void DisconnectFromRemote()
        {
            Remoting.AppRemoting.Disconnect();
            if (m_appRemotingMode == AppRemotingMode.listen)
            {
                m_disconnectedOnListenMode = true;
            }
            ShowConnection2DUI();
        }

        /*private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }*/

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

        private void ShowConnection2DUI()
        {
            SetObjectActive(immersiveUI, false);
            SetObjectActive(flatUI, true);
        }

        private void HideConnection2DUI()
        {
            SetObjectActive(flatUI, false);
            SetObjectActive(immersiveUI, true);
        }

        private void DisableConnection2DUI()
        {
            SetObjectActive(gameObject, false);
            SetObjectActive(immersiveUI, false);
            SetObjectActive(flatUI, false);
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
