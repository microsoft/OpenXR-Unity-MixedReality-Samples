using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for the Vector2 type
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Returns a vector where each component is inverted (1/x)
        /// </summary>
        /// <param name="vector">The vector which will be inverted</param>
        /// <returns>The inverted vector</returns>
        public static Vector2 Inverse(this Vector2 vector)
        {
            return new Vector2(1.0f / vector.x, 1.0f / vector.y);
        }

        /// <summary>
        /// Returns the minimum of all vector components
        /// </summary>
        /// <param name="vector">The vector whose minimum component will be returned</param>
        /// <returns>The minimum value</returns>
        public static float MinComponent(this Vector2 vector)
        {
            return Mathf.Min(vector.x, vector.y);
        }

        /// <summary>
        /// Returns the maximum of all vector components
        /// </summary>
        /// <param name="vector">The vector whose maximum component will be returned</param>
        /// <returns>The maximum value</returns>
        public static float MaxComponent(this Vector2 vector)
        {
            return Mathf.Max(vector.x, vector.y);
        }

        /// <summary>
        /// Returns a vector where each component is the absolute value of the original (abs(x))
        /// </summary>
        /// <returns>The absolute value vector</returns>
        /// <param name="vector">The vector whose absolute value will be returned</param>
        /// <returns>The absolute value of this vector</returns>
        public static Vector2 Abs(this Vector2 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            return vector;
        }
    }
}
