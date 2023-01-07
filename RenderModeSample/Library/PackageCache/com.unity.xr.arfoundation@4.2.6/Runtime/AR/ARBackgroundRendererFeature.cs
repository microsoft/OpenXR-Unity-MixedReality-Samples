using System;
using UnityEngine;
using UnityEngine.Rendering;
#if MODULE_URP_ENABLED
using UnityEngine.Rendering.Universal;
#elif MODULE_LWRP_ENABLED
using UnityEngine.Rendering.LWRP;
#else
using ScriptableRendererFeature = UnityEngine.ScriptableObject;
#endif

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A render feature for rendering the camera background for AR devies.
    /// </summary>
    public class ARBackgroundRendererFeature : ScriptableRendererFeature
    {
#if MODULE_URP_ENABLED || MODULE_LWRP_ENABLED
        /// <summary>
        /// The scriptable render pass to be added to the renderer when the camera background is to be rendered.
        /// </summary>
        CustomRenderPass m_ScriptablePass;

        /// <summary>
        /// The mesh for rendering the background shader.
        /// </summary>
        Mesh m_BackgroundMesh;

        /// <summary>
        /// Create the scriptable render pass.
        /// </summary>
        public override void Create()
        {
#if !UNITY_EDITOR
            m_ScriptablePass = new CustomRenderPass(RenderPassEvent.BeforeRenderingOpaques);

            m_BackgroundMesh = new Mesh();
            m_BackgroundMesh.vertices =  new Vector3[]
            {
                new Vector3(0f, 0f, 0.1f),
                new Vector3(0f, 1f, 0.1f),
                new Vector3(1f, 1f, 0.1f),
                new Vector3(1f, 0f, 0.1f),
            };
            m_BackgroundMesh.uv = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
            };
            m_BackgroundMesh.triangles = new int[] {0, 1, 2, 0, 2, 3};
#endif // !UNITY_EDITOR
        }

        /// <summary>
        /// Add the background rendering pass when rendering a game camera with an enabled AR camera background component.
        /// </summary>
        /// <param name="renderer">The scriptable renderer in which to enqueue the render pass.</param>
        /// <param name="renderingData">Additional rendering data about the current state of rendering.</param>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if !UNITY_EDITOR
            Camera currentCamera = renderingData.cameraData.camera;
            if ((currentCamera != null) && (currentCamera.cameraType == CameraType.Game))
            {
                ARCameraBackground cameraBackground = currentCamera.gameObject.GetComponent<ARCameraBackground>();
                if ((cameraBackground != null) && cameraBackground.backgroundRenderingEnabled
                    && (cameraBackground.material != null))
                {
                    bool invertCulling = cameraBackground.GetComponent<ARCameraManager>()?.subsystem?.invertCulling ?? false;
                    m_ScriptablePass.Setup(m_BackgroundMesh, cameraBackground.material, invertCulling);
                    renderer.EnqueuePass(m_ScriptablePass);
                }
            }
#endif // !UNITY_EDITOR
        }

        /// <summary>
        /// The custom render pass to render the camera background.
        /// </summary>
        class CustomRenderPass : ScriptableRenderPass
        {
            /// <summary>
            /// The name for the custom render pass which will display in graphics debugging tools.
            /// </summary>
            const string k_CustomRenderPassName = "AR Background Pass (URP)";

            /// <summary>
            /// The orthogonal projection matrix for the background rendering.
            /// </summary>
            static readonly Matrix4x4 k_BackgroundOrthoProjection = Matrix4x4.Ortho(0f, 1f, 0f, 1f, -0.1f, 9.9f);

            /// <summary>
            /// The mesh for rendering the background material.
            /// </summary>
            Mesh m_BackgroundMesh;

            /// <summary>
            /// The material used for rendering the device background using the camera video texture and potentially
            /// other device-specific properties and textures.
            /// </summary>
            Material m_BackgroundMaterial;

            /// <summary>
            /// Whether the culling mode should be inverted.
            /// ([CommandBuffer.SetInvertCulling](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.SetInvertCulling.html)).
            /// </summary>
            bool m_InvertCulling;

            /// <summary>
            /// Constructs the background render pass.
            /// </summary>
            /// <param name="renderPassEvent">The render pass event when this pass should be rendered.</param>
            public CustomRenderPass(RenderPassEvent renderPassEvent)
            {
                this.renderPassEvent = renderPassEvent;
            }

            /// <summary>
            /// Set up the background render pass.
            /// </summary>
            /// <param name="backgroundMesh">The mesh used for rendering the device background.</param>
            /// <param name="backgroundMaterial">The material used for rendering the device background.</param>
            /// <param name="invertCulling">Whether the culling mode should be inverted.</param>
            public void Setup(Mesh backgroundMesh, Material backgroundMaterial, bool invertCulling)
            {
                m_BackgroundMesh = backgroundMesh;
                m_BackgroundMaterial = backgroundMaterial;
                m_InvertCulling = invertCulling;
            }

            /// <summary>
            /// Configure the render pass by configuring the render target and clear values.
            /// </summary>
            /// <param name="commandBuffer">The command buffer for configuration.</param>
            /// <param name="renderTextureDescriptor">The descriptor of the target render texture.</param>
            public override void Configure(CommandBuffer commandBuffer, RenderTextureDescriptor renderTextureDescriptor)
            {
                ConfigureClear(ClearFlag.Depth, Color.clear);
            }

            /// <summary>
            /// Execute the commands to render the camera background.
            /// </summary>
            /// <param name="context">The render context for executing the render commands.</param>
            /// <param name="renderingData">Additional rendering data about the current state of rendering.</param>
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get(k_CustomRenderPassName);
                cmd.BeginSample(k_CustomRenderPassName);

                ARCameraBackground.AddBeforeBackgroundRenderHandler(cmd);

                // callback to schedule the release of the metal textures after rendering is complete
                var callback = ARCameraBackground.NativeApi.Unity_Camera_GetTextureReleaseCallbackHandle();
                if (callback != IntPtr.Zero)
                {
                    cmd.IssuePluginEvent(callback, 1);
                }

                cmd.SetInvertCulling(m_InvertCulling);

                cmd.SetViewProjectionMatrices(Matrix4x4.identity, k_BackgroundOrthoProjection);
                cmd.DrawMesh(m_BackgroundMesh, Matrix4x4.identity, m_BackgroundMaterial);
                cmd.SetViewProjectionMatrices(renderingData.cameraData.camera.worldToCameraMatrix,
                                              renderingData.cameraData.camera.projectionMatrix);

                cmd.EndSample(k_CustomRenderPassName);
                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);
            }

            /// <summary>
            /// Clean up any resources for the render pass.
            /// </summary>
            /// <param name="commandBuffer">The command buffer for frame cleanup.</param>
            public override void FrameCleanup(CommandBuffer commandBuffer)
            {
            }
        }
#endif // MODULE_URP_ENABLED || MODULE_LWRP_ENABLED
    }
}
