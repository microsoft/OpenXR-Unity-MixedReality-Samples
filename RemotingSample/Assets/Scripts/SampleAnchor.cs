// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Microsoft.MixedReality.OpenXR.Samples
{
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

        private ARAnchor m_arAnchor;
        private string m_name = "";
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
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

        private void UpdateText()
        {
            if (m_arAnchor == null) return;
            text.text = (Persisted ? $"\"{m_name}\": " : "") + m_arAnchor.trackableId.ToString();
        }

        private void Start()
        {
            m_arAnchor = GetComponent<ARAnchor>();
            if (m_arAnchor == null)
            {
                Debug.LogWarning("Anchor Prefab could not find ARAnchor Component!");
                return;
            }
            UpdateText();
        }
    }
}