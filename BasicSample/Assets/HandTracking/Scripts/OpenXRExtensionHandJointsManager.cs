// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class OpenXRExtensionHandJointsManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The prefab to use for rendering hand joints in the scene. (optional)")]
        private GameObject handJointPrefab = null;

        private Hand leftHand = null;
        private Hand rightHand = null;

        private static readonly HandJointLocation[] HandJointLocations = new HandJointLocation[HandTracker.JointCount];

        private void OnEnable()
        {
            Application.onBeforeRender += Application_onBeforeRender;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= Application_onBeforeRender;
            leftHand?.DisableHandJoints();
            rightHand?.DisableHandJoints();
        }

        private void Application_onBeforeRender()
        {
            UpdateHandJoints(HandTracker.Left, leftHand, FrameTime.OnBeforeRender);
            UpdateHandJoints(HandTracker.Right, rightHand, FrameTime.OnBeforeRender);
        }

        private void Start()
        {
            leftHand = new Hand(handJointPrefab);
            rightHand = new Hand(handJointPrefab);
        }

        private void Update()
        {
            UpdateHandJoints(HandTracker.Left, leftHand, FrameTime.OnUpdate);
            UpdateHandJoints(HandTracker.Right, rightHand, FrameTime.OnUpdate);
        }

        private static void UpdateHandJoints(HandTracker handTracker, Hand hand, FrameTime frameTime)
        {
            if (handTracker.TryLocateHandJoints(frameTime, HandJointLocations))
            {
                hand?.UpdateHandJoints(HandJointLocations);
            }
            else
            {
                hand?.DisableHandJoints();
            }
        }
    }
}
