using System;

namespace Unity.Services.Core.Environments
{
    /// <summary>
    /// Initialization option extensions related to environments.
    /// </summary>
    public static class EnvironmentsOptionsExtensions
    {
        internal const string EnvironmentNameKey = "com.unity.services.core.environment-name";

        /// <summary>
        /// An extension to set the environment to use.
        /// </summary>
        /// <param name="self">The InitializationOptions object to modify</param>
        /// <param name="environmentName">The name of the environment to use</param>
        /// <exception cref="ArgumentException">Throws a <see cref="ArgumentException"/> if environmentName is null or empty.</exception>
        /// <returns>
        /// Return <paramref name="self"/>.
        /// Fluent interface pattern to make it easier to chain set options operations.
        /// </returns>
        public static InitializationOptions SetEnvironmentName(this InitializationOptions self, string environmentName)
        {
            if (string.IsNullOrEmpty(environmentName))
                throw new ArgumentException("Environment name cannot be null or empty.", nameof(environmentName));

            self.SetOption(EnvironmentNameKey, environmentName);
            return self;
        }
    }
}
