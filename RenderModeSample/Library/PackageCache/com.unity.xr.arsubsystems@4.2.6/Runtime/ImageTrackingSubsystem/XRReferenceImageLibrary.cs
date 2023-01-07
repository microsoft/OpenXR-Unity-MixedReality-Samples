using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A reference image library is a collection of images to search for in
    /// the physical environment when image tracking is enabled.
    /// </summary>
    /// <remarks>
    /// Image libraries are immutable at runtime. To create and manipulate
    /// an image library via Editor scripts, see the extension methods in
    /// <see cref="UnityEditor.XR.ARSubsystems.XRReferenceImageLibraryExtensions"/>.
    /// If you need to mutate the library at runtime, see <see cref="MutableRuntimeReferenceImageLibrary"/>.
    /// </remarks>
    [CreateAssetMenu(fileName="ReferenceImageLibrary", menuName="XR/Reference Image Library", order=1001)]
    [HelpURL(HelpUrls.Manual + "image-tracking.html")]
    public class XRReferenceImageLibrary
        : ScriptableObject
        , IReferenceImageLibrary
        , ISerializationCallbackReceiver
        , IEnumerable<XRReferenceImage>
    {
        /// <summary>
        /// The number of images in the library.
        /// </summary>
        public int count => m_Images.Count;

        /// <summary>
        /// (Read Only) Binary data associated with a string key.
        /// </summary>
        /// <remarks>
        /// This is used by providers to associate provider-specific data with the library. During Player Build (in an
        /// [IPreprocessBuildWithReport.OnPreprocessBuild](xref:UnityEditor.Build.IPreprocessBuildWithReport.OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport))
        /// callback), the data store is first cleared. Each enabled provider then has an opportunity to add one or more
        /// entries for itself.
        ///
        /// Providers can use this to store a serialized version of the image library specific to that provider.
        /// Set data with <see cref="UnityEditor.XR.ARSubsystems.XRReferenceImageLibraryExtensions.SetDataForKey"/>.
        /// </remarks>
        public IReadOnlyDictionary<string, byte[]> dataStore => m_DataStore.dictionary;

        /// <summary>
        /// Gets an enumerator which can be used to iterate over the reference images in this library.
        /// </summary>
        /// <example>
        /// This examples iterates over the reference images contained in the library.
        /// <code>
        /// XRReferenceImageLibrary imageLibrary = ...
        /// foreach (var referenceImage in imageLibrary)
        /// {
        ///     Debug.LogFormat("Image guid: {0}", referenceImage.guid);
        /// }
        /// </code>
        /// </example>
        /// <returns>Returns an enumerator which can be used to iterate over the reference images in the library.</returns>
        public List<XRReferenceImage>.Enumerator GetEnumerator() => m_Images.GetEnumerator();

        /// <summary>
        /// Gets an enumerator which can be used to iterate over the reference images in this library.
        /// </summary>
        /// <returns>Returns an object which can be used to iterate over the reference images in this library.</returns>
        IEnumerator<XRReferenceImage> IEnumerable<XRReferenceImage>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets an enumerator which can be used to iterate over the reference images in this library.
        /// </summary>
        /// <returns>Returns an object which can be used to iterate over the reference images in this library.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Get an image by index.
        /// </summary>
        /// <param name="index">The index of the image in the library. Must be between 0 and count - 1.</param>
        /// <returns>The <see cref="XRReferenceImage"/> at <paramref name="index"/>.</returns>
        /// <exception cref="System.IndexOutOfRangeException">Thrown if <paramref name="index"/> is not between 0 and <see cref="count"/><c> - 1</c>.</exception>
        public XRReferenceImage this[int index]
        {
            get
            {
                if (count == 0)
                    throw new IndexOutOfRangeException("The reference image library is empty; cannot index into it.");

                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException(string.Format("{0} is out of range. 'index' must be between 0 and {1}", index, count - 1));

                return m_Images[index];
            }
        }

        /// <summary>
        /// Get the index of <paramref name="referenceImage"/> in the image library.
        /// </summary>
        /// <param name="referenceImage">The <see cref="XRReferenceImage"/> to find.</param>
        /// <returns>The zero-based index of the <paramref name="referenceImage"/>, or -1 if not found.</returns>
        public int indexOf(XRReferenceImage referenceImage)
        {
            return m_Images.IndexOf(referenceImage);
        }

        /// <summary>
        /// A <c>GUID</c> associated with this reference library.
        /// The GUID is used to uniquely identify this library at runtime.
        /// </summary>
        public Guid guid => GuidUtil.Compose(m_GuidLow, m_GuidHigh);

        /// <summary>
        /// Invoked before serialization.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize() => m_DataStore.Serialize();

        /// <summary>
        /// Invoked after serialization.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize() => m_DataStore.Deserialize();

#if UNITY_EDITOR
        internal static IEnumerable<XRReferenceImageLibrary> All() => AssetDatabase
            .FindAssets($"t:{nameof(XRReferenceImageLibrary)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>);

        void Awake()
        {
            // We need to generate a new guid for new assets
            var shouldGenerateNewGuid = (m_GuidLow == 0 && m_GuidHigh == 0);

            // If this asset was duplicated from another, then we need to generate a unique guid, so
            // check against all existing XRReferenceImageLibraries in the asset database.
            if (!shouldGenerateNewGuid)
            {
                var currentGuid = guid;
                shouldGenerateNewGuid = All().Any(library => library != this && library.guid.Equals(currentGuid));
            }

            if (shouldGenerateNewGuid)
            {
                var bytes = Guid.NewGuid().ToByteArray();
                m_GuidLow = BitConverter.ToUInt64(bytes, 0);
                m_GuidHigh = BitConverter.ToUInt64(bytes, 8);
                EditorUtility.SetDirty(this);
            }
        }

        internal void InternalClearDataStore()
        {
            m_DataStore.dictionary.Clear();
            EditorUtility.SetDirty(this);
        }

        internal void InternalSetDataForKey(string key, byte[] data)
        {
            bool isDirty;
            if (data == null)
            {
                isDirty = m_DataStore.dictionary.Remove(key);
            }
            else
            {
                m_DataStore.dictionary[key] = data;
                isDirty = true;
            }

            if (isDirty)
            {
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
        SerializableDictionary<string, byte[]> m_DataStore = new SerializableDictionary<string, byte[]>();

        [SerializeField]
        internal List<XRReferenceImage> m_Images = new List<XRReferenceImage>();
    }
}
