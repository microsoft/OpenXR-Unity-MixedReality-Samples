// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// Helper script for automatically connecting an OpenXR app to a specific remote device.
    /// </summary>
    public class AppRemoting : MonoBehaviour
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

            if (immersiveUI != null)
            {
                immersiveUI.SetActive(false);
            }

            if (flatUI != null)
            {
                flatUI.SetActive(true);
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
                if (outputText != null)
                {
                    outputText.text = $"No IP address was provided to {nameof(AppRemoting)}. Returning without connecting.";
                }
                return;
            }

            if (outputText != null)
            {
                outputText.text = $"Connecting to {remotingConfiguration.RemoteHostName}...";
            }

            StartCoroutine(Remoting.AppRemoting.Connect(remotingConfiguration));

            if (flatUI != null)
            {
                flatUI.SetActive(false);
            }

            if (immersiveUI != null)
            {
                immersiveUI.SetActive(true);
            }
        }

        /// <summary>
        /// Disconnects from the remote session.
        /// </summary>
        public void DisconnectFromRemote()
        {
            Remoting.AppRemoting.Disconnect();

            if (immersiveUI != null)
            {
                immersiveUI.SetActive(false);
            }

            if (flatUI != null)
            {
                flatUI.SetActive(true);
            }

            if (outputText != null)
            {
                outputText.text = "Disconnected";
            }
        }
    }
}
