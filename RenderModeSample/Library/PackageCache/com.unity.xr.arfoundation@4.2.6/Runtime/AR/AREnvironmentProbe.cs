using System;
using UnityEngine.Rendering;
using UnityEngine.XR.ARSubsystems;

using Object = UnityEngine.Object;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A GameObject component to manage the reflection probe settings as the environment probe changes are applied.
    /// </summary>
    [RequireComponent(typeof(ReflectionProbe))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ARUpdateOrder.k_EnvironmentProbe)]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(AREnvironmentProbe) + ".html")]
    public class AREnvironmentProbe : ARTrackable<XREnvironmentProbe, AREnvironmentProbe>
    {
        /// <summary>
        /// The reflection probe component attached to the GameObject.
        /// </summary>
        /// <value>
        /// The reflection probe component attached to the GameObject.
        /// </value>
        ReflectionProbe m_ReflectionProbe;

        /// <summary>
        /// The current environment texture data wrapping the reflection probe environment texture.
        /// </summary>
        /// <value>
        /// The current environment texture data wrapping the reflection probe environment texture.
        /// </value>
        XRTextureDescriptor m_CurrentTextureDescriptor;

        /// <summary>
        /// The texture info of the custom baked texture from the reflection probe.
        /// </summary>
        /// <value>
        /// The texture info of the custom baked texture from the reflection probe.
        /// </value>
        ARTextureInfo m_CustomBakedTextureInfo;

        /// <summary>
        /// Specifies the texture filter mode to be used with the environment texture.
        /// </summary>
        /// <value>
        /// The texture filter mode to be used with the environment texture.
        /// </value>
        public FilterMode environmentTextureFilterMode
        {
            get => m_EnvironmentTextureFilterMode;
            set
            {
                m_EnvironmentTextureFilterMode = value;
                if ((m_ReflectionProbe != null) && (m_ReflectionProbe.customBakedTexture != null))
                {
                    m_ReflectionProbe.customBakedTexture.filterMode = m_EnvironmentTextureFilterMode;
                }
            }
        }

        FilterMode m_EnvironmentTextureFilterMode = FilterMode.Trilinear;

        /// <summary>
        /// The placement type (for example, manual or automatic). If manual, this probe was created by <see cref="AREnvironmentProbeManager.AddEnvironmentProbe(Pose, Vector3, Vector3)"/>.
        /// </summary>
        public AREnvironmentProbePlacementType placementType { get; internal set; }

        /// <summary>
        /// The size (dimensions) of the environment probe.
        /// </summary>
        public Vector3 size => sessionRelativeData.size;

        /// <summary>
        /// The extents of the environment probe. This is always half the <see cref="size"/>.
        /// </summary>
        public Vector3 extents => size * .5f;

        /// <summary>
        /// A native pointer associated with this environment probe.
        /// The data pointed to by this pointer is implementation defined.
        /// While the lifetime is also implementation defined, it should be valid at
        /// least until the next frame.
        /// </summary>
        public IntPtr nativePtr => sessionRelativeData.nativePtr;

        /// <summary>
        /// The <c>XRTextureDescriptor</c> associated with this environment probe. This is used to generate the cubemap texture on the reflection probe component.
        /// </summary>
        public XRTextureDescriptor textureDescriptor => m_CurrentTextureDescriptor;

        /// <summary>
        /// Initializes the GameObject transform and reflection probe component from the scene.
        /// </summary>
        void Awake()
        {
            m_ReflectionProbe = GetComponent<ReflectionProbe>();

            // Set the reflection probe mode to use a custom baked texture.
            m_ReflectionProbe.mode = ReflectionProbeMode.Custom;
        }

        Transform GetTrackablesParent()
        {
            if (AREnvironmentProbeManager.instance is AREnvironmentProbeManager manager)
            {
                return manager.GetComponent<ARSessionOrigin>().trackablesParent;
            }

            return null;
        }

        internal protected override void OnAfterSetSessionRelativeData()
        {
            var trackablesParent = GetTrackablesParent();
            if (trackablesParent && transform.parent != trackablesParent)
            {
                var pose = sessionRelativeData.pose;

                // Compute the desired world space transform matrix for the environment probe
                var desiredLocalToWorld = trackablesParent.localToWorldMatrix * Matrix4x4.TRS(pose.position, pose.rotation, sessionRelativeData.scale);
                if (transform.parent)
                {
                    // If the probe has a parent (i.e., it is not at the root), then we need to compute
                    // its localScale such that its final world-space scale will match what it would be
                    // if it were under the ARSessionOrigin.
                    var localToParent = transform.parent.worldToLocalMatrix * desiredLocalToWorld;
                    transform.localScale = localToParent.lossyScale;
                }
                else
                {
                    // Otherwise, it is at the scene root, so just set its localScale is its world-space scale.
                    transform.localScale = desiredLocalToWorld.lossyScale;
                }
            }
            else
            {
                transform.localScale = sessionRelativeData.scale;
            }

            // Update the environment texture if the environment texture is valid.
            m_ReflectionProbe.enabled = sessionRelativeData.textureDescriptor.valid;
            if (sessionRelativeData.textureDescriptor.valid)
            {
                UpdateEnvironmentTexture(sessionRelativeData.textureDescriptor);
            }

            // Update the reflection probe box.
            m_ReflectionProbe.center = Vector3.zero;
            m_ReflectionProbe.size = sessionRelativeData.size;
            m_ReflectionProbe.boxProjection = !float.IsInfinity(m_ReflectionProbe.size.x);

            // Manual placement is set by the manager. Unknown means it must have been added automatically.
            if (placementType == AREnvironmentProbePlacementType.Unknown)
            {
                placementType = AREnvironmentProbePlacementType.Automatic;
            }
        }

        /// <summary>
        /// Applies the texture data in the <c>XRTextureDescriptor</c> to the reflection probe settings.
        /// </summary>
        /// <param name="textureDescriptor">The environment texture data to apply to the reflection probe baked
        /// texture.</param>
        void UpdateEnvironmentTexture(XRTextureDescriptor textureDescriptor)
        {
            if(m_ReflectionProbe.customBakedTexture == null)
            {
                m_CustomBakedTextureInfo = new ARTextureInfo(textureDescriptor);
            }
            else
            {
                m_CustomBakedTextureInfo = ARTextureInfo.GetUpdatedTextureInfo(m_CustomBakedTextureInfo, textureDescriptor);
            }

            m_CustomBakedTextureInfo.texture.filterMode = m_EnvironmentTextureFilterMode;
            m_ReflectionProbe.customBakedTexture = m_CustomBakedTextureInfo.texture as Cubemap;

            // Update the current environment texture metadata.
            m_CurrentTextureDescriptor = textureDescriptor;
        }

        /// <summary>
        /// Generates a string representation of this <see cref="AREnvironmentProbe"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="AREnvironmentProbe"/>.</returns>
        public override string ToString() => $"{name} [trackableId:{trackableId.ToString()}]";

        void OnEnable()
        {
            if (AREnvironmentProbeManager.instance is AREnvironmentProbeManager manager)
            {
                manager.TryAddEnvironmentProbe(this);
            }
            else
            {
                pending = true;
            }
        }

        void Update()
        {
            if (trackableId.Equals(TrackableId.invalidId) && AREnvironmentProbeManager.instance is AREnvironmentProbeManager manager)
            {
                manager.TryAddEnvironmentProbe(this);
            }
        }

        void OnDisable()
        {
            if (AREnvironmentProbeManager.instance is AREnvironmentProbeManager manager)
            {
                manager.TryRemoveEnvironmentProbe(this);
            }
        }
    }
}
