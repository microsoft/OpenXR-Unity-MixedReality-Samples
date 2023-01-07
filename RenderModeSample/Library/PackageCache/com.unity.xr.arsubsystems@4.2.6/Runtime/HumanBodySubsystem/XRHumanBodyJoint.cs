using System;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Containter for the human body joint data.
    /// </summary>
    public struct XRHumanBodyJoint : IEquatable<XRHumanBodyJoint>
    {
        /// <summary>
        /// The index for the joint in the skeleton hierachy.
        /// </summary>
        /// <value>
        /// The index for the joint in the skeleton hierachy.
        /// </value>
        /// <remarks>
        /// All indices will be non-negative.
        /// </remarks>
        public int index => m_Index;
        int m_Index;

        /// <summary>
        /// The index for the parent joint in the skeleton hierachy.
        /// </summary>
        /// <value>
        /// The index for the parent joint in the skeleton hierachy.
        /// </value>
        /// <remarks>
        /// A negative parent index means the joint has no parent in the hierachy.
        /// </remarks>
        public int parentIndex => m_ParentIndex;
        int m_ParentIndex;

        /// <summary>
        /// The scale relative to the parent joint.
        /// </summary>
        /// <value>
        /// The scale relative to the parent joint.
        /// </value>
        public Vector3 localScale => m_LocalScale;
        Vector3 m_LocalScale;

        /// <summary>
        /// The pose relative to the parent joint.
        /// </summary>
        /// <value>
        /// The pose relative to the parent joint.
        /// </value>
        public Pose localPose => m_LocalPose;
        Pose m_LocalPose;

        /// <summary>
        /// The scale relative to the human body origin.
        /// </summary>
        /// <value>
        /// The scale relative to the human body origin.
        /// </value>
        public Vector3 anchorScale => m_AnchorScale;
        Vector3 m_AnchorScale;

        /// <summary>
        /// The pose relative to the human body origin.
        /// </summary>
        /// <value>
        /// The pose relative to the human body origin.
        /// </value>
        public Pose anchorPose => m_AnchorPose;
        Pose m_AnchorPose;

        /// <summary>
        /// Whether the joint is tracked.
        /// </summary>
        /// <value>
        /// <c>true</c> if the joint is tracked. Otherwise, <c>false</c>.
        /// </value>
        public bool tracked => (m_Tracked != 0);
        int m_Tracked;

        /// <summary>
        /// Construct the human body joint.
        /// </summary>
        /// <param name="index">The index for the joint in the skeleton.</param>
        /// <param name="parentIndex">The index for the parent joint in the skeleton.</param>
        /// <param name="localScale">The scale relative to the parent joint.</param>
        /// <param name="localPose">The pose relative to the parent joint.</param>
        /// <param name="anchorScale">The scale relative to the human body origin.</param>
        /// <param name="anchorPose">The pose relative to the human body origin.</param>
        /// <param name="tracked">Whether the joint is tracked.</param>
        public XRHumanBodyJoint(int index, int parentIndex, Vector3 localScale, Pose localPose, Vector3 anchorScale, Pose anchorPose, bool tracked)
        {
            m_Index = index;
            m_ParentIndex = parentIndex;
            m_LocalScale = localScale;
            m_LocalPose = localPose;
            m_AnchorScale = anchorScale;
            m_AnchorPose = anchorPose;
            m_Tracked = tracked ? 1 : 0;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRHumanBodyJoint"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRHumanBodyJoint"/>, otherwise false.</returns>
        public bool Equals(XRHumanBodyJoint other)
        {
            return (m_Index.Equals(other.m_Index) && m_ParentIndex.Equals(other.m_ParentIndex)
                    && m_LocalScale.Equals(other.m_LocalScale) && m_LocalPose.Equals(other.m_LocalPose)
                    && m_AnchorScale.Equals(other.m_AnchorScale) && m_AnchorPose.Equals(other.m_AnchorPose)
                    && m_Tracked.Equals(other.m_Tracked));
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRHumanBodyJoint"/> and
        /// <see cref="Equals(XRHumanBodyJoint)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj)
        {
            return ((obj is XRHumanBodyJoint) && Equals((XRHumanBodyJoint)obj));
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRHumanBodyJoint)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRHumanBodyJoint lhs, XRHumanBodyJoint rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRHumanBodyJoint)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRHumanBodyJoint lhs, XRHumanBodyJoint rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            int hashCode = 486187739;
            unchecked
            {
                hashCode = (hashCode * 486187739) + m_Index.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ParentIndex.GetHashCode();
                hashCode = (hashCode * 486187739) + m_LocalScale.GetHashCode();
                hashCode = (hashCode * 486187739) + m_LocalPose.GetHashCode();
                hashCode = (hashCode * 486187739) + m_AnchorScale.GetHashCode();
                hashCode = (hashCode * 486187739) + m_AnchorPose.GetHashCode();
                hashCode = (hashCode * 486187739) + m_Tracked.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Generates a string representation of this <see cref="XRHumanBodyJoint"/>. Floating point
        /// values using the ["F5"](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)
        /// format specifier.
        /// </summary>
        /// <returns>A string representation of this <see cref="XRHumanBodyJoint"/>.</returns>
        public override string ToString() => ToString("F5");

        /// <summary>
        /// Generates a string representation of this <see cref="XRHumanBodyJoint"/>.
        /// </summary>
        /// <param name="format">A format specifier used for the floating point fields.</param>
        /// <returns>A string representation of this <see cref="XRHumanBodyJoint"/>.</returns>
        public string ToString(string format)
        {
            return String.Format("joint [{0}] -> [{1}] localScale:{2} localPose:{3} anchorScale:{4} anchorPose:{5} tracked:{6}",
                                 m_Index, m_ParentIndex, m_LocalScale.ToString(format), m_LocalPose.ToString(format),
                                 m_AnchorScale.ToString(format), m_AnchorPose.ToString(format), tracked.ToString());
        }
    }
}
