using Microsoft.MixedReality.OpenXR.Samples;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class StereoSeparationSlider : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_pinchSlider = null;

        public RenderingSettings m_renderingSettings;

        // Start is called before the first frame update
        void Start()
        {
            if (m_pinchSlider)
            {
                m_pinchSlider.GetComponent<PinchSlider>().SliderValue = (float)((m_renderingSettings.m_stereoSeparationAdjustment + 0.05) * 10);
            }

        }

        public void AdjustStereoSeparationSlider(SliderEventData sliderEventData)
        {
            m_renderingSettings.m_stereoSeparationAdjustment = (float)Math.Round((sliderEventData.NewValue - 0.5) / 10, 3);
            m_renderingSettings.m_stereoSeparationAdjustmentChanged = true;
        }
    }
}
