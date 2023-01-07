using System;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents an entry in an <see cref="XRReferenceImageLibrary"/>.
    /// </summary>
    /// <remarks>
    /// A reference image is an image to look for in the physical environment.
    /// The <see cref="XRReferenceImage"/> does not directly reference a <c>Texture2D</c>
    /// or other image data; it only stores the GUID of the <c>Texture2D</c> as it
    /// appears in the <c>AssetDatabase</c>. At build time, platform-specific build steps
    /// can use the GUIDs to look up the source textures and generate an appropriate
    /// image database. At runtime, detected images can be matched up with the source
    /// <see cref="XRReferenceImage"/>.
    /// </remarks>
    [Serializable]
    public struct XRReferenceImage : IEquatable<XRReferenceImage>
    {
        /// <summary>
        /// Constructs a <see cref="XRReferenceImage"/>.
        /// </summary>
        /// <param name="guid">The [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid?view=netframework-4.8)
        /// associated with this image.</param>
        /// <param name="textureGuid">
        /// The [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid?view=netframework-4.8)
        /// of the source texture as it appeared in the
        /// [AssetDatabase](https://docs.unity3d.com/ScriptReference/AssetDatabase.html)
        /// in the Editor.
        /// </param>
        /// <param name="size">
        /// Optional. The size of the image, in meters. This can improve image detection,
        /// and might be required by some platforms.
        /// </param>
        /// <param name="name">A name associated with this reference image.</param>
        /// <param name="texture">
        /// The source texture which this reference image represents.
        /// This can be `null` to avoid including the texture in
        /// the Player build if you don't want that. See `XRReferenceImageLibraryExtensions.SetTexture`
        /// for more details.
        /// </param>
        public XRReferenceImage(
            SerializableGuid guid, SerializableGuid textureGuid,
            Vector2? size, string name, Texture2D texture)
        {
            m_SerializedGuid = guid;
            m_SerializedTextureGuid = textureGuid;
            m_SpecifySize = size.HasValue;
            m_Size = size.HasValue ? size.Value : Vector2.zero;
            m_Name = name;
            m_Texture = texture;
        }

        /// <summary>
        /// The [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid?view=netframework-4.8)
        /// associated with this image.
        /// </summary>
        public Guid guid => m_SerializedGuid.guid;

        /// <summary>
        /// The [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid?view=netframework-4.8)
        /// of the source texture as it appears in the
        /// [AssetDatabase](https://docs.unity3d.com/ScriptReference/AssetDatabase.html)
        /// in the Editor.
        /// </summary>
        public Guid textureGuid => m_SerializedTextureGuid.guid;

        /// <summary>
        /// Must be set to true for <see cref="size"/> to be used.
        /// </summary>
        public bool specifySize => m_SpecifySize;

        /// <summary>
        /// The size of the image, in meters. This can improve image detection,
        /// and might be required by some platforms.
        /// </summary>
        public Vector2 size => m_Size;

        /// <summary>
        /// The width of the image, in meters.
        /// </summary>
        public float width => m_Size.x;

        /// <summary>
        /// The height of the image, in meters.
        /// </summary>
        public float height => m_Size.y;

        /// <summary>
        /// A name associated with this reference image.
        /// </summary>
        public string name => m_Name;

        /// <summary>
        /// The source texture which this reference image represents.
        /// This may be <c>null</c> to avoid including the texture in
        /// the Player build if you don't want that. See
        /// <c>UnityEditor.XR.ARSubsystems.XRReferenceImageLibraryExtensions.SetTexture</c>
        /// for more details.
        /// </summary>
        public Texture2D texture => m_Texture;

        /// <summary>
        /// Provides a string representation suitable for debug logging.
        /// </summary>
        /// <returns>A string representation of the reference image.</returns>
        public override string ToString() =>
            $"GUID: '{guid}', Texture GUID: '{textureGuid}` Size: {(m_SpecifySize ? "" : "NOT ")} specified {m_Size}";

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's <see cref="guid"/>.</returns>
        public override int GetHashCode() => guid.GetHashCode();

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRReferenceImage"/> and
        /// <see cref="Equals(XRReferenceImage)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => (obj is XRReferenceImage) && Equals((XRReferenceImage)obj);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRReferenceImage"/> to compare against.</param>
        /// <returns>`True` if the <see cref="guid"/> of this reference image matches <paramref name="other"/>'s, otherwise false.</returns>
        public bool Equals(XRReferenceImage other) => guid.Equals(other.guid);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRReferenceImage)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRReferenceImage lhs, XRReferenceImage rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRReferenceImage)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRReferenceImage lhs, XRReferenceImage rhs) => !lhs.Equals(rhs);

#pragma warning disable CS0649
        [SerializeField]
        internal SerializableGuid m_SerializedGuid;

        [SerializeField]
        internal SerializableGuid m_SerializedTextureGuid;

        [SerializeField]
        internal Vector2 m_Size;

        [SerializeField]
        internal bool m_SpecifySize;

        [SerializeField]
        internal string m_Name;

        [SerializeField]
        internal Texture2D m_Texture;
#pragma warning restore CS0649
    }
}
