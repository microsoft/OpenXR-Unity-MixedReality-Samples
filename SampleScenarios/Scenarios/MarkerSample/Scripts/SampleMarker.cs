using Microsoft.MixedReality.OpenXR;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using Microsoft.MixedReality.OpenXR.Sample;
using System;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class SampleMarker : MonoBehaviour, ITextProvider
    {
        [SerializeField]
        private ARMarker m_marker;
        [SerializeField]
        private MeshRenderer m_markerRenderer;

        private string m_text = "Sample Marker";
        private const int m_countToUpdateFrame = 10;
        private int m_countUntilNextUpdate = 0;
        public string UpdateText()
        {
            if (m_marker != null && m_countUntilNextUpdate-- <= 0)
            {
                m_countUntilNextUpdate = m_countToUpdateFrame;
                m_text = $"{m_marker.trackableId} {m_marker.trackingState}\nDurationSinceLastSeen (s): {Math.Round(Time.realtimeSinceStartup - m_marker.lastSeenTime, 2)}\nLastSeenTime (s): {Math.Round(m_marker.lastSeenTime, 2)}";
            }
            return m_text;
        }

        public void Update()
        {
            if (m_marker != null && m_markerRenderer != null)
            {
                if (m_marker.trackingState != TrackingState.Tracking)
                {
                    m_markerRenderer.material.color = Color.gray;
                }
                else
                {
                    m_markerRenderer.material.color = Color.red;
                }
            }
        }
    }
}