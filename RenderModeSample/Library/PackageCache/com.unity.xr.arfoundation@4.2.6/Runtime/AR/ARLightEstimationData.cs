using System;
using UnityEngine.Rendering;

using StringBuilder = System.Text.StringBuilder;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A structure for light estimation information provided by the AR device.
    /// </summary>
    public struct ARLightEstimationData : IEquatable<ARLightEstimationData>
    {
        /// <summary>
        /// An estimate for the average brightness in the scene.
        /// Use <c>averageBrightness.HasValue</c> to determine if this information is available.
        /// </summary>
        /// <remarks>
        /// <see cref="averageBrightness"/> can be null when light estimation is not enabled in the <see cref="ARSession"/>,
        /// or if a platform-specific error has occurred.
        /// </remarks>
        public float? averageBrightness
        {
            get
            {
                if (m_AverageBrightness.HasValue)
                    return m_AverageBrightness;
                else if (m_AverageIntensityInLumens.HasValue)
                    return ConvertLumensToBrightness(m_AverageIntensityInLumens.Value);

                return null;
            }
            set => m_AverageBrightness = value;
        }

        /// <summary>
        /// An estimate for the average color temperature of the scene.
        /// Use <c>averageColorTemperature.HasValue</c> to determine if this information is available.
        /// </summary>
        /// <remarks>
        /// <see cref="averageColorTemperature"/> can be null when light estimation is not enabled in the <see cref="ARSession"/>,
        /// if the platform does not support it, or if a platform-specific error has occurred.
        /// </remarks>
        public float? averageColorTemperature { get; set; }

        /// <summary>
        /// The scaling factors used for color correction.
        /// The RGB scale factors are used to match the color of the light
        /// in the scene. The alpha channel value is platform-specific.
        /// </summary>
        /// <remarks>
        /// <see cref="colorCorrection"/> can be null when light estimation is not enabled in the <see cref="ARSession"/>,
        /// if the platform does not support it, or if a platform-specific error has occurred.
        /// </remarks>
        public Color? colorCorrection { get; set; }

        /// <summary>
        /// An estimate for the average intensity, in lumens, in the scene.
        /// Use <c>averageIntensityInLumens.HasValue</c> to determine if this information is available.
        /// </summary>
        /// <remarks>
        /// <see cref="averageIntensityInLumens"/> can be null when light estimation is not enabled in the <see cref="ARSession"/>,
        ///  or if a platform-specific error has occurred.
        /// </remarks>
        public float? averageIntensityInLumens
        {
            get
            {
                if (m_AverageIntensityInLumens.HasValue)
                    return m_AverageIntensityInLumens;
                else if (m_AverageBrightness.HasValue)
                    return ConvertBrightnessToLumens(m_AverageBrightness.Value);

                return null;
            }
            set => m_AverageIntensityInLumens = value;
        }

        /// <summary>
        /// An estimate of the intensity of the main light in lumens.
        /// </summary>
        /// <remarks>
        /// <see cref="mainLightIntensityLumens"/> can be null when light estimation is not enabled in the <see cref="ARSession"/>
        /// or a platform-specific error has occurred.
        /// </remarks>
        public float? mainLightIntensityLumens { get; set; }

        /// <summary>
        /// An estimate for the average brightness of the main light estimate in the scene.
        /// Use <c>averageMainLightBrightness.HasValue</c> to determine if this information is available.
        /// </summary>
        /// <remarks>
        /// <see cref="averageMainLightBrightness"/> can be null when light estimation is not enabled in the <see cref="ARSession"/>,
        /// or if a platform-specific error has occurred.
        /// </remarks>
        public float? averageMainLightBrightness
        {
            get 
            {
                if (m_MainLightBrightness.HasValue)
                    return m_MainLightBrightness;
                else if (mainLightIntensityLumens.HasValue)
                    return ConvertLumensToBrightness(mainLightIntensityLumens.Value);

                return null;
            }
            set => m_MainLightBrightness = value;
        }

        /// <summary>
        /// The estimated color of the main light.
        /// </summary>
        /// <remarks>
        /// <see cref="mainLightColor"/> can be null when light estimation is not enabled in the <see cref="ARSession"/>
        /// or a platform-specific error has occurred.
        /// </remarks>
        public Color? mainLightColor { get; set; }

        /// <summary>
        /// An estimate of where the main light of the scene is coming from.
        /// </summary>
        /// <remarks>
        /// <see cref="mainLightDirection"/> can be null when light estimation is not enabled in the <see cref="ARSession"/>
        /// or a platform-specific error has occurred.
        /// </remarks>
        public Vector3? mainLightDirection { get; set; }

        /// <summary>
        /// An estimation of the ambient scene lighting using spherical harmonics at the Level 2.
        /// </summary>
        /// <remarks>
        /// <see cref="ambientSphericalHarmonics"/> can be null when light estimation is not enabled in the <see cref="ARSession"/>
        /// or a platform-specific error has occurred.
        /// </remarks>
        public SphericalHarmonicsL2? ambientSphericalHarmonics { get; set; }

        /// <summary>
        /// Generates a hash code suitable for use in <c>HashSet</c> and <c>Dictionary</c>.
        /// </summary>
        /// <returns>A hash of the <see cref="ARLightEstimationData"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return
                    ((averageBrightness.GetHashCode() * 486187739 +
                    averageColorTemperature.GetHashCode()) * 486187739 +
                    colorCorrection.GetHashCode()) * 486187739 +
                    averageIntensityInLumens.GetHashCode() * 486187739 +
                    mainLightDirection.GetHashCode() * 486187739 +
                    mainLightColor.GetHashCode() * 486187739 +
                    mainLightIntensityLumens.GetHashCode() * 486187739 +
                    ambientSphericalHarmonics.GetHashCode() * 486187739;
            }
        }

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="obj">An <c>object</c> to compare against.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is an <see cref="ARLightEstimationData"/> and
        /// <see cref="Equals(ARLightEstimationData)"/> is also <c>true</c>. Otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ARLightEstimationData))
                return false;

            return Equals((ARLightEstimationData)obj);
        }

        /// <summary>
        /// Generates a string representation of this <see cref="ARLightEstimationData"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="ARLightEstimationData"/>.</returns>
        public override string ToString()
        {
            var sphericalHarmonicsCoefficientsStr = new StringBuilder("");

            if (ambientSphericalHarmonics.HasValue)
            {
                sphericalHarmonicsCoefficientsStr.Append("[");
                for (int i = 0; i < 3; ++i)
                {
                    for (int j = 0; j < 9; ++j)
                    {
                        sphericalHarmonicsCoefficientsStr.Append(i * 3 + j != 26 ? $"{ambientSphericalHarmonics.Value[i, j]}, " : $"{ambientSphericalHarmonics.Value[i, j]}]");
                    }
                }
            }

            return $"(Avg. Brightness: {averageBrightness}, Avg. Color Temperature: {averageColorTemperature}, " 
                + $"Color Correction: {colorCorrection}, Avg. Intensity in Lumens: {averageIntensityInLumens}, "
                + $"Est. Main Light Direction: {mainLightDirection}, Est. Main Light Channel Intensity: {mainLightColor}, "
                + $"Est. Main Light Intensity in Lumens: {mainLightIntensityLumens}, Est. Ambient Spherical Harmonics:\n{sphericalHarmonicsCoefficientsStr}";
        }

        /// <summary>
        /// Compares for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARLightEstimationData"/> to compare against.</param>
        /// <returns><c>true</c> if the <see cref="ARLightEstimationData"/> represents the same object.</returns>
        public bool Equals(ARLightEstimationData other)
        {
            return
                (averageBrightness == other.averageBrightness) &&
                (averageColorTemperature == other.averageColorTemperature) &&
                (colorCorrection == other.colorCorrection) &&
                (averageIntensityInLumens == other.averageIntensityInLumens) &&
                (mainLightColor == other.mainLightColor) &&
                (mainLightDirection == other.mainLightDirection) &&
                (mainLightIntensityLumens == other.mainLightIntensityLumens) &&
                (ambientSphericalHarmonics == other.ambientSphericalHarmonics);
        }

        /// <summary>
        /// Compares <paramref name="lhs"/> and <paramref name="rhs"/> for equality using <see cref="Equals(ARLightEstimationData)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand-side <see cref="ARLightEstimationData"/> of the comparison.</param>
        /// <param name="rhs">The right-hand-side <see cref="ARLightEstimationData"/> of the comparison.</param>
        /// <returns><c>true</c> if <paramref name="lhs"/> compares equal to <paramref name="rhs"/>, <c>false</c> otherwise.</returns>
        public static bool operator ==(ARLightEstimationData lhs, ARLightEstimationData rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Compares <paramref name="lhs"/> and <paramref name="rhs"/> for inequality using <see cref="Equals(ARLightEstimationData)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand-side <see cref="ARLightEstimationData"/> of the comparison.</param>
        /// <param name="rhs">The right-hand-side <see cref="ARLightEstimationData"/> of the comparison.</param>
        /// <returns><c>false</c> if <paramref name="lhs"/> compares equal to <paramref name="rhs"/>, <c>true</c> otherwise.</returns>
        public static bool operator !=(ARLightEstimationData lhs, ARLightEstimationData rhs) => !lhs.Equals(rhs);

        float ConvertBrightnessToLumens(float brightness)
        {
            return Mathf.Clamp(brightness*k_MaxLuminosity, 0f, k_MaxLuminosity);
        }

        float ConvertLumensToBrightness(float lumens)
        {
            return Mathf.Clamp(lumens/k_MaxLuminosity, 0f, 1f);
        }

        private float? m_AverageBrightness;
        private float? m_AverageIntensityInLumens;
        private float? m_MainLightBrightness;

        const float k_MaxLuminosity = 2000.0f;
    }
}
