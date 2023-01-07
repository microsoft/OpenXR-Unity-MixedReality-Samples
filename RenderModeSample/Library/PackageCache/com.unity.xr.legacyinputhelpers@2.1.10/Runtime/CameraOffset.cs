using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if ENABLE_VR || ENABLE_AR
using UnityEngine.XR;

namespace UnityEditor.XR.LegacyInputHelpers
{

    public enum UserRequestedTrackingMode
    {
        Default,
        Device,
        Floor,
    }

    [AddComponentMenu("XR/Camera Offset")]
    public class CameraOffset : MonoBehaviour
    {

        const float k_DefaultCameraYOffset = 1.36144f;

        [SerializeField]
        [Tooltip("GameObject to move to desired height off the floor (defaults to this object if none provided).")]
        GameObject m_CameraFloorOffsetObject = null;
        /// <summary>Gets or sets the GameObject to move to desired height off the floor (defaults to this object if none provided).</summary>
        public GameObject cameraFloorOffsetObject { get { return m_CameraFloorOffsetObject; } set { m_CameraFloorOffsetObject = value; UpdateTrackingOrigin(m_TrackingOriginMode); } }

        [SerializeField]
        [Tooltip("What the user wants the tracking origin mode to be")]
        UserRequestedTrackingMode m_RequestedTrackingMode = UserRequestedTrackingMode.Default;
        public UserRequestedTrackingMode requestedTrackingMode { get { return m_RequestedTrackingMode; } set { m_RequestedTrackingMode = value; TryInitializeCamera(); } }

#if UNITY_2019_3_OR_NEWER
        [SerializeField]
        [Tooltip("Sets the type of tracking origin to use for this Rig. Tracking origins identify where 0,0,0 is in the world of tracking.")]
        /// <summary>Gets or sets the type of tracking origin to use for this Rig. Tracking origins identify where 0,0,0 is in the world of tracking. Not all devices support all tracking spaces; if the selected tracking space is not set it will fall back to Stationary.</summary>
        TrackingOriginModeFlags m_TrackingOriginMode = TrackingOriginModeFlags.Unknown;
        public TrackingOriginModeFlags TrackingOriginMode { get { return m_TrackingOriginMode; } set { m_TrackingOriginMode = value; TryInitializeCamera(); } }
#endif

        // Disable Obsolete warnings for TrackingSpaceType, explicitly to read in old data and upgrade.
#pragma warning disable 0618
        [SerializeField]
        [Tooltip("Set if the XR experience is Room Scale or Stationary.")]
        TrackingSpaceType m_TrackingSpace = TrackingSpaceType.Stationary;

        /// <summary>Gets or sets if the experience is rooms scale or stationary.  Not all devices support all tracking spaces; if the selected tracking space is not set it will fall back to Stationary.</summary>
#if UNITY_2019_3_OR_NEWER
        [Obsolete("CameraOffset.trackingSpace is obsolete.  Please use CameraOffset.trackingOriginMode.")]
#endif
        public TrackingSpaceType trackingSpace { get { return m_TrackingSpace; } set { m_TrackingSpace = value; TryInitializeCamera(); } }
#pragma warning restore 0618

        [SerializeField]
        [Tooltip("Camera Height to be used when in Device tracking space.")]
        float m_CameraYOffset = k_DefaultCameraYOffset;
        /// <summary>Gets or sets the amount the camera is offset from the floor (by moving the camera offset object).</summary>
        public float cameraYOffset { get { return m_CameraYOffset; } set { m_CameraYOffset = value; UpdateTrackingOrigin(m_TrackingOriginMode); } }

