// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class Tappable : MonoBehaviour
    {
        public UnityEvent OnAirTapped;
        public UnityEvent OnToggledOn;
        public UnityEvent OnToggledOff;

        public Material HoveredMaterial;
        public Material TappedMaterial;
        public MeshRenderer VolumeRenderer;
        public BoxCollider VolumeBox;
        public MeshRenderer TogglePlate;

        public bool IsToggleable = false;
        public bool IsToggledOn = false;
        private bool[] m_wasHandTapping = { false, false };

        private const float TAPPABLE_TAP_RANGE = 0.5f;

        private void Update()
        {
            bool showVolumeHovered = false;
            bool showVolumeTapped = false;

            Vector3 handPosition;
            bool isHandTapping;
            for (int i = 0; i < 2; i++)
            {
                InputDevice device = InputDevices.GetDeviceAtXRNode((i == 0) ? XRNode.RightHand : XRNode.LeftHand);
                if (!device.TryGetFeatureValue(CommonUsages.primaryButton, out isHandTapping)) continue;
                if (!device.TryGetFeatureValue(CommonUsages.devicePosition, out handPosition)) continue;

                // Transform the hand position into a coordinate system defined by the volume's transform.
                // If it's within the cube at the origin in these coordinates, it's within the box in the Unity space.
                Vector3 positionInVolumeCoordinates = VolumeBox.transform.InverseTransformPoint(handPosition);
                if (Mathf.Abs(positionInVolumeCoordinates.x) < TAPPABLE_TAP_RANGE &&
                    Mathf.Abs(positionInVolumeCoordinates.y) < TAPPABLE_TAP_RANGE &&
                    Mathf.Abs(positionInVolumeCoordinates.z) < TAPPABLE_TAP_RANGE)
                {
                    showVolumeHovered = true;

                    if (isHandTapping)
                    {
                        showVolumeTapped = true;

                        if (!m_wasHandTapping[i])
                        {
                            OnAirTapped?.Invoke();
                            if (IsToggleable)
                            {
                                IsToggledOn = !IsToggledOn;
                                (IsToggledOn ? OnToggledOn : OnToggledOff)?.Invoke();
                            }
                        }
                    }
                }
                m_wasHandTapping[i] = isHandTapping;
            }

            VolumeRenderer.enabled = showVolumeHovered;
            VolumeRenderer.material = showVolumeTapped ? TappedMaterial : HoveredMaterial;
            TogglePlate.enabled = IsToggleable && IsToggledOn;
        }
    }
}
