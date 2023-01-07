using System;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Contains the parameters for creating a new <see cref="XRCameraSubsystemDescriptor"/>.
    /// </summary>
    public struct XRCameraSubsystemCinfo : IEquatable<XRCameraSubsystemCinfo>
    {
        /// <summary>
        /// Specifies an identifier for the provider implementation of the subsystem.
        /// </summary>
        /// <value>
        /// The identifier for the provider implementation of the subsystem.
        /// </value>
        public string id { get; set; }

        /// <summary>
        /// Specifies the provider implementation type to use for instantiation.
        /// </summary>
        /// <value>
        /// The provider implementation type to use for instantiation.
        /// </value>
        public Type providerType { get; set; }

        /// <summary>
        /// Specifies the <c>XRCameraSubsystem</c>-derived type that forwards casted calls to its provider.
        /// </summary>
        /// <value>
        /// The type of the subsystem to use for instantiation. If null, <c>XRCameraSubsystem</c> will be instantiated.
        /// </value>
        public Type subsystemTypeOverride { get; set; }

        /// <summary>
        /// Specifies the provider implementation type to use for instantiation.
        /// </summary>
        /// <value>
        /// The provider implementation type to use for instantiation.
        /// </value>
        [Obsolete("XRCameraSubsystem no longer supports the deprecated set of base classes for subsystems as of Unity 2020.2. Use providerType and, optionally, subsystemTypeOverride instead.", true)]
        public Type implementationType { get; set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide the average brightness.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide the average brightness. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsAverageBrightness { get; set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide the average camera temperature.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide the average camera temperature. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsAverageColorTemperature { get; set; }

        /// <summary>
        /// True if color correction is supported.
        /// </summary>
        public bool supportsColorCorrection { get; set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide a display matrix.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide a display matrix. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsDisplayMatrix { get; set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide a projection matrix.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide a projection matrix. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsProjectionMatrix { get; set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide a timestamp.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide a timestamp. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsTimestamp { get; set; }

        /// <summary>
        /// Specifies if the current subsystem supports camera configurations.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports camera configurations. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsCameraConfigurations { get; set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide camera images.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide camera images. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsCameraImage { get; set; }

        /// <summary>
        /// Specifies if current subsystem is allowed to provide the average intensity in lumens.
        /// </summary>
        /// <value>
        /// <c>true</c> if current subsystem is allowed to provide the average intensity in lumens. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsAverageIntensityInLumens { get; set; }

        /// <summary>
        /// Specifies whether the subsystem supports ambient intensity light estimation while face tracking.
        /// </summary>
        public bool supportsFaceTrackingAmbientIntensityLightEstimation { get; set; }

        /// <summary>
        /// Specifies whether the subsystem supports HDR light estimation while face tracking.
        /// </summary>
        public bool supportsFaceTrackingHDRLightEstimation { get; set; }

        /// <summary>
        /// Specifies whether the subsystem supports ambient intensity light estimation while world tracking.
        /// </summary>
        public bool supportsWorldTrackingAmbientIntensityLightEstimation { get; set; }

        /// <summary>
        /// Specifies whether the subsystem supports HDR light estimation while world tracking.
        /// </summary>
        public bool supportsWorldTrackingHDRLightEstimation { get; set; }

        /// <summary>
        /// Specifies whether the subsystem supports setting the camera's focus mode.
        /// </summary>
        public bool supportsFocusModes { get; set; }

        /// <summary>
        /// Specifies whether the subsystem supports a camera grain effect.
        /// </summary>
        public bool supportsCameraGrain { get; set; }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRCameraSubsystemCinfo"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRCameraSubsystemCinfo"/>, otherwise false.</returns>
        public bool Equals(XRCameraSubsystemCinfo other)
        {
            return
                ReferenceEquals(id, other.id)
                && ReferenceEquals(providerType, other.providerType)
                && ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride)
                && supportsAverageBrightness.Equals(other.supportsAverageBrightness)
                && supportsAverageColorTemperature.Equals(other.supportsAverageColorTemperature)
                && supportsColorCorrection.Equals(other.supportsColorCorrection)
                && supportsDisplayMatrix.Equals(other.supportsDisplayMatrix)
                && supportsProjectionMatrix.Equals(other.supportsProjectionMatrix)
                && supportsTimestamp.Equals(other.supportsTimestamp)
                && supportsCameraConfigurations.Equals(other.supportsCameraConfigurations)
                && supportsCameraImage.Equals(other.supportsCameraImage)
                && supportsAverageIntensityInLumens.Equals(other.supportsAverageIntensityInLumens)
                && supportsFaceTrackingAmbientIntensityLightEstimation.Equals(other.supportsFaceTrackingAmbientIntensityLightEstimation)
                && supportsFaceTrackingHDRLightEstimation.Equals(other.supportsFaceTrackingHDRLightEstimation)
                && supportsWorldTrackingAmbientIntensityLightEstimation.Equals(other.supportsWorldTrackingAmbientIntensityLightEstimation)
                && supportsWorldTrackingHDRLightEstimation.Equals(other.supportsWorldTrackingHDRLightEstimation)
                && supportsFocusModes.Equals(other.supportsFocusModes)
                && supportsCameraGrain.Equals(other.supportsCameraGrain);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRCameraSubsystemCinfo"/> and
        /// <see cref="Equals(XRCameraSubsystemCinfo)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj)
        {
            return ((obj is XRCameraSubsystemCinfo) && Equals((XRCameraSubsystemCinfo)obj));
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRCameraSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRCameraSubsystemCinfo lhs, XRCameraSubsystemCinfo rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRCameraSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRCameraSubsystemCinfo lhs, XRCameraSubsystemCinfo rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            int hashCode = 486187739;
            unchecked
            {
                hashCode = (hashCode * 486187739) + HashCodeUtil.ReferenceHash(id);
                hashCode = (hashCode * 486187739) + HashCodeUtil.ReferenceHash(providerType);
                hashCode = (hashCode * 486187739) + HashCodeUtil.ReferenceHash(subsystemTypeOverride);
                hashCode = (hashCode * 486187739) + supportsAverageBrightness.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsAverageColorTemperature.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsColorCorrection.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsDisplayMatrix.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsProjectionMatrix.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsTimestamp.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsCameraConfigurations.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsCameraImage.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsAverageIntensityInLumens.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsFaceTrackingAmbientIntensityLightEstimation.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsFaceTrackingHDRLightEstimation.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsWorldTrackingAmbientIntensityLightEstimation.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsWorldTrackingHDRLightEstimation.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsFocusModes.GetHashCode();
                hashCode = (hashCode * 486187739) + supportsCameraGrain.GetHashCode();
            }
            return hashCode;
        }
    }

    /// <summary>
    /// Specifies a functionality description that can be registered for each implementation that provides the
    /// <see cref="XRCameraSubsystem"/> interface.
    /// </summary>
    public sealed class XRCameraSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRCameraSubsystem, XRCameraSubsystem.Provider>
    {
        /// <summary>
        /// Constructs a <c>XRCameraSubsystemDescriptor</c> based on the given parameters.
        /// </summary>
        /// <param name="cameraSubsystemParams">The parameters required to initialize the descriptor.</param>
        XRCameraSubsystemDescriptor(XRCameraSubsystemCinfo cameraSubsystemParams)
        {
            id = cameraSubsystemParams.id;
            providerType = cameraSubsystemParams.providerType;
            subsystemTypeOverride = cameraSubsystemParams.subsystemTypeOverride;
            supportsAverageBrightness = cameraSubsystemParams.supportsAverageBrightness;
            supportsAverageColorTemperature = cameraSubsystemParams.supportsAverageColorTemperature;
            supportsColorCorrection = cameraSubsystemParams.supportsColorCorrection;
            supportsDisplayMatrix = cameraSubsystemParams.supportsDisplayMatrix;
            supportsProjectionMatrix = cameraSubsystemParams.supportsProjectionMatrix;
            supportsTimestamp = cameraSubsystemParams.supportsTimestamp;
            supportsCameraConfigurations = cameraSubsystemParams.supportsCameraConfigurations;
            supportsCameraImage = cameraSubsystemParams.supportsCameraImage;
            supportsAverageIntensityInLumens = cameraSubsystemParams.supportsAverageIntensityInLumens;
            supportsFocusModes = cameraSubsystemParams.supportsFocusModes;
            supportsFaceTrackingAmbientIntensityLightEstimation = cameraSubsystemParams.supportsFaceTrackingAmbientIntensityLightEstimation;
            supportsFaceTrackingHDRLightEstimation = cameraSubsystemParams.supportsFaceTrackingHDRLightEstimation;
            supportsWorldTrackingAmbientIntensityLightEstimation = cameraSubsystemParams.supportsWorldTrackingAmbientIntensityLightEstimation;
            supportsWorldTrackingHDRLightEstimation = cameraSubsystemParams.supportsWorldTrackingHDRLightEstimation;
            supportsCameraGrain = cameraSubsystemParams.supportsCameraGrain;
        }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide the average brightness.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide the average brightness. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsAverageBrightness { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide the average camera temperature.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide the average camera temperature. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsAverageColorTemperature { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide color correction.
        /// </summary>
        public bool supportsColorCorrection { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide a display matrix.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide a display matrix. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsDisplayMatrix { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide a projection matrix.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide a projection matrix. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsProjectionMatrix { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide the timestamp.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide the timestamp. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsTimestamp { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem supports camera configurations.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem supports camera configurations. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsCameraConfigurations { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide camera images.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide camera images. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsCameraImage { get; private set; }

        /// <summary>
        /// Specifies if the current subsystem is allowed to provide the average intensity in lumens.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current subsystem is allowed to provide the average intensity in lumens. Otherwise, <c>false</c>.
        /// </value>
        public bool supportsAverageIntensityInLumens { get; private set; }

        /// <summary>
        /// <c>True</c> if the subsystem supports setting the camera's focus mode.
        /// </summary>
        public bool supportsFocusModes { get; private set; }

        /// <summary>
        /// <c>True</c> if the subsystem supports ambient intensity light estimation while face tracking.
        /// </summary>
        public bool supportsFaceTrackingAmbientIntensityLightEstimation { get; private set; }

        /// <summary>
        /// <c>True</c> if the subsystem supports HDR light estimation while face tracking.
        /// </summary>
        public bool supportsFaceTrackingHDRLightEstimation { get; private set; }

        /// <summary>
        /// <c>True</c> if the subsystem supports ambient intensity light estimation while world tracking.
        /// </summary>
        public bool supportsWorldTrackingAmbientIntensityLightEstimation { get; private set; }

        /// <summary>
        /// <c>True</c> if the subsystem supports HDR light estimation while world tracking.
        /// </summary>
        public bool supportsWorldTrackingHDRLightEstimation { get; private set; }

        /// <summary>
        /// <c>True</c> if the subsystem supports the camera grain effect.
        /// </summary>
        public bool supportsCameraGrain { get; private set; }

        /// <summary>
        /// Creates a <c>XRCameraSubsystemDescriptor</c> based on the given parameters and validates that the
        /// <see cref="XRCameraSubsystemCinfo.id"/> and <see cref="XRCameraSubsystemCinfo.implementationType"/>
        /// properties are properly specified.
        /// </summary>
        /// <param name="cameraSubsystemParams">The parameters that define how to initialize the descriptor.</param>
        /// <returns>
        /// The created <c>XRCameraSubsystemDescriptor</c>.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the values specified in the
        /// <see cref="XRCameraSubsystemCinfo"/> parameter are invalid. Typically, this happens:
        /// <list type="bullet">
        /// <item>
        /// <description>If <see cref="XRCameraSubsystemCinfo.id"/> is <c>null</c> or empty.</description>
        /// </item>
        /// <item>
        /// <description>If <see cref="XRCameraSubsystemCinfo.implementationType"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description>If <see cref="XRCameraSubsystemCinfo.implementationType"/> does not derive from the
        /// <see cref="XRCameraSubsystem"/> class.
        /// </description>
        /// </item>
        /// </list>
        /// </exception>
        internal static XRCameraSubsystemDescriptor Create(XRCameraSubsystemCinfo cameraSubsystemParams)
        {
            if (String.IsNullOrEmpty(cameraSubsystemParams.id))
            {
                throw new ArgumentException("Cannot create camera subsystem descriptor because id is invalid",
                                            "cameraSubsystemParams");
            }

            if (cameraSubsystemParams.providerType == null
                || !cameraSubsystemParams.providerType.IsSubclassOf(typeof(XRCameraSubsystem.Provider)))
            {
                throw new ArgumentException("Cannot create camera subsystem descriptor because providerType is invalid", "cameraSubsystemParams");
            }

            if (cameraSubsystemParams.subsystemTypeOverride != null
                && !cameraSubsystemParams.subsystemTypeOverride.IsSubclassOf(typeof(XRCameraSubsystem)))
            {
                throw new ArgumentException("Cannot create camera subsystem descriptor because subsystemTypeOverride is invalid", "cameraSubsystemParams");
            }

            return new XRCameraSubsystemDescriptor(cameraSubsystemParams);
        }
    }
}
