// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Interop functions for Windows Perception APIs
    /// </summary>
    public static class PerceptionInterop
    {
        /// <summary>
        /// Get a COM wrapper object of a Windows.Perception.Spatial.SpatialCoordinateSystem object
        /// located at the given pose in the current Unity scene.
        /// If failed, the function returns nullptr.
        /// The application should acquire a new one when session origin is changed or tracking mode is changed
        /// by listening to XRInputSubsystem.trackingOriginUpdated and monitoring ARSession.currentTrackingMode.
        /// </summary>
        /// <param name="poseInScene">The pose of returned coordinate system in the current Unity scene.
        /// If input Pose.identity, the returned coordinate system will be at the origin of the current Unity scene.</param>
        /// <returns>Returns a COM wrapper C# object of type Windows.Perception.Spatial.SpatialCoordinateSystem. 
        /// Returns null if such coordinate system cannot be found at the moment.</returns>
        public static object GetSceneCoordinateSystem(Pose poseInScene)
        {
            if (m_feature != null && m_feature.enabled)
            {
                IntPtr unknown = NativeLib.TryAcquireSceneCoordinateSystem(m_feature.NativeLibToken, poseInScene);
                if (unknown != IntPtr.Zero)
                {
                    object result = Marshal.GetObjectForIUnknown(unknown);
                    Marshal.Release(unknown);   // Balance the ref count because "feature.TryAcquire" increment it on return.
                    return result;
                }
            }
            return null;
        }

        private readonly static MixedRealityFeaturePlugin m_feature = OpenXRSettings.Instance.GetFeature<MixedRealityFeaturePlugin>();
    }
}