        // Bookkeeping to track lazy initialization of the tracking space type.
        bool m_CameraInitialized = false;
        bool m_CameraInitializing = false;

#if UNITY_2019_3_OR_NEWER
        /// <summary>
        /// Used to cache the input subsystems without creating additional garbage.
        /// </summary>
        static List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();
#endif
        /// Utility helper to migrate from TrackingSpace to TrackingOrigin seamlessly	
        void UpgradeTrackingSpaceToTrackingOriginMode()
        {
#if UNITY_2019_3_OR_NEWER
            // Disable Obsolete warnings for TrackingSpaceType, explicitly to allow a proper upgrade path.	
#pragma warning disable 0618
            if (m_TrackingOriginMode == TrackingOriginModeFlags.Unknown && m_TrackingSpace <= TrackingSpaceType.RoomScale)
            {
                switch (m_TrackingSpace)
                {
                    case TrackingSpaceType.RoomScale:
                        {
                            m_TrackingOriginMode = TrackingOriginModeFlags.Floor;
                            break;
                        }
                    case TrackingSpaceType.Stationary:
                        {
                            m_TrackingOriginMode = TrackingOriginModeFlags.Device;
                            break;
                        }
                    default:
                        break;
                }

                // Tag is Invalid not to be used.	
                m_TrackingSpace = (TrackingSpaceType)3;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif //UNITY_EDITOR	
#pragma warning restore 0618
            }
#endif //UNITY_2019_3_OR_NEWER	
        }

        void Awake()
        {
            if (!m_CameraFloorOffsetObject)
            {
                Debug.LogWarning("No camera container specified for XR Rig, using attached GameObject");
                m_CameraFloorOffsetObject = this.gameObject;
            }
        }

        void Start()
        {
            TryInitializeCamera();
        }

        void OnValidate()
        {
            UpgradeTrackingSpaceToTrackingOriginMode();
            TryInitializeCamera();
        }

        void TryInitializeCamera()
        {
           
            m_CameraInitialized = SetupCamera();
            if (!m_CameraInitialized & !m_CameraInitializing)
                StartCoroutine(RepeatInitializeCamera());
        }

        /// <summary>
        /// Repeatedly attempt to initialize the camera.
        /// </summary>
        /// <returns></returns>
        IEnumerator RepeatInitializeCamera()
        {
            m_CameraInitializing = true;
            yield return null;
            while (!m_CameraInitialized)
            {
                m_CameraInitialized = SetupCamera();
                yield return null;
            }
            m_CameraInitializing = false;
        }

        /// <summary>
        /// Handles re-centering and off-setting the camera in space depending on which tracking space it is setup in.
        /// </summary>
#if UNITY_2019_3_OR_NEWER
        bool SetupCamera()
        {
            SubsystemManager.GetInstances<XRInputSubsystem>(s_InputSubsystems);

            bool initialized = true;
            if (s_InputSubsystems.Count != 0)
            {
                for (int i = 0; i < s_InputSubsystems.Count; i++)
                {
                    var result = SetupCamera(s_InputSubsystems[i]);

                    // After the camera is successfully set up register the callback for
                    // handing tracking origin changes.  It is possible this could happen more than
                    // once so unregister the callback first just in case.
                    if (result)
                    {
                        s_InputSubsystems[i].trackingOriginUpdated -= OnTrackingOriginUpdated;
                        s_InputSubsystems[i].trackingOriginUpdated += OnTrackingOriginUpdated;
                    }

                    initialized &= result;
                }
            }
            else
            {
                // Disable Obsolete warnings for TrackingSpaceType, explicitly to allow a proper upgrade path.
#pragma warning disable 0618

                if (m_RequestedTrackingMode == UserRequestedTrackingMode.Floor)
                {
                    SetupCameraLegacy(TrackingSpaceType.RoomScale);
                }
                else 
                {
                    SetupCameraLegacy(TrackingSpaceType.Stationary);
                }

#pragma warning restore 0618
            }

            return initialized;
        }

