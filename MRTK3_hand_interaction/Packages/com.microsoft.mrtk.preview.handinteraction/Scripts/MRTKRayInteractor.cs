// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Microsoft.MixedReality.Toolkit.Preview.HandInteraction
{
    public class MRTKRayInteractor : Input.MRTKRayInteractor
    {
        [SerializeField]
        [Tooltip("An action representing if the pose is ready for pointing.")]
        private InputActionReference pointerActivateReady;

        /// <inheritdoc />
        public override bool isHoverActive
        {
            get
            {
                // When the gaze pinch interactor is already selecting an object, use the default interactor behavior
                if (hasSelection)
                {
                    return base.isHoverActive;
                }
                // Otherwise, this selector is only allowed to hover if we can tell that the palm for the corresponding hand/controller is facing away from the user.
                else
                {
                    bool hoverActive = allowHover;
                    if (hoverActive)
                    {
                        if (pointerActivateReady.action.HasAnyControls())
                        {
                            hoverActive &= pointerActivateReady.action.IsPressed();
                        }
                        else
                        {
                            return base.isHoverActive;
                        }
                    }

                    return hoverActive && xrController.currentControllerState.inputTrackingState.HasPositionAndRotation();
                }
            }
        }
    }
}
