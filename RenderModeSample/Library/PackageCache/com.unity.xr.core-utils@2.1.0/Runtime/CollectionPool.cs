using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// A generic collection pool for avoiding allocations when new empty collections are needed frequently
    /// </summary>
    /// <typeparam name="TCollection">The desired type of collection</typeparam>
    /// <typeparam name="TValue">The value type of the ICollection specified in TCollection</typeparam>
    public static class CollectionPool<TCollection, TValue> where TCollection : ICollection<TValue>, new()
    {
        static readonly Queue<TCollection> k_CollectionQueue = new Queue<TCollection>();

        /// <summary>
        /// Get a collection of the given type from the pool, or create a new one if the pool is empty
        /// </summary>
        /// <returns>An empty collection</returns>
        public static TCollection GetCollection()
        {
            return k_CollectionQueue.Count > 0 ? k_CollectionQueue.Dequeue() : new TCollection();
        }

        /// <summary>
        /// Return a collection to the pool
        /// </summary>
        /// <param name="collection">The collection to be added to the pool</param>
        public static void RecycleCollection(TCollection collection)
        {
            collection.Clear();
            k_CollectionQueue.Enqueue(collection);
        }
    }
}
