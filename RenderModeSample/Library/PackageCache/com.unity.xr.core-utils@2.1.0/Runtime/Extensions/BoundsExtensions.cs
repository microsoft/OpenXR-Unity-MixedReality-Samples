using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for the Bounds type
    /// </summary>
    public static class BoundsExtensions
    {
        /// <summary>
        /// Returns a whether the given bounds are contained completely within this one
        /// </summary>
        /// <param name="outerBounds">The outer bounds which may contain the inner bounds</param>
        /// <param name="innerBounds">The inner bounds that may or may not fit within outerBounds</param>
        /// <returns>True if outerBounds contains innerBounds completely</returns>
        public static bool ContainsCompletely(this Bounds outerBounds, Bounds innerBounds)
        {
            var outerBoundsMax = outerBounds.max;
            var outerBoundsMin = outerBounds.min;
            var innerBoundsMax = innerBounds.max;
            var innerBoundsMin = innerBounds.min;
            return outerBoundsMax.x >= innerBoundsMax.x && outerBoundsMax.y >= innerBoundsMax.y && outerBoundsMax.z >= innerBoundsMax.z
                && outerBoundsMin.x <= innerBoundsMin.x && outerBoundsMin.y <= innerBoundsMin.y && outerBoundsMin.z <= innerBoundsMin.z;
        }
    }
}
