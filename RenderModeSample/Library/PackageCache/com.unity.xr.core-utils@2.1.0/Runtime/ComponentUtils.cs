using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Special utility class for getting components in the editor without allocations
    /// </summary>
    /// <typeparam name="T">The type of component for which to be searched</typeparam>
    public static class ComponentUtils<T>
    {
        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<T> k_RetrievalList = new List<T>();

        /// <summary>
        /// Get a single component of type T using the non-allocating GetComponents API
        /// </summary>
        /// <param name="gameObject">The GameObject from which to get the component</param>
        /// <returns>The component, if one exists</returns>
        public static T GetComponent(GameObject gameObject)
        {
            var foundComponent = default(T);
            // k_RetrievalList is cleared by GetComponents
            gameObject.GetComponents(k_RetrievalList);
            if (k_RetrievalList.Count > 0)
                foundComponent = k_RetrievalList[0];

            return foundComponent;
        }

        /// <summary>
        /// Get a single component of type T using the non-allocating GetComponentsInChildren API
        /// </summary>
        /// <param name="gameObject">The GameObject from which to get the component</param>
        /// <returns>The component, if one exists</returns>
        public static T GetComponentInChildren(GameObject gameObject)
        {
            var foundComponent = default(T);
            // k_RetrievalList is cleared by GetComponentsInChildren
            gameObject.GetComponentsInChildren(k_RetrievalList);
            if (k_RetrievalList.Count > 0)
                foundComponent = k_RetrievalList[0];

            return foundComponent;
        }
    }

    /// <summary>
    /// Utility class for working with Components
    /// </summary>
    public static class ComponentUtils
    {
        /// <summary>
        /// Get a component from a GameObject, or add a new one and return it, if specified
        /// </summary>
        /// <param name="gameObject">The GameObject from which the component will be retrieved, or to which a new one will be added</param>
        /// <param name="add">Whether to add a new component of the given type, if one does not already exist</param>
        /// <typeparam name="T">The type of component to get or add</typeparam>
        /// <returns>The new or retrieved component</returns>
        public static T GetOrAddIf<T>(GameObject gameObject, bool add) where T : Component
        {
            var component = gameObject.GetComponent<T>();
#if UNITY_EDITOR
            if (add && component == null)
                component = Undo.AddComponent<T>(gameObject);
#else
            if (add && component == null)
                component = gameObject.AddComponent<T>();
#endif

            return component;
        }
    }
}
