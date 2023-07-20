// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Sample
{
    public class AxesPrefab : PrefabMonoBehaviour
    {
        [SerializeField, Tooltip("OpenXR Mode Axes is right hand system with Z axis inverted vs. Unity's left hand system.")]
        private bool m_openXRMode = false;
        public bool OpenXRMode
        {
            get { return m_openXRMode; }
            set { SetPropertyValue(ref m_openXRMode, value); }
        }

        [SerializeField]
        private float m_centerSize = 0.1f;
        public float CenterSize
        {
            get { return m_centerSize; }
            set { SetPropertyValue(ref m_centerSize, value); }
        }

        [SerializeField]
        private Material m_centerMaterial = null;
        public Material CenterMaterial
        {
            get { return m_centerMaterial; }
            set { SetPropertyObject(ref m_centerMaterial, value); }
        }


        [Serializable]
        private struct Context
        {
            [SerializeField]
            internal GameObject centerObject;
            internal MeshRenderer centerMeshRenderer;

            [SerializeField] internal GameObject xAxis;
            [SerializeField] internal GameObject yAxis;
            [SerializeField] internal GameObject zAxis;
        };

        [SerializeField]
        private Context m_context;

        protected override void InitializeComponents()
        {
#if UNITY_EDITOR
            if (m_context.centerObject == null ||
                m_context.xAxis == null ||
                m_context.yAxis == null ||
                m_context.zAxis == null)
            {
                Debug.LogError("AxesPrefab was not correctly configured!");
                return;
            }
#endif

            m_context.centerMeshRenderer = m_context.centerObject.GetComponent<MeshRenderer>();
        }

        protected override void UpdateChildren()
        {
            if (m_context.centerObject != null)
            {
                m_context.centerObject.transform.localScale = Vector3.one * m_centerSize;
                if (m_centerMaterial != null && m_context.centerMeshRenderer != null)
                {
                    m_context.centerObject.SetActive(true);
                    m_context.centerMeshRenderer.material = m_centerMaterial;
                }
                else
                {
                    m_context.centerObject.SetActive(false);
                }
            }

            const float halfPI = (float)Math.PI / 2;

            if (m_context.zAxis != null)
            {
                m_context.zAxis.transform.localRotation = new Quaternion(0, m_openXRMode ? -halfPI : halfPI, halfPI, 0);
            }
        }
    }
}
