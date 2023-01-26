// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Microsoft.MixedReality.Toolkit.Preview.HandInteraction
{
    public class MRTKGrabInteractor : GrabInteractor
    {
        [SerializeReference]
        [InterfaceSelector(true)]
        [Tooltip("The pose source representing the worldspace pose of the hand grabbing point.")]
        private IPoseSource gripPoseSource;

        /// <summary>
        /// The pose source representing the worldspace pose of the hand pinching point.
        /// </summary>
        protected IPoseSource GripPoseSource { get => gripPoseSource; set => gripPoseSource = value; }

        private static readonly ProfilerMarker ProcessInteractorPerfMarker =
            new ProfilerMarker("[MRTK] GrabInteractor.ProcessInteractor");

        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            using (ProcessInteractorPerfMarker.Auto())
            {
                if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic
                    && hasSelection
                    && firstInteractableSelected is MRTKGrabInteractable grabInteractable
                    && grabInteractable.AlignWithSqueeze
                    && GripPoseSource != null
                    && GripPoseSource.TryGetPose(out Pose pose))
                {
                    // Ensure that the attachTransform tightly follows the grip pose
                    attachTransform.SetPositionAndRotation(pose.position, pose.rotation);
                }
            }
        }
    }
}
