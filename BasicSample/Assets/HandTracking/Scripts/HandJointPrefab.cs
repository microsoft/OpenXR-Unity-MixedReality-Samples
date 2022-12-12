// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class HandJointPrefab : PrefabMonoBehaviour
    {
        [SerializeField]
        private bool m_showRadius = true;
        public bool ShowRadius
        {
            get { return m_showRadius; }
            set { SetPropertyValue(ref m_showRadius, value); }
        }

        [SerializeField]
        private AxesPrefab m_axesPrefab = null;
        public AxesPrefab AxesPrefab => m_axesPrefab;

        [SerializeField]
        private GameObject m_radiusObject = null;

        protected override void UpdateChildren()
        {
            if (m_radiusObject != null)
            {
                m_radiusObject.SetActive(m_showRadius);

                Renderer renderer = m_radiusObject.GetComponent<Renderer>();
                Material material = renderer.sharedMaterial;
                material.SetColor("Wire color", Color.yellow);
            }
        }
    }
}