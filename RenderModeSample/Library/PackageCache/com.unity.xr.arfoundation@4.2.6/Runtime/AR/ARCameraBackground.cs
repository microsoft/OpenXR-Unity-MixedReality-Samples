using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// <para>Add this component to a <c>Camera</c> to copy the color camera's texture onto the background.</para>
    /// <para>If you are using the Lightweight Render Pipeline (version 5.7.2 or later) or the Univerisal Render
    /// Pipeline (version 7.0.0 or later), you must also add the <see cref="ARBackgroundRendererFeature"/> to the list
    /// of render features for the scriptable renderer.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// To add the <see cref="ARBackgroundRendererFeature"/> to the list of render features for the scriptable
    /// renderer:
    /// <list type="bullet">
    /// <item><description>In Project Settings &gt; Graphics, select the render pipeline asset (either
    /// <c>LightweightRenderPipelineAsset</c> or <c>UniversalRenderPipelineAsset</c>) that is in the Scriptable Render
    /// Pipeline Settings field.</description></item>
    /// <item><description>In the render pipeline asset's Inspector window, make sure that the Render Type is set
    /// to Custom.</description></item>
    /// <item><description>In render pipeline asset's Inspector window, select the Render Type &gt; Data
    /// asset which would be of type <c>ForwardRendererData</c>.</description></item>
    /// <item><description>In forward renderer data's Inspector window, ensure the Render Features list
    /// contains a <see cref="ARBackgroundRendererFeature"/>.</description></item>
    /// </list>
    /// </para>
    /// <para>To customize background rendering with the legacy render pipeline, you can override the
    /// <see cref="legacyCameraEvents"/> property and the
    /// <see cref="ConfigureLegacyCommandBuffer(CommandBuffer)"/> method to modify the given
    /// <c>CommandBuffer</c> with rendering commands and to inject the given <c>CommandBuffer</c> into the Camera's
    /// rendering.</para>
    /// <para>To customize background rendering with a scriptable render pipeline, create a
    /// <c>ScriptableRendererFeature</c> with the background rendering commands, and insert the
    /// <c>ScriptableRendererFeature</c> into the list of render features for the scriptable renderer.</para>
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(ARCameraManager))]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARCameraBackground) + ".html")]
    public class ARCameraBackground : MonoBehaviour
    {
        /// <summary>
        /// Name for the custom rendering command buffer.
        /// </summary>
        const string k_CustomRenderPassName = "AR Background Pass (LegacyRP)";

        /// <summary>
        /// Name of the main texture parameter for the material.
        /// </summary>
        const string k_MainTexName = "_MainTex";

        /// <summary>
        /// Name of the shader parameter for the display transform matrix.
        /// </summary>
        const string k_DisplayTransformName = "_UnityDisplayTransform";

        /// <summary>
        /// Property ID for the shader parameter for the main texture.
        /// </summary>
        static readonly int k_MainTexId = Shader.PropertyToID(k_MainTexName);

        /// <summary>
        /// Property ID for the shader parameter for the display transform matrix.
        /// </summary>
        static readonly int k_DisplayTransformId = Shader.PropertyToID(k_DisplayTransformName);

        /// <summary>
        /// The Property ID for the shader parameter for the forward vector's scaled length.
        /// </summary>
        static readonly int k_CameraForwardScaleId = Shader.PropertyToID("_UnityCameraForwardScale");

        /// <summary>
        /// The camera to which the projection matrix is set on each frame event.
        /// </summary>
        Camera m_Camera;

        /// <summary>
        /// The camera manager from which frame information is pulled.
        /// </summary>
        ARCameraManager m_CameraManager;

        /// <summary>
        /// The occlusion manager, which might not exist, from which occlusion information is pulled.
        /// </summary>
        AROcclusionManager m_OcclusionManager;

        /// <summary>
        /// Command buffer for any custom rendering commands.
        /// </summary>
        CommandBuffer m_CommandBuffer;

        /// <summary>
        /// Whether to use the custom material for rendering the background.
        /// </summary>
        [SerializeField, FormerlySerializedAs("m_OverrideMaterial")]
        bool m_UseCustomMaterial;

        /// <summary>
        /// A custom material for rendering the background.
        /// </summary>
        [SerializeField, FormerlySerializedAs("m_Material")]
        Material m_CustomMaterial;

        /// <summary>
        /// The default material for rendering the background.
        /// </summary>
        Material m_DefaultMaterial;

        /// <summary>
        /// The previous clear flags for the camera, if any.
        /// </summary>
        CameraClearFlags? m_PreviousCameraClearFlags;

        /// <summary>
        /// The original field of view of the camera, before enabling background rendering.
        /// </summary>
        float? m_PreviousCameraFieldOfView;

        /// <summary>
        /// True if background rendering is enabled, false otherwise.
        /// </summary>
        bool m_BackgroundRenderingEnabled;

        /// <summary>
        /// The camera to which the projection matrix is set on each frame event.
        /// </summary>
        /// <value>
        /// The camera to which the projection matrix is set on each frame event.
        /// </value>
#if UNITY_EDITOR
        protected new Camera camera => m_Camera;
#else // UNITY_EDITOR
        protected Camera camera => m_Camera;
#endif // UNITY_EDITOR

        /// <summary>
        /// The camera manager from which frame information is pulled.
        /// </summary>
        /// <value>
        /// The camera manager from which frame information is pulled.
        /// </value>
        protected ARCameraManager cameraManager => m_CameraManager;

        /// <summary>
        /// The occlusion manager, which might not exist, from which occlusion information is pulled.
        /// </summary>
        protected AROcclusionManager occlusionManager => m_OcclusionManager;

        /// <summary>
        /// The current <c>Material</c> used for background rendering.
        /// </summary>
        public Material material => (useCustomMaterial && (customMaterial != null)) ? customMaterial : defaultMaterial;

        /// <summary>
        /// Whether to use the custom material for rendering the background.
        /// </summary>
        /// <value>
        /// <c>true</c> if the custom material should be used for rendering the camera background. Otherwise,
        /// <c>false</c>.
        /// </value>
        public bool useCustomMaterial { get => m_UseCustomMaterial; set => m_UseCustomMaterial = value; }

        /// <summary>
        /// A custom material for rendering the background.
        /// </summary>
        /// <value>
        /// A custom material for rendering the background.
        /// </value>
        public Material customMaterial { get => m_CustomMaterial; set => m_CustomMaterial = value; }

        /// <summary>
        /// Whether background rendering is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if background rendering is enabled and if at least one camera frame has been received.
        /// Otherwise, <c>false</c>.
        /// </value>
        public bool backgroundRenderingEnabled => m_BackgroundRenderingEnabled;

        /// <summary>
        /// The default material for rendering the background.
        /// </summary>
        /// <value>
        /// The default material for rendering the background.
        /// </value>
        Material defaultMaterial => cameraManager.cameraMaterial;

        /// <summary>
        /// Whether to use the legacy rendering pipeline.
        /// </summary>
        /// <value>
        /// <c>true</c> if the legacy render pipeline is in use. Otherwise, <c>false</c>.
        /// </value>
        bool useLegacyRenderPipeline => GraphicsSettings.currentRenderPipeline == null;

        /// <summary>
        /// Stores the previous culling state (XRCameraSubsystem.invertCulling).
        /// If the requested culling state changes, the command buffer must be rebuilt.
        /// </summary>
        bool m_CommandBufferCullingState;

        /// <summary>
        /// A function that can be invoked by
        /// [CommandBuffer.IssuePluginEvent](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.IssuePluginEvent.html).
        /// This function calls the XRCameraSubsystem method that should be called immediately before background rendering.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        [MonoPInvokeCallback(typeof(Action<int>))]
        static void BeforeBackgroundRenderHandler(int eventId)
        {
            if (s_CameraSubsystem != null)
            {
                s_CameraSubsystem.OnBeforeBackgroundRender(eventId);
            }
        }

        /// <summary>
        /// A delegate representation of <see cref="BeforeBackgroundRenderHandler(int)"/>. This maintains a strong
        /// reference to the delegate, which is converted to an IntPtr by <see cref="s_BeforeBackgroundRenderHandlerFuncPtr"/>.
        /// </summary>
        /// <seealso cref="AddBeforeBackgroundRenderHandler(CommandBuffer)"/>
        static Action<int> s_BeforeBackgroundRenderHandler = BeforeBackgroundRenderHandler;

        /// <summary>
        /// A pointer to a method to be called immediately before rendering that is implemented in the XRCameraSubsystem implementation.
        /// It is called via [CommandBuffer.IssuePluginEvent](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.IssuePluginEvent.html).
        /// </summary>
        static readonly IntPtr s_BeforeBackgroundRenderHandlerFuncPtr = Marshal.GetFunctionPointerForDelegate(s_BeforeBackgroundRenderHandler);

        /// <summary>
        /// Static reference to the active XRCameraSubsystem. Necessary here for access from a static delegate.
        /// </summary>
        static ARSubsystems.XRCameraSubsystem s_CameraSubsystem;

        /// <summary>
        /// Whether culling should be inverted. Used during command buffer configuration,
        /// see [CommandBuffer.SetInvertCulling](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.SetInvertCulling.html).
        /// </summary>
        /// <seealso cref="ConfigureLegacyCommandBuffer(CommandBuffer)"/>
        protected bool shouldInvertCulling => m_CameraManager?.subsystem?.invertCulling ?? false;

        void Awake()
        {
            m_Camera = GetComponent<Camera>();
            m_CameraManager = GetComponent<ARCameraManager>();
            m_OcclusionManager = GetComponent<AROcclusionManager>();
        }

        void OnEnable()
        {
            // Ensure that background rendering is disabled until the first camera frame is received.
            m_BackgroundRenderingEnabled = false;
            cameraManager.frameReceived += OnCameraFrameReceived;
            if (occlusionManager != null)
            {
                occlusionManager.frameReceived += OnOcclusionFrameReceived;
            }
        }

        void OnDisable()
        {
            if (occlusionManager != null)
            {
                occlusionManager.frameReceived -= OnOcclusionFrameReceived;
            }
            cameraManager.frameReceived -= OnCameraFrameReceived;
            DisableBackgroundRendering();

            // We are no longer setting the projection matrix so tell the camera to resume its normal projection matrix
            // calculations.
            camera.ResetProjectionMatrix();
        }

        /// <summary>
        /// Enable background rendering by disabling the camera's clear flags, and enabling the legacy RP background
        /// rendering if your application is in legacy RP mode.
        /// </summary>
        void EnableBackgroundRendering()
        {
            m_BackgroundRenderingEnabled = true;

            // We must hold a static reference to the camera subsystem so that it is accessible to the
            // static callback needed for calling OnBeforeBackgroundRender() from the render thread
            if (m_CameraManager)
            {
                s_CameraSubsystem = m_CameraManager.subsystem;
            }
            else
            {
                s_CameraSubsystem = null;
            }

            DisableBackgroundClearFlags();

            m_PreviousCameraFieldOfView = m_Camera.fieldOfView;

            Material material = defaultMaterial;
            if (useLegacyRenderPipeline && (material != null))
            {
                EnableLegacyRenderPipelineBackgroundRendering();
            }
        }

        /// <summary>
        /// Disable background rendering by disabling the legacy RP background rendering if your application is  in legacy RP mode
        /// and restoring the camera's clear flags.
        /// </summary>
        void DisableBackgroundRendering()
        {
            m_BackgroundRenderingEnabled = false;

            DisableLegacyRenderPipelineBackgroundRendering();

            RestoreBackgroundClearFlags();

            if (m_PreviousCameraFieldOfView.HasValue)
            {
                m_Camera.fieldOfView = m_PreviousCameraFieldOfView.Value;
                m_PreviousCameraFieldOfView = null;
            }

            s_CameraSubsystem = null;
        }

        /// <summary>
        /// Set the camera's clear flags to do nothing while preserving the previous camera clear flags.
        /// </summary>
        void DisableBackgroundClearFlags()
        {
            m_PreviousCameraClearFlags = m_Camera.clearFlags;
            m_Camera.clearFlags = CameraClearFlags.Nothing;
        }

        /// <summary>
        /// Restore the previous camera's clear flags, if any.
        /// </summary>
        void RestoreBackgroundClearFlags()
        {
            if (m_PreviousCameraClearFlags != null)
            {
                m_Camera.clearFlags = m_PreviousCameraClearFlags.Value;
            }
        }

        /// <summary>
        /// The list of [CameraEvent](https://docs.unity3d.com/ScriptReference/Rendering.CameraEvent.html)s
        /// to add to the [CommandBuffer](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.html).
        /// </summary>
        static readonly CameraEvent[] s_DefaultCameraEvents = new[]
        {
            CameraEvent.BeforeForwardOpaque,
            CameraEvent.BeforeGBuffer
        };

        /// <summary>
        /// The list of [CameraEvent](https://docs.unity3d.com/ScriptReference/Rendering.CameraEvent.html)s
        /// to add to the [CommandBuffer](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.html).
        /// By default, returns
        /// [BeforeForwardOpaque](https://docs.unity3d.com/ScriptReference/Rendering.CameraEvent.BeforeForwardOpaque.html)
        /// and
        /// [BeforeGBuffer](https://docs.unity3d.com/ScriptReference/Rendering.CameraEvent.BeforeGBuffer.html)}.
        /// Override to use different camera events.
        /// </summary>
        protected virtual IEnumerable<CameraEvent> legacyCameraEvents => s_DefaultCameraEvents;

        /// <summary>
        /// Configures the <paramref name="commandBuffer"/> by first clearing it,
        /// and then adding necessary render commands.
        /// </summary>
        /// <param name="commandBuffer">The command buffer to configure.</param>
        protected virtual void ConfigureLegacyCommandBuffer(CommandBuffer commandBuffer)
        {
            Texture texture = !material.HasProperty(k_MainTexName) ? null : material.GetTexture(k_MainTexName);

            commandBuffer.Clear();
            AddBeforeBackgroundRenderHandler(commandBuffer);
            m_CommandBufferCullingState = shouldInvertCulling;
            commandBuffer.SetInvertCulling(m_CommandBufferCullingState);
            commandBuffer.ClearRenderTarget(true, false, Color.clear);
            commandBuffer.Blit(texture, BuiltinRenderTextureType.CameraTarget, material);

            // callback to schedule the release of the metal textures after rendering is complete
            var callback = NativeApi.Unity_Camera_GetTextureReleaseCallbackHandle();
            if (callback != IntPtr.Zero)
            {
                commandBuffer.IssuePluginEvent(callback, 1);
            }
        }

        /// <summary>
        /// Enable background rendering getting a command buffer, and configure it for rendering the background.
        /// </summary>
        void EnableLegacyRenderPipelineBackgroundRendering()
        {
            if (m_CommandBuffer == null)
            {
                m_CommandBuffer = new CommandBuffer();
                m_CommandBuffer.name = k_CustomRenderPassName;

                ConfigureLegacyCommandBuffer(m_CommandBuffer);
                foreach (var cameraEvent in legacyCameraEvents)
                {
                    camera.AddCommandBuffer(cameraEvent, m_CommandBuffer);
                }
            }
        }

        /// <summary>
        /// Disable background rendering by removing the command buffer from the camera.
        /// </summary>
        void DisableLegacyRenderPipelineBackgroundRendering()
        {
            if (m_CommandBuffer != null)
            {
                foreach (var cameraEvent in legacyCameraEvents)
                {
                    camera.RemoveCommandBuffer(cameraEvent, m_CommandBuffer);
                }

                m_CommandBuffer = null;
            }
        }

        /// <summary>
        /// This adds a command to the <paramref name="commandBuffer"/> to make call from the render thread
        /// to a callback on the `XRCameraSubsystem` implementation. The callback handles any implementation-specific
        /// functionality needed immediately before the background is rendered.
        /// </summary>
        /// <param name="commandBuffer">The [CommandBuffer](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.html)
        /// to add the command to.</param>
        internal static void AddBeforeBackgroundRenderHandler(CommandBuffer commandBuffer)
        {
            commandBuffer.IssuePluginEvent(s_BeforeBackgroundRenderHandlerFuncPtr, 0);
        }

        /// <summary>
        /// Callback for the camera frame event.
        /// </summary>
        /// <param name="eventArgs">The camera event arguments.</param>
        void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            // Enable background rendering when first frame is received.
            if (m_BackgroundRenderingEnabled)
            {
                if (eventArgs.textures.Count == 0)
                {
                    DisableBackgroundRendering();
                }
                else if (m_CommandBuffer != null && m_CommandBufferCullingState != shouldInvertCulling)
                {
                    ConfigureLegacyCommandBuffer(m_CommandBuffer);
                }
            }
            else if (eventArgs.textures.Count > 0)
            {
                EnableBackgroundRendering();
            }

            Material material = this.material;
            if (material != null)
            {
                var count = eventArgs.textures.Count;
                for (int i = 0; i < count; ++i)
                {
                    material.SetTexture(eventArgs.propertyNameIds[i], eventArgs.textures[i]);
                }

                if (eventArgs.displayMatrix.HasValue)
                {
                    material.SetMatrix(k_DisplayTransformId, eventArgs.displayMatrix.Value);
                }

                SetMaterialKeywords(material, eventArgs.enabledMaterialKeywords, eventArgs.disabledMaterialKeywords);
            }

            if (eventArgs.projectionMatrix.HasValue)
            {
                camera.projectionMatrix = eventArgs.projectionMatrix.Value;
                const float twiceRad2Deg = 2 * Mathf.Rad2Deg;
                var halfHeightOverNear = 1 / camera.projectionMatrix[1, 1];
                camera.fieldOfView = Mathf.Atan(halfHeightOverNear) * twiceRad2Deg;
            }
        }

        /// <summary>
        /// Callback for the occlusion frame event.
        /// </summary>
        /// <param name="eventArgs">The occlusion frame event arguments.</param>
        void OnOcclusionFrameReceived(AROcclusionFrameEventArgs eventArgs)
        {
            Material material = this.material;
            if (material != null)
            {
                var count = eventArgs.textures.Count;
                for (int i = 0; i < count; ++i)
                {
                    material.SetTexture(eventArgs.propertyNameIds[i], eventArgs.textures[i]);
                }

                SetMaterialKeywords(material, eventArgs.enabledMaterialKeywords, eventArgs.disabledMaterialKeywords);

                // Set scale: this computes the affect the camera's localToWorld has on the the length of the
                // forward vector, i.e., how much farther from the camera are things than with unit scale.
                var forward = transform.localToWorldMatrix.GetColumn(2);
                var scale = forward.magnitude;
                material.SetFloat(k_CameraForwardScaleId, scale);
            }
        }

        void SetMaterialKeywords(Material material, List<string> enabledMaterialKeywords,
                                 List<string> disabledMaterialKeywords)
        {
            if (enabledMaterialKeywords != null)
            {
                foreach (var materialKeyword in enabledMaterialKeywords)
                {
                    if (!material.IsKeywordEnabled(materialKeyword))
                    {
                        material.EnableKeyword(materialKeyword);
                    }
                }
            }

            if (disabledMaterialKeywords != null)
            {
                foreach (var materialKeyword in disabledMaterialKeywords)
                {
                    if (material.IsKeywordEnabled(materialKeyword))
                    {
                        material.DisableKeyword(materialKeyword);
                    }
                }
            }
        }

        /// <summary>
        /// Container to wrap the native APIs.
        /// </summary>
        internal static class NativeApi
        {
#if UNITY_XR_ARKIT_LOADER_ENABLED && !UNITY_EDITOR
            [DllImport("__Internal", EntryPoint = "UnityARKit_Camera_GetTextureReleaseCallbackHandle")]
            public static extern IntPtr Unity_Camera_GetTextureReleaseCallbackHandle();
#else
            public static IntPtr Unity_Camera_GetTextureReleaseCallbackHandle() => IntPtr.Zero;
#endif
        }
    }
}
