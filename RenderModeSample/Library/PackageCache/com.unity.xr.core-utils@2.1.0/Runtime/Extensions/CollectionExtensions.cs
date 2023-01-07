using System.Collections.Generic;
using System.Text;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for ICollection objects
    /// </summary>
    public static class CollectionExtensions
    {
        static readonly StringBuilder k_String = new StringBuilder();

        /// <summary>
        /// Creates a comma separated string of all elements in the collection. Each collection element is implicitly converted
        /// to a string and added to the list.
        /// </summary>
        /// <param name="collection">The collection to create a string from</param>
        /// <typeparam name="T">The type of objects in the collection</typeparam>
        /// <returns>A string with all elements in the collection converted to strings and separated by commas.</returns>
        public static string Stringify<T>(this ICollection<T> collection)
        {
            k_String.Length = 0;
            var endIndex = collection.Count - 1;
            var counter = 0;
            foreach (var t in collection)
            {
                k_String.AppendFormat(counter++ == endIndex ? "{0}" : "{0}, ", t);
            }

            return k_String.ToString();
        }
    }
}
