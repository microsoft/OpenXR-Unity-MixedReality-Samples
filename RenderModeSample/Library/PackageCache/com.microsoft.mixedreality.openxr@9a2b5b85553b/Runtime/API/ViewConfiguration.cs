// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// A view configuration is a semantically meaningful set of one or more views for which an application can render images.
    /// </summary>
    public enum ViewConfigurationType
    {
        /// <summary>
        /// A primary view configuration is a view configuration intended to be presented to the viewer interacting with the XR application.
        /// </summary>
        PrimaryStereo = 2,

        /// <summary>
        /// This first-person observer view configuration intended for a first-person view of the scene to be composed onto video frames
        /// being captured from a camera attached to and moved with the primary display on the form factor, which is generally for viewing
        /// on a 2D screen by an external observer. This first-person camera will be facing forward with roughly the same perspective as
        /// the primary views, and so the application should render its view to show objects that surround the user and avoid rendering the user's body avatar.
        /// </summary>
        SecondaryMonoFirstPersonObserver = 1000054000
    }

    /// <summary>
    /// Observe and manage a view configuration of the current XR session.
    /// </summary>
    public class ViewConfiguration
    {
        private static readonly MixedRealityFeaturePlugin m_feature = OpenXRSettings.Instance.GetFeature<MixedRealityFeaturePlugin>();
        internal readonly OpenXRViewConfiguration m_openxrViewConfiguration;

        /// <summary>
        /// Get all enabled view configurations when the XR session is started.
        /// </summary>
        public static IReadOnlyList<ViewConfiguration> EnabledViewConfigurations
        {
            get
            {
                return (m_feature != null && m_feature.enabled)
                    ? m_feature.EnabledViewConfigurations
                    : Array.Empty<ViewConfiguration>();
            }
        }

        /// <summary>
        /// Get the primary view configuration of the XR session.
        /// </summary>
        public static ViewConfiguration Primary
        {
            get
            {
                return (m_feature != null && m_feature.enabled)
                    ? m_feature.PrimaryViewConfiguration
                    : null;
            }
        }

        internal ViewConfiguration(OpenXRViewConfiguration openxrViewConfiguration)
        {
            m_openxrViewConfiguration = openxrViewConfiguration;
        }

        /// <summary>
        /// Get the view configuration type
        /// </summary>
        public ViewConfigurationType ViewConfigurationType => m_openxrViewConfiguration.ViewConfigurationType;

        /// <summary>
        /// Get whether or not this view configuration is active for the current frame.
        /// If IsActive is false, the rendering into the view configuration will be ignored and not visible to user.
        /// </summary>
        public bool IsActive => m_openxrViewConfiguration.IsActive;

        /// <summary>
        /// Adjustment to stereo separation in meters for primary stereo view configuration.
        /// The value will be ignored for mono or secondary view configurations.
        /// </summary>
        public float StereoSeparationAdjustment
        {
            set => m_openxrViewConfiguration.SetStereoSeparationAdjustment(value);
            get => m_openxrViewConfiguration.StereoSeparationAdjustment;
        }

        /// <summary>
        /// Get all supported reprojection modes for this view configuration.
        /// </summary>
        public IReadOnlyList<ReprojectionMode> SupportedReprojectionModes => m_openxrViewConfiguration.SupportedReprojectionModes;

        /// <summary>
        /// Set the reprojection settings for the view configuration that will be used for the current frame.
        /// </summary>
        /// <remarks>
        /// The given setting only affects the current frame, and must be set for each frame to maintain the effect.
        /// </remarks>
        /// <param name="settings">The reprojection settings to be set.</param>
        public void SetReprojectionSettings(ReprojectionSettings settings) => m_openxrViewConfiguration.SetReprojectionSettings(settings);
    }

    /// <summary>
    /// The ReprojectionMode describes the reprojection mode of a projection composition layer.
    /// </summary>
    public enum ReprojectionMode
    {
        /// <summary>
        /// Indicates the rendering may benefit from per-pixel depth reprojection.
        /// This mode is typically used for world-locked content that should remain physically stationary as the user walks around.
        /// </summary>
        Depth = 1,

        /// <summary>
        /// Indicates the rendering may benefit from planar reprojection and the plane can be calculated from the corresponding depth information.
        /// This mode works better when the application knows the content is mostly placed on a plane.
        /// </summary>
        PlanarFromDepth = 2,

        /// <summary>
        /// Indicates that the rendering may benefit from planar reprojection.
        /// The application can customize the plane by ReprojectionSettings.
        /// The app can also omit the plane override, indicating the runtime should use the default reprojection plane settings.
        /// This mode works better when the application knows the content is mostly placed on a plane, or when it cannot afford to submit depth information.
        /// </summary>
        PlanarManual = 3,

        /// <summary>
        /// Indicates the layer should be stabilized only for changes to orientation, ignoring positional changes.
        /// This mode works better for body-locked content that should follow the user as they walk around, such as 360-degree video.
        /// </summary>
        OrientationOnly = 4,

        /// <summary>
        /// Indicates the rendering should not be stabilized by the runtime.
        /// </summary>
        NoReprojection = -1
    }

    /// <summary>
    /// The settings to control the reprojection of current rendering frame,
    /// including the reprojection mode and optional stabilization plane override.
    /// </summary>
    public struct ReprojectionSettings
    {
        /// <summary>
        /// The reprojection mode to be used with this view configuration. Overrides any reprojection mode
        /// set in XRDisplaySubsystem. The default value is ReprojectionMode.Depth.
        /// </summary>
        public ReprojectionMode ReprojectionMode
        {
            get => m_reprojectionMode ?? ReprojectionMode.Depth;
            set => m_reprojectionMode = value;
        }
        private ReprojectionMode? m_reprojectionMode;

        /// <summary>
        /// When the application is confident that overriding the reprojection plane can benefit hologram
        /// stability, it can provide this override to further help the runtime fine tune the reprojection
        /// details. This Vector3 describes the position of the focus plane represented in the Unity scene.
        /// </summary>
        public Vector3? ReprojectionPlaneOverridePosition;

        /// <summary>
        /// When the application is confident that overriding the reprojection plane can benefit hologram
        /// stability, it can provide this override to further help the runtime fine tune the reprojection
        /// details. This Vector3 is a unit vector describing the focus plane normal represented in the
        /// Unity scene.
        /// </summary>
        public Vector3? ReprojectionPlaneOverrideNormal;

        /// <summary>
        /// When the application is confident that overriding the reprojection plane can benefit hologram
        /// stability, it can provide this override to further help the runtime fine tune the reprojection
        /// details. This Vector3 is a velocity of the position in the Unity scene, measured in meters per
        /// second.
        /// </summary>
        public Vector3? ReprojectionPlaneOverrideVelocity;
    }
}
