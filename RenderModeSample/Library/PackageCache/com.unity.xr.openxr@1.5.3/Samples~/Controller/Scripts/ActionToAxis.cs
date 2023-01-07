using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToAxis : ActionToControl
    {
        [Tooltip("Slider controlled by the action value")]
        [SerializeField] private Slider _slider = null;

        protected override void OnActionPerformed(InputAction.CallbackContext ctx) => UpdateValue(ctx);
        protected override void OnActionStarted(InputAction.CallbackContext ctx) => UpdateValue(ctx);
        protected override void OnActionCanceled(InputAction.CallbackContext ctx) => UpdateValue(ctx);

        private void UpdateValue(InputAction.CallbackContext ctx) => _slider.value = ctx.ReadValue<float>();
    }
}
