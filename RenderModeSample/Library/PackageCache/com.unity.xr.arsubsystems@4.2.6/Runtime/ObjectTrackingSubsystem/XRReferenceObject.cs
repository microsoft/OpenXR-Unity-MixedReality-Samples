using System;
using System.Collections.Generic;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents a 3D scan of a real object that can be recognized in the environment.
    /// </summary>
    /// <remarks>
    /// Reference objects contain a list of provider-specific entries.
    /// Each entry must have previously been generated in a
    /// format specific to its implementation of the <see cref="XRObjectTrackingSubsystem"/>.
    /// </remarks>
    /// <seealso cref="XRReferenceObjectLibrary"/>
    /// <seealso cref="XRReferenceObjectEntry"/>
    [Serializable]
    public struct XRReferenceObject : IEquatable<XRReferenceObject>
    {
        /// <summary>
        /// Creates a new reference object. This is most commonly used to construct a new reference object at runtime.
        /// See <see cref="XRReferenceObjectLibrary.Add(XRReferenceObject)"/>
        /// </summary>
        /// <param name="name">The name of the new reference object.</param>
        public XRReferenceObject(string name)
        {
            m_Name = name;
            (m_GuidLow, m_GuidHigh) = GuidUtil.Decompose(Guid.NewGuid());
            m_Entries = new List<XRReferenceObjectEntry>();
        }

        /// <summary>
        /// Adds a provider-specific entry to this reference object.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        /// <exception cref="System.ArgumentException">Thrown if an entry of the same type as <paramref name="entry"/>
        ///     has already been added.</exception>
        public void AddEntry(XRReferenceObjectEntry entry)
        {
            if (FindEntry(entry.GetType()) != null)
                throw new ArgumentException($"The {nameof(XRReferenceObject)} already contains an entry for type {entry.GetType()}");

            m_Entries.Add(entry);
        }

        /// <summary>
        /// A string name for this reference object.
        /// </summary>
        public string name => m_Name;

        /// <summary>
        /// A <c>GUID</c> unique to this reference object.
        /// </summary>
        public Guid guid => GuidUtil.Compose(m_GuidLow, m_GuidHigh);

        /// <summary>
        /// Finds an <see cref="XRReferenceObjectEntry"/> by type.
        /// </summary>
        /// <typeparam name="T">The specific type of <see cref="XRReferenceObjectEntry"/> to find.</typeparam>
        /// <returns>The provider-specific <see cref="XRReferenceObjectEntry"/> if found, otherwise <c>null</c>.</returns>
        /// <seealso cref="FindEntry(Type)"/>
        public T FindEntry<T>() where T : XRReferenceObjectEntry => FindEntry(typeof(T)) as T;

        /// <summary>
        /// Finds an <see cref="XRReferenceObjectEntry"/> by type.
        /// </summary>
        /// <param name="type">The specific type of <see cref="XRReferenceObjectEntry"/> to find.</param>
        /// <returns>The provider-specific <see cref="XRReferenceObjectEntry"/> if found, otherwise <c>null</c>.</returns>
        /// <seealso cref="FindEntry{T}"/>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public XRReferenceObjectEntry FindEntry(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            foreach (var entry in m_Entries)
            {
                if (entry != null && entry.GetType() == type)
                {
                    return entry;
                }
            }

            return null;
        }

        internal void OnAddToLibrary(XRReferenceObjectLibrary library)
        {
            foreach (var entry in m_Entries)
            {
                entry.OnAddToLibrary(library, this);
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRReferenceObject"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRReferenceObject"/>, otherwise `false`.</returns>
        public bool Equals(XRReferenceObject other)
        {
            return
                (m_GuidLow == other.m_GuidLow) &&
                (m_GuidHigh == other.m_GuidHigh) &&
                string.Equals(m_Name, other.m_Name) &&
                ReferenceEquals(m_Entries, other.m_Entries);
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = m_GuidLow.GetHashCode();
                hash = hash * 486187739 + m_GuidHigh.GetHashCode();
                hash = hash * 486187739 + HashCodeUtil.ReferenceHash(m_Name);
                hash = hash * 486187739 + HashCodeUtil.ReferenceHash(m_Entries);
                return hash;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRReferenceObject"/> and
        /// <see cref="Equals(XRReferenceObject)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is XRReferenceObject && Equals((XRReferenceObject)obj);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRReferenceObject)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRReferenceObject lhs, XRReferenceObject rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRReferenceObject)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRReferenceObject lhs, XRReferenceObject rhs) => !lhs.Equals(rhs);

#pragma warning disable CS0649
        [SerializeField]
        internal ulong m_GuidLow;

        [SerializeField]
        internal ulong m_GuidHigh;

        [SerializeField]
        internal string m_Name;

        [SerializeField]
        internal List<XRReferenceObjectEntry> m_Entries;
#pragma warning restore CS0649
    }
}
