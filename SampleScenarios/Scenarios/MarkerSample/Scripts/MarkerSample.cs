// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    [RequireComponent(typeof(ARMarkerManager))]
    public class MarkerSample : MonoBehaviour
    {
        [SerializeField]
        private TextMesh m_sampleText;
        private ARMarkerManager m_arMarkerManager;
        private TransformMode m_transformMode = TransformMode.MostStable;

        private void Awake()
        {
            m_arMarkerManager = GetComponent<ARMarkerManager>();
            if (!m_arMarkerManager.enabled || m_arMarkerManager.subsystem == null)
            {
                Debug.Log($"ARMarkerManager not enabled or available; sample marker functionality will not be enabled.");
                return;
            }
            m_transformMode = m_arMarkerManager.defaultTransformMode;
        }

        private void Update()
        {
            m_sampleText.text = $"Markers found: {m_arMarkerManager.trackables.count}\n" +
                $"Transform Mode: {m_transformMode}\n" +
                $"Default Transform Mode: {m_arMarkerManager.defaultTransformMode}";
        }

        /// <summary>
        /// Toggle the default transform mode for new detected markers.
        /// </summary>
        public void ToggleDefaultTransformMode()
        {
            if (m_arMarkerManager.defaultTransformMode == TransformMode.MostStable)
            {
                m_arMarkerManager.defaultTransformMode = TransformMode.Center;
            }
            else
            {
                m_arMarkerManager.defaultTransformMode = TransformMode.MostStable;
            }
        }

        /// <summary>
        /// Toggle the transform mode for each detected ARMarker.
        /// </summary>
        public void ToggleTransformMode()
        {
            if (m_transformMode == TransformMode.MostStable)
            {
                m_transformMode = TransformMode.Center;
            }
            else
            {
                m_transformMode = TransformMode.MostStable;
            }

            Debug.Log($"Time: {DateTime.Now} Toggle transform mode to {m_transformMode}");
            foreach (ARMarker marker in ARMarkerManager.Instance.trackables)
            {
                marker.transformMode = m_transformMode;
            }
        }
    }
}
