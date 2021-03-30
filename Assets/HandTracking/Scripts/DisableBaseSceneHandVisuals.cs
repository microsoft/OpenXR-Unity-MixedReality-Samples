// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    /// <summary>
    /// The base scene for these samples contains visuals for hand joints. This script finds and disables them for the duration of this feature scene.
    /// </summary>
    public class DisableBaseSceneHandVisuals : MonoBehaviour
    {
        private GameObject m_baseSceneHandVisuals;

        void OnEnable()
        {
            GameObject sampleSceneUtilities = GameObject.Find("SampleSceneUtilities");
            if (sampleSceneUtilities == null)
                return;

            m_baseSceneHandVisuals = sampleSceneUtilities.transform.Find("Hand Visuals").gameObject;
            m_baseSceneHandVisuals?.SetActive(false);
        }

        void OnDisable()
        {
            m_baseSceneHandVisuals?.SetActive(true);
        }
    }

}