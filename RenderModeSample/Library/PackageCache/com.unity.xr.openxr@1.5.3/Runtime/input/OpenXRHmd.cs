using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Input
{
    [Preserve, InputControlLayout(displayName = "OpenXR HMD")]
    internal class OpenXRHmd : XRHMD
    {
        [Preserve, InputControl] ButtonControl userPresence { get; set; }

        /// <inheritdoc/>
        protected override void FinishSetup()
        {
            base.FinishSetup();
            userPresence = GetChildControl<ButtonControl>("UserPresence");
        }
    }
}
