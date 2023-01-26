// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Microsoft.MixedReality.Toolkit.Preview.HandInteraction
{
    public class PokePoseSource : HandBasedPoseSource
    {
        [SerializeField]
        private InputActionProperty pokeRadiusActionProperty;

        [SerializeField]
        private InputActionProperty trackingStateActionProperty;

        [SerializeField]
        private InputActionProperty positionActionProperty;

        [SerializeField]
        private InputActionProperty rotationActionProperty;

        /// <summary>
        /// Tries to get the pose in worldspace composed of the provided input action properties when the position and rotation are tracked
        /// </summary>
        public override bool TryGetPose(out Pose pose)
        {
            InputAction trackingStateAction = trackingStateActionProperty.action;
            InputAction positionAction = positionActionProperty.action;
            InputAction rotationAction = rotationActionProperty.action;
            InputAction pokeRadiusAction = pokeRadiusActionProperty.action;

            if (trackingStateAction.HasAnyControls()
                && positionAction.HasAnyControls()
                && rotationAction.HasAnyControls()
                && ((InputTrackingState)trackingStateAction.ReadValue<int>() & (InputTrackingState.Position | InputTrackingState.Rotation)) != 0)
            {
                // Transform the pose into worldspace, as input actions are returned
                // in floor-offset-relative coordinates.
                pose = PlayspaceUtilities.TransformPose(
                    new Pose(
                        positionAction.ReadValue<Vector3>(),
                        rotationAction.ReadValue<Quaternion>()));

                float radius = -1;

                // Check if this can be offset by the index tip radius
                if (pokeRadiusAction.HasAnyControls())
                {
                    radius = pokeRadiusAction.ReadValue<float>();
                }
                else
                {
                    XRNode? handNode = Hand.ToXRNode();
                    if (handNode.HasValue
                        && HandsAggregator != null
                        && HandsAggregator.TryGetJoint(TrackedHandJoint.IndexTip, handNode.Value, out HandJointPose handJointPose))
                    {
                        radius = handJointPose.Radius;
                    }
                }

                Debug.Log(radius);

                if (radius > 0)
                {
                    pose.position -= pose.forward * radius;
                }

                return true;
            }
            else
            {
                pose = Pose.identity;
                return false;
            }
        }
    }
}
