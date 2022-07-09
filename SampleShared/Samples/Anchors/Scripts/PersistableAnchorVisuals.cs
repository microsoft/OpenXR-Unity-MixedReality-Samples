// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    /// <summary>
    /// A component to be used in various anchor sample scenarios, providing visuals
    /// to indicate this anchor's name, tracking state, and persistence status. 
    /// </summary>
    [RequireComponent(typeof(ARAnchor))]
    public class PersistableAnchorVisuals : MonoBehaviour
    {
        [SerializeField]
        private TextMesh textMesh = null;
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

        /// <summary>
        /// Whether this script should manage the TrackingState property.
        /// This helps keep some sample scenarios more simple, at the cost of suboptimal performance.
        /// </summary>
        public bool ManageOwnTrackingState { get; set; } = false;

        /// <summary>
        /// A function which will format the text on this anchor. Can be overridden for use in various samples.
        /// </summary>
        public Func<PersistableAnchorVisuals, string> AnchorTextFormatter { get; set; }

        private string DefaultAnchorTextFormatter(PersistableAnchorVisuals visuals)
        {
            return $"{visuals.m_arAnchor.trackableId}\n{(visuals.Persisted ? $"Name: \"{visuals.Name}\", " : "")}Tracking State: {visuals.TrackingState}";
        }

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

        private PersistableAnchorVisuals()
        {
            // Initialize this before Awake() and Start() so other scripts can consistently overwrite it
            AnchorTextFormatter = DefaultAnchorTextFormatter;
        }

        private void Start()
        {
            m_arAnchor = GetComponent<ARAnchor>();
            TrackingState = m_arAnchor.trackingState;
        }

        private void Update()
        {
            if (ManageOwnTrackingState)
            {
                TrackingState = m_arAnchor.trackingState;
            }

            if (m_textChanged && textMesh != null)
            {
                string info = AnchorTextFormatter != null ? AnchorTextFormatter(this) : "";

                if (textMesh.text != info)
                {
                    textMesh.text = info;
                }

                m_textChanged = false;
            }
        }
    }
}
