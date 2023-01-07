using System;
using System.Text.RegularExpressions;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Structure for storing the package version in a logically comparable format.
    ///
    /// The version information stored in the <see cref="PackageVersion"/> follows the Semantic Versioning Specification
    /// (SemVer) https://semver.org/ The version consists of a <see cref="MajorVersion"/>.<see cref="MinorVersion"/>.<see cref="PatchVersion"/>
    /// numerical value followed by optional -<see cref="Prerelease"/>+<see cref="BuildMetaData"/> version information.
    ///
    /// <remarks><see cref="PackageVersion"/> follows all the standard for valid and invalid formatting of a version
    /// except for limiting the <see cref="MajorVersion"/>, <see cref="MinorVersion"/>, and <see cref="PatchVersion"/>
    /// to the <c>ulong.MaxValue</c>. Their is no such restriction for the values of <see cref="Prerelease"/> or <see cref="BuildMetaData"/>.</remarks>
    /// </summary>
    public readonly struct PackageVersion : IEquatable<PackageVersion>, IComparable<PackageVersion>
    {
        const string k_Major = "major";
        const string k_Minor = "minor";
        const string k_Patch = "patch";
        const string k_Prerelease = "prerelease";
        const string k_BuildMetaData = "buildmetadata";

        // From https://semver.org/ standard https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
        // ^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$
        static readonly Regex k_Regex = new Regex($@"^(?<{k_Major}>0|[1-9]\d*)\.(?<{k_Minor}>0|[1-9]\d*)\." +
            $@"(?<{k_Patch}>0|[1-9]\d*)(?:-(?<{k_Prerelease}>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)" +
            $@"(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<{k_BuildMetaData}>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$");

        readonly string m_Version;

        readonly ulong m_MajorVersion;
        readonly ulong m_MinorVersion;
        readonly ulong m_PatchVersion;
        readonly string m_Prerelease;
        readonly string m_BuildMetaData;

        /// <summary>
        /// Major version number
        /// </summary>
        public ulong MajorVersion => m_MajorVersion;
        
        /// <summary>
        /// Minor version number
        /// </summary>
        public ulong MinorVersion => m_MinorVersion;
        
        /// <summary>
        /// Patch version number
        /// </summary>
        public ulong PatchVersion => m_PatchVersion;
        
        /// <summary>
        /// Is the package pre release
        /// </summary>
        public bool IsPrerelease => !string.IsNullOrEmpty(m_Prerelease);
        
        /// <summary>
        /// The prerelease version information
        /// </summary>
        public string Prerelease => m_Prerelease;
        
        /// <summary>
        /// The build metadata version information
        /// </summary>
        public string BuildMetaData => m_BuildMetaData;

        /// <summary>
        /// Creates a <see cref="PackageVersion"/> structure from a valid <paramref name="version"/> string.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <exception cref="FormatException">Thrown when the <paramref name="version"/> is not valid.</exception>
        public PackageVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                m_MajorVersion = default;
                m_MinorVersion = default;
                m_PatchVersion = default;
                m_Prerelease = default;
                m_BuildMetaData = default;
                m_Version = default;
                return;
            }

            m_MajorVersion = 0;
            m_MinorVersion = 0;
            m_PatchVersion = 0;
            m_Prerelease = string.Empty;
            m_BuildMetaData = string.Empty;
            m_Version = version;

            var match = k_Regex.Match(version);
            if (match.Success)
            {
                var groups = match.Groups;
                m_MajorVersion = ulong.Parse(groups[k_Major].Value);
                m_MinorVersion = ulong.Parse(groups[k_Minor].Value);
                m_PatchVersion = ulong.Parse(groups[k_Patch].Value);

                if (groups[k_Prerelease].Success)
                    m_Prerelease = groups[k_Prerelease].Value;

                if (groups[k_BuildMetaData].Success)
                    m_BuildMetaData = groups[k_BuildMetaData].Value;
            }
            else
                throw new FormatException($"Malformed package version string: {version}");
        }

        /// <summary>
        /// Creates a <see cref="PackageVersion"/> structure from the individual version components.
        /// </summary>
        /// <param name="major">The major version value.</param>
        /// <param name="minor">The minor version value.</param>
        /// <param name="patch">The patch version value.</param>
        /// <param name="prerelease">The prerelease version information.</param>
        /// <param name="buildMetaData">The build metadata version information.</param>
        public PackageVersion(ulong major, ulong minor, ulong patch, string prerelease, string buildMetaData)
            : this(GetValueString(major, minor, patch, prerelease, buildMetaData))
        {
        }

        internal static string GetValueString(ulong major, ulong minor, ulong patch, string prerelease, string buildMetaData)
        {
            var prereleaseString = string.Empty;
            if (!string.IsNullOrEmpty(prerelease))
                prereleaseString += $"-{prerelease}";

            var buildMetaDataString = string.Empty;
            if (!string.IsNullOrEmpty(buildMetaData))
                buildMetaDataString += $"+{buildMetaData}";

            return $"{major}.{minor}.{patch}{prereleaseString}{buildMetaDataString}";
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(PackageVersion other)
        {
            return m_MajorVersion == other.MajorVersion
                && m_MinorVersion == other.MinorVersion
                && m_PatchVersion == other.PatchVersion
                && m_Prerelease == other.Prerelease
                && m_BuildMetaData == other.BuildMetaData;
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public override bool Equals(object obj)
        {
            return obj is PackageVersion other && Equals(other);
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)m_MajorVersion;
                hashCode = (hashCode * 397) ^ (int)m_MinorVersion;
                hashCode = (hashCode * 397) ^ (int)m_PatchVersion;
                hashCode = (hashCode * 397) ^ m_Prerelease.GetHashCode();
                hashCode = (hashCode * 397) ^ m_BuildMetaData.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/>s are equal
        /// <returns><c>true</c> if operands are equal, <c>false</c> otherwise.</returns>
        /// </summary>
        public static bool operator==(PackageVersion left, PackageVersion right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/>s are not equal
        /// <returns><c>true</c> if operands are not equal, <c>false</c> otherwise.</returns>
        /// </summary>
        public static bool operator!=(PackageVersion left, PackageVersion right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/> <paramref name="left"/> is greater than <paramref name="right"/>.
        /// <returns><c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>, <c>false</c> otherwise.</returns>
        /// </summary>
        public static bool operator>(PackageVersion left, PackageVersion right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/> <paramref name="left"/> is less than <paramref name="right"/>.
        /// <returns><c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>, <c>false</c> otherwise.</returns>
        /// </summary>
        public static bool operator<(PackageVersion left, PackageVersion right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/> <paramref name="left"/> is greater than or equals <paramref name="right"/>.
        /// <returns><c>true</c> if <paramref name="left"/> is greater than or equals <paramref name="right"/>, <c>false</c> otherwise.</returns>
        /// </summary>
        public static bool operator>=(PackageVersion left, PackageVersion right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/> <paramref name="left"/> is less than or equals <paramref name="right"/>.
        /// <returns><c>true</c> if <paramref name="left"/> is less than or equals <paramref name="right"/>, <c>false</c> otherwise.</returns>
        /// </summary>
        public static bool operator<=(PackageVersion left, PackageVersion right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <inheritdoc cref="IComparable"/>
        public int CompareTo(PackageVersion other)
        {
            var compare = m_MajorVersion.CompareTo(other.MajorVersion);
            if (compare != 0)
                return compare;

            compare = m_MinorVersion.CompareTo(other.MinorVersion);
            if (compare != 0)
                return compare;

            compare = m_PatchVersion.CompareTo(other.PatchVersion);
            if (compare != 0)
                return compare;

            compare = PackageVersionUtility.EmptyOrNullSubVersionCompare(m_Prerelease, other.Prerelease);
            if (compare != 0)
                return compare;

            compare = PackageVersionUtility.EmptyOrNullSubVersionCompare(m_BuildMetaData, other.BuildMetaData);

            return compare;
        }

        /// <summary>
        /// Implicitly creates a new package version from a string.
        /// </summary>
        /// <param name="version">The package version string value.</param>
        /// <returns>A new <see cref="PackageVersion"/> structure.</returns>
        public static implicit operator PackageVersion(string version) => new PackageVersion(version);

        /// <summary>
        /// Returns a properly formatted version string for the <see cref="PackageVersion"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_Version;
        }
    }
}
