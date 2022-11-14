// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.Sample;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class RenderingSettings : MonoBehaviour, ITextProvider
    {
        private string m_text;

        private ReprojectionMode[] allReprojectionModes = (ReprojectionMode[])Enum.GetValues(typeof(Microsoft.MixedReality.OpenXR.ReprojectionMode));
        private ReprojectionMode targetReprojectionMode = ReprojectionMode.Depth;
        public float m_stereoSeparationAdjustment { get; set; } = 0;
        public bool m_stereoSeparationAdjustmentChanged { get; set; } = false;

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

        void Start()
        {
            if (ViewConfiguration.Primary != null)
            {
                m_stereoSeparationAdjustment = ViewConfiguration.Primary.StereoSeparationAdjustment;
            }
        }

        void Update()
        {
            m_text = $"Current Render Mode: {OpenXRSettings.Instance.renderMode}\n";
            m_text += $"Target Reprojection Mode: {targetReprojectionMode}\n";

            foreach (ViewConfiguration viewConfiguration in ViewConfiguration.EnabledViewConfigurations)
            {
                m_text += $"View Configuration {viewConfiguration.ViewConfigurationType}: - {(viewConfiguration.IsActive ? "Active" : "Not Active")}\n";
                if (viewConfiguration.IsActive)
                {
                    m_text += $"\tSupports Modes: [{string.Join(", ", viewConfiguration.SupportedReprojectionModes)}]\n";
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
                        m_text += "\tUsing reprojection override plane 10m. away\n";
                    }
                    viewConfiguration.SetReprojectionSettings(settings);
                }
                else
                {
                    m_text += "\tTarget reprojection mode not supported!\n";
                }

            }

            if (Camera.allCameras.Length > 0)
            {
                // The stereo separation value may be incorrect due to a known Unity bug that will be fixed in a future release.
                m_text += $"\tStereo separation: {Camera.allCameras[0].stereoSeparation}\n";
            }
            m_text += $"\tStereo separation adjustment: {m_stereoSeparationAdjustment}";
            if (m_stereoSeparationAdjustmentChanged)
            {
                m_stereoSeparationAdjustmentChanged = false;
                if (ViewConfiguration.Primary != null)
                {
                    ViewConfiguration.Primary.StereoSeparationAdjustment = m_stereoSeparationAdjustment;
                }
            }
        }

        public string UpdateText()
        {
            return m_text;
        }
    }
}
