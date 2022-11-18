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
        private GameObject pinchSlider = null;

        [SerializeField]
        private RenderingSettings renderingSettings;

        // Start is called before the first frame update
        void Start()
        {
            if (pinchSlider)
            {
                pinchSlider.GetComponent<PinchSlider>().SliderValue = (float)((renderingSettings.stereoSeparationAdjustment + 0.05) * 10);
            }

        }

        public void AdjustStereoSeparationSlider(SliderEventData sliderEventData)
        {
            renderingSettings.stereoSeparationAdjustment = (float)Math.Round((sliderEventData.NewValue - 0.5) / 10, 3);
        }
    }
}
