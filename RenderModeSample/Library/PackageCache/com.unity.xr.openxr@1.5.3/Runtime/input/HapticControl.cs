using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Input
{
    /// <summary>
    /// Data that the HapticControl represents.  Since haptics are an Output the Haptic struct is empty.
    /// </summary>
    public struct Haptic
    {
    }

    /// <summary>
    /// Input System control that wraps up a <see cref="Haptic"/> structure.
    /// </summary>
    [Preserve]
    public class HapticControl : InputControl<Haptic>
    {
        /// <summary>
        /// Default Constructor required by the Input System for instantiation.
        /// </summary>
        public HapticControl()
        {
            // Since the haptic control has no children and the the Haptic data structure has no members we need
            // to fake having a size or the InputSystem will think this control is misconfigured and throw an exception.
            m_StateBlock.sizeInBits = 1;
            m_StateBlock.bitOffset = 0;
            m_StateBlock.byteOffset = 0;
        }

        /// <summary>
        /// Returns an empty haptic structure since haptics are an output and have no data
        /// </summary>
        /// <param name="statePtr">Raw state data to read from</param>
        /// <returns>Empty haptic structure</returns>
        public override unsafe Haptic ReadUnprocessedValueFromState(void* statePtr) => new Haptic();
    }
}