using System.Collections.Generic;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for HashSet&lt;T&gt; objects
    /// </summary>
    public static class HashSetExtensions
    {
        /// <summary>
        /// Like HashSet.ExceptWith() but without any garbage allocation
        /// </summary>
        /// <param name="self">The set to remove from</param>
        /// <param name="other">The set of elements to remove</param>
        /// <typeparam name="T">The type contained in the set</typeparam>
        public static void ExceptWithNonAlloc<T>(this HashSet<T> self, HashSet<T> other)
        {
            foreach (var entry in other)
                self.Remove(entry);
        }

        /// <summary>
        /// Like LINQ's .First(), but does not allocate
        /// </summary>
        /// <param name="set">Set to retrieve the element from</param>
        /// <typeparam name="T">Type contained in the set</typeparam>
        /// <returns>The first element in the set</returns>
        public static T First<T>(this HashSet<T> set)
        {
            var enumerator = set.GetEnumerator();
            var value = enumerator.MoveNext() ? enumerator.Current : default;
            enumerator.Dispose();
            return value;
        }
    }
}
