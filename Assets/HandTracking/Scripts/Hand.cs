// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class Hand
    {
        public Hand(GameObject handJointPrefab)
        {
            this.handJointPrefab = handJointPrefab;
            handRoot = new GameObject("HandParent");
        }

        private readonly GameObject handJointPrefab = null;
        private readonly GameObject handRoot = null;

        // Visualize hand joints when using FeatureUsages, HandFinger / TryGetFeatureValue hand joints
        private static readonly HandFinger[] HandFingers = Enum.GetValues(typeof(HandFinger)) as HandFinger[];
        private readonly Dictionary<HandFinger, GameObject[]> handFingerGameObjects = new Dictionary<HandFinger, GameObject[]>();
        private readonly List<Bone> fingerBones = new List<Bone>();
        private GameObject palmGameObject = null;

        // Visualize hand joints when using OpenXR HandTracker.LocateJoints
        private static readonly HandJoint[] HandJoints = Enum.GetValues(typeof(HandJoint)) as HandJoint[];
        private readonly Dictionary<HandJoint, GameObject> handJointGameObjects = new Dictionary<HandJoint, GameObject>();

        /// <summary>
        /// Instantiates either a predefined prefab or a new cube primitive if no prefab is provided.
        /// </summary>
        /// <returns>True if the returned new object is a cube primitive and needs coloring.</returns>
        private bool InstantiateJointPrefab(out GameObject gameObject)
        {
            if (handJointPrefab != null)
            {
                gameObject = UnityEngine.Object.Instantiate(handJointPrefab);
                gameObject.transform.parent = handRoot.transform;
                return false;
            }
            else
            {
                gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
                gameObject.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
                gameObject.transform.parent = handRoot.transform;
                return true;
            }
        }

        /// <summary>
        /// When this hand becomes inactive, it's best practice to clean up the in-scene representation.
        /// </summary>
        public void DisableHandJoints()
        {
            if (handRoot != null)
            {
                handRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Update this hand's internal state with the Unity XR InputDevice's hand data.
        /// </summary>
        /// <param name="device">The InputDevice to get the CommonUsages.handData feature value from.</param>
        public void UpdateHandJoints(HandJointLocation[] locations)
        {
            // If the hand was previously disabled, this is the first new update and it should be re-enabled
            if (!handRoot.activeSelf)
            {
                handRoot.SetActive(true);
            }

            foreach (HandJoint handJoint in HandJoints)
            {
                if (!handJointGameObjects.ContainsKey(handJoint))
                {
                    if (InstantiateJointPrefab(out GameObject jointObject))
                    {
                        ColorJointObject(jointObject, GetHandFinger(handJoint), GetIndexOnFinger(handJoint));
                    }
                    handJointGameObjects[handJoint] = jointObject;
                }

                GameObject handJointGameObject = handJointGameObjects[handJoint];
                HandJointLocation handJointLocation = locations[(int)handJoint];
                handJointGameObject.transform.SetPositionAndRotation(handJointLocation.Pose.position, handJointLocation.Pose.rotation);
                handJointGameObject.transform.localScale = Vector3.one * handJointLocation.Radius;
            }
        }

        /// <summary>
        /// Update this hand's internal state with the Unity XR InputDevice's hand data.
        /// </summary>
        /// <param name="device">The InputDevice to get the CommonUsages.handData feature value from.</param>
        public void UpdateHandJoints(UnityEngine.XR.Hand hand)
        {
            // If the hand was previously disabled, this is the first new update and it should be re-enabled
            if (!handRoot.activeSelf)
            {
                handRoot.SetActive(true);
            }

            if (hand.TryGetRootBone(out Bone palm))
            {
                if (palmGameObject == null)
                {
                    if (InstantiateJointPrefab(out palmGameObject))
                    {
                        ColorJointObject(palmGameObject, null, null);
                    }
                }

                bool positionAvailable = palm.TryGetPosition(out Vector3 position);
                bool rotationAvailable = palm.TryGetRotation(out Quaternion rotation);

                if (positionAvailable || rotationAvailable)
                {
                    palmGameObject.transform.SetPositionAndRotation(position, rotation);
                }
            }

            foreach (HandFinger finger in HandFingers)
            {
                if (hand.TryGetFingerBones(finger, fingerBones))
                {
                    if (!handFingerGameObjects.ContainsKey(finger))
                    {
                        GameObject[] jointArray = new GameObject[fingerBones.Count];
                        for (int i = 0; i < fingerBones.Count; i++)
                        {
                            if (InstantiateJointPrefab(out GameObject jointObject))
                            {
                                ColorJointObject(jointObject, finger, i);
                            }
                            jointArray[i] = jointObject;
                        }
                        handFingerGameObjects[finger] = jointArray;
                    }

                    GameObject[] fingerJointGameObjects = handFingerGameObjects[finger];

                    for (int i = 0; i < fingerBones.Count; i++)
                    {
                        Bone bone = fingerBones[i];

                        bool positionAvailable = bone.TryGetPosition(out Vector3 position);
                        bool rotationAvailable = bone.TryGetRotation(out Quaternion rotation);

                        if (positionAvailable || rotationAvailable)
                        {
                            fingerJointGameObjects[i].transform.SetPositionAndRotation(position, rotation);
                        }
                    }
                }
            }
        }

        private static void ColorJointObject(GameObject jointObject, HandFinger? finger, int? index)
        {
            Color jointColor = Color.magenta;
            switch (finger)
            {
                case HandFinger.Thumb:
                    // The wrist is the 0th entry on the thumb, so we want to keep that the default color
                    jointColor = (index == 0) ? Color.magenta : Color.red;
                    break;
                case HandFinger.Index:
                    // Orange
                    jointColor = new Color(1.0f, 0.647f, 0.0f);
                    break;
                case HandFinger.Middle:
                    jointColor = Color.yellow;
                    break;
                case HandFinger.Ring:
                    jointColor = Color.green;
                    break;
                case HandFinger.Pinky:
                    jointColor = Color.blue;
                    break;
            }

            // Lighten the colors as we go up the finger.
            int i = index ?? 0;
            float saturation = 1.0f - (i * 0.15f);
            Color.RGBToHSV(jointColor, out float h, out float _, out float v);
            jointObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(h, saturation, v);
        }

        private static HandFinger? GetHandFinger(HandJoint handJoint)
        {
            switch (handJoint)
            {
                case HandJoint.ThumbMetacarpal:
                case HandJoint.ThumbProximal:
                case HandJoint.ThumbDistal:
                case HandJoint.ThumbTip:
                    return HandFinger.Thumb;
                case HandJoint.IndexMetacarpal:
                case HandJoint.IndexProximal:
                case HandJoint.IndexIntermediate:
                case HandJoint.IndexDistal:
                case HandJoint.IndexTip:
                    return HandFinger.Index;
                case HandJoint.MiddleMetacarpal:
                case HandJoint.MiddleProximal:
                case HandJoint.MiddleIntermediate:
                case HandJoint.MiddleDistal:
                case HandJoint.MiddleTip:
                    return HandFinger.Middle;
                case HandJoint.RingMetacarpal:
                case HandJoint.RingProximal:
                case HandJoint.RingIntermediate:
                case HandJoint.RingDistal:
                case HandJoint.RingTip:
                    return HandFinger.Ring;
                case HandJoint.LittleMetacarpal:
                case HandJoint.LittleProximal:
                case HandJoint.LittleIntermediate:
                case HandJoint.LittleDistal:
                case HandJoint.LittleTip:
                    return HandFinger.Pinky;
                default:
                    return null;
            }
        }

        /// <summary>
        /// These indices map to the order they're provided through Unity's HandData.
        /// </summary>
        /// <param name="handJoint">The OpenXR enum value to map to Unity's indices.</param>
        /// <returns>The mapped index or null if not available.</returns>
        private static int? GetIndexOnFinger(HandJoint handJoint)
        {
            switch (handJoint)
            {
                case HandJoint.IndexMetacarpal:
                case HandJoint.MiddleMetacarpal:
                case HandJoint.RingMetacarpal:
                case HandJoint.LittleMetacarpal:
                    return 0;
                // Thumb doesn't have an intermediate joint, so metacarpal and proximal are moved up one tier to align color ordering
                case HandJoint.ThumbMetacarpal:
                case HandJoint.IndexProximal:
                case HandJoint.MiddleProximal:
                case HandJoint.RingProximal:
                case HandJoint.LittleProximal:
                    return 1;
                case HandJoint.ThumbProximal:
                case HandJoint.IndexIntermediate:
                case HandJoint.MiddleIntermediate:
                case HandJoint.RingIntermediate:
                case HandJoint.LittleIntermediate:
                    return 2;
                case HandJoint.ThumbDistal:
                case HandJoint.IndexDistal:
                case HandJoint.MiddleDistal:
                case HandJoint.RingDistal:
                case HandJoint.LittleDistal:
                    return 3;
                case HandJoint.ThumbTip:
                case HandJoint.IndexTip:
                case HandJoint.MiddleTip:
                case HandJoint.RingTip:
                case HandJoint.LittleTip:
                    return 4;
                default:
                    return null;
            }
        }
    }
}
