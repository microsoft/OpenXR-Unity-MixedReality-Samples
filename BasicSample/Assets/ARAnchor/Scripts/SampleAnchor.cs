// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// A sample anchor to be used with <c>AnchorsSample.cs</c>, providing extra visuals to indicate its persistence status. 
    /// </summary>
    [RequireComponent(typeof(ARAnchor))]
    public class SampleAnchor : MonoBehaviour
    {
        [SerializeField]
        private TextMesh text = null;
        [SerializeField]
        private MeshRenderer meshRenderer = null;
        [SerializeField]
        private Material persistentAnchorMaterial = null;
        [SerializeField]
        private Material transientAnchorMaterial = null;
        [SerializeField]
        private Material untrackedAnchorMaterial = null;

        private bool m_textChanged = true;
        private ARAnchor m_arAnchor;

        private string m_name = "";
        public string Name
        {
            get => m_name;
            set
            {
                if (m_name != value)
                {
                    m_name = value;
                    m_textChanged = true;
                }
            }
        }

        private bool m_persisted = false;
        public bool Persisted
        {
            get => m_persisted;
            set
            {
                if (m_persisted != value)
                {
                    m_persisted = value;
                    m_textChanged = true;
                    meshRenderer.material = m_trackingState == TrackingState.Tracking
                        ? (m_persisted ? persistentAnchorMaterial : transientAnchorMaterial) : untrackedAnchorMaterial;
                }
            }
        }

        private TrackingState m_trackingState;
        public TrackingState TrackingState
        {
            get => m_trackingState;
            set
            {
                if (m_trackingState != value)
                {
                    m_trackingState = value;
                    m_textChanged = true;
                    meshRenderer.material = m_trackingState == TrackingState.Tracking
                        ? (m_persisted ? persistentAnchorMaterial : transientAnchorMaterial) : untrackedAnchorMaterial;
                }
            }
        }

        private void Start()
        {
            m_arAnchor = GetComponent<ARAnchor>();
        }

        private void Update()
        {
            if (m_textChanged && text != null)
            {
                string info = Persisted ? $"Name: \"{Name}\"" : "";
                if (m_arAnchor != null)
                {
                    info = $"{m_arAnchor.trackableId}\n{info}{(Persisted ? ", " : "")}Tracking State: {TrackingState}";
                }

                if (text.text != info)
                {
                    text.text = info;
                }
            }
        }
    }
}
