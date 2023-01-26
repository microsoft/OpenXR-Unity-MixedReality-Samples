// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Microsoft.MixedReality.Toolkit.Preview.HandInteraction
{
    internal class TestInputAction : MonoBehaviour
    {
        [SerializeField]
        private InputAction inputAction;

        [SerializeField]
        private UnityEvent startedAction;

        [SerializeField]
        private UnityEvent endedAction;

        private void OnEnable()
        {
            if (inputAction == null)
            {
                return;
            }

            inputAction.started += InputAction_started;
            inputAction.canceled += InputAction_canceled;
            inputAction.Enable();
        }

        private void InputAction_started(InputAction.CallbackContext obj)
        {
            startedAction.Invoke();
        }

        private void InputAction_canceled(InputAction.CallbackContext obj)
        {
            endedAction.Invoke();
        }

        private void OnDisable()
        {
            if (inputAction == null)
            {
                return;
            }

            inputAction.Disable();
            inputAction.canceled -= InputAction_canceled;
            inputAction.started -= InputAction_started;
        }
    }
}
