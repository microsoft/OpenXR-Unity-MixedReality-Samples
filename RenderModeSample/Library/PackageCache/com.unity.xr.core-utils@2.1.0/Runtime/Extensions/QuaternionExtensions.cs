using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for Quaternion structs
    /// </summary>
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Returns a rotation which only contains the yaw component of the given rotation.
        /// The resulting rotation is not normalized.
        /// </summary>
        /// <param name="rotation">The rotation we would like to constrain</param>
        /// <returns>A yaw-only rotation which matches the input's yaw</returns>
        public static Quaternion ConstrainYaw(this Quaternion rotation)
        {
            rotation.x = 0;
            rotation.z = 0;
            return rotation;
        }

        /// <summary>
        /// Returns a rotation which only contains the yaw component of the given rotation
        /// </summary>
        /// <param name="rotation">The rotation we would like to constrain</param>
        /// <returns>A yaw-only rotation which matches the input's yaw</returns>
        public static Quaternion ConstrainYawNormalized(this Quaternion rotation)
        {
            rotation.x = 0;
            rotation.z = 0;
            rotation.Normalize();
            return rotation;
        }

        /// <summary>
        /// Returns a rotation which only contains the yaw and pitch component of the given rotation
        /// </summary>
        /// <param name="rotation">The rotation we would like to constrain</param>
        /// <returns>A yaw/pitch-only rotation which matches the input's yaw and pitch</returns>
        public static Quaternion ConstrainYawPitchNormalized(this Quaternion rotation)
        {
            var euler = rotation.eulerAngles;
            euler.z = 0;
            return Quaternion.Euler(euler);
        }
    }
}
