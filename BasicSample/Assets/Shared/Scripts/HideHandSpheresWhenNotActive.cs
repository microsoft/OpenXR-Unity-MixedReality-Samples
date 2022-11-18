using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class HideHandSpheresWhenNotActive : MonoBehaviour
    {
        [SerializeField, Tooltip("Sphere UI")]
        private GameObject[] handSpheres = new GameObject[2];

        void Update()
        {
            bool leftHandSphereVisible = IsTracked(XRNode.LeftHand) ? true : false;
            bool rightHandSphereVisible = IsTracked(XRNode.RightHand) ? true : false;

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
