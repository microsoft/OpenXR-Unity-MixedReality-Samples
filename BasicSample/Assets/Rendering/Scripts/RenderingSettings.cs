// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
		}
	}
}
