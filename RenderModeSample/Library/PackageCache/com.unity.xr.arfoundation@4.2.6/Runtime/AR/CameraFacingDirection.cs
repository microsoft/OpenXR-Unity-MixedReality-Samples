using System.ComponentModel;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Represents the camera used when supplying the video feed.
    /// </summary>
    public enum CameraFacingDirection
    {
        /// <summary>
        /// No video feed should be provided.
        /// </summary>
        [Description("No textures will be available.")]
        None,

        /// <summary>
        /// Provide the video feed from the world-facing camera. On a phone, this
        /// is usually the rear camera.
        /// </summary>
        [Description("Textures from the world-facing camera will be available.")]
        World,

        /// <summary>
        /// Provide the video feed from the user-facing camera. On a phone,
        /// this is usually the front ("selfie") camera.
        /// </summary>
        [Description("Textures from the user-facing camera will be available.")]
        User,
    }

    /// <summary>
    /// Extensions related to the <see cref="CameraFacingDirection"/> enum.
    /// </summary>
    public static class CameraModeExtensions
    {
        /// <summary>
        /// Converts <paramref name="self"/> to a <see cref="CameraFacingDirection"/>.
        /// </summary>
        /// <param name="self">The <c>Feature</c> being extended.</param>
        /// <returns>The <see cref="CameraFacingDirection"/> represented by <paramref name="self"/>.</returns>
        public static CameraFacingDirection ToCameraFacingDirection(this Feature self)
        {
            var cameraModes = self.Cameras();
            switch (cameraModes)
            {
                case Feature.UserFacingCamera: return CameraFacingDirection.User;
                case Feature.WorldFacingCamera: return CameraFacingDirection.World;
                default: return CameraFacingDirection.None;
            }
        }

        /// <summary>
        /// Converts <paramref name="self"/> to a <c>Feature</c>.
        /// </summary>
        /// <param name="self">The <see cref="CameraFacingDirection"/> being extended.</param>
        /// <returns>The <c>Feature</c> representation of the camera facing direction.</returns>
        public static Feature ToFeature(this CameraFacingDirection self)
        {
            switch (self)
            {
                case CameraFacingDirection.World: return Feature.WorldFacingCamera;
                case CameraFacingDirection.User: return Feature.UserFacingCamera;
                default: return Feature.None;
            }
        }
    }
}
