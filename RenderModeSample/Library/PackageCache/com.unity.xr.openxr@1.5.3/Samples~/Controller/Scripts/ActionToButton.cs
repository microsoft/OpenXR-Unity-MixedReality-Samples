using System;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToButton : ActionToControl
    {
        [SerializeField] private Image _image = null;

        [SerializeField] private Color _normalColor = Color.red;

        [SerializeField] private Color _pressedColor = Color.green;

        private void Awake()
        {
            if (_image != null)
            {
                _image.enabled = false;
                _image.color = _normalColor;
            }
        }

        protected override void OnActionStarted(InputAction.CallbackContext ctx)
        {
            if (_image != null)
                _image.color = _pressedColor;
        }

        protected override void OnActionCanceled(InputAction.CallbackContext ctx)
        {
            if (_image != null)
                _image.color = _normalColor;
        }

        protected override void OnActionBound()
        {
            if(_image != null)
                _image.enabled = true;
        }
    }
}
