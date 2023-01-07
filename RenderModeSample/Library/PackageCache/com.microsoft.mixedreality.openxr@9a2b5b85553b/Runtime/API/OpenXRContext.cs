// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// Retrieve the current OpenXR instance, session handles and states.
    /// </summary>
    public class OpenXRContext
    {
        /// <summary>
        /// Get the current OpenXR context.
        /// </summary>
        public static OpenXRContext Current => m_current;

        /// <summary>
        /// The XrInstance handle, or 0 when instance is not initialized.
        /// </summary>
        public ulong Instance => m_feature != null && m_feature.enabled ? m_feature.Instance : 0;

        /// <summary>
        /// The XrSystemId, or 0 when system is not available.
        /// </summary>
        public ulong SystemId => m_feature != null && m_feature.enabled ? m_feature.SystemId : 0;

        /// <summary>
        /// The XrSession handle, or 0 when session is not created.
        /// </summary>
        public ulong Session => m_feature != null && m_feature.enabled ? m_feature.Session : 0;

        /// <summary>
        /// Whether the current XrSession is running, i.e. when the frame loop is in progress.
        /// </summary>
        public bool IsSessionRunning => m_feature != null && m_feature.enabled && m_feature.IsSessionRunning;

        /// <summary>
        /// An XrSpace handle to the reference space of the current Unity scene origin, or 0 when not available.
        /// It's typically a LOCAL, a STAGE or an UNBOUNDED reference space handle when available.
        /// </summary>
        public ulong SceneOriginSpace => m_feature != null && m_feature.enabled ? m_feature.SceneOriginSpace : 0;

        /// <summary>
        /// Get the function pointer to PFN_xrGetInstanceProcAddr that includes Unity OpenXR plugin and features overrides.
        /// Returns 0 when XR is not loaded in Unity or xrInstance handle above is 0.
        /// </summary>
        public IntPtr PFN_xrGetInstanceProcAddr =>
            (m_feature != null && m_feature.enabled) ? m_feature.PFN_xrGetInstanceProcAddr : IntPtr.Zero;

        private OpenXRContext() { } // use static Current property instead.
        private static OpenXRContext m_current = new OpenXRContext();
        private readonly MixedRealityFeaturePlugin m_feature = OpenXRSettings.Instance.GetFeature<MixedRealityFeaturePlugin>();
    }
}
