// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.XR.ARFoundation;

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

        public string Name { get; set; }

        private ARAnchor m_arAnchor;
        private bool m_persisted = false;
        public bool Persisted
        {
            get => m_persisted;
            set
            {
                if (m_persisted != value)
                {
                    m_persisted = value;
                    meshRenderer.material = m_persisted ? persistentAnchorMaterial : transientAnchorMaterial;
                }
            }
        }

        private void Start()
        {
            m_arAnchor = GetComponent<ARAnchor>();
        }

        private void Update()
        {
            string info = Persisted ? $"\"{Name}\": " : "";
            if (m_arAnchor != null)
            {
                info = $"{m_arAnchor.trackableId}\n{m_arAnchor.trackingState} " + info;
            }

            if (text.text != info)
            {
                text.text = info;
            }
        }
    }
}