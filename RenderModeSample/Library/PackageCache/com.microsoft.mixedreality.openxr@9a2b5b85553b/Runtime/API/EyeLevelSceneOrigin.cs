// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Add the EyeLevelSceneOrigin component to the scene, it will automatically
    /// switch the Unity's scene origin to an eye level experiences.
    /// It will try to use "Unbounded" origin mode when it's supported.
    /// </summary>
    public class EyeLevelSceneOrigin : MonoBehaviour
    {
        private XRInputSubsystem m_inputSubsystem;

        private void OnEnable()
        {
            XRGeneralSettings xrSettings = XRGeneralSettings.Instance;
            if (xrSettings == null)
            {
                Debug.LogWarning($"EyeLevelSceneOrigin: XRGeneralSettings is null.");
                return;
            }

            XRManagerSettings xrManager = xrSettings.Manager;
            if (xrManager == null)
            {
                Debug.LogWarning($"EyeLevelSceneOrigin: XRManagerSettings is null.");
                return;
            }

            XRLoader xrLoader = xrManager.activeLoader;
            if (xrLoader == null)
            {
                Debug.LogWarning($"EyeLevelSceneOrigin: XRLoader is null.");
                return;
            }

            m_inputSubsystem = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();
            if (m_inputSubsystem == null)
            {
                Debug.LogWarning($"EyeLevelSceneOrigin: XRInputSubsystem is null.");
                return;
            }

            m_inputSubsystem.trackingOriginUpdated += XrInput_trackingOriginUpdated;

            EnsureSceneOriginAtEyeLevel();
        }

        private void OnDisable()
        {
            if (m_inputSubsystem != null)
            {
                m_inputSubsystem.trackingOriginUpdated -= XrInput_trackingOriginUpdated;
                m_inputSubsystem = null;
            }
        }

        private void XrInput_trackingOriginUpdated(XRInputSubsystem obj)
        {
            if (isActiveAndEnabled)
            {
                EnsureSceneOriginAtEyeLevel();
            }
        }

        private void EnsureSceneOriginAtEyeLevel()
        {
            TrackingOriginModeFlags currentMode = m_inputSubsystem.GetTrackingOriginMode();
            TrackingOriginModeFlags desiredMode = GetDesiredTrackingOriginMode(m_inputSubsystem);
            bool isEyeLevel = currentMode == TrackingOriginModeFlags.Device || currentMode == TrackingOriginModeFlags.Unbounded;
            if (!isEyeLevel || currentMode != desiredMode)
            {
                Debug.Log($"EyeLevelSceneOrigin: TrySetTrackingOriginMode to {desiredMode}");
                if (!m_inputSubsystem.TrySetTrackingOriginMode(desiredMode))
                {
                    Debug.LogWarning($"EyeLevelSceneOrigin: Failed to set tracking origin to {desiredMode}.");
                }
            }
        }

        private static TrackingOriginModeFlags GetDesiredTrackingOriginMode(XRInputSubsystem xrInput)
        {
            TrackingOriginModeFlags supportedFlags = xrInput.GetSupportedTrackingOriginModes();
            TrackingOriginModeFlags targetFlag = TrackingOriginModeFlags.Device;   // All OpenXR runtime must support LOCAL space

            if (supportedFlags.HasFlag(TrackingOriginModeFlags.Unbounded))
            {
                targetFlag = TrackingOriginModeFlags.Unbounded;
            }

            return targetFlag;
        }
    }
}