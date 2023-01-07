using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A structure for camera-related information pertaining to a particular frame.
    /// This is used to communicate information in the <see cref="ARCameraManager.frameReceived" /> event.
    /// </summary>
    public struct ARCameraFrameEventArgs : IEquatable<ARCameraFrameEventArgs>
    {
        /// <summary>
        /// The <see cref="ARLightEstimationData" /> associated with this frame.
        /// </summary>
        public ARLightEstimationData lightEstimation { get; set; }

        /// <summary>
        /// The time, in nanoseconds, associated with this frame.
        /// Use <c>timestampNs.HasValue</c> to determine if this data is available.
        /// </summary>
        public long? timestampNs { get; set; }

        /// <summary>
        /// Gets or sets the projection matrix for the AR Camera. Use
        /// <c>projectionMatrix.HasValue</c> to determine if this data is available.
        /// </summary>
        public Matrix4x4? projectionMatrix { get; set; }

        /// <summary>
        /// Gets or sets the display matrix for use in the shader used
        /// by the <see cref="ARCameraBackground"/>.
        /// Use <c>displayMatrix.HasValue</c> to determine if this data is available.
        /// </summary>
        public Matrix4x4? displayMatrix { get; set; }

        /// <summary>
        /// The textures associated with this camera frame. These are generally
        /// external textures, which exist only on the GPU. To use them on the
        /// CPU, e.g., for computer vision processing, you will need to read
        /// them back from the GPU.
        /// </summary>
        public List<Texture2D> textures { get; set; }

        /// <summary>
        /// Ids of the property name associated with each texture. This is a
        /// parallel <c>List</c> to the <see cref="textures"/> list.
        /// </summary>
        public List<int> propertyNameIds { get; set; }

        /// <summary>
        /// The exposure duration in seconds with sub-millisecond precision. Used in calculating motion blur.
        /// </summary>
        /// <remarks>
        /// <see cref="exposureDuration"/> can be null if platform does not support exposure duration.
        /// </remarks>
        public double? exposureDuration { get; set; }

        /// <summary>
        /// The offset of camera exposure. Used to scale scene lighting in post-processed lighting stage.
        /// </summary>
        /// <remarks>
        /// <see cref="exposureOffset"/> can be null if platform does not support exposure offset.
        /// </remarks>
        public float? exposureOffset { get; set; }

        /// <summary>
        /// The list of keywords to be enabled for the material.
        /// </summary>
        public List<string> enabledMaterialKeywords { get; internal set; }

        /// <summary>
        /// The list of keywords to be disabled for the material.
        /// </summary>
        public List<string> disabledMaterialKeywords { get; internal set; }

       /// <summary>
        /// The camera grain texture effect.
        /// </summary>
        public Texture cameraGrainTexture { get; internal set; }

        /// <summary>
        /// The camera grain noise intensity.
        /// </summary>
        public float noiseIntensity { get; internal set; }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = lightEstimation.GetHashCode();
                hash = hash * 486187739 + timestampNs.GetHashCode();
                hash = hash * 486187739 + projectionMatrix.GetHashCode();
                hash = hash * 486187739 + displayMatrix.GetHashCode();
                hash = hash * 486187739 + HashCodeUtil.ReferenceHash(textures);
                hash = hash * 486187739 + HashCodeUtil.ReferenceHash(propertyNameIds);
                hash = hash * 486187739 + exposureDuration.GetHashCode();
                hash = hash * 486187739 + exposureOffset.GetHashCode();
                hash = hash * 486187739 + cameraGrainTexture.GetHashCode();
                hash = hash * 486187739 + noiseIntensity.GetHashCode();
                hash = hash * 486187739 + (enabledMaterialKeywords == null ? 0 : enabledMaterialKeywords.GetHashCode());
                hash = hash * 486187739 + (disabledMaterialKeywords == null ? 0 : disabledMaterialKeywords.GetHashCode());
                return hash;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARCameraFrameEventArgs"/> and
        /// <see cref="Equals(ARCameraFrameEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj)
            => (obj is ARCameraFrameEventArgs) && Equals((ARCameraFrameEventArgs)obj);

        /// <summary>
        /// Generates a string representation of this struct suitable for debug
        /// logging.
        /// </summary>
        /// <returns>A string representation of this struct suitable for debug
        /// logging.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("lightEstimation: " + lightEstimation.ToString());
            stringBuilder.Append("\ntimestamp: "  + timestampNs);
            if (timestampNs.HasValue)
                stringBuilder.Append("ns");
            stringBuilder.Append("\nprojectionMatrix: " + projectionMatrix);
            stringBuilder.Append("\ndisplayMatrix: " + displayMatrix);
            stringBuilder.Append("\ntexture count: " + (textures == null ? 0 : textures.Count));
            stringBuilder.Append("\npropertyNameId count: " + (propertyNameIds == null ? 0 : propertyNameIds.Count));

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARCameraFrameEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARCameraFrameEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARCameraFrameEventArgs other)
        {
            return
                lightEstimation.Equals(other.lightEstimation)
                && timestampNs == other.timestampNs
                && projectionMatrix == other.projectionMatrix
                && displayMatrix == other.displayMatrix
                && ((textures == null) ? (other.textures == null) : textures.Equals(other.textures))
                && ((propertyNameIds == null) ? (other.propertyNameIds == null)
                    : propertyNameIds.Equals(other.propertyNameIds))
                && (exposureDuration == other.exposureDuration)
                && (exposureOffset == other.exposureOffset)
                && (cameraGrainTexture == other.cameraGrainTexture)
                && (noiseIntensity == other.noiseIntensity)
                && ((enabledMaterialKeywords == null) ? (other.enabledMaterialKeywords == null)
                    : enabledMaterialKeywords.Equals(other.enabledMaterialKeywords))
                && ((disabledMaterialKeywords == null) ? (other.disabledMaterialKeywords == null)
                    : disabledMaterialKeywords.Equals(other.disabledMaterialKeywords));
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARCameraFrameEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARCameraFrameEventArgs lhs, ARCameraFrameEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARCameraFrameEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARCameraFrameEventArgs lhs, ARCameraFrameEventArgs rhs) => !lhs.Equals(rhs);
    }
}
