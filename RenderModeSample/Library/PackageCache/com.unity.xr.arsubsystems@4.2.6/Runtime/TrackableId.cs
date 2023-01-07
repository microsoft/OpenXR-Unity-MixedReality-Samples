using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A session-unique identifier for trackables in the real-world environment, such as planes and feature points.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ids are generally unique to a particular session, but multiple sessions might produce
    /// identical ids for different trackables.
    /// </para><para>
    /// A trackable id is a 128 bit number, stored as two <c>ulong</c>s. This makes it large enough to hold a <c>Guid</c>.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct TrackableId : IEquatable<TrackableId>
    {
        /// <summary>
        /// Regular expression that matches a valid trackable identifier.
        /// </summary>
        static readonly Regex s_TrackableIdRegex = new Regex(@"^(?<part1>[a-fA-F\d]{16})-(?<part2>[a-fA-F\d]{16})$",
                                                             RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.Singleline|RegexOptions.ExplicitCapture);

        /// <summary>
        /// Get the invalid id.
        /// </summary>
        public static TrackableId invalidId => s_InvalidId;

        /// <summary>
        /// The first half of the id.
        /// </summary>
        public ulong subId1
        {
            get => m_SubId1;
            set => m_SubId1 = value;
        }

        /// <summary>
        /// The second half of the id.
        /// </summary>
        public ulong subId2
        {
            get => m_SubId2;
            set => m_SubId2 = value;
        }

        /// <summary>
        /// Constructs a <c>TrackableId</c> from two <c>ulong</c>s.
        /// </summary>
        /// <param name="subId1">The first half of the id.</param>
        /// <param name="subId2">The second half of the id.</param>
        public TrackableId(ulong subId1, ulong subId2)
        {
            m_SubId1 = subId1;
            m_SubId2 = subId2;
        }

        /// <summary>
        /// Construct a trackable identifier by parsing the given identifier string.
        /// </summary>
        /// <param name="idString">An identifier string.</param>
        /// <exception cref="System.FormatException">Thrown if the given identifier string cannot be parsed.</exception>
        public TrackableId(string idString)
        {
            var regexMatch = s_TrackableIdRegex.Match(idString);
            if (!regexMatch.Success)
            {
                throw new FormatException($"trackable ID '{idString}' does not match expected format");
            }

            try
            {
                m_SubId1 = ulong.Parse(regexMatch.Groups["part1"].Value, System.Globalization.NumberStyles.HexNumber);
                m_SubId2 = ulong.Parse(regexMatch.Groups["part2"].Value, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception e)
            {
                throw new FormatException($"cannot parse trackable ID '{idString}'", e);
            }
        }

        /// <summary>
        /// Generates a string representation of the id suitable for debugging.
        /// </summary>
        /// <returns>A string representation of the id.</returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}",
                m_SubId1.ToString("X16"),
                m_SubId2.ToString("X16"));
        }

        /// <summary>
        /// Generates a hash code suitable for use in a <c>Dictionary</c> or <c>Set</c>.
        /// </summary>
        /// <returns>A hash code for participation in certain collections.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(m_SubId1.GetHashCode(), m_SubId2.GetHashCode());

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="TrackableId"/> and
        /// <see cref="Equals(TrackableId)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is TrackableId && Equals((TrackableId)obj);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="TrackableId"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="TrackableId"/>, otherwise `false`.</returns>
        public bool Equals(TrackableId other) => (m_SubId1 == other.m_SubId1) && (m_SubId2 == other.m_SubId2);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(TrackableId)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator==(TrackableId lhs, TrackableId rhs) =>
            (lhs.m_SubId1 == rhs.m_SubId1) &&
            (lhs.m_SubId2 == rhs.m_SubId2);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(TrackableId)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator!=(TrackableId lhs, TrackableId rhs) =>
            (lhs.m_SubId1 != rhs.m_SubId1) ||
            (lhs.m_SubId2 != rhs.m_SubId2);

        static TrackableId s_InvalidId = new TrackableId(0, 0);

        ulong m_SubId1;

        ulong m_SubId2;
    }
}
