// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// If late XR initialization is being used, ARFoundation managers will not function until being restarted after XR initialization. Restarting ensures they connect properly to their subsystems.
    /// Additionally, AR components linked in <see cref="additionalARComponents"/> are deactivated and activated according to the avaialability of XR session.  
    /// </summary>
    /// <remarks>
    /// ARFoundation trackable managers will not connect to XR subsystems if the trackable managers are active in the Unity scene
    /// before XR initialization. The application must disable then reenable these trackable managers after XR initialization, or
    /// wait to add active trackable managers to the scene until after XR initialization.
    /// Additional AR components which need xr session to function properly have to be activated only when XR session is available and deactivated otherwise.
    /// </remarks>
    public class HandleLateXRInitialization : MonoBehaviour
    {
        /// <summary>
        /// If late XR initialization is being used, it is recommended to set this field to true
        /// to ensure AR Foundation managers are restarted after XR initializes and connect properly to their subsystems.
        /// </summary>
        [SerializeField]
        private bool restartARFoundationManagers = true;

        /// <summary>
        /// If late XR initialization is being used, and any additional AR components need to be activated and deactived 
        /// according to xr session availability, add them to the list below.
        /// </summary>
        [SerializeField]
        public List<GameObject> additionalARComponents = new List<GameObject>();

        private ARSession[] m_arSessions = null;
        private ARAnchorManager[] m_arAnchorManagers = null;
        private ARMeshManager[] m_arMeshManagers = null;
        private ARPlaneManager[] m_arPlaneManagers = null;
        private ARRaycastManager[] m_arRaycastManagers = null;

        private void Awake()
        {
            if(!restartARFoundationManagers && additionalARComponents.Count == 0)
            {
                Debug.LogWarning("Restarting AR Foundation Managers is disabled and no AR Components are connected to this script." + 
                "This script ensures that AR Foundation managers are restarted and AR components are only active when an XR session is available,and sets them to inactive otherwise." +
                "Enable restarting AR Foundation managers and Connect AR Components which should only be active during an XR session to this script," +
                "or disable this script if it is not needed.");
            }

            m_arSessions = FindObjectsOfType<ARSession>();
            m_arAnchorManagers = FindObjectsOfType<ARAnchorManager>();
            m_arMeshManagers = FindObjectsOfType<ARMeshManager>();
            m_arPlaneManagers = FindObjectsOfType<ARPlaneManager>();
            m_arRaycastManagers = FindObjectsOfType<ARRaycastManager>();

            StartCoroutine(ManageARFoundationManagersAndARComponents());
        }

        /// <summary>
        /// Deactivate and activate AR Components according to XR Session availability.
        /// Restart AR Foundation managers after XR session starts.
        /// </summary>
        private IEnumerator ManageARFoundationManagersAndARComponents()
        {
            // Set the AR Components to inactive if XR session is not running.
            if(OpenXRContext.Current.Session != 0)
            {
                yield return new WaitUntil(() => OpenXRContext.Current.Session == 0);
            }
            SetARComponentsToInactive();

            // Wait until XR session is running to restart AR Foundation managers and set AR components active.
            yield return new WaitUntil(() => OpenXRContext.Current.Session != 0);
            if(restartARFoundationManagers)
            {
                RestartARFoundationManagers(m_arSessions);
                RestartARFoundationManagers(m_arAnchorManagers);
                RestartARFoundationManagers(m_arMeshManagers, true);
                RestartARFoundationManagers(m_arPlaneManagers);
                RestartARFoundationManagers(m_arRaycastManagers);
            }
            SetARComponentsToActive();

            // Restart the coroutine to handle subsequent remoting sessions.
            StartCoroutine(ManageARFoundationManagersAndARComponents());
        }

        private void RestartARFoundationManagers(Behaviour[] behaviours, bool ignoreBehaviourEnabled = false)
        {
            foreach(var behaviour in behaviours)
            {
                if(behaviour.enabled || ignoreBehaviourEnabled)
                {
                    behaviour.enabled = false;
                    behaviour.enabled = true;
                }
            }
        }

        private void SetARComponentsToInactive()
        {
            foreach(GameObject ARComponent in additionalARComponents)
            {
                ARComponent.SetActive(false);
            }
        }

        private void SetARComponentsToActive()
        {
            foreach(GameObject ARComponent in additionalARComponents)
            {
                ARComponent.SetActive(true);
            }
        }

    }
}