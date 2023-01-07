using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;
using TrackingState = UnityEngine.XR.InputTrackingState;

namespace UnityEngine.XR.OpenXR.Input
{
    /// <summary>
    /// Represents a tracked object in real-world space. All poses are given in the same root space, dictated by <see cref="UnityEngine.XR.TrackingOriginModeFlags"/>.
    /// </summary>
    public struct Pose
    {
        /// <summary>
        /// If true, this position is being accurately tracked in real-world space. This value means that no values are estimated and tracking is not currently inhibited in any way.
        /// </summary>
        public bool isTracked { get; set; }

        /// <summary>
        /// A series of flags that identify which tracking values currently have data. That data can be measured by the real world or estimated when tracking is inhibited.
        /// </summary>
        public TrackingState trackingState { get; set; }

        /// <summary>
        /// The position, in meters, of the object in real-world space. This will be available if <see cref="trackingState"/> contains the <see cref="TrackingState.Position"/> flag.
        /// </summary>
        public Vector3 position { get; set; }

        /// <summary>
        /// The rotation, in radians, of the object in real-world space. This will be available if <see cref="trackingState"/> contains the <see cref="TrackingState.Rotation"/> flag.
        /// </summary>
        public Quaternion rotation { get; set; }

        /// <summary>
        /// The velocity, in meters per second, of the object in real-world space. This will be available if <see cref="trackingState"/> contains the <see cref="TrackingState.Velocity"/> flag.
        /// </summary>
        public Vector3 velocity { get; set; }

        /// <summary>
        /// The position, in radians per second, of the object in-real world space. This will be available if <see cref="trackingState"/> contains the <see cref="TrackingState.AngularVelocity"/> flag.
        /// </summary>
        public Vector3 angularVelocity { get; set; }
    }

    /// <summary>
    /// Input System control that wraps up a <see cref="Pose"/> structure. All individual pose elements can be referenced separately. See <see cref="InputControl"/> for more details.
    /// </summary>
    public class PoseControl : InputControl<Pose>
    {
        /// <summary>
        /// Separate access to the <see cref="Pose.isTracked"/> value.
        /// </summary>
        [Preserve]
        [InputControl(offset = 0)]
        public ButtonControl isTracked { get; private set; }

        /// <summary>
        /// Separate access to the <see cref="Pose.trackingState"/> value.
        /// </summary>
        [Preserve]
        [InputControl(offset = 4)]
        public IntegerControl trackingState { get; private set; }

        /// <summary>
        /// Separate access to the <see cref="Pose.position"/> value.
        /// </summary>
        [Preserve]
        [InputControl(offset = 8, noisy = true)]
        public Vector3Control position { get; private set; }

        /// <summary>
        /// Separate access to the <see cref="Pose.rotation"/> value.
        /// </summary>
        [Preserve]
        [InputControl(offset = 20, noisy = true)]
        public QuaternionControl rotation { get; private set; }

        /// <summary>
        /// Separate access to the <see cref="Pose.velocity"/> value.
        /// </summary>
        [Preserve]
        [InputControl(offset = 36, noisy = true)]
        public Vector3Control velocity { get; private set; }

        /// <summary>
        /// Separate access to the <see cref="Pose.angularVelocity"/> value.
        /// </summary>
        [Preserve]
        [InputControl(offset = 48, noisy = true)]
        public Vector3Control angularVelocity { get; private set; }

        /// <summary>
        /// Default Constructor required by the Input System for instantiation.
        /// </summary>
        public PoseControl()
        { }

        /// <summary>
        /// See [InputControl.FinishSetup](xref:UnityEngine.InputSystem.InputControl.FinishSetup)
        /// </summary>
        protected override void FinishSetup()
        {
            isTracked = GetChildControl<ButtonControl>("isTracked");
            trackingState = GetChildControl<IntegerControl>("trackingState");
            position = GetChildControl<Vector3Control>("position");
            rotation = GetChildControl<QuaternionControl>("rotation");
            velocity = GetChildControl<Vector3Control>("velocity");
            angularVelocity = GetChildControl<Vector3Control>("angularVelocity");

            base.FinishSetup();
        }

        /// <summary>
        /// Read unprocessed state values from the input control state.
        /// </summary>
        /// <param name="statePtr">State data to read from.</param>
        /// <returns>The pose data from the unprocessed state.</returns>
        public override unsafe Pose ReadUnprocessedValueFromState(void* statePtr)
        {
            return new Pose()
            {
                isTracked = isTracked.ReadUnprocessedValueFromState(statePtr) > 0.5f,
                trackingState = (TrackingState)trackingState.ReadUnprocessedValueFromState(statePtr),
                position = position.ReadUnprocessedValueFromState(statePtr),
                rotation = rotation.ReadUnprocessedValueFromState(statePtr),
                velocity = velocity.ReadUnprocessedValueFromState(statePtr),
                angularVelocity = angularVelocity.ReadUnprocessedValueFromState(statePtr),
            };
        }

        /// <summary>
        /// Write value data into input control state.
        /// </summary>
        /// <param name="value">The value to write into the control state.</param>
        /// <param name="statePtr">A pointer to the control state data.</param>
        public override unsafe void WriteValueIntoState(Pose value, void* statePtr)
        {
            isTracked.WriteValueIntoState(value.isTracked, statePtr);
            trackingState.WriteValueIntoState((uint)value.trackingState, statePtr);
            position.WriteValueIntoState(value.position, statePtr);
            rotation.WriteValueIntoState(value.rotation, statePtr);
            velocity.WriteValueIntoState(value.velocity, statePtr);
            angularVelocity.WriteValueIntoState(value.angularVelocity, statePtr);
        }
    }
}
