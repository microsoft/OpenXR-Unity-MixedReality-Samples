// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace Microsoft.MixedReality.OpenXR.Sample
{

    /// <summary>
    /// Toggles the active tracking map type between Shared (default) and AppExclusive,
    /// so long as AppExclusive mode is supported on this device.
    /// </summary>
    public class ToggleAppExclusiveMaps : MonoBehaviour
    {
        private TrackingMapManager m_trackingMapManager;
        private TrackingMapType m_trackingMapType = TrackingMapType.Shared;
        private bool m_supportsApplicationExclusiveMaps = false;
        private Guid? m_guid = null;

        [SerializeField]
        private TextMesh tappableText;

        protected void UpdateVisuals() => tappableText.text = $"{MapLabel}: {MapValue}\n\nTap to change";
        protected string MapValue { get; set; }
        protected string MapLabel => (m_supportsApplicationExclusiveMaps ? "Active Map Type" : "App Exclusive Maps");

        protected void Start()
        {
            Initialize();
            UpdateVisuals();
        }

        protected async void Initialize()
        {
            m_trackingMapManager = await Microsoft.MixedReality.OpenXR.TrackingMapManager.GetAsync();
            m_supportsApplicationExclusiveMaps = m_trackingMapManager.IsSupported(TrackingMapType.ApplicationExclusive);
            MapValue = (m_supportsApplicationExclusiveMaps ? m_trackingMapType.ToString() : "Not Supported");
            UpdateVisuals();
        }

        protected void Update()
        {
            if ((m_trackingMapManager != null) && m_supportsApplicationExclusiveMaps)
            {
                TrackingMapType newTrackingMapType = m_trackingMapManager.ActiveTrackingMapType;
                if (m_trackingMapType != newTrackingMapType)
                {
                    m_trackingMapType = newTrackingMapType;
                    MapValue = m_trackingMapType.ToString();
                    UpdateVisuals();
                }
            }
        }

        /// <summary>
        /// Toggles between Default Shared and AppExclusive Maps. 
        /// When toggling to AppExclusive map, will attempt to use the ID of the previous AppExclusive map. 
        /// Otherwise, a new AppExclusive map will be created. 
        /// </summary>
        public async void ToggleMapMode()
        {
            Debug.Log($"TrackingMapManager supports ApplicationExclusive maps: {m_supportsApplicationExclusiveMaps}");
            if (m_supportsApplicationExclusiveMaps)
            {
                if (m_trackingMapType == TrackingMapType.Shared)
                {
                    Guid newGUID = await m_trackingMapManager.ActivateApplicationExclusiveMapAsync(m_guid);
                    if (newGUID == m_guid)
                    {
                        Debug.Log("Activated AppExclusive Map");
                    }
                    else
                    {
                        m_guid = newGUID;
                        Debug.Log("Created New AppExclusive Map");
                    }
                }
                else
                {
                    await m_trackingMapManager.ActivateSharedMapAsync();
                    Debug.Log("Activated Default Shared Map");
                }
            }
        }
    }
}
