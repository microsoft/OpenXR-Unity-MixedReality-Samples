using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR
{
    public partial class OpenXRSettings
    {
        /// <summary>
        /// Stereo rendering mode.
        /// </summary>
        public enum RenderMode
        {
            /// <summary>
            /// Submit separate draw calls for each eye.
            /// </summary>
            MultiPass,

            /// <summary>
            /// Submit one draw call for both eyes.
            /// </summary>
            SinglePassInstanced,
        };

        /// <summary>
        /// Stereo rendering mode.
        /// </summary>
        [SerializeField] private RenderMode m_renderMode = RenderMode.SinglePassInstanced;

        /// <summary>
        /// Runtime Stereo rendering mode.
        /// </summary>
        public RenderMode renderMode
        {
            get
            {
                if (OpenXRLoaderBase.Instance != null)
                    return Internal_GetRenderMode();
                else
                    return m_renderMode;
            }
            set
            {
                if (OpenXRLoaderBase.Instance != null)
                    Internal_SetRenderMode(value);
                else
                    m_renderMode = value;
            }
        }

        /// <summary>
        /// Runtime Depth submission mode.
        /// </summary>
        public enum DepthSubmissionMode
        {
            /// <summary>
            /// No depth is submitted to the OpenXR compositor.
            /// </summary>
            None,

            /// <summary>
            /// 16-bit depth is submitted to the OpenXR compositor.
            /// </summary>
            Depth16Bit,

            /// <summary>
            /// 24-bit depth is submitted to the OpenXR compositor.
            /// </summary>
            Depth24Bit,
        }

        /// <summary>
        /// Enables XR_KHR_composition_layer_depth if possible and resolves or submits depth to OpenXR runtime.
        /// </summary>
        [SerializeField] private DepthSubmissionMode m_depthSubmissionMode = DepthSubmissionMode.None;

        /// <summary>
        /// Enables XR_KHR_composition_layer_depth if possible and resolves or submits depth to OpenXR runtime.
        /// </summary>
        public DepthSubmissionMode depthSubmissionMode
        {
            get
            {
                if (OpenXRLoaderBase.Instance != null)
                    return Internal_GetDepthSubmissionMode();
                else
                    return m_depthSubmissionMode;
            }
            set
            {
                if (OpenXRLoaderBase.Instance != null)
                    Internal_SetDepthSubmissionMode(value);
                else
                    m_depthSubmissionMode = value;
            }
        }

        private void ApplyRenderSettings()
        {
            Internal_SetRenderMode(m_renderMode);
            Internal_SetDepthSubmissionMode(m_depthSubmissionMode);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                ApplyRenderSettings();
        }
#endif

        private const string LibraryName = "UnityOpenXR";

        [DllImport(LibraryName, EntryPoint = "NativeConfig_SetRenderMode")]
        private static extern void Internal_SetRenderMode(RenderMode renderMode);

        [DllImport(LibraryName, EntryPoint = "NativeConfig_GetRenderMode")]
        private static extern RenderMode Internal_GetRenderMode();

        [DllImport(LibraryName, EntryPoint = "NativeConfig_SetDepthSubmissionMode")]
        private static extern void Internal_SetDepthSubmissionMode(DepthSubmissionMode depthSubmissionMode);

        [DllImport(LibraryName, EntryPoint = "NativeConfig_GetDepthSubmissionMode")]
        private static extern DepthSubmissionMode Internal_GetDepthSubmissionMode();
    }
}
