// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class SamplePlane : MonoBehaviour
    {
        private ARPlane m_arPlane;

        [SerializeField]
        private TextMesh m_text = null;

        [SerializeField]
        private Transform m_planeSpaceTransform;

        [SerializeField]
        private MeshRenderer[] m_planeMeshRenderers;

        [SerializeField]
        private PlaneMaterialLookup m_planeMaterialLookup;

        private void OnEnable()
        {
            m_arPlane = GetComponent<ARPlane>();
            if (m_arPlane == null)
            {
                Debug.LogWarning("Plane Prefab could not find ARPlane Component!");
                return;
            }

            if (m_text != null)
            {
                m_text.text = $"{m_arPlane.trackableId.ToString()}\n" +
                        Enum.GetName(typeof(UnityEngine.XR.ARSubsystems.PlaneClassification), m_arPlane.classification);
            }

            m_planeSpaceTransform.transform.localScale = new Vector3(m_arPlane.size.x, 1, m_arPlane.size.y);

            foreach (MeshRenderer mesh in m_planeMeshRenderers)
                mesh.material = m_planeMaterialLookup.GetMaterialFromClassification(m_arPlane.classification);
        }
    }
}
