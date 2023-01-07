using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace UnityEditor.XR.ARSubsystems
{
    /// <summary>
    /// Event arguments for the <see cref="ARBuildProcessor.IPreprocessBuild.OnPreprocessBuild"/> event.
    /// </summary>
    public readonly struct PreprocessBuildEventArgs : IEquatable<PreprocessBuildEventArgs>
    {
        /// <summary>
        /// The build target which is being built.
        /// </summary>
        public BuildTarget buildTarget { get; }

        /// <summary>
        /// The collection of active [XRLoaders](xref:UnityEngine.XR.Management.XRLoader) enabled for
        /// <see cref="buildTarget"/>.
        /// </summary>
        /// <remarks>
        /// Implementors of <see cref="ARBuildProcessor.IPreprocessBuild.OnPreprocessBuild"/> should check that an
        /// appropriate loader is present before continuing to execute any preprocessor logic.
        /// </remarks>
        public IReadOnlyList<XRLoader> activeLoadersForBuildTarget { get; }

        /// <summary>
        /// Construct for <see cref="PreprocessBuildEventArgs"/>.
        /// </summary>
        /// <param name="buildTarget">The build target which is being built.</param>
        /// <param name="activeLoadersForBuildTarget"> The collection of active
        /// [XRLoaders](xref:UnityEngine.XR.Management.XRLoader) enabled for <paramref name="buildTarget"/>.</param>
        public PreprocessBuildEventArgs(BuildTarget buildTarget, IReadOnlyList<XRLoader> activeLoadersForBuildTarget) =>
            (this.buildTarget, this.activeLoadersForBuildTarget) = (buildTarget, activeLoadersForBuildTarget);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The event args to compare for equality.</param>
        /// <returns>`True` if all properties are the same; `false` otherwise.</returns>
        public bool Equals(PreprocessBuildEventArgs other) =>
            buildTarget == other.buildTarget &&
            activeLoadersForBuildTarget?.Equals(other.activeLoadersForBuildTarget) == true;

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare with this object.</param>
        /// <returns>`True` if <paramref name="obj"/> is a <see cref="PreprocessBuildEventArgs"/> and
        /// <see cref="Equals(UnityEditor.XR.ARSubsystems.PreprocessBuildEventArgs)"/> is `true`, otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is PreprocessBuildEventArgs other && Equals(other);

        /// <summary>
        /// Computes a hash code from all properties suitable for use in a `Dictionary` or `HashSet`.
        /// </summary>
        /// <returns>Returns a hashcode of this object.</returns>
        public override int GetHashCode() =>
            HashCodeUtil.Combine(((int)buildTarget).GetHashCode(), HashCodeUtil.ReferenceHash(activeLoadersForBuildTarget));

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(UnityEditor.XR.ARSubsystems.PreprocessBuildEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns the same value as <see cref="Equals(UnityEditor.XR.ARSubsystems.PreprocessBuildEventArgs)"/>.</returns>
        public static bool operator ==(PreprocessBuildEventArgs lhs, PreprocessBuildEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as !<see cref="Equals(UnityEditor.XR.ARSubsystems.PreprocessBuildEventArgs)"/>
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns the negation of <see cref="Equals(UnityEditor.XR.ARSubsystems.PreprocessBuildEventArgs)"/>.</returns>
        public static bool operator !=(PreprocessBuildEventArgs lhs, PreprocessBuildEventArgs rhs) => !lhs.Equals(rhs);
    }
}
