using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utility methods for working with UnityEngine.Object types
    /// </summary>
    public static class UnityObjectUtils
    {
        /// <summary>
        /// Calls the proper Destroy method on an object based on if application is playing
        /// </summary>
        /// <param name="obj">Object to be destroyed</param>
        /// <param name="withUndo">Whether to record undo for the destroy action</param>
        public static void Destroy(UnityObject obj, bool withUndo = false)
        {
            if (Application.isPlaying)
            {
                UnityObject.Destroy(obj);
            }
#if UNITY_EDITOR
            else
            {
                if (withUndo)
                    Undo.DestroyObjectImmediate(obj);
                else
                    UnityObject.DestroyImmediate(obj);
            }
#endif
        }

        /// <summary>
        /// Takes a Unity Object and returns a component of the specified type if possible.
        /// This first checks if the object is already the given type. If not it checks whether it is a game object and gets the first component of the specified type.
        /// If it is component of a different type, it will try to return the first component on the same object of the specified type.
        /// </summary>
        /// <param name="objectIn">The Unity Object reference to convert</param>
        /// <typeparam name="T"> The type to convert to</typeparam>
        /// <returns> The component of the specified type, if found on the object. Otherwise returns null.</returns>
        public static T ConvertUnityObjectToType<T>(UnityObject objectIn) where T : class
        {
            var interfaceOut = objectIn as T;
            if (interfaceOut == null && objectIn != null)
            {
                var go = objectIn as GameObject;
                if (go != null)
                {
                    interfaceOut = go.GetComponent<T>();
                    return interfaceOut;
                }

                var comp = objectIn as Component;
                if (comp != null)
                    interfaceOut = comp.GetComponent<T>();
            }

            return interfaceOut;
        }

        /// <summary>
        /// Remove UnityObjects from a dictionary which have been destroyed
        /// </summary>
        /// <typeparam name="T">The specific type of UnityObject in the dictionary</typeparam>
        /// <param name="list">A dictionary of UnityObjects that may contain destroyed objects</param>
        public static void RemoveDestroyedObjects<T>(List<T> list) where T : UnityObject
        {
            var removeList = CollectionPool<List<T>, T>.GetCollection();
            foreach (var component in list)
            {
                if (component == null)
                    removeList.Add(component);
            }

            foreach (var entry in removeList)
            {
                list.Remove(entry);
            }

            CollectionPool<List<T>, T>.RecycleCollection(removeList);
        }

        /// <summary>
        /// Remove UnityObjects keys from a dictionary which have been destroyed
        /// </summary>
        /// <typeparam name="TKey">The specific type of UnityObject in the dictionary</typeparam>
        /// <typeparam name="TValue">The value type of the dictionary</typeparam>
        /// <param name="dictionary">A dictionary of UnityObjects that may contain destroyed objects</param>
        public static void RemoveDestroyedKeys<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : UnityObject
        {
            var removeList = CollectionPool<List<TKey>, TKey>.GetCollection();
            foreach (var kvp in dictionary)
            {
                var key = kvp.Key;
                if (key == null)
                    removeList.Add(key);
            }

            foreach (var key in removeList)
            {
                dictionary.Remove(key);
            }

            CollectionPool<List<TKey>, TKey>.RecycleCollection(removeList);
        }
    }
}
