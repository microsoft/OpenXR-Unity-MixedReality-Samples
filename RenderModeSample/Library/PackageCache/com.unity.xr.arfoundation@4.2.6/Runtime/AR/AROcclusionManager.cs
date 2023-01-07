using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Serialization;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Rendering;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// The manager for the occlusion subsystem.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ARUpdateOrder.k_OcclusionManager)]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(AROcclusionManager) + ".html")]
    public sealed class AROcclusionManager :
        SubsystemLifecycleManager<XROcclusionSubsystem, XROcclusionSubsystemDescriptor, XROcclusionSubsystem.Provider>
    {
        /// <summary>
        /// The list of occlusion texture infos.
        /// </summary>
        /// <value>
        /// The list of occlusion texture infos.
        /// </value>
        readonly List<ARTextureInfo> m_TextureInfos = new List<ARTextureInfo>();

        /// <summary>
        /// The list of occlusion textures.
        /// </summary>
        /// <value>
        /// The list of occlusion textures.
        /// </value>
        readonly List<Texture2D> m_Textures = new List<Texture2D>();

        /// <summary>
        /// The list of occlusion texture property IDs.
        /// </summary>
        /// <value>
        /// The list of occlusion texture property IDs.
        /// </value>
        readonly List<int> m_TexturePropertyIds = new List<int>();

        /// <summary>
        /// The human stencil texture info.
        /// </summary>
        /// <value>
        /// The human stencil texture info.
        /// </value>
        ARTextureInfo m_HumanStencilTextureInfo;

        /// <summary>
        /// The human depth texture info.
        /// </summary>
        /// <value>
        /// The human depth texture info.
        /// </value>
        ARTextureInfo m_HumanDepthTextureInfo;

        /// <summary>
        /// The environment depth texture info.
        /// </summary>
        /// <value>
        /// The environment depth texture info.
        /// </value>
        ARTextureInfo m_EnvironmentDepthTextureInfo;

        /// <summary>
        /// The environment depth confidence texture info.
        /// </summary>
        /// <value>
        /// The environment depth confidence texture info.
        /// </value>
        ARTextureInfo m_EnvironmentDepthConfidenceTextureInfo;

        /// <summary>
        /// An event which fires each time an occlusion camera frame is received.
        /// </summary>
        public event Action<AROcclusionFrameEventArgs> frameReceived;

        /// <summary>
        /// The mode for generating the human segmentation stencil texture.
        /// This method is obsolete.
        /// Use <see cref="requestedHumanStencilMode"/>
        /// or  <see cref="currentHumanStencilMode"/> instead.
        /// </summary>
        [Obsolete("Use requestedSegmentationStencilMode or currentSegmentationStencilMode instead. (2020-01-14)")]
        public HumanSegmentationStencilMode humanSegmentationStencilMode
        {
            get => m_HumanSegmentationStencilMode;
            set => requestedHumanStencilMode = value;
        }

        /// <summary>
        /// The requested mode for generating the human segmentation stencil texture.
        /// </summary>
        public HumanSegmentationStencilMode requestedHumanStencilMode
        {
            get => subsystem?.requestedHumanStencilMode ?? m_HumanSegmentationStencilMode;
            set
            {
                m_HumanSegmentationStencilMode = value;
                if (enabled && descriptor?.humanSegmentationStencilImageSupported == Supported.Supported)
                {
                    subsystem.requestedHumanStencilMode = value;
                }
            }
        }

        /// <summary>
        /// Get the current mode in use for generating the human segmentation stencil mode.
        /// </summary>
        public HumanSegmentationStencilMode currentHumanStencilMode => subsystem?.currentHumanStencilMode ?? HumanSegmentationStencilMode.Disabled;

        [SerializeField]
        [Tooltip("The mode for generating human segmentation stencil texture.\n\n"
                 + "Disabled -- No human stencil texture produced.\n"
                 + "Fastest -- Minimal rendering quality. Minimal frame computation.\n"
                 + "Medium -- Medium rendering quality. Medium frame computation.\n"
                 + "Best -- Best rendering quality. Increased frame computation.")]
        HumanSegmentationStencilMode m_HumanSegmentationStencilMode = HumanSegmentationStencilMode.Disabled;

        /// <summary>
        /// The mode for generating the human segmentation depth texture.
        /// This method is obsolete.
        /// Use <see cref="requestedHumanDepthMode"/>
        /// or  <see cref="currentHumanDepthMode"/> instead.
        /// </summary>
        [Obsolete("Use requestedSegmentationDepthMode or currentSegmentationDepthMode instead. (2020-01-15)")]
        public HumanSegmentationDepthMode humanSegmentationDepthMode
        {
            get => m_HumanSegmentationDepthMode;
            set => requestedHumanDepthMode = value;
        }

        /// <summary>
        /// Get or set the requested human segmentation depth mode.
        /// </summary>
        public HumanSegmentationDepthMode requestedHumanDepthMode
        {
            get => subsystem?.requestedHumanDepthMode ?? m_HumanSegmentationDepthMode;
            set
            {
                m_HumanSegmentationDepthMode = value;
                if (enabled && descriptor?.humanSegmentationDepthImageSupported == Supported.Supported)
                {
                    subsystem.requestedHumanDepthMode = value;
                }
            }
        }

        /// <summary>
        /// Get the current human segmentation depth mode in use by the subsystem.
        /// </summary>
        public HumanSegmentationDepthMode currentHumanDepthMode => subsystem?.currentHumanDepthMode ?? HumanSegmentationDepthMode.Disabled;

        [SerializeField]
        [Tooltip("The mode for generating human segmentation depth texture.\n\n"
                 + "Disabled -- No human depth texture produced.\n"
                 + "Fastest -- Minimal rendering quality. Minimal frame computation.\n"
                 + "Best -- Best rendering quality. Increased frame computation.")]
        HumanSegmentationDepthMode m_HumanSegmentationDepthMode = HumanSegmentationDepthMode.Disabled;

        /// <summary>
        /// Get or set the requested environment depth mode.
        /// </summary>
        public EnvironmentDepthMode requestedEnvironmentDepthMode
        {
            get => subsystem?.requestedEnvironmentDepthMode ?? m_EnvironmentDepthMode;
            set
            {
                m_EnvironmentDepthMode = value;
                if (enabled && descriptor?.environmentDepthImageSupported == Supported.Supported)
                {
                    subsystem.requestedEnvironmentDepthMode = value;
                }
            }
        }

        /// <summary>
        /// Get the current environment depth mode in use by the subsystem.
        /// </summary>
        public EnvironmentDepthMode currentEnvironmentDepthMode => subsystem?.currentEnvironmentDepthMode ?? EnvironmentDepthMode.Disabled;

        [SerializeField]
        [Tooltip("The mode for generating the environment depth texture.\n\n"
                 + "Disabled -- No environment depth texture produced.\n"
                 + "Fastest -- Minimal rendering quality. Minimal frame computation.\n"
                 + "Medium -- Medium rendering quality. Medium frame computation.\n"
                 + "Best -- Best rendering quality. Increased frame computation.")]
        EnvironmentDepthMode m_EnvironmentDepthMode = EnvironmentDepthMode.Fastest;

        [SerializeField]
        bool m_EnvironmentDepthTemporalSmoothing = true;

        /// <summary>
        /// Whether temporal smoothing should be applied to the environment depth image. Query for support with
        /// [environmentDepthTemporalSmoothingSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthTemporalSmoothingSupported).
        /// </summary>
        /// <value>When `true`, temporal smoothing is applied to the environment depth image. Otherwise, no temporal smoothing is applied.</value>
        public bool environmentDepthTemporalSmoothingRequested
        {
            get => subsystem?.environmentDepthTemporalSmoothingRequested ?? m_EnvironmentDepthTemporalSmoothing;
            set
            {
                m_EnvironmentDepthTemporalSmoothing = value;
                if (enabled && descriptor?.environmentDepthTemporalSmoothingSupported == Supported.Supported)
                {
                    subsystem.environmentDepthTemporalSmoothingRequested = value;
                }
            }
        }

        /// <summary>
        /// Whether temporal smoothing is applied to the environment depth image. Query for support with
        /// [environmentDepthTemporalSmoothingSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthTemporalSmoothingSupported).
        /// </summary>
        /// <value>Read Only.</value>
        public bool environmentDepthTemporalSmoothingEnabled => subsystem?.environmentDepthTemporalSmoothingEnabled ?? false;

        /// <summary>
        /// Get or set the requested occlusion preference mode.
        /// </summary>
        public OcclusionPreferenceMode requestedOcclusionPreferenceMode
        {
            get => subsystem?.requestedOcclusionPreferenceMode ?? m_OcclusionPreferenceMode;
            set
            {
                m_OcclusionPreferenceMode = value;
                if (enabled && subsystem != null)
                {
                    subsystem.requestedOcclusionPreferenceMode = value;
                }
            }
        }

        /// <summary>
        /// Get the current occlusion preference mode in use by the subsystem.
        /// </summary>
        public OcclusionPreferenceMode currentOcclusionPreferenceMode => subsystem?.currentOcclusionPreferenceMode ?? OcclusionPreferenceMode.PreferEnvironmentOcclusion;

        [SerializeField]
        [Tooltip("If both environment texture and human stencil & depth textures are available, this mode specifies which should be used for occlusion.")]
        OcclusionPreferenceMode m_OcclusionPreferenceMode = OcclusionPreferenceMode.PreferEnvironmentOcclusion;

        /// <summary>
        /// The human segmentation stencil texture.
        /// </summary>
        /// <value>
        /// The human segmentation stencil texture, if any. Otherwise, <c>null</c>.
        /// </value>
        public Texture2D humanStencilTexture
        {
            get
            {
                if (descriptor?.humanSegmentationStencilImageSupported == Supported.Supported &&
                    subsystem.TryGetHumanStencil(out var humanStencilDescriptor))
                {
                    m_HumanStencilTextureInfo = ARTextureInfo.GetUpdatedTextureInfo(m_HumanStencilTextureInfo,
                                                                                    humanStencilDescriptor);
                    DebugAssert.That(((m_HumanStencilTextureInfo.descriptor.dimension == TextureDimension.Tex2D)
                                  || (m_HumanStencilTextureInfo.descriptor.dimension == TextureDimension.None)))?.
                        WithMessage("Human Stencil Texture needs to be a Texture 2D, but instead is "
                                    + $"{m_HumanStencilTextureInfo.descriptor.dimension.ToString()}.");
                    return m_HumanStencilTextureInfo.texture as Texture2D;
                }
                return null;
            }
        }

        /// <summary>
        /// Attempt to get the latest human stencil CPU image. This provides directly access to the raw pixel data.
        /// </summary>
        /// <remarks>
        /// The `XRCpuImage` must be disposed to avoid resource leaks.
        /// </remarks>
        /// <param name="cpuImage">If this method returns `true`, an acquired `XRCpuImage`.</param>
        /// <returns>Returns `true` if the CPU image was acquired. Returns `false` otherwise.</returns>
        public bool TryAcquireHumanStencilCpuImage(out XRCpuImage cpuImage)
        {
            if (descriptor?.humanSegmentationStencilImageSupported == Supported.Supported)
            {
                return subsystem.TryAcquireHumanStencilCpuImage(out cpuImage);
            }

            cpuImage = default;
            return false;
        }

        /// <summary>
        /// The human segmentation depth texture.
        /// </summary>
        /// <value>
        /// The human segmentation depth texture, if any. Otherwise, <c>null</c>.
        /// </value>
        public Texture2D humanDepthTexture
        {
            get
            {
                if (descriptor?.humanSegmentationDepthImageSupported == Supported.Supported &&
                    subsystem.TryGetHumanDepth(out var humanDepthDescriptor))
                {
                    m_HumanDepthTextureInfo = ARTextureInfo.GetUpdatedTextureInfo(m_HumanDepthTextureInfo,
                                                                                  humanDepthDescriptor);
                    DebugAssert.That(m_HumanDepthTextureInfo.descriptor.dimension == TextureDimension.Tex2D
                                  || m_HumanDepthTextureInfo.descriptor.dimension == TextureDimension.None)?.
                        WithMessage("Human Depth Texture needs to be a Texture 2D, but instead is "
                                    + $"{m_HumanDepthTextureInfo.descriptor.dimension.ToString()}.");
                    return m_HumanDepthTextureInfo.texture as Texture2D;
                }
                return null;
            }
        }

        /// <summary>
        /// Attempt to get the latest environment depth confidence CPU image. This provides direct access to the
        /// raw pixel data.
        /// </summary>
        /// <remarks>
        /// The `XRCpuImage` must be disposed to avoid resource leaks.
        /// </remarks>
        /// <param name="cpuImage">If this method returns `true`, an acquired `XRCpuImage`.</param>
        /// <returns>Returns `true` if the CPU image was acquired. Returns `false` otherwise.</returns>
        public bool TryAcquireEnvironmentDepthConfidenceCpuImage(out XRCpuImage cpuImage)
        {
            if (descriptor?.environmentDepthConfidenceImageSupported == Supported.Supported)
            {
                return subsystem.TryAcquireEnvironmentDepthConfidenceCpuImage(out cpuImage);
            }

            cpuImage = default;
            return false;
        }

        /// <summary>
        /// The environment depth confidence texture.
        /// </summary>
        /// <value>
        /// The environment depth confidence texture, if any. Otherwise, <c>null</c>.
        /// </value>
        public Texture2D environmentDepthConfidenceTexture
        {
            get
            {
                if (descriptor?.environmentDepthConfidenceImageSupported == Supported.Supported
                    && subsystem.TryGetEnvironmentDepthConfidence(out var environmentDepthConfidenceDescriptor))
                {
                    m_EnvironmentDepthConfidenceTextureInfo = ARTextureInfo.GetUpdatedTextureInfo(m_EnvironmentDepthConfidenceTextureInfo,
                                                                                                  environmentDepthConfidenceDescriptor);
                    DebugAssert.That(m_EnvironmentDepthConfidenceTextureInfo.descriptor.dimension == TextureDimension.Tex2D
                                  || m_EnvironmentDepthConfidenceTextureInfo.descriptor.dimension == TextureDimension.None)?.
                        WithMessage("Environment depth confidence texture needs to be a Texture 2D, but instead is "
                                    + $"{m_EnvironmentDepthConfidenceTextureInfo.descriptor.dimension.ToString()}.");
                    return m_EnvironmentDepthConfidenceTextureInfo.texture as Texture2D;
                }
                return null;
            }
        }


        /// <summary>
        /// Attempt to get the latest human depth CPU image. This provides direct access to the raw pixel data.
        /// </summary>
        /// <remarks>
        /// The `XRCpuImage` must be disposed to avoid resource leaks.
        /// </remarks>
        /// <param name="cpuImage">If this method returns `true`, an acquired `XRCpuImage`.</param>
        /// <returns>Returns `true` if the CPU image was acquired. Returns `false` otherwise.</returns>
        public bool TryAcquireHumanDepthCpuImage(out XRCpuImage cpuImage)
        {
            if (descriptor?.humanSegmentationDepthImageSupported == Supported.Supported)
            {
                return subsystem.TryAcquireHumanDepthCpuImage(out cpuImage);
            }

            cpuImage = default;
            return false;
        }

        /// <summary>
        /// The environment depth texture.
        /// </summary>
        /// <value>
        /// The environment depth texture, if any. Otherwise, <c>null</c>.
        /// </value>
        public Texture2D environmentDepthTexture
        {
            get
            {
                if (descriptor?.environmentDepthImageSupported == Supported.Supported
                    && subsystem.TryGetEnvironmentDepth(out var environmentDepthDescriptor))
                {
                    m_EnvironmentDepthTextureInfo = ARTextureInfo.GetUpdatedTextureInfo(m_EnvironmentDepthTextureInfo,
                                                                                        environmentDepthDescriptor);
                    DebugAssert.That(m_EnvironmentDepthTextureInfo.descriptor.dimension == TextureDimension.Tex2D
                                  || m_EnvironmentDepthTextureInfo.descriptor.dimension == TextureDimension.None)?.
                        WithMessage("Environment depth texture needs to be a Texture 2D, but instead is "
                                    + $"{m_EnvironmentDepthTextureInfo.descriptor.dimension.ToString()}.");
                    return m_EnvironmentDepthTextureInfo.texture as Texture2D;
                }
                return null;
            }
        }

        /// <summary>
        /// Attempt to get the latest environment depth CPU image. This provides direct access to the raw pixel data.
        /// </summary>
        /// <remarks>
        /// The `XRCpuImage` must be disposed to avoid resource leaks.
        /// </remarks>
        /// <param name="cpuImage">If this method returns `true`, an acquired `XRCpuImage`.</param>
        /// <returns>Returns `true` if the CPU image was acquired. Returns `false` otherwise.</returns>
        public bool TryAcquireEnvironmentDepthCpuImage(out XRCpuImage cpuImage)
        {
            if (descriptor?.environmentDepthImageSupported == Supported.Supported)
            {
                return subsystem.TryAcquireEnvironmentDepthCpuImage(out cpuImage);
            }

            cpuImage = default;
            return false;
        }

        /// <summary>
        /// Attempt to get the latest raw environment depth CPU image. This provides direct access to the raw pixel data.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > The `XRCpuImage` must be disposed to avoid resource leaks.
        /// This differs from <see cref="TryAcquireEnvironmentDepthCpuImage"/> in that it always tries to acquire the
        /// raw environment depth image, whereas <see cref="TryAcquireEnvironmentDepthCpuImage"/> depends on the value
        /// of <see cref="environmentDepthTemporalSmoothingEnabled"/>.
        /// </remarks>
        /// <param name="cpuImage">If this method returns `true`, an acquired `XRCpuImage`.</param>
        /// <returns>Returns `true` if the CPU image was acquired. Returns `false` otherwise.</returns>
        public bool TryAcquireRawEnvironmentDepthCpuImage(out XRCpuImage cpuImage)
        {
            if (subsystem == null)
            {
                cpuImage = default;
                return false;
            }

            return subsystem.TryAcquireRawEnvironmentDepthCpuImage(out cpuImage);
        }

        /// <summary>
        /// Attempt to get the latest smoothed environment depth CPU image. This provides direct access to
        /// the raw pixel data.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > The `XRCpuImage` must be disposed to avoid resource leaks.
        /// This differs from <see cref="TryAcquireEnvironmentDepthCpuImage"/> in that it always tries to acquire the
        /// smoothed environment depth image, whereas <see cref="TryAcquireEnvironmentDepthCpuImage"/>
        /// depends on the value of <see cref="environmentDepthTemporalSmoothingEnabled"/>.
        /// </remarks>
        /// <param name="cpuImage">If this method returns `true`, an acquired `XRCpuImage`.</param>
        /// <returns>Returns `true` if the CPU image was acquired. Returns `false` otherwise.</returns>
        public bool TryAcquireSmoothedEnvironmentDepthCpuImage(out XRCpuImage cpuImage)
        {
            if (subsystem == null)
            {
                cpuImage = default;
                return false;
            }

            return subsystem.TryAcquireSmoothedEnvironmentDepthCpuImage(out cpuImage);
        }

        /// <summary>
        /// Callback before the subsystem is started (but after it is created).
        /// </summary>
        protected override void OnBeforeStart()
        {
            requestedHumanStencilMode = m_HumanSegmentationStencilMode;
            requestedHumanDepthMode = m_HumanSegmentationDepthMode;
            requestedEnvironmentDepthMode = m_EnvironmentDepthMode;
            requestedOcclusionPreferenceMode = m_OcclusionPreferenceMode;
            environmentDepthTemporalSmoothingRequested = m_EnvironmentDepthTemporalSmoothing;

            ResetTextureInfos();
        }

        /// <summary>
        /// Callback when the manager is being disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            ResetTextureInfos();
            InvokeFrameReceived();
        }

        /// <summary>
        /// Callback as the manager is being updated.
        /// </summary>
        public void Update()
        {
            if (subsystem != null)
            {
                UpdateTexturesInfos();
                InvokeFrameReceived();

                requestedEnvironmentDepthMode = m_EnvironmentDepthMode;
                requestedHumanDepthMode = m_HumanSegmentationDepthMode;
                requestedHumanStencilMode = m_HumanSegmentationStencilMode;
                requestedOcclusionPreferenceMode = m_OcclusionPreferenceMode;
                environmentDepthTemporalSmoothingRequested = m_EnvironmentDepthTemporalSmoothing;
            }
        }

        void ResetTextureInfos()
        {
            m_HumanStencilTextureInfo.Reset();
            m_HumanDepthTextureInfo.Reset();
            m_EnvironmentDepthTextureInfo.Reset();
            m_EnvironmentDepthConfidenceTextureInfo.Reset();
        }

        /// <summary>
        /// Pull the texture descriptors from the occlusion subsystem, and update the texture information maintained by
        /// this component.
        /// </summary>
        void UpdateTexturesInfos()
        {
            var textureDescriptors = subsystem.GetTextureDescriptors(Allocator.Temp);
            try
            {
                int numUpdated = Math.Min(m_TextureInfos.Count, textureDescriptors.Length);

                // Update the existing textures that are in common between the two arrays.
                for (int i = 0; i < numUpdated; ++i)
                {
                    m_TextureInfos[i] = ARTextureInfo.GetUpdatedTextureInfo(m_TextureInfos[i], textureDescriptors[i]);
                }

                // If there are fewer textures in the current frame than we had previously, destroy any remaining unneeded
                // textures.
                if (numUpdated < m_TextureInfos.Count)
                {
                    for (int i = numUpdated; i < m_TextureInfos.Count; ++i)
                    {
                        m_TextureInfos[i].Reset();
                    }
                    m_TextureInfos.RemoveRange(numUpdated, (m_TextureInfos.Count - numUpdated));
                }
                // Else, if there are more textures in the current frame than we have previously, add new textures for any
                // additional descriptors.
                else if (textureDescriptors.Length > m_TextureInfos.Count)
                {
                    for (int i = numUpdated; i < textureDescriptors.Length; ++i)
                    {
                        m_TextureInfos.Add(new ARTextureInfo(textureDescriptors[i]));
                    }
                }
            }
            finally
            {
                if (textureDescriptors.IsCreated)
                {
                    textureDescriptors.Dispose();
                }
            }
        }

        /// <summary>
        /// Invoke the occlusion frame received event with the updated textures and texture property IDs.
        /// </summary>
        void InvokeFrameReceived()
        {
            if (frameReceived != null)
            {
                int numTextureInfos = m_TextureInfos.Count;

                m_Textures.Clear();
                m_TexturePropertyIds.Clear();

                m_Textures.Capacity = numTextureInfos;
                m_TexturePropertyIds.Capacity = numTextureInfos;

                for (int i = 0; i < numTextureInfos; ++i)
                {
                    DebugAssert.That(m_TextureInfos[i].descriptor.dimension == TextureDimension.Tex2D)?.
                        WithMessage($"Texture needs to be a Texture 2D, but instead is {m_TextureInfos[i].descriptor.dimension.ToString()}.");

                    m_Textures.Add((Texture2D)m_TextureInfos[i].texture);
                    m_TexturePropertyIds.Add(m_TextureInfos[i].descriptor.propertyNameId);
                }

                subsystem.GetMaterialKeywords(out List<string> enabledMaterialKeywords, out List<string>disabledMaterialKeywords);

                AROcclusionFrameEventArgs args = new AROcclusionFrameEventArgs();
                args.textures = m_Textures;
                args.propertyNameIds = m_TexturePropertyIds;
                args.enabledMaterialKeywords = enabledMaterialKeywords;
                args.disabledMaterialKeywords = disabledMaterialKeywords;

                frameReceived(args);
            }
        }
    }
}
