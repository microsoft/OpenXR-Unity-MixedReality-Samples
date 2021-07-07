// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class FeatureUsageHandJointsManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The prefab to use for rendering hand joints in the scene. (optional)")]
        private GameObject handJointPrefab = null;

        private Hand leftHand = null;
        private Hand rightHand = null;

        private void Start()
        {
            leftHand = new Hand(handJointPrefab);
            rightHand = new Hand(handJointPrefab);
        }

        private void Update()
        {
            UpdateHandJoints(InputDeviceCharacteristics.Left, leftHand);
            UpdateHandJoints(InputDeviceCharacteristics.Right, rightHand);
        }

        private void OnDisable()
        {
            leftHand?.DisableHandJoints();
            rightHand?.DisableHandJoints();
        }

        private void OnDestroy()
        {
            leftHand?.DestroyHandJoints();
            rightHand?.DestroyHandJoints();
        }

        private static void UpdateHandJoints(InputDeviceCharacteristics flag, Hand hand)
        {
            List<InputDevice> inputDeviceList = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking | flag, inputDeviceList);
            UnityEngine.XR.Hand xrHand = default;
            foreach (InputDevice device in inputDeviceList)
            {
                if (device.TryGetFeatureValue(CommonUsages.handData, out xrHand))
                {
                    break;
                }
            }

            if (xrHand != default)
            {
                hand?.UpdateHandJoints(xrHand);
            }
            else
            {
                // If we get here, we didn't successfully update hand joints for any tracked input device
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

            // Visualize hand joints when using FeatureUsages, HandFinger / TryGetFeatureValue hand joints
            private static readonly HandFinger[] HandFingers = System.Enum.GetValues(typeof(HandFinger)) as HandFinger[];
            private readonly Dictionary<HandFinger, GameObject[]> handFingerGameObjects = new Dictionary<HandFinger, GameObject[]>();
            private readonly List<Bone> fingerBones = new List<Bone>();
            private GameObject palmGameObject = null;

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
        }
    }
}
