// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
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

        private void Application_onBeforeRender()
        {
            UpdateHandJoints(HandTracker.Left, leftHand, FrameTime.OnBeforeRender);
            UpdateHandJoints(HandTracker.Right, rightHand, FrameTime.OnBeforeRender);
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= Application_onBeforeRender;
            leftHand?.DisableHandJoints();
            rightHand?.DisableHandJoints();
        }

        private void OnDestroy()
        {
            leftHand?.DestroyHandJoints();
            rightHand?.DestroyHandJoints();
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

        private class Hand
        {
            public Hand(GameObject handJointPrefab)
            {
                this.handJointPrefab = handJointPrefab;
                handRoot = new GameObject("HandParent");
            }

            private readonly GameObject handJointPrefab = null;
            private readonly GameObject handRoot = null;

            // Visualize hand joints when using OpenXR HandTracker.LocateJoints
            private static readonly HandJoint[] HandJoints = System.Enum.GetValues(typeof(HandJoint)) as HandJoint[];
            private readonly Dictionary<HandJoint, GameObject> handJointGameObjects = new Dictionary<HandJoint, GameObject>();

            /// <summary>
            /// Instantiates either a predefined prefab or a new cube primitive if no prefab is provided.
            /// </summary>
            /// <returns>True if the returned new object is a cube primitive and needs coloring.</returns>
            private bool InstantiateJointPrefab(out GameObject gameObject)
            {
                if (handJointPrefab != null)
                {
                    gameObject = Instantiate(handJointPrefab);
                    gameObject.transform.parent = handRoot.transform;
                    return false;
                }
                else
                {
                    gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(gameObject.GetComponent<Collider>());
                    gameObject.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
                    gameObject.transform.parent = handRoot.transform;
                    return true;
                }
            }

            /// <summary>
            /// Update this hand's internal state with the Mixed Reality OpenXR HandJointLocation array.
            /// </summary>
            /// <param name="locations">The locations from a <see cref="HandTracker"/>.</param>
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
                            ColorJointObject(jointObject, handJoint, GetIndexOnFinger(handJoint));
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
            /// When this hand becomes inactive, it's best practice to hide the in-scene representation.
            /// </summary>
            public void DisableHandJoints()
            {
                if (handRoot != null)
                {
                    handRoot.SetActive(false);
                }
            }

            /// <summary>
            /// Destroys the hand representation when the hand needs to be cleaned up.
            /// </summary>
            public void DestroyHandJoints()
            {
                if (handRoot != null)
                {
                    Destroy(handRoot);
                }
            }

            private static void ColorJointObject(GameObject jointObject, HandJoint finger, int? index)
            {
                Color jointColor = Color.magenta;
                string fingerName = finger.ToString();

                const string Thumb = "Thumb";
                const string Index = "Index";
                const string Middle = "Middle";
                const string Ring = "Ring";
                const string Little = "Little";

                if (fingerName.StartsWith(Thumb))
                {
                    jointColor = Color.red;
                }
                else if (fingerName.StartsWith(Index))
                {
                    jointColor = new Color(1.0f, 0.647f, 0.0f); // Orange
                }
                else if (fingerName.StartsWith(Middle))
                {
                    jointColor = Color.yellow;
                }
                else if (fingerName.StartsWith(Ring))
                {
                    jointColor = Color.green;
                }
                else if (fingerName.StartsWith(Little))
                {
                    jointColor = Color.blue;
                }

                // Lighten the colors as we go up the finger.
                int i = index ?? 0;
                float saturation = 1.0f - (i * 0.15f);
                Color.RGBToHSV(jointColor, out float h, out float _, out float v);
                jointObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(h, saturation, v);
            }

            /// <summary>
            /// These indices map to a consistent color shading gradient, to ensure the hand joints are being rendered in the correct order.
            /// </summary>
            /// <param name="handJoint">The OpenXR enum value to map to a specific index.</param>
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
}
