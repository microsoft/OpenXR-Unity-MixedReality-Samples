using System;
using Unity.Collections;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A default implementation of a <see cref="ConfigurationChooser"/>.
    /// </summary>
    public class DefaultConfigurationChooser : ConfigurationChooser
    {
        /// <summary>
        /// Selects a configuration from the given <paramref name="descriptors"/> and <paramref name="requestedFeatures"/>.
        /// </summary>
        /// <remarks>
        /// Selection works as follows:
        /// For each of the configuration <paramref name="descriptors"/>, compute the number of supported
        /// <see cref="Feature"/>s that are present in <paramref name="requestedFeatures"/> and choose the
        /// configuration descriptor with the highest count. <see cref="ConfigurationDescriptor.rank"/> is
        /// used to break ties.
        /// </remarks>
        /// <param name="descriptors">A set of <see cref="ConfigurationDescriptor"/>s supported by the <see cref="XRSessionSubsystem"/>.</param>
        /// <param name="requestedFeatures">A set of requested <see cref="Feature"/>s.</param>
        /// <returns>The configuration that best matches the <paramref name="requestedFeatures"/>.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="descriptors"/> does not contain any descriptors.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="requestedFeatures"/> contains more than one tracking mode.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="requestedFeatures"/> contains more than one camera mode.</exception>
        public override Configuration ChooseConfiguration(NativeSlice<ConfigurationDescriptor> descriptors, Feature requestedFeatures)
        {
            if (descriptors.Length == 0)
                throw new ArgumentException("No configuration descriptors to choose from.", nameof(descriptors));

            if (requestedFeatures.Intersection(Feature.AnyTrackingMode).Count() > 1)
                throw new ArgumentException($"Zero or one tracking mode must be requested. Requested tracking modes => {requestedFeatures.Intersection(Feature.AnyTrackingMode).ToStringList()}", nameof(requestedFeatures));

            if (requestedFeatures.Intersection(Feature.AnyCamera).Count() > 1)
                throw new ArgumentException($"Zero or one camera mode must be requested. Requested camera modes => {requestedFeatures.Intersection(Feature.AnyCamera).ToStringList()}", nameof(requestedFeatures));

            int highestFeatureCount = -1;
            int highestRank = int.MinValue;
            ConfigurationDescriptor bestDescriptor = default;
            foreach (var descriptor in descriptors)
            {
                int featureCount = requestedFeatures.Intersection(descriptor.capabilities).Count();
                if ((featureCount > highestFeatureCount) ||
                    (featureCount == highestFeatureCount && descriptor.rank > highestRank))
                {
                    highestFeatureCount = featureCount;
                    highestRank = descriptor.rank;
                    bestDescriptor = descriptor;
                }
            }

            return new Configuration(bestDescriptor, requestedFeatures.Intersection(bestDescriptor.capabilities));
        }
    }
}