        bool SetupCamera(XRInputSubsystem subsystem)
        {
            if (subsystem == null)
                return false;

            bool trackingSettingsSet = false;

            var currentMode = subsystem.GetTrackingOriginMode();
            var supportedModes = subsystem.GetSupportedTrackingOriginModes();
            TrackingOriginModeFlags requestedMode = TrackingOriginModeFlags.Unknown;

            // map between the user requested options, and the actual options.
            if (m_RequestedTrackingMode == UserRequestedTrackingMode.Default)
            {
                requestedMode = currentMode;
            }
            else if(m_RequestedTrackingMode == UserRequestedTrackingMode.Device)
            {
                requestedMode = TrackingOriginModeFlags.Device;
            }
            else if (m_RequestedTrackingMode == UserRequestedTrackingMode.Floor)
            {
                requestedMode = TrackingOriginModeFlags.Floor;
            }
            else
            {
                Debug.LogWarning("Unknown Requested Tracking Mode");
            }

            // now we've mapped em. actually go set em.
            if (requestedMode == TrackingOriginModeFlags.Floor)
            {
                // We need to check for Unknown because we may not be in a state where we can read this data yet.
                if ((supportedModes & (TrackingOriginModeFlags.Floor | TrackingOriginModeFlags.Unknown)) == 0)
                    Debug.LogWarning("CameraOffset.SetupCamera: Attempting to set the tracking space to Floor, but that is not supported by the SDK.");
                else
                    trackingSettingsSet = subsystem.TrySetTrackingOriginMode(requestedMode);
            }
            else if (requestedMode == TrackingOriginModeFlags.Device)
            {
                // We need to check for Unknown because we may not be in a state where we can read this data yet.
                if ((supportedModes & (TrackingOriginModeFlags.Device | TrackingOriginModeFlags.Unknown)) == 0)
                    Debug.LogWarning("CameraOffset.SetupCamera: Attempting to set the tracking space to Device, but that is not supported by the SDK.");
                else
                    trackingSettingsSet = subsystem.TrySetTrackingOriginMode(requestedMode) && subsystem.TryRecenter();
            }

            if(trackingSettingsSet)
                UpdateTrackingOrigin(subsystem.GetTrackingOriginMode());
            
            return trackingSettingsSet;
        }

        private void UpdateTrackingOrigin(TrackingOriginModeFlags trackingOriginModeFlags)
        {
            m_TrackingOriginMode = trackingOriginModeFlags;
            
            if (m_CameraFloorOffsetObject != null)
                m_CameraFloorOffsetObject.transform.localPosition = new Vector3(
                    m_CameraFloorOffsetObject.transform.localPosition.x, 
                    m_TrackingOriginMode == TrackingOriginModeFlags.Device ? cameraYOffset : 0.0f, 
                    m_CameraFloorOffsetObject.transform.localPosition.z);
        }

        private void OnTrackingOriginUpdated(XRInputSubsystem subsystem) => UpdateTrackingOrigin(subsystem.GetTrackingOriginMode());

        private void OnDestroy()
        {
            SubsystemManager.GetInstances(s_InputSubsystems);
            foreach (var subsystem in s_InputSubsystems)
                subsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
        }
        
#else
        bool SetupCamera()
        {

            if (m_RequestedTrackingMode == UserRequestedTrackingMode.Floor)
            {
                SetupCameraLegacy(TrackingSpaceType.RoomScale);                
            }
            else if(m_RequestedTrackingMode == UserRequestedTrackingMode.Device)
            {
                SetupCameraLegacy(TrackingSpaceType.Stationary);
            }
            else if (m_RequestedTrackingMode == UserRequestedTrackingMode.Default)
            {
                TrackingSpaceType tst = XRDevice.GetTrackingSpaceType();
                SetupCameraLegacy(tst);
            }
            else
            {
                Debug.LogWarning("CameraOffset.SetupCamera: Unknown requested ");
            }

            return true;
        }
#endif

        // Disable Obsolete warnings for TrackingSpaceType, explicitly to allow for using legacy data if available.
#pragma warning disable 0618
        void SetupCameraLegacy(TrackingSpaceType trackingSpace)
        {
            float cameraYOffset = m_CameraYOffset;
            XRDevice.SetTrackingSpaceType(trackingSpace);
            if (trackingSpace == TrackingSpaceType.Stationary)
                InputTracking.Recenter();
            else if (trackingSpace == TrackingSpaceType.RoomScale)
                cameraYOffset = 0;

            m_TrackingSpace = trackingSpace;

            // Move camera to correct height
            if (m_CameraFloorOffsetObject)
                m_CameraFloorOffsetObject.transform.localPosition = new Vector3(m_CameraFloorOffsetObject.transform.localPosition.x, cameraYOffset, m_CameraFloorOffsetObject.transform.localPosition.z);
        }
#pragma warning restore 0618
    }
}

#endif