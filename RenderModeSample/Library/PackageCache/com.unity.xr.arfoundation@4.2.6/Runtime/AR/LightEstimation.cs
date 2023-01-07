using System;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Defines types of light estimation.
    /// </summary>
    [Flags]
    public enum LightEstimation
    {
        /// <summary>
        /// No light estimation.
        /// </summary>
        None = 0,

        /// <summary>
        /// An estimate for the average ambient intensity of the environment.
        /// </summary>
        AmbientIntensity = 1 << 0,

        /// <summary>
        /// An estimate for the average ambient color of the environment.
        /// </summary>
        AmbientColor = 1 << 1,

        /// <summary>
        /// Estimate for the spherical harmonics representing the average ambient lighting of the environment.
        /// </summary>
        AmbientSphericalHarmonics = 1 << 2,

        /// <summary>
        /// An estimate for the direction of the main light in the environment.
        /// </summary>
        MainLightDirection = 1 << 3,

        /// <summary>
        /// An estimate for the intensity of the main light in the environment.
        /// </summary>
        MainLightIntensity = 1 << 4,
    }

    /// <summary>
    /// Extension methods for the <see cref="LightEstimation"/> enum.
    /// </summary>
    public static class LightEstimationExtensions
    {
        /// <summary>
        /// Converts a <see cref="LightEstimation"/> to a <c>Feature</c>.
        /// </summary>
        /// <param name="self">The <see cref="LightEstimation"/> being extended.</param>
        /// <returns>A <c>Feature</c> representation of the <see cref="LightEstimation"/> with the relevant bits enabled.</returns>
        public static Feature ToFeature(this LightEstimation self)
        {
            var feature = Feature.None;
            if ((self & LightEstimation.AmbientIntensity) != LightEstimation.None)
                feature |= Feature.LightEstimationAmbientIntensity;
            if ((self & LightEstimation.AmbientColor) != LightEstimation.None)
                feature |= Feature.LightEstimationAmbientColor;
            if ((self & LightEstimation.AmbientSphericalHarmonics) != LightEstimation.None)
                feature |= Feature.LightEstimationAmbientSphericalHarmonics;
            if ((self & LightEstimation.MainLightDirection) != LightEstimation.None)
                feature |= Feature.LightEstimationMainLightDirection;
            if ((self & LightEstimation.MainLightIntensity) != LightEstimation.None)
                feature |= Feature.LightEstimationMainLightIntensity;

            return feature;
        }

        /// <summary>
        /// Converts a <c>Feature</c> to a <see cref="LightEstimation"/> enum by extracting the
        /// relevant light estimation bits from <paramref name="self"/>.
        /// </summary>
        /// <param name="self">The <c>Feature</c> being extended.</param>
        /// <returns>A <see cref="LightEstimation"/> representation of the relevant <c>Feature</c> bits.</returns>
        public static LightEstimation ToLightEstimation(this Feature self)
        {
            var lightEstimation = LightEstimation.None;
            if ((self & Feature.LightEstimationAmbientIntensity) != Feature.None)
                lightEstimation |= LightEstimation.AmbientIntensity;
            if ((self & Feature.LightEstimationAmbientColor) != Feature.None)
                lightEstimation |= LightEstimation.AmbientColor;
            if ((self & Feature.LightEstimationAmbientSphericalHarmonics) != Feature.None)
                lightEstimation |= LightEstimation.AmbientSphericalHarmonics;
            if ((self & Feature.LightEstimationMainLightDirection) != Feature.None)
                lightEstimation |= LightEstimation.MainLightDirection;
            if ((self & Feature.LightEstimationMainLightIntensity) != Feature.None)
                lightEstimation |= LightEstimation.MainLightIntensity;

            return lightEstimation;
        }

        /// <summary>
        /// Convert the deprecated <c>LightEstimationMode</c> to <see cref="LightEstimation"/>.
        /// </summary>
        /// <param name="mode">The <c>LightEstimationMode</c> to convert.</param>
        /// <returns>The <see cref="LightEstimation"/> representation of the <c>LightEstimationMode</c>.</returns>
        public static LightEstimation ToLightEstimation(this LightEstimationMode mode)
        {
            switch (mode)
            {
                case LightEstimationMode.AmbientIntensity:
                    return LightEstimation.AmbientIntensity
                         | LightEstimation.AmbientColor;
                case LightEstimationMode.EnvironmentalHDR:
                    return LightEstimation.AmbientSphericalHarmonics
                         | LightEstimation.MainLightIntensity
                         | LightEstimation.MainLightDirection;
                default:
                    return LightEstimation.None;
            }
        }

        /// <summary>
        /// Converts a <see cref="LightEstimation"/> to the deprecated <c>LightEstimationMode</c>.
        /// </summary>
        /// <param name="self">The <see cref="LightEstimation"/> being converted.</param>
        /// <returns>A <c>LightEstimationMode</c> representation of the <see cref="LightEstimation"/>.</returns>
        public static LightEstimationMode ToLightEstimationMode(this LightEstimation self)
        {
            switch (self)
            {
                case LightEstimation.AmbientColor:
                case LightEstimation.AmbientIntensity:
                    return LightEstimationMode.AmbientIntensity;
                case LightEstimation.AmbientSphericalHarmonics:
                case LightEstimation.MainLightDirection:
                case LightEstimation.MainLightIntensity:
                    return LightEstimationMode.EnvironmentalHDR;
                default:
                    return LightEstimationMode.Disabled;
            }
        }
    }
}
