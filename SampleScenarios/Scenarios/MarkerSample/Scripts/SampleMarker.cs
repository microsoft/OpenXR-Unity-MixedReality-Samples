using Microsoft.MixedReality.OpenXR;
using UnityEngine;
using Microsoft.MixedReality.OpenXR.Sample;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class SampleMarker : MonoBehaviour, ITextProvider
    {
        [SerializeField]
        private ARMarker m_marker;
        private string m_text = "Sample Marker";
        public string UpdateText()
        {
            if (m_marker != null)
            {
                m_text = $"{m_marker.trackableId} {m_marker.trackingState}\nTimeSinceLastSeen (ms): {(Time.realtimeSinceStartup - m_marker.lastSeenTime) * 1000}";
            }
            return m_text;
        }
    }
}