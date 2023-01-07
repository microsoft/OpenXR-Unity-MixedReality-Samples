using System;
using System.Collections.Generic;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A set of flags that represent features available in AR.
    /// </summary>
    [Flags]
    public enum Feature : ulong
    {
        /// <summary>
        /// No features are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// The world-facing camera. On a phone, this is the rear camera.
        /// </summary>
        WorldFacingCamera = 1 << 0,

        /// <summary>
        /// The user-facing camera. On a phone, this is the front camera.
        /// </summary>
        UserFacingCamera = 1 << 1,

        /// <summary>
        /// Either camera (<see cref="WorldFacingCamera"/> or <see cref="UserFacingCamera"/>).
        /// </summary>
        AnyCamera = WorldFacingCamera | UserFacingCamera,

        /// <summary>
        /// Rotation-only tracking (that is, 3 degrees of freedom without positional tracking).
        /// </summary>
        RotationOnly = 1 << 2,

        /// <summary>
        /// Both position and rotation tracking (that is, 6 degrees of freedom).
        /// </summary>
        PositionAndRotation = 1 << 3,

        /// <summary>
        /// Any tracking mode (<see cref="RotationOnly"/> or <see cref="PositionAndRotation"/>).
        /// </summary>
        AnyTrackingMode = RotationOnly | PositionAndRotation,

        /// <summary>
        /// Face detection. See <see cref="XRFaceSubsystem"/>.
        /// </summary>
        FaceTracking = 1 << 4,

        /// <summary>
        /// Plane detection. See <see cref="XRPlaneSubsystem"/>.
        /// </summary>
        PlaneTracking = 1 << 5,

        /// <summary>
        /// Image detection. See <see cref="XRImageTrackingSubsystem"/>.
        /// </summary>
        ImageTracking = 1 << 6,

        /// <summary>
        /// 3D object detection. See <see cref="XRObjectTrackingSubsystem"/>.
        /// </summary>
        ObjectTracking = 1 << 7,

        /// <summary>
        /// Environment probes. See <see cref="XREnvironmentProbeSubsystem"/>.
        /// </summary>
        EnvironmentProbes = 1 << 8,

        /// <summary>
        /// 2D human body tracking. See <see cref="XRHumanBodySubsystem"/>.
        /// </summary>
        Body2D = 1 << 9,

        /// <summary>
        /// 3D human body tracking. See <see cref="XRHumanBodySubsystem"/>.
        /// </summary>
        Body3D = 1 << 10,

        /// <summary>
        /// Estimate scale when performing 3D human body tracking. See <see cref="Body3D"/>.
        /// </summary>
        Body3DScaleEstimation = 1 << 11,

        /// <summary>
        /// People occlusion with stencil texture enabled. See <see cref="XROcclusionSubsystem"/>.
        /// </summary>
        PeopleOcclusionStencil = 1 << 12,

        /// <summary>
        /// People occlusion with depth texture enabled. See <see cref="XROcclusionSubsystem"/>.
        /// </summary>
        PeopleOcclusionDepth = 1 << 13,

        /// <summary>
        /// Collaborative session. See <see cref="XRParticipantSubsystem"/>.
        /// </summary>
        Collaboration = 1 << 14,

        /// <summary>
        /// Auto focus enabled.
        /// </summary>
        AutoFocus = 1 << 15,

        /// <summary>
        /// Light estimation for ambient intensity.
        /// </summary>
        LightEstimationAmbientIntensity = 1 << 16,

        /// <summary>
        /// Light estimation for ambient color.
        /// </summary>
        LightEstimationAmbientColor = 1 << 17,

        /// <summary>
        /// Light estimation for ambient spherical harmonics.
        /// </summary>
        LightEstimationAmbientSphericalHarmonics = 1 << 18,

        /// <summary>
        /// Light estimation for the main light's direction.
        /// </summary>
        LightEstimationMainLightDirection = 1 << 19,

        /// <summary>
        /// Light estimation for the main light's intensity.
        /// </summary>
        LightEstimationMainLightIntensity = 1 << 20,

        /// <summary>
        /// A value with all light estimation related bits set.
        /// </summary>
        AnyLightEstimation = LightEstimationAmbientIntensity | LightEstimationAmbientColor | LightEstimationAmbientSphericalHarmonics | LightEstimationMainLightDirection | LightEstimationMainLightIntensity,

        /// <summary>
        /// Instant and Tracked raycasts.
        /// </summary>
        Raycast = 1 << 21,

        /// <summary>
        /// A feature that describes real-time meshing capability.
        /// </summary>
        Meshing = 1 << 22,

        /// <summary>
        /// A feature that describes classification for <see cref="Meshing"/>.
        /// </summary>
        MeshClassification = 1 << 23,

        /// <summary>
        /// A feature that describes the ability to surface point clouds.
        /// </summary>
        PointCloud = 1 << 24,

        /// <summary>
        /// A feature that allows environment depth images to be captured.
        /// </summary>
        EnvironmentDepth = 1 << 25,

        /// <summary>
        /// A feature that applies temporal smoothing to environment depth images.
        /// </summary>
        /// <seealso cref="EnvironmentDepth"/>
        EnvironmentDepthTemporalSmoothing = 1 << 26,
    }

    /// <summary>
    /// Extension methods for <see cref="Feature"/> flags.
    /// </summary>
    public static class FeatureExtensions
    {
        /// <summary>
        /// Tests whether any of the features in <paramref name="features"/> are present in <paramref name="self"/>.
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <param name="features">The Features to test against.</param>
        /// <returns><c>true</c> if any of the features in <paramref name="features"/> are also in <paramref name="self"/>,
        /// otherwise <c>false</c>.</returns>
        public static bool Any(this Feature self, Feature features) => (self & features) != Feature.None;

        /// <summary>
        /// Tests whether all the features in <paramref name="features"/> are present in <paramref name="self"/>.
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <param name="features">The Features to test against.</param>
        /// <returns><c>true</c> if all the features in <paramref name="features"/> are also in <paramref name="self"/>,
        /// otherwise <c>false</c>.</returns>
        public static bool All(this Feature self, Feature features) => (self & features) == features;

        /// <summary>
        /// Tests whether there are any common features between <paramref name="self"/> and <paramref name="features"/>.
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <param name="features">The Features to test against.</param>
        /// <returns><c>true</c> if none of the features in <paramref name="self"/> are in <paramref name="features"/>,
        /// otherwise <c>false</c>.</returns>
        public static bool None(this Feature self, Feature features) => (self & features) == Feature.None;

        /// <summary>
        /// Computes the union of <paramref name="self"/> and <paramref name="features"/> (that is,
        /// the set of features in <paramref name="self"/> or <paramref name="features"/> or both).
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <param name="features">Features to union with <paramref name="self"/></param>
        /// <returns>The union of <paramref name="self"/> and <paramref name="features"/>
        /// (that is, the set of features in <paramref name="self"/> or <paramref name="features"/> or both).</returns>
        public static Feature Union(this Feature self, Feature features) => self | features;

        /// <summary>
        /// Computes the intersection of <paramref name="self"/> and <paramref name="features"/>
        /// (that is, the set of features present in both <paramref name="self"/> and <paramref name="features"/>).
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <param name="features">Features to intersect with <paramref name="self"/></param>
        /// <returns>The intersection of <paramref name="self"/> and <paramref name="features"/>
        /// (that is, the set of features common to both <paramref name="self"/> and <paramref name="features"/>).</returns>
        public static Feature Intersection(this Feature self, Feature features) => self & features;

        /// <summary>
        /// Computes the set difference (that is, removes all flags in <paramref name="features"/> from <paramref name="self"/>).
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <param name="features">Features to remove from <paramref name="self"/></param>
        /// <returns>The set difference of <paramref name="self"/> and <paramref name="features"/>
        /// (that is, all members of <paramref name="self"/> which do not belong to <paramref name="features"/>).</returns>
        public static Feature SetDifference(this Feature self, Feature features) => self & ~features;

        /// <summary>
        /// Computes the symmetric difference (that is,
        /// the set of all features that belong to exactly one of <paramref name="self"/> and <paramref name="features"/>,
        /// present in one but not both).
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <param name="features">Features with which to compute the symmetric difference against <paramref name="self"/></param>
        /// <returns>The symmetric difference of <paramref name="self"/> and <paramref name="features"/>
        /// (that is, the features that belong to <paramref name="self"/> or <paramref name="features"/>, but not both).</returns>
        public static Feature SymmetricDifference(this Feature self, Feature features) => self ^ features;

        /// <summary>
        /// Sets or removes one or more <see cref="Feature"/> flags.
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <param name="feature">The <see cref="Feature"/>(s) to set or remove.</param>
        /// <param name="enabled">If <c>true</c>, the flag(s) in <paramref name="feature"/> will be set. If <c>false</c>, it/they will be removed.</param>
        /// <returns><paramref name="self"/> with the <see cref="Feature"/>(s) set or removed according to the value of <paramref name="enabled"/>.</returns>
        public static Feature SetEnabled(this Feature self, Feature feature, bool enabled) => enabled ? self | feature : self & ~feature;

        /// <summary>
        /// Filters just the camera-related <see cref="Feature"/> flags.
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <returns>The camera-related <see cref="Feature"/> flags from <paramref name="self"/>.</returns>
        /// <seealso cref="Feature.AnyCamera"/>
        public static Feature Cameras(this Feature self) => self.Intersection(Feature.AnyCamera);

        /// <summary>
        /// Filters just the tracking-related <see cref="Feature"/> flags.
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <returns>The tracking-related <see cref="Feature"/> flags from <paramref name="self"/>.</returns>
        /// <seealso cref="Feature.AnyTrackingMode"/>
        public static Feature TrackingModes(this Feature self) => self.Intersection(Feature.AnyTrackingMode);

        /// <summary>
        /// Filters just the light estimation related <see cref="Feature"/> flags.
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <returns>The light estimation-related <see cref="Feature"/> flags from <paramref name="self"/>.</returns>
        public static Feature LightEstimation(this Feature self) => self.Intersection(Feature.AnyLightEstimation);

        /// <summary>
        /// Removes all camera and tracking-related <see cref="Feature"/> flags.
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <returns><paramref name="self"/> with camera and tracking-related bits removed.</returns>
        /// <seealso cref="Feature.AnyCamera"/>
        /// <seealso cref="Feature.AnyTrackingMode"/>
        public static Feature WithoutCameraOrTracking(this Feature self) => self.SetDifference(Feature.AnyCamera.Union(Feature.AnyTrackingMode));

        static Feature LowestBit(this Feature self)
        {
            return self & (self ^ (self - 1));
        }

        /// <summary>
        /// Generates a single string representing the list of enabled features separated by <paramref name="separator"/>, or a comma if not specified.
        /// </summary>
        /// <remarks>
        /// This method makes several heap allocations, generating garbage. It is intended for debugging purposes and
        /// should not be called frequently in a production application.
        /// </remarks>
        /// <param name="features">The <see cref="Feature"/> being extended.</param>
        /// <param name="separator">The string separator to insert between elements of the list, or ", " if omitted.</param>
        /// <returns>A string of <see cref="Feature"/>s separated by <paramref name="separator"/>. If none of the features are enabled, returns "(None)".</returns>
        public static string ToStringList(this Feature features, string separator = ", ")
        {
            var names = new List<string>();
            while (features != 0)
            {
                var feature = features.LowestBit();
                switch (feature)
                {
                    case Feature.WorldFacingCamera:
                        names.Add("World Facing Camera");
                        break;
                    case Feature.UserFacingCamera:
                        names.Add("User Facing Camera");
                        break;
                    case Feature.RotationOnly:
                        names.Add("Rotation Only");
                        break;
                    case Feature.PositionAndRotation:
                        names.Add("Rotation and Orientation");
                        break;
                    case Feature.FaceTracking:
                        names.Add("Face Tracking");
                        break;
                    case Feature.PlaneTracking:
                        names.Add("Plane Tracking");
                        break;
                    case Feature.ImageTracking:
                        names.Add("Image Tracking");
                        break;
                    case Feature.ObjectTracking:
                        names.Add("Object Tracking");
                        break;
                    case Feature.EnvironmentProbes:
                        names.Add("Environment Probes");
                        break;
                    case Feature.Body2D:
                        names.Add("2D Body Tracking");
                        break;
                    case Feature.Body3D:
                        names.Add("3D Body Tracking");
                        break;
                    case Feature.Body3DScaleEstimation:
                        names.Add("3D Body Scale Estimation");
                        break;
                    case Feature.PeopleOcclusionStencil:
                        names.Add("People Occlusion Stencil");
                        break;
                    case Feature.PeopleOcclusionDepth:
                        names.Add("People Occlusion Depth");
                        break;
                    case Feature.Collaboration:
                        names.Add("Collaboration");
                        break;
                    case Feature.AutoFocus:
                        names.Add("Auto-Focus");
                        break;
                    case Feature.LightEstimationAmbientIntensity:
                        names.Add("Light Estimation (Ambient Intensity)");
                        break;
                    case Feature.LightEstimationAmbientColor:
                        names.Add("Light Estimation (Ambient Color)");
                        break;
                    case Feature.LightEstimationAmbientSphericalHarmonics:
                        names.Add("Light Estimation (Spherical Harmonics)");
                        break;
                    case Feature.LightEstimationMainLightDirection:
                        names.Add("Light Estimation (Main Light Direction)");
                        break;
                    case Feature.LightEstimationMainLightIntensity:
                        names.Add("Light Estimation (Main Light Intensity)");
                        break;
                    case Feature.Raycast:
                        names.Add("Raycast");
                        break;
                    case Feature.Meshing:
                        names.Add("Meshing");
                        break;
                    case Feature.MeshClassification:
                        names.Add("Mesh Classification");
                        break;
                    case Feature.PointCloud:
                        names.Add("Point Cloud");
                        break;
                    case Feature.EnvironmentDepth:
                        names.Add("Environment Depth");
                        break;
                    case Feature.EnvironmentDepthTemporalSmoothing:
                        names.Add("Environment Depth Temporal Smoothing");
                        break;
                    default:
                        names.Add(feature.ToString());
                        break;
                }

                features &= (features - 1);
            }

            return names.Count > 0 ? string.Join(separator, names) : "(None)";
        }

        /// <summary>
        /// Calculates the number of enabled features in <paramref name="self"/>.
        /// </summary>
        /// <param name="self">The <see cref="Feature"/> being extended.</param>
        /// <returns>The number of enabled <see cref="Feature"/> flags.</returns>
        public static int Count(this Feature self)
        {
            int count = 0;
            ulong features = (ulong)self;
            while (features != 0)
            {
                ++count;
                // set lowest bit to zero
                features &= (features - 1);
            }

            return count;
        }
    }
}
