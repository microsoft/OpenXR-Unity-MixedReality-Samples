using System;
using Unity.Collections;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the data (arrays of positions, confidence values, and identifiers) associated with a point cloud.
    /// </summary>
    public struct XRPointCloudData : IEquatable<XRPointCloudData>, IDisposable
    {
        /// <summary>
        /// Positions for each point in the point cloud. This array is parallel
        /// to <see cref="confidenceValues"/> and <see cref="identifiers"/>.
        /// Use <c>positions.IsCreated</c> to check if these exist.
        /// </summary>
        public NativeArray<Vector3> positions
        {
            get => m_Positions;
            set => m_Positions = value;
        }
        NativeArray<Vector3> m_Positions;

        /// <summary>
        /// Confidence values for each point in the point cloud. This array is parallel
        /// to <see cref="positions"/> and <see cref="identifiers"/>.
        /// Use <c>confidenceValues.IsCreated</c> to check if these exist.
        /// </summary>
        public NativeArray<float> confidenceValues
        {
            get => m_ConfidenceValues;
            set => m_ConfidenceValues = value;
        }
        NativeArray<float> m_ConfidenceValues;

        /// <summary>
        /// Identifiers for each point in the point cloud. This array is parallel
        /// to <see cref="positions"/> and <see cref="confidenceValues"/>.
        /// Use <c>identifiers.IsCreated</c> to check if these exist.
        /// </summary>
        /// <remarks>
        /// Identifiers are unique to a particular session, which means you can use
        /// the identifier to match a particular point in the point cloud with a
        /// previously detected point.
        /// </remarks>
        public NativeArray<ulong> identifiers
        {
            get => m_Identifiers;
            set => m_Identifiers = value;
        }
        NativeArray<ulong> m_Identifiers;

        /// <summary>
        /// Checks for the existence of the <c>NativeArray</c>s and disposes them.
        /// </summary>
        public void Dispose()
        {
            if (m_Positions.IsCreated)
                m_Positions.Dispose();
            if (m_ConfidenceValues.IsCreated)
                m_ConfidenceValues.Dispose();
            if (m_Identifiers.IsCreated)
                m_Identifiers.Dispose();
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = m_Positions.GetHashCode();
                hash = hash * 486187739 + m_ConfidenceValues.GetHashCode();
                hash = hash * 486187739 + m_Identifiers.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRPointCloudData"/> and
        /// <see cref="Equals(XRPointCloudData)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is XRPointCloudData && Equals((XRPointCloudData)obj);

        /// <summary>
        /// Generates a string representation of this <see cref="XRPointCloudData"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="XRPointCloudData"/>.</returns>
        public override string ToString()
        {
            return string.Format("XRPointCloudData: {0} positions {1} confidence values {2} identifiers",
                m_Positions.Length, m_ConfidenceValues.Length, m_Identifiers.Length);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRPointCloudData"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRPointCloudData"/>, otherwise false.</returns>
        public bool Equals(XRPointCloudData other)
        {
            return
                m_Positions.Equals(other.m_Positions) &&
                m_ConfidenceValues.Equals(other.m_ConfidenceValues) &&
                m_Identifiers.Equals(other.m_Identifiers);
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRPointCloudData)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRPointCloudData lhs, XRPointCloudData rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRPointCloudData)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRPointCloudData lhs, XRPointCloudData rhs) => !lhs.Equals(rhs);
    }
}
