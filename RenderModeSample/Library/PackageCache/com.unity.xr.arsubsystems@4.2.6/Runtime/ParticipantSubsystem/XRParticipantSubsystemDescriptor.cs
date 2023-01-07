using System;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// The descriptor of the <see cref="XRParticipantSubsystem"/> that shows which features are available on that XRSubsystem.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Register{T}"/> to register a subsystem with the global <c>SubsystemManager</c>.
    /// </remarks>
    public sealed class XRParticipantSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRParticipantSubsystem, XRParticipantSubsystem.Provider>
    {
        /// <summary>
        /// The capabilities of a particular <see cref="XRParticipantSubsystem"/>. This is typically
        /// used to query a subsystem for capabilities that might vary by platform or implementation.
        /// </summary>
        [Flags]
        public enum Capabilities
        {
            /// <summary>
            /// The <see cref="XRParticipantSubsystem"/> implementation has no
            /// additional capabilities other than the basic, required functionality.
            /// </summary>
            None = 0,
        }

        /// <summary>
        /// The capabilities provided by this implementation.
        /// </summary>
        public Capabilities capabilities { get; private set; }

        /// <summary>
        /// Register a provider implementation.
        /// This should only be used by subsystem implementors.
        /// </summary>
        /// <param name="subsystemId">The name of the specific subsystem implementation.</param>
        /// <param name="capabilities">The <see cref="Capabilities"/> of the specific subsystem implementation.</param>
        /// <typeparam name="T">The concrete type derived from <see cref="XRParticipantSubsystem"/> being registered.</typeparam>
        public static void Register<T>(string subsystemId, Capabilities capabilities)
            where T : XRParticipantSubsystem.Provider
        {
            SubsystemDescriptorStore.RegisterDescriptor(new XRParticipantSubsystemDescriptor(subsystemId, typeof(T), null, capabilities));
        }

        /// <summary>
        /// Register a provider implementation and subsystem override.
        /// This should only be used by subsystem implementors.
        /// </summary>
        /// <param name="subsystemId">The name of the specific subsystem implementation.</param>
        /// <param name="capabilities">The <see cref="Capabilities"/> of the specific subsystem implementation.</param>
        /// <typeparam name="TProvider">The concrete type of the provider being registered.</typeparam>
        /// <typeparam name="TSubsystemOverride">The concrete type of the subsystem being registered.</typeparam>
        public static void Register<TProvider, TSubsystemOverride>(string subsystemId, Capabilities capabilities)
            where TProvider : XRParticipantSubsystem.Provider
            where TSubsystemOverride : XRParticipantSubsystem
        {
            SubsystemDescriptorStore.RegisterDescriptor(new XRParticipantSubsystemDescriptor(subsystemId, typeof(TProvider), typeof(TSubsystemOverride), capabilities));
        }

        XRParticipantSubsystemDescriptor(string subsystemId, Type providerType, Type subsystemTypeOverride, Capabilities capabilities)
        {
            id = subsystemId;
            this.providerType = providerType;
            this.subsystemTypeOverride = subsystemTypeOverride;
            this.capabilities = capabilities;
        }
    }
}
