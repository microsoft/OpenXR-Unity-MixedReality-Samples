using System;

namespace Unity.Services.Core.Analytics
{
    /// <summary>
    /// Extensions of InitializationOptions for Analytics
    /// </summary>
    public static class AnalyticsOptionsExtensions
    {
        internal const string AnalyticsUserIdKey = "com.unity.services.core.analytics-user-id";

        /// <summary>
        /// Stores the user id for Analytics.
        /// </summary>
        /// <param name="self">The InitializationOptions object to modify</param>
        /// <param name="id">The custom user id for Analytics.</param>
        /// <exception cref="ArgumentException">Throws a <see cref="ArgumentException"/> if id is null or empty.</exception>
        /// <returns>
        /// Return <paramref name="self"/>.
        /// Fluent interface pattern to make it easier to chain set options operations.
        /// </returns>
        [Obsolete("SetAnalyticsUserId is deprecated. Please use UnityServices.ExternalUserId instead.", false)]
        public static InitializationOptions SetAnalyticsUserId(this InitializationOptions self, string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Analytics user id cannot be null or empty.", nameof(id));
            return self.SetOption(AnalyticsUserIdKey, id);
        }
    }
}
