using System.Collections.Generic;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for List&lt;T&gt; objects
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Fill the list with default objects of type <typeparamref name="T"/>
        /// </summary>
        /// <param name="list">The list</param>
        /// <param name="count">The number of items to fill the list with</param>
        /// <typeparam name="T">The type of objects in this list</typeparam>
        /// <returns>The list that was filled</returns>
        public static List<T> Fill<T>(this List<T> list, int count)
            where T: new()
        {
            for (var i = 0; i < count; i++)
            {
                list.Add(new T());
            }

            return list;
        }

        /// <summary>
        /// Ensure that the capacity of this list is at least as large the given value
        /// </summary>
        /// <typeparam name="T">The list element type</typeparam>
        /// <param name="list">The list whose capacity will be ensured</param>
        /// <param name="capacity">The minimum number of elements the list storage must contain</param>
        public static void EnsureCapacity<T>(this List<T> list, int capacity)
        {
            if (list.Capacity < capacity)
                list.Capacity = capacity;
        }
    }
}
