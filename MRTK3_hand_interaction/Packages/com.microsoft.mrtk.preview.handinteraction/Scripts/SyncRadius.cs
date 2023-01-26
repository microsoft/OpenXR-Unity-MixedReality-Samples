// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Microsoft.MixedReality.Toolkit.Preview.HandInteraction
{
    internal class SyncRadius : MonoBehaviour
    {
        [SerializeReference, InterfaceSelector]
        private IPoseSource pokePoseSource;

        [SerializeField]
        private InputAction pokeRadiusAction;

        private void OnEnable()
        {
            pokeRadiusAction?.Enable();
        }

        private void Update()
        {
            if (pokePoseSource != null && pokePoseSource.TryGetPose(out Pose pose))
            {
                transform.SetPositionAndRotation(pose.position, pose.rotation);
            }

            if (pokeRadiusAction?.HasAnyControls() ?? false)
            {
                Vector3 newScale = 2 * pokeRadiusAction.ReadValue<float>() * Vector3.one;

                // To make sure this is set in world scale, we need to adjust for the parent's world scale
                if (transform.parent != null)
                {
                    Vector3 parentScale = transform.parent.lossyScale;
                    newScale = Vector3.Scale(new Vector3(1 / parentScale.x, 1 / parentScale.y, 1 / parentScale.z), newScale);
                }
                
                transform.localScale = newScale;
            }
        }

        private void OnDisable()
        {
            pokeRadiusAction?.Disable();
        }
    }
}
