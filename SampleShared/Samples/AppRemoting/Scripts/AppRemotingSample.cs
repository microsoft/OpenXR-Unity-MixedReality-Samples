// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using Microsoft.MixedReality.OpenXR.Remoting;

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

        [SerializeField, Tooltip("The UI to be displayed in the 3D app session, when remoting and XR have both been established.")]
        private GameObject immersiveUI = null;

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

        private static readonly List<XRDisplaySubsystem> XRDisplaySubsystems = new List<XRDisplaySubsystem>();
        private static bool s_connected = false;
        private static RemotingDisconnectReason s_disconnectReason = RemotingDisconnectReason.None;
        private bool m_remotingInProgress = false;
        private AppRemotingMode m_appRemotingMode = AppRemotingMode.none;
        private bool m_showFlatUI = true;

        private void Awake()
        {
            // The app remoting flat UI menu is collapsed in editor. User can click on it to expand the menu and enter the details for app remoting.
            // The purpose of collapsible UI is for MRTK in-editor simulation.
            if (Application.isEditor)
            {
                m_showFlatUI = false;
            }

            SubsystemManager.GetInstances(XRDisplaySubsystems);
            foreach (XRDisplaySubsystem xrDisplaySubsystem in XRDisplaySubsystems)
            {
                // If a running XR display is found, assume an XR headset is attached.
                // In this case, don't display the UI, since the app has already launched
                // into an XR experience and it's too late to connect remoting.
                if (xrDisplaySubsystem.running && !Application.isEditor)
                {
                    if (!s_connected)
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
            SubscribeToAppRemotingEvents();
        }

        private void OnEnable()
        {
            SubscribeToAppRemotingEvents();
        }

        private void OnDisable()
        {
            UnSubscribeToAppRemotingEvents();
        }

        private void Update()
        {
            var ip = textInput.text;
            var hostIp = GetLocalIPAddress();
            var connectPort = remotingConnectConfiguration.RemotePort;
            var listenPort = remotingListenConfiguration.TransportListenPort;
    
            if (s_connected)
            {
                HideConnection2DUI();
            }
            else
            {
                ShowConnection2DUI();
            }

            string commonMessage = "Welcome to App Remoting! Provide IP address & click Connect or click StartListening";

            string connectMessage = s_connected
                            ? $"Connected to {ip}:{connectPort}."
                                : !m_remotingInProgress
                                    ? $"Disconnected from {ip}:{connectPort}. Reason is {s_disconnectReason}"
                                    : $"Connecting to {ip}:{connectPort}...";

            string listenMessage = s_connected
                            ? $"Connected on {hostIp}."
                            : !m_remotingInProgress
                                ? $"Stopped listening on {hostIp}:{listenPort}"
                                : $"Listening to incoming connection on {hostIp}";

            switch (m_appRemotingMode)
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

        // Connects to
        //     1. the IP address parameter, if one is passed in
        //     2. the serialized input field's text, if no IP address is passed in and the input field exists
        public void Connect()
        {
            if (m_remotingInProgress)
            {
                Debug.LogWarning("Current session is still in progress, try to connect again after completion");
                return;
            }
            s_connected = false; 
            m_remotingInProgress = true;
            m_appRemotingMode = AppRemotingMode.connect;

            if (textInput != null)
            {
                remotingConnectConfiguration.RemoteHostName = textInput.text;
            }

            if (string.IsNullOrWhiteSpace(remotingConnectConfiguration.RemoteHostName))
            {
                Debug.LogWarning($"No IP address was provided to {nameof(AppRemoting)}. Returning without connecting.");
                return;
            }

            StartCoroutine(InvokeConnectToPlayer());
        }

        // Coroutine that waits on completion of <see cref="Remoting.ConnectToPlayer"/>.
        public System.Collections.IEnumerator InvokeConnectToPlayer()
        {
            DisableButtons();
            yield return Remoting.AppRemoting.ConnectToPlayer(remotingConnectConfiguration);
            m_remotingInProgress = false;
            EnableButtons();
        }

        // Listens to the incoming connections as specified in the <see cref="Remoting.RemotingListenConfiguration"/>.
        public void Listen()
        {
            if (m_remotingInProgress)
            {
                Debug.LogWarning("Current session is still in progress, try to listen again after completion");
                return;
            }
            s_connected = false;
            m_remotingInProgress = true;
            m_appRemotingMode = AppRemotingMode.listen;
            StartCoroutine(InvokeStartListeningForPlayer());
        }

        // Coroutine that waits on completion of <see cref="Remoting.StartListeningForPlayer"/>.
        public System.Collections.IEnumerator InvokeStartListeningForPlayer()
        {
            DisableButtons();
            yield return Remoting.AppRemoting.StartListeningForPlayer(remotingListenConfiguration);
            m_remotingInProgress = false;
            EnableButtons();
        }

        // Disconnects from the remote session.
        public void DisconnectFromRemote()
        {
            Remoting.AppRemoting.Disconnect();
            ShowConnection2DUI();
        }

        // Stops listening for remote connections.        
        public void StopListeningForRemote()
        {
            Remoting.AppRemoting.StopListening();
            ShowConnection2DUI();
        }

        private static void OnConnected()
        {
            s_connected = true;
        }

        private static void OnDisconnecting(RemotingDisconnectReason disconnectReason)
        {
            s_disconnectReason = disconnectReason;
            s_connected = false;
        }

        private void SubscribeToAppRemotingEvents()
        {
            Remoting.AppRemoting.Connected += OnConnected;
            Remoting.AppRemoting.Disconnecting += OnDisconnecting;
        }

        private void UnSubscribeToAppRemotingEvents()
        {
            Remoting.AppRemoting.Connected -= OnConnected;
            Remoting.AppRemoting.Disconnecting -= OnDisconnecting;
        }

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

        private void EnableButtons()
        {
            connectButton.interactable = true;
            listenButton.interactable = true;
            stopListeningButton.interactable = true;
        }

        private void DisableButtons()
        {            
            if ((m_appRemotingMode == AppRemotingMode.connect))
            {
                stopListeningButton.interactable = false;
            }
            connectButton.interactable = false;
            listenButton.interactable = false;
        }

        private void ShowConnection2DUI()
        {
            SetObjectActive(immersiveUI, false);
            
            if (m_showFlatUI || flatUI.activeSelf)
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

        private void HideConnection2DUI()
        {
            SetObjectActive(flatUI, false);
            SetObjectActive(collapsedFlatUI, false);
            SetObjectActive(immersiveUI, true);
        }

        private void DisableConnection2DUI()
        {
            SetObjectActive(gameObject, false);
            SetObjectActive(immersiveUI, false);
            SetObjectActive(flatUI, false);
        }

        public void ShowOrHideRemotingFlatUI()
        {
            SetObjectActive(immersiveUI, false);
            if (!flatUI.activeSelf)
            {
                SetObjectActive(flatUI, true);
                SetObjectActive(collapsedFlatUI, false);
            }
            else
            {
                SetObjectActive(collapsedFlatUI, true);
                SetObjectActive(flatUI, false);
            }
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
