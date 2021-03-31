// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// The base scene for these samples contains visuals for hand joints. This script finds and disables them for the duration of this feature scene.
    /// </summary>
    public class DisableBaseSceneHandVisuals : MonoBehaviour
    {
        private GameObject m_baseSceneHandVisuals = null;

        void OnEnable()
        {
            GameObject sampleSceneUtilities = GameObject.Find("SampleSceneUtilities");
            if (sampleSceneUtilities == null)
                return;

            m_baseSceneHandVisuals = sampleSceneUtilities.transform.Find("Hand Visuals").gameObject;
            if (m_baseSceneHandVisuals != null)
            {
                m_baseSceneHandVisuals.SetActive(false);
            }
        }

        void OnDisable()
        {
            if (m_baseSceneHandVisuals != null)
            {
                m_baseSceneHandVisuals.SetActive(true);
            }
        }
    }

}