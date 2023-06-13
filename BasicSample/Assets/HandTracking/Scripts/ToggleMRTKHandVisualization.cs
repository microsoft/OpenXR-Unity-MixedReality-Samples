// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    /// <summary>
    /// Turns the MRTK hand visualization off when the scene is loaded, to defer to the sample scene's representation.
    /// Returns the MRTK settings to their original state when the scene is unloaded.
    /// </summary>
    public class ToggleMRTKHandVisualization : MonoBehaviour
    {
        private bool originalHandMeshVisualization = true;
        private bool originalHandJointVisualization = true;
        private MixedRealityHandTrackingProfile handTrackingProfile;

        /// <summary>
        /// Turns the MRTK hand visualization off when the scene is loaded, to defer to the sample scene's representation.
        /// </summary>
        private void Start()
        {
            MixedRealityInputSystemProfile inputSystemProfile = CoreServices.InputSystem?.InputSystemProfile;
            if (inputSystemProfile == null)
            {
                return;
            }

            handTrackingProfile = inputSystemProfile.HandTrackingProfile;
            if (handTrackingProfile != null)
            {
                originalHandMeshVisualization = handTrackingProfile.EnableHandMeshVisualization;
                originalHandJointVisualization = handTrackingProfile.EnableHandJointVisualization;
                handTrackingProfile.EnableHandMeshVisualization = false;
                handTrackingProfile.EnableHandJointVisualization = false;
            }
        }

        /// <summary>
        /// Returns the MRTK settings to their original state when the scene is unloaded.
        /// </summary>
        private void OnDestroy()
        {
            if (handTrackingProfile != null)
            {
                handTrackingProfile.EnableHandMeshVisualization = originalHandMeshVisualization;
                handTrackingProfile.EnableHandJointVisualization = originalHandJointVisualization;
            }
        }
    }
}
