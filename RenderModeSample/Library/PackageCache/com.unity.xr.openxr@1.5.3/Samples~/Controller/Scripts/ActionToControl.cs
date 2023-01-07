using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToControl : MonoBehaviour
    {
        [Tooltip("Action Reference that represents the control")]
        [SerializeField] private InputActionReference _actionReference = null;

        [Tooltip("Optional text element that will be set to the name of the action")]
        [SerializeField] private Text _text = null;

        protected virtual void OnEnable()
        {
            if (_actionReference == null || _actionReference.action == null)
                return;

            _actionReference.action.started += OnActionStarted;
            _actionReference.action.performed += OnActionPerformed;
            _actionReference.action.canceled += OnActionCanceled;

            StartCoroutine(UpdateBinding());
        }

        protected virtual void OnDisable()
        {
            if (_actionReference == null || _actionReference.action == null)
                return;

            _actionReference.action.started -= OnActionStarted;
            _actionReference.action.performed -= OnActionPerformed;
            _actionReference.action.canceled -= OnActionCanceled;
        }

        private IEnumerator UpdateBinding ()
        {
            if(null != _text)
                _text.text = _actionReference.action.name;

            while (isActiveAndEnabled)
            {
                if(_actionReference.action != null &&
                    _actionReference.action.controls.Count > 0 &&
                    _actionReference.action.controls[0].device != null &&
                    OpenXRInput.TryGetInputSourceName(_actionReference.action, 0, out var actionName, OpenXRInput.InputSourceNameFlags.Component, _actionReference.action.controls[0].device))
                {
                    if(null != _text)
                        _text.text = actionName;
                    OnActionBound();
                    break;
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        protected virtual void OnActionStarted(InputAction.CallbackContext ctx) { }

        protected virtual void OnActionPerformed(InputAction.CallbackContext ctx) { }

        protected virtual void OnActionCanceled(InputAction.CallbackContext ctx) { }

        protected virtual void OnActionBound()
        {
        }
    }
}