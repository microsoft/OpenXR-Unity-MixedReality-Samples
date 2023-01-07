using System;
using System.Text;
using Unity.Collections;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Container for mesh data associated with an <see cref="XRFace"/>. Not all implementations
    /// support all data. Check for existence with <c>NativeArray</c>'s <c>IsCreated</c> property.
    /// </summary>
    public struct XRFaceMesh : IEquatable<XRFaceMesh>, IDisposable
    {
        /// <summary>
        /// Attributes associated with the face mesh, such as normals and texture coordinates.
        /// Vertex positions and triangle indices are assumed to exist already. These are
        /// optional attributes. Used with <see cref="Resize(int, int, Attributes, Allocator)"/>.
        /// </summary>
        [Flags]
        public enum Attributes
        {
            /// <summary>
            /// No attributes specified.
            /// </summary>
            None = 0,

            /// <summary>
            /// The face mesh contains normals.
            /// </summary>
            Normals = 1 << 0,

            /// <summary>
            /// The face mesh contains texture coordinates.
            /// </summary>
            UVs = 1 << 1
        }

        /// <summary>
        /// Resize the <c>NativeArray</c>s held by this struct. This method will deallocate
        /// the <c>NativeArray</c>s if they are not needed or if they are not the correct size.
        /// If they are already the correct size, this method does not mutate those <c>NativeArray</c>s.
        /// This facilitate memory reuse when the number of vertices or triangles in the face does
        /// not change frequently.
        /// </summary>
        /// <param name="vertexCount">The number of vertices in the mesh.</param>
        /// <param name="triangleCount">The number of triangles in the mesh.</param>
        /// <param name="attributes">Optional mesh attributes. This affects whether <see cref="normals"/>
        /// and <see cref="uvs"/> will be (re)allocated or disposed.</param>
        /// <param name="allocator">If a reallocation is required, this allocator will be used.</param>
        public void Resize(
            int vertexCount,
            int triangleCount,
            Attributes attributes,
            Allocator allocator)
        {
            Resize(vertexCount, allocator, ref m_Vertices, true);
            Resize(vertexCount, allocator, ref m_Normals, (attributes & Attributes.Normals) != 0);
            Resize(vertexCount, allocator, ref m_UVs, (attributes & Attributes.UVs) != 0);
            Resize(triangleCount * 3, allocator, ref m_Indices, true);
        }

        /// <summary>
        /// The vertices in the mesh. This is a parallel array to
        /// <see cref="normals"/> and <see cref="uvs"/>.
        /// </summary>
        public NativeArray<Vector3> vertices => m_Vertices;
        NativeArray<Vector3> m_Vertices;

        /// <summary>
        /// The normals in the mesh. This is a parallel array to
        /// <see cref="vertices"/> and <see cref="uvs"/>.
        /// </summary>
        public NativeArray<Vector3> normals => m_Normals;
        NativeArray<Vector3> m_Normals;

        /// <summary>
        /// The triangle indices of the mesh. There are three times as many indices as triangles.
        /// </summary>
        public NativeArray<int> indices => m_Indices;
        NativeArray<int> m_Indices;

        /// <summary>
        /// The texture coordinates for the mesh. This is a parallel
        /// array to <see cref="vertices"/> and <see cref="normals"/>.
        /// </summary>
        public NativeArray<Vector2> uvs => m_UVs;
        NativeArray<Vector2> m_UVs;

        /// <summary>
        /// Disposes of the all four native arrays:
        /// <see cref="vertices"/>, <see cref="normals"/>, <see cref="uvs"/>, and <see cref="indices"/>.
        /// Checking for creation before calling Dispose.
        /// </summary>
        public void Dispose()
        {
            if (m_Vertices.IsCreated)
                m_Vertices.Dispose();
            if (m_Normals.IsCreated)
                m_Normals.Dispose();
            if (m_Indices.IsCreated)
                m_Indices.Dispose();
            if (m_UVs.IsCreated)
                m_UVs.Dispose();
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = m_Vertices.GetHashCode();
                hash = hash * 486187739 + m_Normals.GetHashCode();
                hash = hash * 486187739 + m_Indices.GetHashCode();
                hash = hash * 486187739 + m_UVs.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRFaceMesh"/> and
        /// <see cref="Equals(XRFaceMesh)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is XRFaceMesh))
                return false;

            return Equals((XRFaceMesh)obj);
        }

        /// <summary>
        /// Generates a string suitable for debugging purposes.
        /// </summary>
        /// <returns>A string representation of this <see cref="XRFaceMesh"/>.</returns>
        public override string ToString()
        {
            return string.Format("XRFaceMesh: {0} vertices {1} normals {2} indices {3} uvs",
                m_Vertices.Length, m_Normals.Length, m_Indices.Length, m_UVs.Length);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRFaceMesh"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRFaceMesh"/>, otherwise false.</returns>
        public bool Equals(XRFaceMesh other)
        {
            return
                m_Vertices.Equals(other.m_Vertices) &&
                m_Normals.Equals(other.m_Normals) &&
                m_Indices.Equals(other.m_Indices) &&
                m_UVs.Equals(other.m_UVs);
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRFaceMesh)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRFaceMesh lhs, XRFaceMesh rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRFaceMesh)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRFaceMesh lhs, XRFaceMesh rhs) => !lhs.Equals(rhs);

        static void Resize<T>(int length, Allocator allocator, ref NativeArray<T> array, bool shouldExist) where T : struct
        {
            if (shouldExist)
            {
                if (array.IsCreated)
                {
                    if (array.Length != length)
                    {
                        array.Dispose();
                        array = new NativeArray<T>(length, allocator);
                    }
                }
                else
                {
                    array = new NativeArray<T>(length, allocator);
                }
            }
            else if (array.IsCreated)
            {
                array.Dispose();
            }
        }
    }
}
