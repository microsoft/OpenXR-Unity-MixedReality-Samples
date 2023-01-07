using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Information about a configuration supported by a <see cref="XRSessionSubsystem"/>.
    /// Used by a <see cref="ConfigurationChooser"/> to select a configuration for the session.
    /// </summary>
    /// <remarks>
    /// A session provider may have multiple, discrete "modes" of operation each with a different set of capabilities.
    /// A configuration descriptor represents the capabilities of a single "mode" of operation,
    /// which may be a subset of the session's overal capabilities.
    /// That is, the session may support many features, but not all at the same time.
    /// </remarks>
    /// <seealso cref="ConfigurationChooser"/>
    /// <seealso cref="XRSessionSubsystem.DetermineConfiguration(Feature)"/>
    /// <seealso cref="XRSessionSubsystem.GetConfigurationDescriptors(Unity.Collections.Allocator)"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct ConfigurationDescriptor : IEquatable<ConfigurationDescriptor>
    {
        IntPtr m_Identifier;

        Feature m_Capabilities;

        int m_Rank;

        /// <summary>
        /// A unique identifier for this descriptor.
        /// </summary>
        public IntPtr identifier => m_Identifier;

        /// <summary>
        /// The capabilities of this configuration.
        /// </summary>
        public Feature capabilities => m_Capabilities;

        /// <summary>
        /// The "rank" of this configuration relative to other configurations.
        /// This can be used by a <see cref="ConfigurationChooser"/> when deciding
        /// between multiple configurations that support the requested <see cref="Feature"/>s.
        /// </summary>
        public int rank => m_Rank;

        /// <summary>
        /// Constructs a <see cref="ConfigurationDescriptor"/>.
        /// </summary>
        /// <param name="identifier">A unique identifier for this descriptor.</param>
        /// <param name="capabilities">The supported capabilities of the configuration.</param>
        /// <param name="rank">Higher values indicate this configuration should be chosen over another, otherwise equivalent configuration.</param>
        public ConfigurationDescriptor(IntPtr identifier, Feature capabilities, int rank)
        {
            m_Identifier = identifier;
            m_Capabilities = capabilities;
            m_Rank = rank;
        }

        unsafe string HexString(IntPtr ptr) => sizeof(IntPtr) == 4 ? $"0x{ptr.ToInt32():x}" : $"0x{ptr.ToInt64():x}";

        /// <summary>
        /// Generates a string representation suitable for debugging.
        /// </summary>
        /// <returns>A string representation suitable for debugging.</returns>
        public override string ToString() => $"(Identifier: {HexString(identifier)}, Rank: {rank}, Capabilities: {capabilities.ToStringList()})";

        /// <summary>
        /// Generates a hash code suitable for use in a Dictionary or HashSet.
        /// </summary>
        /// <returns>A hash code of this <see cref="ConfigurationDescriptor"/>.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(m_Identifier.GetHashCode(), ((ulong)m_Capabilities).GetHashCode(), m_Rank.GetHashCode());

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ConfigurationDescriptor"/> to compare against.</param>
        /// <returns><c>true</c> if the other <see cref="ConfigurationDescriptor"/> is equal to this one.</returns>
        public bool Equals(ConfigurationDescriptor other) =>
                (m_Identifier == other.m_Identifier) &&
                (m_Capabilities == other.m_Capabilities) &&
                (m_Rank == other.m_Rank);

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="obj">The <c>object</c> to compare against.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="ConfigurationDescriptor"/> and <see cref="Equals(ConfigurationDescriptor)"/> is <c>true</c>.</returns>
        public override bool Equals(object obj) => (obj is ConfigurationDescriptor) && Equals((ConfigurationDescriptor)obj);

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>The same as <see cref="Equals(ConfigurationDescriptor)"/>.</returns>
        public static bool operator==(ConfigurationDescriptor lhs, ConfigurationDescriptor rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Compares for inequality.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>The negation of <see cref="Equals(ConfigurationDescriptor)"/>.</returns>
        public static bool operator!=(ConfigurationDescriptor lhs, ConfigurationDescriptor rhs) => !lhs.Equals(rhs);
    }
}
