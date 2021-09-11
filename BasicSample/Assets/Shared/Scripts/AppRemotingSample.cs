// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

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

        private static readonly List<XRDisplaySubsystem> XRDisplaySubsystems = new List<XRDisplaySubsystem>();
        private Remoting.ConnectionState m_connectionState = Remoting.ConnectionState.Disconnected;
        private Remoting.DisconnectReason m_disconnectReason = Remoting.DisconnectReason.None;

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
                    DisableConnection2DUI();
                    return;
                }
            }

            ShowConnection2DUI();
        }

        private void Update()
        {
            var ip = textInput.text;
            var port = remotingConfiguration.RemotePort;

            if (Remoting.AppRemoting.TryGetConnectionState(
                out Remoting.ConnectionState connectionState,
                out Remoting.DisconnectReason disconnectReason))
            {
                if (m_connectionState != connectionState || disconnectReason != m_disconnectReason)
                {
                    m_connectionState = connectionState;
                    m_disconnectReason = disconnectReason;

                    Debug.Log($"Connection state changed : {ip}:{port}, {connectionState}, {m_disconnectReason}");

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
                            DisconnectFromRemote();
                            break;
                    }
                }
            }

            string message = string.IsNullOrWhiteSpace(ip)
                        ? $"No IP address was provided to {nameof(Remoting.AppRemoting)}."
                        : OpenXRContext.Current.Instance == 0
                            ? $"Click button below to connect to {ip}:{port}."
                            : m_connectionState == Remoting.ConnectionState.Connected
                                ? $"Connected to {ip}:{port}."
                                : m_connectionState == Remoting.ConnectionState.Connecting
                                    ? $"Connecting to {ip}:{port}..."
                                    : $"Disconnected to {ip}:{port}. Reason is {m_disconnectReason}";

            if (outputText.text != message)
            {
                outputText.text = message;
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
        /// Disconnects from the remote session.
        /// </summary>
        public void DisconnectFromRemote()
        {
            Remoting.AppRemoting.Disconnect();
            ShowConnection2DUI();
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
}
