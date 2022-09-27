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
        private float m_viewDistanceAdjustment = 0;
        private bool m_viewDistanceAdjustmentChanged = false;

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

        public void AdjustViewDistanceSlider(SliderEventData sliderEventData)
        {
            m_viewDistanceAdjustment = (float)Math.Round((sliderEventData.NewValue - 0.5)/10, 3);
            m_viewDistanceAdjustmentChanged = true;
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

            m_statusPanel.text += $"\tView Distance adjustment: {m_viewDistanceAdjustment}";
            if (m_viewDistanceAdjustmentChanged)
            {
                m_viewDistanceAdjustmentChanged = false;
                if (ViewConfiguration.Primary != null)
                {
                    ViewConfiguration primary = (ViewConfiguration)ViewConfiguration.Primary;
                    primary.ViewDistanceAdjustment = m_viewDistanceAdjustment;
                }
            }
        }
    }
}
