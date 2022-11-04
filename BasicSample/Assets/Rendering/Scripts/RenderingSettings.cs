// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class RenderingSettings : MonoBehaviour
    {
        [SerializeField]
        private TextMesh m_statusPanel;

        private ReprojectionMode[] allReprojectionModes = (ReprojectionMode[])Enum.GetValues(typeof(Microsoft.MixedReality.OpenXR.ReprojectionMode));
        private ReprojectionMode targetReprojectionMode = ReprojectionMode.Depth;
        private float m_stereoSeparationAdjustment = 0;
        private bool m_stereoSeparationAdjustmentChanged = false;

        public void ChangeRenderMode()
        {
            if (OpenXRSettings.Instance.renderMode == OpenXRSettings.RenderMode.SinglePassInstanced)
            {
                OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.MultiPass;
            }
            else
            {
                OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
            }
        }

        public void ChangeTargetReprojectionMode()
        {
            int idx = Array.IndexOf(allReprojectionModes, targetReprojectionMode);
            idx = (idx + 1) % allReprojectionModes.Count();
            targetReprojectionMode = allReprojectionModes[idx];
        }

        public void AdjustStereoSeparationSlider(SliderEventData sliderEventData)
        {
            m_stereoSeparationAdjustment = (float)Math.Round((sliderEventData.NewValue - 0.5) / 10, 3);
            m_stereoSeparationAdjustmentChanged = true;
        }

        void Start()
        {
            if (ViewConfiguration.Primary != null)
            {
                m_stereoSeparationAdjustment = ViewConfiguration.Primary.StereoSeparationAdjustment;
            }

            GameObject pinchSlider = GameObject.Find("PinchSlider");
            if (pinchSlider)
            {
                pinchSlider.GetComponent<PinchSlider>().SliderValue = (float)((m_stereoSeparationAdjustment + 0.05) * 10);
            }
        }

        void Update()
        {
            m_statusPanel.text = $"Current Render Mode: {OpenXRSettings.Instance.renderMode}\n";
            m_statusPanel.text += $"Target Reprojection Mode: {targetReprojectionMode}\n";

            foreach (ViewConfiguration viewConfiguration in ViewConfiguration.EnabledViewConfigurations)
            {
                m_statusPanel.text += $"View Configuration {viewConfiguration.ViewConfigurationType}: - {(viewConfiguration.IsActive ? "Active" : "Not Active")}\n";
                if (viewConfiguration.IsActive)
                {
                    m_statusPanel.text += $"\tSupports Modes: [{string.Join(", ", viewConfiguration.SupportedReprojectionModes)}]\n";
                }

                if (viewConfiguration.SupportedReprojectionModes.Contains(targetReprojectionMode))
                {
                    ReprojectionSettings settings = new ReprojectionSettings();
                    settings.ReprojectionMode = targetReprojectionMode;
                    if (targetReprojectionMode == ReprojectionMode.PlanarManual)
                    {
                        // Override the reprojection plane to be 10m. away for more obvious results.
                        settings.ReprojectionPlaneOverridePosition = new Vector3(0, 0, 10);
                        settings.ReprojectionPlaneOverrideNormal = new Vector3(0, 0, 1);
                        settings.ReprojectionPlaneOverrideVelocity = new Vector3(0, 0, 0);
                        m_statusPanel.text += "\tUsing reprojection override plane 10m. away\n";
                    }
                    viewConfiguration.SetReprojectionSettings(settings);
                }
                else
                {
                    m_statusPanel.text += "\tTarget reprojection mode not supported!\n";
                }

            }

            if (Camera.allCameras.Length > 0)
            {
                Vector3 rightEye = (Vector3)(Camera.allCameras[0].GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse.GetColumn(3));
                Vector3 leftEye = (Vector3)(Camera.allCameras[0].GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse.GetColumn(3));
                m_statusPanel.text += $"\tStereo separation: {(rightEye - leftEye).magnitude}";
            }
            m_statusPanel.text += $"\tStereo separation adjustment: {m_stereoSeparationAdjustment}";
            if (m_stereoSeparationAdjustmentChanged)
            {
                m_stereoSeparationAdjustmentChanged = false;
                if (ViewConfiguration.Primary != null)
                {
                    ViewConfiguration.Primary.StereoSeparationAdjustment = m_stereoSeparationAdjustment;
                }
            }
        }
    }
}
