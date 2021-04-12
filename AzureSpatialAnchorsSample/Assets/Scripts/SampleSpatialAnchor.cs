// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class SampleSpatialAnchor : MonoBehaviour
    {
        [SerializeField]
        private TextMesh text = null;
        [SerializeField]
        private MeshRenderer meshRenderer = null;
        [SerializeField]
        private Material persistentAnchorMaterial = null;
        [SerializeField]
        private Material transientAnchorMaterial = null;

        private string m_identifier = "";
        public string Identifier
        {
            get => m_identifier;
            set
            {
                m_identifier = value;
                UpdateText();
            }
        }

        private bool m_persisted = false;
        public bool Persisted
        {
            get => m_persisted;
            set
            {
                m_persisted = value;
                meshRenderer.material = m_persisted ? persistentAnchorMaterial : transientAnchorMaterial;
                UpdateText();
            }
        }

        private void Start() => UpdateText();

        private void UpdateText()
        {
            string positionString = transform.position.ToString("F3");
            string persistenceString = Persisted ? m_identifier : "Air tap inside box to save to cloud";
            text.text = positionString + "\n" + persistenceString;
        }
    }
}