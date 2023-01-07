using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the properties included in the camera frame.
    /// </summary>
    [Flags]
    public enum XRCameraFrameProperties
    {
        /// <summary>
        /// The timestamp of the frame is included.
        /// </summary>
        [Description("Timestamp")]
        Timestamp = (1 << 0),

        /// <summary>
        /// The average brightness of the frame is included.
        /// </summary>
        [Description("AverageBrightness")]
        AverageBrightness = (1 << 1),

        /// <summary>
        /// The average color temperature of the frame is included.
        /// </summary>
        [Description("AverageColorTemperature")]
        AverageColorTemperature = (1 << 2),

        /// <summary>
        /// The color correction value of the frame is included.
        /// </summary>
        [Description("ColorCorrection")]
        ColorCorrection = (1 << 3),

        /// <summary>
        /// The project matrix for the frame is included.
        /// </summary>
        [Description("ProjectionMatrix")]
        ProjectionMatrix = (1 << 4),

        /// <summary>
        /// The display matrix for the frame is included.
        /// </summary>
        [Description("DisplayMatrix")]
        DisplayMatrix = (1 << 5),

        /// <summary>
        /// The average intensity in lumens is included.
        /// </summary>
        [Description("AverageIntensityInLumens")]
        AverageIntensityInLumens = (1 << 6),

        /// <summary>
        /// The camera exposure duration is included.
        /// </summary>
        [Description("ExposureDuration")]
        ExposureDuration = (1 << 7),

        /// <summary>
        /// The camera exposure offset is included.
        /// </summary>
        [Description("ExposureOffset")]
        ExposureOffset = (1 << 8),

        /// <summary>
        /// The estimated scene main light direction is included.
        /// </summary>
        [Description("MainLightDirection")]
        MainLightDirection = (1 << 9),

        /// <summary>
        /// The estimated scene main light color is included.
        /// </summary>
        [Description("MainLightColor")]
        MainLightColor = (1 << 10),

        /// <summary>
        /// The estimated scene main light intensity in lumens is included.
        /// </summary>
        [Description("MainLightIntensityLumens")]
        MainLightIntensityLumens = (1 << 11),

        /// <summary>
        /// Ambient spherical harmonics are included.
        /// </summary>
        [Description("AmbientSphericalHarmonics")]
        AmbientSphericalHarmonics = (1 << 12),
        
        /// <summary>
        /// The camera grain texture is included.
        /// </summary>
        [Description("CameraGrain")]
        CameraGrain = (1 << 13),

        /// <summary>
        /// The camera grain noise intensity is included.
        /// </summary>
        [Description("NoiseIntensity")]
        NoiseIntensity = (1 << 14),
    }

    /// <summary>
    /// Parameters of the Unity <c>Camera</c> that might be necessary or useful to the provider.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRCameraFrame : IEquatable<XRCameraFrame>
    {
        /// <summary>
        /// The timestamp, in nanoseconds, associated with this frame.
        /// </summary>
        /// <value>
        /// The timestamp, in nanoseconds, associated with this frame.
        /// </value>
        public long timestampNs => m_TimestampNs;
        long m_TimestampNs;

        /// <summary>
        /// The estimated brightness of the scene.
        /// </summary>
        /// <value>
        /// The estimated brightness of the scene.
        /// </value>
        public float averageBrightness => m_AverageBrightness;
        float m_AverageBrightness;

        /// <summary>
        /// The estimated color temperature of the scene.
        /// </summary>
        /// <value>
        /// The estimated color temperature of the scene.
        /// </value>
        public float averageColorTemperature => m_AverageColorTemperature;
        float m_AverageColorTemperature;

        /// <summary>
        /// The estimated color correction value of the scene.
        /// </summary>
        /// <value>
        /// The estimated color correction value of the scene.
        /// </value>
        public Color colorCorrection => m_ColorCorrection;
        Color m_ColorCorrection;

        /// <summary>
        /// The 4x4 projection matrix for the camera frame.
        /// </summary>
        /// <value>
        /// The 4x4 projection matrix for the camera frame.
        /// </value>
        public Matrix4x4 projectionMatrix => m_ProjectionMatrix;
        Matrix4x4 m_ProjectionMatrix;

        /// <summary>
        /// The 4x4 display matrix for the camera frame.
        /// </summary>
        /// <value>
        /// The 4x4 display matrix for the camera frame.
        /// </value>
        public Matrix4x4 displayMatrix => m_DisplayMatrix;
        Matrix4x4 m_DisplayMatrix;

        /// <summary>
        /// The <see cref="TrackingState"/> associated with the camera.
        /// </summary>
        /// <value>
        /// The tracking state associated with the camera.
        /// </value>
        public TrackingState trackingState => m_TrackingState;
        TrackingState m_TrackingState;

        /// <summary>
        /// A native pointer associated with this frame. The data
        /// pointed to by this pointer is specific to provider implementation.
        /// </summary>
        /// <value>
        /// The native pointer associated with this frame.
        /// </value>
        public IntPtr nativePtr => m_NativePtr;
        IntPtr m_NativePtr;

        /// <summary>
        /// The set of all flags indicating which properties are included in the frame.
        /// </summary>
        /// <value>
        /// The set of all flags indicating which properties are included in the frame.
        /// </value>
        public XRCameraFrameProperties properties => m_Properties;
        XRCameraFrameProperties m_Properties;

        /// <summary>
        /// The estimated intensity, in lumens, of the scene.
        /// </summary>
        /// <value>
        /// The estimated intensity, in lumens, of the scene.
        /// </value>
        public float averageIntensityInLumens => m_AverageIntensityInLumens;
        float m_AverageIntensityInLumens;

        /// <summary>
        /// The camera exposure duration, in seconds with sub-millisecond precision, of the scene.
        /// </summary>
        /// <value>
        /// The camera exposure duration, in seconds with sub-millisecond precision, of the scene.
        /// </value>
        public double exposureDuration => m_ExposureDuration;
        double m_ExposureDuration;

        /// <summary>
        /// The camera exposure offset of the scene for lighting scaling.
        /// </summary>
        /// <value>
        /// The camera exposure offset of the scene for lighting scaling.
        /// </value>
        public float exposureOffset => m_ExposureOffset;
        float m_ExposureOffset;

        /// <summary>
        /// The estimated intensity in lumens of the most influential real-world light in the scene.
        /// </summary>
        /// <value>
        /// The estimated intensity in lumens of the most influential real-world light in the scene.
        /// </value>
        public float mainLightIntensityLumens => m_MainLightIntensityLumens;
        float m_MainLightIntensityLumens;

        /// <summary>
        /// The estimated color of the most influential real-world light in the scene.
        /// </summary>
        /// <value>
        /// The estimated color of the most influential real-world light in the scene.
        /// </value>
        public Color mainLightColor => m_MainLightColor;
        Color m_MainLightColor;

        /// <summary>
        /// The estimated direction of the most influential real-world light in the scene.
        /// </summary>
        /// <value>
        /// The estimated direction of the most influential real-world light in the scene.
        /// </value>
        public Vector3 mainLightDirection => m_MainLightDirection;
        Vector3 m_MainLightDirection;

        /// <summary>
        /// The ambient spherical harmonic coefficients that represent lighting in the real-world.
        /// </summary>
        /// <value>
        /// The ambient spherical harmonic coefficients that represent lighting in the real-world.
        /// </value>
        /// <remarks>
        /// See <see href="https://docs.unity3d.com/ScriptReference/Rendering.SphericalHarmonicsL2.html">Rendering.SphericalHarmonicsL2</see> for further details.
        /// </remarks>
        public SphericalHarmonicsL2 ambientSphericalHarmonics => m_AmbientSphericalHarmonics;
        SphericalHarmonicsL2 m_AmbientSphericalHarmonics;
        
        /// <summary>
        /// A texture that simulates the camera's noise.
        /// </summary>
        /// <value>
        /// A texture that simulates the camera's noise.
        /// </value>
        public XRTextureDescriptor cameraGrain => m_CameraGrain;
        XRTextureDescriptor m_CameraGrain;

        /// <summary>
        /// The level of intensity of camera grain noise in a scene.
        /// </summary>
        /// <value>
        /// The level of intensity of camera grain noise in a scene.
        /// </value>
        public float noiseIntensity => m_NoiseIntensity;
        float m_NoiseIntensity;

        /// <summary>
        /// <c>True</c> if the frame has a timestamp.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has a timestamp.
        /// </value>
        public bool hasTimestamp => (m_Properties & XRCameraFrameProperties.Timestamp) != 0;

        /// <summary>
        /// <c>True</c> if the frame has an average brightness.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has an average brightness.
        /// </value>
        public bool hasAverageBrightness => (m_Properties & XRCameraFrameProperties.AverageBrightness) != 0;

        /// <summary>
        /// <c>True</c> if the frame has an average color temperature.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has an average color temperature.
        /// </value>
        public bool hasAverageColorTemperature => (m_Properties & XRCameraFrameProperties.AverageColorTemperature) != 0;

        /// <summary>
        /// <c>True</c> if the frame has a color correction value.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has a color correction value.
        /// </value>
        public bool hasColorCorrection => (m_Properties & XRCameraFrameProperties.ColorCorrection) != 0;

        /// <summary>
        /// <c>True</c> if the frame has a projection matrix.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has a projection matrix.
        /// </value>
        public bool hasProjectionMatrix => (m_Properties & XRCameraFrameProperties.ProjectionMatrix) != 0;

        /// <summary>
        /// <c>True</c> if the frame has a display matrix.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has a display matrix.
        /// </value>
        public bool hasDisplayMatrix => (m_Properties & XRCameraFrameProperties.DisplayMatrix) != 0;

        /// <summary>
        /// <c>True</c> if the frame has an average intensity in lumens.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has an average intensity in lumens.
        /// </value>
        public bool hasAverageIntensityInLumens => (m_Properties & XRCameraFrameProperties.AverageIntensityInLumens) != 0;

        /// <summary>
        /// <c>True</c> if the frame has an exposure duration in seconds with sub-millisecond precision.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has an exposure duration in seconds with sub-millisecond precision.
        /// </value>
        public bool hasExposureDuration => (m_Properties & XRCameraFrameProperties.ExposureDuration) != 0;

        /// <summary>
        /// <c>True</c> if the frame has an exposure offset for scaling lighting.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has an exposure offset for scaling lighting.
        /// </value>
        public bool hasExposureOffset => (m_Properties & XRCameraFrameProperties.ExposureOffset) != 0;

        /// <summary>
        /// <c>True</c> if the frame has the estimated main light channel-wise intensity of the scene.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has the estimated main light channel-wise intensity of the scene.
        /// </value>
        public bool hasMainLightIntensityLumens => (m_Properties & XRCameraFrameProperties.MainLightIntensityLumens) != 0;

        /// <summary>
        /// <c>True</c> if the frame has the estimated main light color of the scene.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has the estimated main light color of the scene.
        /// </value>
        public bool hasMainLightColor => (m_Properties & XRCameraFrameProperties.MainLightColor) != 0;

        /// <summary>
        /// <c>True</c> if the frame has the estimated main light direction of the scene.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has the estimated main light direction of the scene.
        /// </value>
        public bool hasMainLightDirection => (m_Properties & XRCameraFrameProperties.MainLightDirection) != 0;

        /// <summary>
        /// <c>True</c> if the frame has the ambient spherical harmonics coefficients of the scene.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has the ambient spherical harmonics coefficients of the scene.
        /// </value>
        public bool hasAmbientSphericalHarmonics => (m_Properties & XRCameraFrameProperties.AmbientSphericalHarmonics) != 0;

        /// <summary>
        /// <c>True</c> if the frame has a camera grain texture.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has a camera grain texture.
        /// </value>
        public bool hasCameraGrain => (m_Properties & XRCameraFrameProperties.CameraGrain) != 0;

        /// <summary>
        /// <c>True</c> if the frame has a camera grain noise.
        /// </summary>
        /// <value>
        /// <c>True</c> if the frame has a camera grain noise.
        /// </value>
        public bool hasNoiseIntensity => (m_Properties & XRCameraFrameProperties.NoiseIntensity) != 0;

        /// <summary>
        /// Provides a timestamp of the camera frame.
        /// </summary>
        /// <param name="timestampNs">The timestamp of the camera frame.</param>
        /// <returns>
        /// <c>true</c> if the timestamp was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetTimestamp(out long timestampNs)
        {
            timestampNs = this.timestampNs;
            return hasTimestamp;
        }

        /// <summary>
        /// Provides the brightness for the whole image as an average of all pixels' brightness.
        /// </summary>
        /// <param name="averageBrightness">An estimated average brightness for the environment.</param>
        /// <returns>
        /// <c>true</c> if average brightness was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetAverageBrightness(out float averageBrightness)
        {
            averageBrightness = this.averageBrightness;
            return hasAverageBrightness;
        }

        /// <summary>
        /// Provides the color temperature for the whole image as an average of all pixels' color temperature.
        /// </summary>
        /// <param name="averageColorTemperature">An estimated color temperature.</param>
        /// <returns>
        /// <c>true</c> if average color temperature was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetAverageColorTemperature(out float averageColorTemperature)
        {
            averageColorTemperature = this.averageColorTemperature;
            return hasAverageColorTemperature;
        }

        /// <summary>
        /// Provides the projection matrix for the camera frame.
        /// </summary>
        /// <param name="projectionMatrix">The projection matrix used by the <c>XRCameraSubsystem</c>.</param>
        /// <returns>
        /// <c>true</c> if the projection matrix was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetProjectionMatrix(out Matrix4x4 projectionMatrix)
        {
            projectionMatrix = this.projectionMatrix;
            return this.hasProjectionMatrix;
        }

        /// <summary>
        /// Provides the display matrix defining how texture is being rendered on the screen.
        /// </summary>
        /// <param name="displayMatrix">The display matrix for rendering.</param>
        /// <returns>
        /// <c>true</c> if the display matrix was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetDisplayMatrix(out Matrix4x4 displayMatrix)
        {
            displayMatrix = this.displayMatrix;
            return hasDisplayMatrix;
        }

        /// <summary>
        /// Provides the intensity, in lumens, for the environment.
        /// </summary>
        /// <param name="averageIntensityInLumens">An estimated average intensity, in lumens, for the environment.</param>
        /// <returns>
        /// <c>true</c> if the average intensity was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetAverageIntensityInLumens(out float averageIntensityInLumens)
        {
            averageIntensityInLumens = this.averageIntensityInLumens;
            return hasAverageIntensityInLumens;
        }

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRCameraFrame"/> to compare against.</param>
        /// <returns><c>true</c> if the <see cref="XRCameraFrame"/> represents the same object.</returns>
        public bool Equals(XRCameraFrame other)
        {
            return (m_TimestampNs.Equals(other.m_TimestampNs) && m_AverageBrightness.Equals(other.m_AverageBrightness)
                    && m_AverageColorTemperature.Equals(other.m_AverageColorTemperature)
                    && m_ProjectionMatrix.Equals(other.m_ProjectionMatrix)
                    && m_DisplayMatrix.Equals(other.m_DisplayMatrix)
                    && m_AverageIntensityInLumens.Equals(other.m_AverageIntensityInLumens)
                    && m_ExposureDuration.Equals(other.m_ExposureDuration)
                    && m_ExposureOffset.Equals(other.m_ExposureOffset)
                    && m_MainLightDirection.Equals(other.m_MainLightDirection)
                    && m_MainLightIntensityLumens.Equals(other.m_MainLightIntensityLumens)
                    && m_MainLightColor.Equals(other.m_MainLightColor)
                    && m_AmbientSphericalHarmonics.Equals(other.m_AmbientSphericalHarmonics)
                    && m_CameraGrain.Equals(other.m_CameraGrain)
                    && m_NoiseIntensity.Equals(other.m_NoiseIntensity)
                    && (m_Properties == other.m_Properties));
        }

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="obj">An <c>object</c> to compare against.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is an <see cref="XRCameraFrame"/> and
        /// <see cref="Equals(XRCameraFrame)"/> is also <c>true</c>. Otherwise, <c>false</c>.</returns>
        public override bool Equals(System.Object obj)
        {
            return ((obj is XRCameraFrame) && Equals((XRCameraFrame)obj));
        }


        /// <summary>
        /// Compares <paramref name="lhs"/> and <paramref name="rhs"/> for equality using <see cref="Equals(XRCameraFrame)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand-side <see cref="XRCameraFrame"/> of the comparison.</param>
        /// <param name="rhs">The right-hand-side <see cref="XRCameraFrame"/> of the comparison.</param>
        /// <returns><c>true</c> if <paramref name="lhs"/> compares equal to <paramref name="rhs"/>, <c>false</c> otherwise.</returns>
        public static bool operator ==(XRCameraFrame lhs, XRCameraFrame rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Compares <paramref name="lhs"/> and <paramref name="rhs"/> for inequality using <see cref="Equals(XRCameraFrame)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand-side <see cref="XRCameraFrame"/> of the comparison.</param>
        /// <param name="rhs">The right-hand-side <see cref="XRCameraFrame"/> of the comparison.</param>
        /// <returns><c>false</c> if <paramref name="lhs"/> compares equal to <paramref name="rhs"/>, <c>true</c> otherwise.</returns>
        public static bool operator !=(XRCameraFrame lhs, XRCameraFrame rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Generates a hash code suitable for use in <c>HashSet</c> and <c>Dictionary</c>.
        /// </summary>
        /// <returns>A hash of the <see cref="XRCameraFrame"/>.</returns>
        public override int GetHashCode()
        {
            int hashCode = 486187739;
            unchecked
            {
                hashCode = (hashCode * 486187739) + m_TimestampNs.GetHashCode();
                hashCode = (hashCode * 486187739) + m_AverageBrightness.GetHashCode();
                hashCode = (hashCode * 486187739) + m_AverageColorTemperature.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ColorCorrection.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ProjectionMatrix.GetHashCode();
                hashCode = (hashCode * 486187739) + m_DisplayMatrix.GetHashCode();
                hashCode = (hashCode * 486187739) + m_AverageIntensityInLumens.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ExposureDuration.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ExposureOffset.GetHashCode();
                hashCode = (hashCode * 486187739) + m_MainLightDirection.GetHashCode();
                hashCode = (hashCode * 486187739) + m_MainLightColor.GetHashCode();
                hashCode = (hashCode * 486187739) + m_AmbientSphericalHarmonics.GetHashCode();
                hashCode = (hashCode * 486187739) + m_MainLightIntensityLumens.GetHashCode();
                hashCode = (hashCode * 486187739) + m_CameraGrain.GetHashCode();
                hashCode = (hashCode * 486187739) + m_NoiseIntensity.GetHashCode();
                hashCode = (hashCode * 486187739) + m_NativePtr.GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_Properties).GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Generates a string representation of this <see cref="XRCameraFrame"/> suitable for debugging purposes.
        /// </summary>
        /// <returns>A string representation of this <see cref="XRCameraFrame"/>.</returns>
        public override string ToString()
        {
            return $"properties:{m_Properties}\n   timestamp:{m_TimestampNs}ns\n   avgBrightness:{m_AverageBrightness.ToString("0.000")}\n"
                + $"   avgColorTemp:{m_AverageColorTemperature.ToString("0.000")}\n   colorCorrection:{m_ColorCorrection}\n"
                + $"   projection:\n{m_ProjectionMatrix.ToString("0.000")}\n   display:\n{m_DisplayMatrix.ToString("0.000")}\n"
                + $"   exposureDuration: {m_ExposureDuration.ToString("0.000")}sec\n   exposureOffset:{m_ExposureOffset}\n"
                + $"   mainLightDirection: {m_MainLightDirection.ToString("0.000")}\n   mainLightIntensityLumens: {m_MainLightIntensityLumens.ToString("0.000")}\n"
                + $"   MainLightColor: {m_MainLightColor.ToString("0.000")}\n   ambientSphericalHarmonics: \n{m_AmbientSphericalHarmonics}\n"
                + $"   nativePtr: {m_NativePtr.ToString("X16")}\n";
        }
    }
}
