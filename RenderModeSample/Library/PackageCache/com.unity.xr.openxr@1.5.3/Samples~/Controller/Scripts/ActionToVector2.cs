using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToVector2 : ActionToControl
    {
        [SerializeField] public RectTransform _handle = null;

        protected override void OnActionPerformed(InputAction.CallbackContext ctx) => UpdateHandle(ctx);

        protected override void OnActionStarted(InputAction.CallbackContext ctx) => UpdateHandle(ctx);

        protected override void OnActionCanceled(InputAction.CallbackContext ctx) => UpdateHandle(ctx);

        private void UpdateHandle(InputAction.CallbackContext ctx)
        {
            _handle.anchorMin = _handle.anchorMax = (ctx.ReadValue<Vector2>() + Vector2.one) * 0.5f;
        }
    }
}
