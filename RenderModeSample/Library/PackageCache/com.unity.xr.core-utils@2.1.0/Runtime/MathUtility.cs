using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Math utilities
    /// </summary>
    public static class MathUtility
    {
        // constants used in approximate equality checks
        internal static readonly float EpsilonScaled = Mathf.Epsilon * 8;

        /// <summary>
        /// A faster drop-in replacement for Mathf.Approximately(a, b).
        /// Compares two floating point values and returns true if they are similar.
        /// As an optimization, this method does not take into account the magnitude of the values it is comparing.
        /// This method may not provide the same results as Mathf.Approximately for extremely large values
        /// </summary>
        /// <param name="a">The first float being compared</param>
        /// <param name="b">The second float being compared</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b)
        {
            var d = b - a;
            var absDiff = d >= 0f ? d : -d;
            return absDiff < EpsilonScaled;
        }

        /// <summary>
        /// A slightly faster way to do Approximately(a, 0f).
        /// </summary>
        /// <param name="a">The floating point value to compare with 0</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyZero(float a)
        {
            return (a >= 0f ? a : -a) < EpsilonScaled;
        }
        
        /// <summary>
        /// Constrain a value between a minimum and a maximum
        /// </summary>
        /// <param name="input">The input number</param>
        /// <param name="min">The minimum output</param>
        /// <param name="max">The maximum output</param>
        /// <returns>The <paramref name="input"/> number, clamped between <paramref name="min"/> and <paramref name="max"/> </returns>
        public static double Clamp(double input, double min, double max)
        {
            if (input > max)
                return max;

            return input < min ? min : input;
        }

        /// <summary>
        /// Finds the shortest angle distance between two angle values
        /// </summary>
        /// <param name="start">The start value</param>
        /// <param name="end">The end value</param>
        /// <param name="halfMax">Half of the max angle</param>
        /// <param name="max">The max angle value</param>
        /// <returns>The angle distance between start and end</returns>
        public static double ShortestAngleDistance(double start, double end, double halfMax, double max)
        {
            var angleDelta = end - start;
            var angleSign = Math.Sign(angleDelta);

            angleDelta = Math.Abs(angleDelta) % max;
            if (angleDelta > halfMax)
                angleDelta = -(max - angleDelta);

            return angleDelta * angleSign;
        }

        /// <summary>
        /// Finds the shortest angle distance between two angle values
        /// </summary>
        /// <param name="start">The start value</param>
        /// <param name="end">The end value</param>
        /// <param name="halfMax">Half of the max angle</param>
        /// <param name="max">The max angle value</param>
        /// <returns>The angle distance between start and end</returns>
        public static float ShortestAngleDistance(float start, float end, float halfMax, float max)
        {
            var angleDelta = end - start;
            var angleSign = Mathf.Sign(angleDelta);

            angleDelta = Math.Abs(angleDelta) % max;
            if (angleDelta > halfMax)
                angleDelta = -(max - angleDelta);

            return angleDelta * angleSign;
        }

        /// <summary>
        /// Is the float value infinity or NaN?
        /// </summary>
        /// <param name="value">The float value</param>
        /// <returns>True if the value is infinity or NaN (not a number), otherwise false</returns>
        public static bool IsUndefined(this float value)
        {
            return float.IsInfinity(value) || float.IsNaN(value);
        }

        /// <summary>
        /// Checks if a vector is aligned with one of the axis vectors
        /// </summary>
        /// <param name="v"> The vector </param>
        /// <returns>True if the vector is aligned with any axis, otherwise false</returns>
        public static bool IsAxisAligned(this Vector3 v)
        {
            return ApproximatelyZero(v.x * v.y) && ApproximatelyZero(v.y * v.z) && ApproximatelyZero(v.z * v.x);
        }

        /// <summary>
        /// Check if a value is a positive power of two
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is a positive power of two, false otherwise</returns>
        public static bool IsPositivePowerOfTwo(int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }

        /// <summary>
        /// Return the index of the first flag bit set to true
        /// </summary>
        /// <param name="value">The flags value to check</param>
        /// <returns>The index of the first active flag</returns>
        public static int FirstActiveFlagIndex(int value)
        {
            if (value == 0)
                return 0;

            const int bits = sizeof(int) * 8;
            for (var i = 0; i < bits; i++)
                if ((value & 1 << i) != 0)
                    return i;

            return 0;
        }
    }
}
