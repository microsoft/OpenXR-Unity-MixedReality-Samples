using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Utility methods for using <see cref="PackageVersion"/>.
    /// </summary>
    public static class PackageVersionUtility
    {
        const string k_PackageCacheName = "PackageCache";
        static bool s_PackageLogLock;
        static Dictionary<string, PackageVersion> s_PackageCache;

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnReloadScripts()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isBatchMode)
                return;

            Application.logMessageReceived += OnLogMessage;
        }

        static void OnLogMessage(string message, string stackTrace, LogType logType)
        {
            if (logType == LogType.Error && message.Contains(k_PackageCacheName) && !s_PackageLogLock)
            {
                UpdatePackageVersions();
                s_PackageLogLock = true;
            }
        }

        static void UpdatePackageVersions()
        {
            if (s_PackageCache == null)
                s_PackageCache = new Dictionary<string, PackageVersion>();

            s_PackageCache.Clear();
            var request = UnityEditor.PackageManager.Client.List(true, true);
            while (!request.IsCompleted)
            {
                System.Threading.Thread.Sleep(50);
            }

            foreach (var package in request.Result)
            {
                var version = new PackageVersion(package.version);
                s_PackageCache.Add(package.name, version);
            }

            s_PackageLogLock = false;
        }

        /// <summary>
        /// Get the <see cref="PackageVersion"/> struct for a given <paramref name="packageName"/>.
        /// </summary>
        /// <param name="packageName">The package in the Package Manager <see cref="UnityEditor.PackageManager.Client"/>
        /// that we want the <see cref="PackageVersion"/> information.</param>
        /// <returns>If getting the package was successful, the package's <see cref="PackageVersion"/> is returned,
        /// otherwise the <c>default</c> value is returned.</returns>
        public static PackageVersion GetPackageVersion(string packageName)
        {
            if (s_PackageCache == null)
            {
                s_PackageCache = new Dictionary<string, PackageVersion>();
                UpdatePackageVersions();
            }

            return s_PackageCache.TryGetValue(packageName, out var version) ? version : default;
        }

        /// <summary>
        /// Is the a package with the <paramref name="packageName"/> installed in the Package Manager
        /// <see cref="UnityEditor.PackageManager.Client"/>.
        /// </summary>
        /// <param name="packageName">The package in the Package Manager.</param>
        /// <returns><c>true</c> if the package manager has the package installed, other <c>false</c>.</returns>
        public static bool IsPackageInstalled(string packageName)
        {
            return GetPackageVersion(packageName) != default;
        }

        /// <summary>
        /// Creates a new <see cref="PackageVersion"/> struct that only includes the Major version number with all other
        /// data being default values.
        /// </summary>
        /// <param name="value"><see cref="PackageVersion"/> to have other version data removed.</param>
        /// <returns>A new <see cref="PackageVersion"/> that includes only the Major version information.</returns>
        public static PackageVersion ToMajor(this PackageVersion value)
        {
            return new PackageVersion(value.MajorVersion, 0, 0, null, null);
        }

        /// <summary>
        /// Creates a new <see cref="PackageVersion"/> struct that only includes the Major and Minor version number
        /// with all other data being default values.
        /// </summary>
        /// <param name="value"><see cref="PackageVersion"/> to have other version data removed.</param>
        /// <returns>A new <see cref="PackageVersion"/> that includes only the Major and Minor version information.</returns>
        public static PackageVersion ToMajorMinor(this PackageVersion value)
        {
            return new PackageVersion(value.MajorVersion, value.MinorVersion, 0, null, null);
        }

        /// <summary>
        /// Creates a new <see cref="PackageVersion"/> struct that only includes the Major, Minor, and Patch version
        /// number with all other data being default values.
        /// </summary>
        /// <param name="value"><see cref="PackageVersion"/> to have other version data removed.</param>
        /// <returns>A new <see cref="PackageVersion"/> that includes only the Major, Minor, and Patch version information.</returns>
        public static PackageVersion ToMajorMinorPatch(this PackageVersion value)
        {
            return new PackageVersion(value.MajorVersion, value.MinorVersion, value.PatchVersion, null, null);
        }

        /// <summary>
        /// Compares the left hand string to the right hand string and returns an integer that indicates their
        /// relationship to one another.
        /// </summary>
        /// <param name="lh">left hand version value string</param>
        /// <param name="rh">right hand version value string</param>
        /// <returns>
        /// A signed integer that indicates the relative values of the <paramref name="lh"/> and <paramref name="rh"/>>
        /// | Return Value | Condition |
        /// | -- | -- |
        /// | Less than zero | <paramref name="lh"/> is null or empty and <paramref name="rh"/> contains a <c>value</c>
        /// or <paramref name="lh"/> would sort before <paramref name="rh"/>. |
        /// | Zero | <paramref name="lh"/> and <paramref name="rh"></paramref> are equal. |
        /// | Greater than zero | <paramref name="lh"> contains a <c>value</c> and <paramref name="rh"/> </paramref> is null
        /// or empty or <paramref name="lh"/> would sort after <paramref name="rh"/>. |
        /// </returns>
        internal static int EmptyOrNullSubVersionCompare(string lh, string rh)
        {
            var lhSplit = lh.Split('.');
            var rhSplit = rh.Split('.');
            var lhSplitLength = lhSplit.Length;
            var rhSplitLenght = rhSplit.Length;

            var compare = 0;
            for (var i = 0; i < lhSplit.Length + 1; i++)
            {
                var lhSub = i < lhSplitLength ? lhSplit[i] : string.Empty;
                var rhSub = i < rhSplitLenght ? rhSplit[i] : string.Empty;

                // first compare to check if on side has a value and the other does not.
                // Sub version strings that are empty or null are considered > those that have a value.
                compare = string.IsNullOrEmpty(lhSub).CompareTo(string.IsNullOrEmpty(rhSub));
                if (compare == 0)
                {
                    if (string.IsNullOrEmpty(lhSub) && string.IsNullOrEmpty(rhSub))
                        return compare;

                    compare = lhSub.CompareTo(rhSub);
                    if (compare != 0)
                        return compare;
                }
                else
                    return compare;
            }

            return compare;
        }
    }
}
