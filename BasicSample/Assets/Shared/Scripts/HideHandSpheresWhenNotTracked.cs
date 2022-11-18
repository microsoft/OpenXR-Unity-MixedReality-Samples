using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    // An XRPoseDriver will reset its attached GameObject's position to origin when tracking is lost.
    // The user may find these gameobjects when moving around. This script enable or disables
    // GameObjects according to the tracking state of their related XRNode.
    public class HideHandSpheresWhenNotTracked : MonoBehaviour
    {
        [SerializeField, Tooltip("Hand Sphere UI")]
        private GameObject[] handSpheres = new GameObject[2];

        void Update()
        {
            bool leftHandSphereVisible = IsTracked(XRNode.LeftHand);
            bool rightHandSphereVisible = IsTracked(XRNode.RightHand);

            handSpheres[0].SetActive(leftHandSphereVisible);
            handSpheres[1].SetActive(rightHandSphereVisible);
        }

        private static bool IsTracked(XRNode xRNode)
        {
            var inputDevice = InputDevices.GetDeviceAtXRNode(xRNode);
            if (inputDevice.isValid && inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked))
            {
                return tracked;
            }
            return false;
        }
    }
}
