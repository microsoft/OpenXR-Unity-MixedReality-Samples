using System;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Container for a human body pose 2D joint as part of a AR detected screen space skeleton.
    /// </summary>
    public struct XRHumanBodyPose2DJoint : IEquatable<XRHumanBodyPose2DJoint>
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
        /// A negative parent index means that the joint has no parent in the hierachy.
        /// </remarks>
        public int parentIndex => m_ParentIndex;
        int m_ParentIndex;

        /// <summary>
        /// The position of the joint in 2D screenspace.
        /// </summary>
        /// <value>
        /// The position of the joint in 2D screenspace.
        /// </value>
        public Vector2 position => m_Position;
        Vector2 m_Position;

        /// <summary>
        /// Whether the joint is tracked.
        /// </summary>
        /// <value>
        /// <c>true</c> if the joint is tracked. Otherwise, <c>false</c>.
        /// </value>
        public bool tracked => (m_Tracked != 0);
        int m_Tracked;

        /// <summary>
        /// Constructs a <c>XRHumanBodyPose2DJoint</c> with the given parameters.
        /// </summary>
        /// <param name="index">The index of the joint in the skeleton hierachy.</param>
        /// <param name="parentIndex">The index of the parent joint in the skeleton hierarchy.</param>
        /// <param name="position">The position of the joint in 2D screenspace.</param>
        /// <param name="tracked">Whether the joint is tracked.</param>
        public XRHumanBodyPose2DJoint(int index, int parentIndex, Vector2 position, bool tracked)
        {
            m_Index = index;
            m_ParentIndex = parentIndex;
            m_Position = position;
            m_Tracked = tracked ? 1 : 0;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRHumanBodyPose2DJoint"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRHumanBodyPose2DJoint"/>, otherwise false.</returns>
        public bool Equals(XRHumanBodyPose2DJoint other)
        {
            return (m_Index.Equals(other.m_Index) && m_ParentIndex.Equals(other.m_ParentIndex)
                    && m_Position.Equals(other.m_Position) && m_Tracked.Equals(other.m_Tracked));
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRHumanBodyPose2DJoint"/> and
        /// <see cref="Equals(XRHumanBodyPose2DJoint)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj)
        {
            return ((obj is XRHumanBodyPose2DJoint) && Equals((XRHumanBodyPose2DJoint)obj));
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRHumanBodyPose2DJoint)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRHumanBodyPose2DJoint lhs, XRHumanBodyPose2DJoint rhs)  => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRHumanBodyPose2DJoint)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRHumanBodyPose2DJoint lhs, XRHumanBodyPose2DJoint rhs) => !lhs.Equals(rhs);

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
                hashCode = (hashCode * 486187739) + m_Position.GetHashCode();
                hashCode = (hashCode * 486187739) + m_Tracked.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Generates a string representation of this <see cref="XRHumanBodyPose2DJoint"/>. Floating point
        /// values use the ["F5"](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)
        /// format specifier.
        /// </summary>
        /// <returns>A string representation of this <see cref="XRHumanBodyPose2DJoint"/>.</returns>
        public override string ToString()
        {
            return ToString("F5");
        }

        /// <summary>
        /// Generates a string representation of this <see cref="XRHumanBodyPose2DJoint"/>.
        /// </summary>
        /// <param name="format">A format specifier used for the floating point fields.</param>
        /// <returns>A string representation of this <see cref="XRHumanBodyPose2DJoint"/>.</returns>
        public string ToString(string format)
        {
            return String.Format("joint [{0}] -> [{1}] {2}", m_Index, m_ParentIndex,
                                 tracked ? m_Position.ToString(format) : "<not tracked>");
        }
    }
}
