// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace Microsoft.MixedReality.OpenXR.Sample
{
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

        private void Update()
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
