using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for the Vector3 type
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Returns a vector where each component is inverted (1/x)
        /// </summary>
        /// <param name="vector">The vector which will be inverted</param>
        /// <returns>The inverted vector</returns>
        public static Vector3 Inverse(this Vector3 vector)
        {
            return new Vector3(1.0f / vector.x, 1.0f / vector.y, 1.0f / vector.z);
        }

        /// <summary>
        /// Returns the minimum of all vector components
        /// </summary>
        /// <param name="vector">The vector whose minimum component will be returned</param>
        /// <returns>The minimum value</returns>
        public static float MinComponent(this Vector3 vector)
        {
            return Mathf.Min(Mathf.Min(vector.x, vector.y), vector.z);
        }

        /// <summary>
        /// Returns the maximum of all vector components
        /// </summary>
        /// <param name="vector">The vector whose maximum component will be returned</param>
        /// <returns>The maximum value</returns>
        public static float MaxComponent(this Vector3 vector)
        {
            return Mathf.Max(Mathf.Max(vector.x, vector.y), vector.z);
        }

        /// <summary>
        /// Returns a vector where each component is the absolute value of the original (abs(x))
        /// </summary>
        /// <returns>The absolute value vector</returns>
        /// <param name="vector">The vector whose absolute value will be returned</param>
        /// <returns>The absolute value of this vector</returns>
        public static Vector3 Abs(this Vector3 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            vector.z = Mathf.Abs(vector.z);
            return vector;
        }
    }
}
