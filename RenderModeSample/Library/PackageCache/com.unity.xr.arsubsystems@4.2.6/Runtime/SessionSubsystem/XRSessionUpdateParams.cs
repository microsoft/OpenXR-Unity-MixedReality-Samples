using System;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Update parameters for <see cref="XRSessionSubsystem.Update(XRSessionUpdateParams)"/>.
    /// </summary>
    public struct XRSessionUpdateParams : IEquatable<XRSessionUpdateParams>
    {
        /// <summary>
        /// The current screen orientation.
        /// </summary>
        public ScreenOrientation screenOrientation { get; set; }

        /// <summary>
        /// The current screen dimensions.
        /// </summary>
        public Vector2Int screenDimensions { get; set; }

        /// <summary>
        /// Generates a hash code suitable for use in a `Dictionary` or `HashSet`.
        /// </summary>
        /// <returns>A hash code of this <see cref="XRSessionUpdateParams"/>.</returns>
        public override int GetHashCode() => HashCodeUtil.Combine(((int)screenOrientation).GetHashCode(), screenDimensions.GetHashCode());

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="obj">The <c>object</c> to compare against.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="XRSessionUpdateParams"/> and <see cref="Equals(XRSessionUpdateParams)"/> is <c>true</c>.</returns>
        public override bool Equals(object obj) => (obj is XRSessionUpdateParams) && Equals((XRSessionUpdateParams)obj);

        /// <summary>
        /// Generates a string suitable for debugging.
        /// </summary>
        /// <returns>A string representation of the update parameters.</returns>
        public override string ToString() => $"Screen Orientation: {screenOrientation}, Screen Dimensions: {screenDimensions}";

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRSessionUpdateParams"/> to compare against.</param>
        /// <returns><c>true</c> if the other <see cref="XRSessionUpdateParams"/> is equal to this one.</returns>
        public bool Equals(XRSessionUpdateParams other) =>
            (screenOrientation == other.screenOrientation) &&
            screenDimensions.Equals(other.screenDimensions);


        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>The same as <see cref="Equals(XRSessionUpdateParams)"/>.</returns>
        public static bool operator ==(XRSessionUpdateParams lhs, XRSessionUpdateParams rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Compares for inequality.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>The negation of <see cref="Equals(XRSessionUpdateParams)"/>.</returns>
        public static bool operator !=(XRSessionUpdateParams lhs, XRSessionUpdateParams rhs) => !lhs.Equals(rhs);
    }
}
