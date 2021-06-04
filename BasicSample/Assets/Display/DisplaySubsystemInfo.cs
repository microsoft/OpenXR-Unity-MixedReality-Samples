// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Microsoft.MixedReality.OpenXR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class DisplaySubsystemInfo : MonoBehaviour
    {
        [SerializeField]
        private TextMesh m_statusPanel;

        private ReprojectionMode[] allReprojectionModes = (ReprojectionMode[])Enum.GetValues(typeof(Microsoft.MixedReality.OpenXR.ReprojectionMode));
        private int reprojectionModeIdx = 0;

        void OnEnable()
        {
            ChangeReprojectionMode();
        }

        public void ChangeReprojectionMode()
        {
            reprojectionModeIdx = (reprojectionModeIdx + 1) % allReprojectionModes.Count();
        }

        void Update()
        {
            ReprojectionMode targetReprojectionMode = allReprojectionModes[reprojectionModeIdx];
            m_statusPanel.text = $"Target Reprojection Mode: {targetReprojectionMode}\n";

            var vcs = ViewConfiguration.EnabledViewConfigurations;

            foreach (ViewConfiguration viewConfiguration in vcs)
            {
                m_statusPanel.text += $"View Configuration:\n\t{viewConfiguration.ViewConfigurationType} - IsActive? {viewConfiguration.IsActive}\n";
                if (viewConfiguration.SupportedReprojectionModes.Contains(targetReprojectionMode))
                {
                    ReprojectionSettings settings = new ReprojectionSettings();
                    settings.ReprojectionMode = targetReprojectionMode;
                    if (targetReprojectionMode == ReprojectionMode.PlanarManual)
                    {
                        settings.ReprojectionPlaneOverridePosition = new Vector3(0, 0, 2);
                        settings.ReprojectionPlaneOverrideNormal = new Vector3(0, 0, 1);
                        settings.ReprojectionPlaneOverrideVelocity = new Vector3(0, 0, 0);
                    }
                    viewConfiguration.SetReprojectionSettings(settings);
                }
                else
                {
                    m_statusPanel.text += "\tReprojection mode not supported!\n";
                }
            }
        }
    }
}
