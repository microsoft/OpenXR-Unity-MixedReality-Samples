using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utilities for manipulating Textures
    /// </summary>
    public static class TextureUtils
    {
        /// <summary>
        /// Copy a given RenderTexture to a Texture2D
        /// This method assumes that both textures exist and are the same size
        /// </summary>
        /// <param name="renderTexture">The source <see cref="RenderTexture" /></param>
        /// <param name="texture">The destination <see cref="Texture2D" /></param>
        public static void RenderTextureToTexture2D(RenderTexture renderTexture, Texture2D texture)
        {
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply();
        }
    }
}
