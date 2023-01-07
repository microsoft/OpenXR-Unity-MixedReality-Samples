using System;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Helper class for caching enum values
    /// </summary>
    /// <typeparam name="T">The enum type whose values should be cached</typeparam>
    public static class EnumValues<T>
    {
        /// <summary>
        /// Cached result of Enum.GetValues
        /// </summary>
        public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
    }
}
