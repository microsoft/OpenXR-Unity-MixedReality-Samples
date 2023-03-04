// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Input;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos
{
    public class PrintHandInteractionData : MonoBehaviour
    {
        [SerializeField]
        private XRNode xrNode = XRNode.LeftHand;

        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private TMP_Text dataText;

        #region Setup Unity input bindings for hand interaction profile
        private static readonly HandInputActions m_leftHandActions = CreateHandInputActions("LeftHand");
        private static readonly HandInputActions m_rightHandActions = CreateHandInputActions("RightHand");

        private static HandInputActions CreateHandInputActions(string hand)
        {
            Debug.Assert(hand == "LeftHand" || hand == "RightHand");
            return new HandInputActions()
            {
                gripPoseActions = new PoseInputActions()    // OpenXR grip pose -> device pose in Unity
                {
                    positionAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/devicePose/position"),
                    rotationAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/devicePose/rotation"),
                    isTrackedAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{" + hand + "}/devicePose/isTracked"),
                    trackingStateAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/devicePose/trackingState"),
                },
                aimPoseActions = new PoseInputActions()    // OpenXR aim pose -> pointer pose in Unity
                {
                    positionAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/aimPose/position"),
                    rotationAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/aimPose/rotation"),
                    isTrackedAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{" + hand + "}/aimPose/isTracked"),
                    trackingStateAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/aimPose/trackingState"),
                },
                pinchPoseActions = new PoseInputActions()    // OpenXR pinch pose -> pinch pose in Unity
                {
                    positionAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/pinchPose/position"),
                    rotationAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/pinchPose/rotation"),
                    isTrackedAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{" + hand + "}/pinchPose/isTracked"),
                    trackingStateAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/pinchPose/trackingState"),
                },
                pokePoseActions = new PoseInputActions()    // OpenXR poke pose -> poke pose in Unity
                {
                    positionAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/pokePose/position"),
                    rotationAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/pokePose/rotation"),
                    isTrackedAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{" + hand + "}/pokePose/isTracked"),
                    trackingStateAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/pokePose/trackingState"),
                },
                aimActivateActions = new GestureInputActions()  // OpenXR aim activate -> pointer activate in Unity
                {
                    valueAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/pointerActivateValue"),
                    isReadyAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{" + hand + "}/pointerActivateReady"),
                },
                graspActions = new GestureInputActions()
                {
                    valueAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/graspValue"),
                    isReadyAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{" + hand + "}/graspReady"),
                },
                pinchActions = new GestureInputActions()
                {
                    valueAction = new InputAction(type: InputActionType.Value, binding: "<XRController>{" + hand + "}/pinchValue"),
                    isReadyAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{" + hand + "}/pinchReady"),
                },
            };
        }
        #endregion

        #region Read input data from Unity's InputAction
        private static bool HasBinding(InputAction action)
        {
            // return action?.activeControl != null; // this seems becoming false when the value is 0.
            //return action?.controls.Count > 0;  // this seems always true even if controller is not powered on.
            return OpenXRInput.GetActionIsActive(action);
        }

        private static void ReadValue<T>(ref T? data, InputAction action) where T : struct
        {
            data = HasBinding(action) ? action.ReadValue<T>() : null;
        }

        private static void ReadBool(ref bool? data, InputAction action)
        {
            data = HasBinding(action) ? action.IsPressed() : null;
        }

        private static void ReadPoseData(ref PoseData data, PoseInputActions inputActions)
        {
            ReadValue<Vector3>(ref data.position, inputActions.positionAction);
            ReadValue<Quaternion>(ref data.rotation, inputActions.rotationAction);
            ReadBool(ref data.isTracked, inputActions.isTrackedAction);
            int? trackingState = null;
            ReadValue<int>(ref trackingState, inputActions.trackingStateAction);
            data.trackingState = (InputTrackingState?)trackingState;
        }

        private static void ReadGestureData(ref GestureData data, GestureInputActions inputActions)
        {
            ReadValue<float>(ref data.value, inputActions.valueAction);
            ReadBool(ref data.isReady, inputActions.isReadyAction);
        }

        private static HandInteractionData ReadValue(HandInputActions actions)
        {
            var data = new HandInteractionData();

            // This "activeControl.device.name" is from xrGetInputSourceLocalizedName
            var activeControl = actions.gripPoseActions.trackingStateAction.activeControl;
            data.sourceName = activeControl == null ? "No Active Binding" : activeControl.device.name;

            ReadPoseData(ref data.gripPose, actions.gripPoseActions);
            ReadPoseData(ref data.aimPose, actions.aimPoseActions);
            ReadPoseData(ref data.pinchPose, actions.pinchPoseActions);
            ReadPoseData(ref data.pokePose, actions.pokePoseActions);
            ReadGestureData(ref data.aimActivate, actions.aimActivateActions);
            ReadGestureData(ref data.grasp, actions.graspActions);
            ReadGestureData(ref data.pinch, actions.pinchActions);
            return data;
        }
        #endregion

        #region Hand interaction data structures
        class HandInteractionData
        {
            public string sourceName;

            public PoseData gripPose;
            public PoseData aimPose;
            public PoseData pinchPose;
            public PoseData pokePose;
            public GestureData aimActivate;
            public GestureData pinch;
            public GestureData grasp;
        }

        class HandInputActions
        {
            public PoseInputActions gripPoseActions;
            public PoseInputActions aimPoseActions;
            public PoseInputActions pinchPoseActions;
            public PoseInputActions pokePoseActions;
            public GestureInputActions aimActivateActions;
            public GestureInputActions pinchActions;
            public GestureInputActions graspActions;
        }

        struct PoseData
        {
            public Vector3? position;
            public Quaternion? rotation;
            public bool? isTracked;
            public InputTrackingState? trackingState;
        }

        struct GestureData
        {
            public float? value;
            public bool? isReady;
        }

        struct PoseInputActions
        {
            public InputAction positionAction;
            public InputAction rotationAction;
            public InputAction isTrackedAction;
            public InputAction trackingStateAction;
        }

        struct GestureInputActions
        {
            public InputAction valueAction;
            public InputAction isReadyAction;
        }

        #endregion

        #region MonoBehaviour functions
        private void Awake()
        {
            if (xrNode != XRNode.LeftHand && xrNode != XRNode.RightHand)
            {
                Debug.LogError($"{nameof(PrintHandInteractionData)} only supports {XRNode.LeftHand} or {XRNode.RightHand} is set to {xrNode}.", gameObject);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (xrNode == XRNode.LeftHand)
            {
                ForEach(m_leftHandActions, (action) => action.Enable());
            }
            else if (xrNode == XRNode.RightHand)
            {
                ForEach(m_rightHandActions, (action) => action.Enable());
            }
        }

        private void OnDisable()
        {
            if (xrNode == XRNode.LeftHand)
            {
                ForEach(m_leftHandActions, (action) => action.Disable());
            }
            else if (xrNode == XRNode.RightHand)
            {
                ForEach(m_rightHandActions, (action) => action.Disable());
            }
        }

        private void Update()
        {
            if (xrNode == XRNode.LeftHand)
            {
                UpdateText(m_leftHandActions);
            }
            else if (xrNode == XRNode.RightHand)
            {
                UpdateText(m_rightHandActions);
            }
        }
        #endregion

        #region Print formatted text
        private int m_updateTextCounter = 0;
        private const int m_updateTextCounterMax = 10;
        private void UpdateText(HandInputActions actions)
        {
            if (m_updateTextCounter++ < m_updateTextCounterMax)
            {
                return; // no need to update too frequently
            }
            m_updateTextCounter = 0;

            var data = ReadValue(actions);
            nameText?.SetText($"{data.sourceName}");
            dataText?.SetText(PrintHandActions(data));
        }

        private string PrintHandActions(HandInteractionData data)
        {
            StringBuilder paragraph = new StringBuilder();
            paragraph.AppendLine($"grasp/value   : {Print(data.grasp.value)}, \t ready: {Print(data.grasp.isReady)}");
            paragraph.AppendLine($"grip/pose     : tracked = {Print(data.gripPose.isTracked)}, state = {Print(data.gripPose.trackingState)}");
            paragraph.AppendLine($"     position : {Print(data.gripPose.position)}");
            paragraph.AppendLine($"     rotation : {Print(data.gripPose.rotation)}");

            paragraph.AppendLine($"aimAct/value  : {Print(data.aimActivate.value)}, \t ready: {Print(data.aimActivate.isReady)}");
            paragraph.AppendLine($"aim/pose      : tracked = {Print(data.aimPose.isTracked)}, state = {Print(data.aimPose.trackingState)}");
            paragraph.AppendLine($"     position : {Print(data.aimPose.position)}");
            paragraph.AppendLine($"     rotation : {Print(data.aimPose.rotation)}");

            paragraph.AppendLine($"pinch/value   : {Print(data.pinch.value)}, \t ready: {Print(data.pinch.isReady)}");
            paragraph.AppendLine($"pinch/pose    : tracked = {Print(data.pinchPose.isTracked)}, state = {Print(data.pinchPose.trackingState)}");
            paragraph.AppendLine($"     position : {Print(data.pinchPose.position)}");
            paragraph.AppendLine($"     rotation : {Print(data.pinchPose.rotation)}");

            paragraph.AppendLine($"poke/pose     : tracked = {Print(data.pokePose.isTracked)}, state = {Print(data.pokePose.trackingState)}");
            paragraph.AppendLine($"     position : {Print(data.pokePose.position)}");
            paragraph.AppendLine($"     rotation : {Print(data.pokePose.rotation)}");
            return paragraph.ToString();
        }

        private string Print(Vector3? v)
        {
            return v.HasValue ? $"{v.Value:0.00}" : "( null position )";
        }

        private string Print(Quaternion? q)
        {
            return q.HasValue ? $"{q.Value:0.00}" : "( null rotation )";
        }

        private string Print(InputTrackingState? t)
        {
            return t.HasValue ? $"( 0x{(byte)t.Value:X} )" : "( null )";
        }

        private string Print(float? f)
        {
            return f.HasValue ? $"( {f.Value:0.000} )" : "( null )";
        }

        private string Print(bool? b)
        {
            return b.HasValue ? $"( {b.Value} )" : "( null )";
        }

        private void ForEach(HandInputActions actions, Action<InputAction> action)
        {
            ForEach(actions.gripPoseActions, action);
            ForEach(actions.aimPoseActions, action);
            ForEach(actions.pinchPoseActions, action);
            ForEach(actions.pokePoseActions, action);
            ForEach(actions.aimActivateActions, action);
            ForEach(actions.pinchActions, action);
            ForEach(actions.graspActions, action);
        }

        private void ForEach(PoseInputActions actions, Action<InputAction> action)
        {
            action(actions.positionAction);
            action(actions.rotationAction);
            action(actions.isTrackedAction);
            action(actions.trackingStateAction);
        }

        private void ForEach(GestureInputActions actions, Action<InputAction> action)
        {
            action(actions.valueAction);
            action(actions.isReadyAction);
        }
        #endregion
    }
}
