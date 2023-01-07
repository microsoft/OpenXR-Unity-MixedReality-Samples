using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A container for reference objects. A reference object represents a 3D scan
    /// of a real object that can be recognized in the environment. A <see cref="XRObjectTrackingSubsystem"/>
    /// will search for objects based on the contents of a <see cref="XRReferenceObjectLibrary"/>.
    /// </summary>
    /// <seealso cref="XRReferenceObject"/>
    /// <seealso cref="XRReferenceObjectEntry"/>
    [CreateAssetMenu(fileName = "ReferenceObjectLibrary" , menuName = "XR/Reference Object Library", order = 3)]
    [HelpURL(HelpUrls.Manual + "object-tracking.html")]
    public class XRReferenceObjectLibrary : ScriptableObject, IEnumerable<XRReferenceObject>
    {
        /// <summary>
        /// The number of reference objects in the library.
        /// </summary>
        public int count => m_ReferenceObjects.Count;

        /// <summary>
        /// Gets an enumerator which can be used to iterate over the reference objects in this library.
        /// </summary>
        /// <example>
        /// This examples iterates over the reference objects contained in the library.
        /// <code>
        /// XRReferenceObjectLibrary library = ...
        /// foreach (var referenceObject in library)
        /// {
        ///     Debug.LogFormat("Reference object guid: {0}", referenceObject.guid);
        /// }
        /// </code>
        /// </example>
        /// <returns>Returns an enumerator which can be used to iterate over the reference objects in this library.</returns>
        public List<XRReferenceObject>.Enumerator GetEnumerator() => m_ReferenceObjects.GetEnumerator();

        /// <summary>
        /// Gets an enumerator which can be used to iterate over the reference objects in this library.
        /// </summary>
        /// <returns>Returns an object which can be used to iterate over the reference objects in this library.</returns>
        IEnumerator<XRReferenceObject> IEnumerable<XRReferenceObject>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets an enumerator which can be used to iterate over the reference objects in this library.
        /// </summary>
        /// <returns>Returns an object which can be used to iterate over the reference objects in this library.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Get a reference object by index.
        /// </summary>
        /// <param name="index">The index in the array of reference objects.
        /// Must be greater than or equal to 0 and less than <see cref="count"/>.</param>
        /// <returns>The <see cref="XRReferenceObject"/> at <paramref name="index"/>.</returns>
        public XRReferenceObject this[int index] => m_ReferenceObjects[index];

        /// <summary>
        /// A <c>GUID</c> associated with this reference library.
        /// The GUID is used to uniquely identify this library at runtime.
        /// </summary>
        public Guid guid => GuidUtil.Compose(m_GuidLow, m_GuidHigh);

        /// <summary>
        /// Get the index of <paramref name="referenceObject"/> in the object library.
        /// </summary>
        /// <param name="referenceObject">The <see cref="XRReferenceObject"/> to find.</param>
        /// <returns>The zero-based index of the <paramref name="referenceObject"/>, or -1 if not found.</returns>
        [Obsolete("Use IndexOf instead (deprecated 2021-02-01).")]
        public int indexOf(XRReferenceObject referenceObject) => IndexOf(referenceObject);

        /// <summary>
        /// Get the index of <paramref name="referenceObject"/> in the object library.
        /// </summary>
        /// <param name="referenceObject">The <see cref="XRReferenceObject"/> to find.</param>
        /// <returns>Returns the zero-based index of the <paramref name="referenceObject"/> if found. Returns -1 if not
        ///     found.</returns>
        public int IndexOf(XRReferenceObject referenceObject) => m_ReferenceObjects.IndexOf(referenceObject);

        /// <summary>
        /// Adds a new <see cref="XRReferenceObject"/> to this library.
        /// </summary>
        /// <param name="referenceObject">The reference object to add.</param>
        public void Add(XRReferenceObject referenceObject)
        {
            referenceObject.OnAddToLibrary(this);
            m_ReferenceObjects.Add(referenceObject);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        void Awake()
        {
            if (m_GuidLow == 0 && m_GuidHigh == 0)
            {
                var bytes = Guid.NewGuid().ToByteArray();
                m_GuidLow = BitConverter.ToUInt64(bytes, 0);
                m_GuidHigh = BitConverter.ToUInt64(bytes, 8);
                EditorUtility.SetDirty(this);
            }
        }
#endif

#pragma warning disable CS0649
        [SerializeField]
        ulong m_GuidLow;

        [SerializeField]
        ulong m_GuidHigh;
#pragma warning restore CS0649

        [SerializeField]
        internal List<XRReferenceObject> m_ReferenceObjects = new List<XRReferenceObject>();
    }
}
