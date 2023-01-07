using System;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToDeviceInfo : MonoBehaviour
    {
        [SerializeField] private InputActionReference _actionReference = null;

        [SerializeField] private Text _text = null;

        private void OnEnable()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (null == _actionReference || null == _actionReference.action || _actionReference.action.controls.Count == 0 || _text == null)
                return;

            var device = _actionReference.action.controls[0].device;
            _text.text = $"{device.name}\n{device.deviceId}\n{string.Join(",", device.usages.Select(u=>u.ToString()))}";
        }
    }
}
