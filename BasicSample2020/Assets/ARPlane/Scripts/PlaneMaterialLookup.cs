// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Provides mappings from SamplePlanes' PlaneClassifications to classification-specific materials to be applied to the plane meshes.
    /// </summary>
    public class PlaneMaterialLookup : ScriptableObject
    {
        [Serializable]
        private class PlaneClassificationPair
        {
            public PlaneClassification classification = PlaneClassification.None;
            public Material material = null;
        };

        [SerializeField]
        private PlaneClassificationPair[] m_supportedClassifications;

        [SerializeField]
        private Material m_defaultMaterial;

        /// <summary>
        /// Provides a material to be used for rendering a SamplePlane with a given PlaneClassification.
        /// If the classification isn't supported for a unique material, the default plane material will be returned.
        /// </summary>
        public Material GetMaterialFromClassification(PlaneClassification classification)
        {
            foreach (PlaneClassificationPair pair in m_supportedClassifications)
                if (pair.classification == classification)
                    return pair.material;

            return m_defaultMaterial;
        }

    }
}