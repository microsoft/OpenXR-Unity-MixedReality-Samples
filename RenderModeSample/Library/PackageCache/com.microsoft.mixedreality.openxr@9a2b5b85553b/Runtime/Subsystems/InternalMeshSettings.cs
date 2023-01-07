// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.MixedReality.OpenXR
{
    internal static class InternalMeshSettings
    {
        private static readonly NativeLibToken NativeLibToken = NativeLibToken.HoloLens;

        /// <summary>
        /// Change the settings for future meshes given by the OpenXR XRMeshSubsystem.
        /// </summary>
        public static bool TrySetMeshComputeSettings(MeshComputeSettings settings)
        {
            return NativeLib.SetMeshComputeSettings(NativeLibToken, settings);
        }
    }
}
