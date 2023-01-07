using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utility methods for creating GameObjects
    /// Allows systems to subscribe to <see cref="GameObjectInstantiated"/>
    /// </summary>
    public static class GameObjectUtils
    {
        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<GameObject> k_GameObjects = new List<GameObject>();
        static readonly List<Transform> k_Transforms = new List<Transform>();

        /// <summary>
        /// Called when a GameObject has been instantiated through GameObjectUtils.Instantiate
        /// </summary>
        public static event Action<GameObject> GameObjectInstantiated;

        /// <summary>
        /// Creates a new GameObject and returns it.
        /// This method also calls <see cref="GameObjectInstantiated"/>.
        /// </summary>
        /// <returns>The new GameObject</returns>
        public static GameObject Create()
        {
            var gameObject = new GameObject();
            GameObjectInstantiated?.Invoke(gameObject);
            return gameObject;
        }

        /// <summary>
        /// Creates a new GameObject and returns it.
        /// This method also calls <see cref="GameObjectInstantiated"/>.
        /// </summary>
        /// <param name="name">The name to be given to the new GameObject</param>
        /// <returns>The new GameObject</returns>
        public static GameObject Create(string name)
        {
            var gameObject = new GameObject(name);
            GameObjectInstantiated?.Invoke(gameObject);
            return gameObject;
        }

        /// <summary>
        /// Clones the GameObject <paramref name="original"/> and returns the clone.
        /// This method also calls <see cref="GameObjectInstantiated"/>.
        /// </summary>
        /// <param name="original">An existing GameObject that you want to make a copy of</param>
        /// <param name="parent">Parent that will be assigned to the new object</param>
        /// <param name="worldPositionStays">Whether the new object will stay at its original position, or use it as an offset from its parent</param>
        /// <returns>The instantiated clone</returns>
        public static GameObject Instantiate(GameObject original, Transform parent = null, bool worldPositionStays = true)
        {
            var gameObject = UnityObject.Instantiate(original, parent, worldPositionStays);
            if (gameObject != null && GameObjectInstantiated != null)
                GameObjectInstantiated(gameObject);

            return gameObject;
        }

        /// <summary>
        /// Clones the GameObject <paramref name="original"/> and returns the clone.
        /// This method also calls <see cref="GameObjectInstantiated"/>.
        /// </summary>
        /// <param name="original">An existing GameObject that you want to make a copy of</param>
        /// <param name="position">Position for the new object.</param>
        /// <param name="rotation">Orientation of the new object.</param>
        /// <returns>The instantiated clone</returns>
        public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation)
        {
            return Instantiate(original, null, position, rotation);
        }

        /// <summary>
        /// Clones the GameObject <paramref name="original"/> and returns the clone.
        /// This method also calls <see cref="GameObjectInstantiated"/>.
        /// </summary>
        /// <param name="original">An existing GameObject that you want to make a copy of</param>
        /// <param name="position">Position for the new object.</param>
        /// <param name="rotation">Orientation of the new object.</param>
        /// <param name="parent">Parent that will be assigned to the new object</param>
        /// <returns>The instantiated clone</returns>
        public static GameObject Instantiate(GameObject original, Transform parent, Vector3 position, Quaternion rotation)
        {
            var gameObject = UnityObject.Instantiate(original, position, rotation, parent);
            if (gameObject != null && GameObjectInstantiated != null)
                GameObjectInstantiated(gameObject);

            return gameObject;
        }

        /// <summary>
        /// Clones the Game Object <paramref name="original"/> and copies the hide flags of each Game Object
        /// in its hierarchy to the corresponding Game Object in the copy's hierarchy.
        /// </summary>
        /// <param name="original">The Game Object to make a copy of</param>
        /// <param name="parent">Optional parent that will be assigned to the clone of the original Game Object</param>
        /// <returns>The clone of the original Game Object</returns>
        public static GameObject CloneWithHideFlags(GameObject original, Transform parent = null)
        {
            var copy = UnityObject.Instantiate(original, parent);
            CopyHideFlagsRecursively(original, copy);
            return copy;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Clones the Prefab Game Object <paramref name="prefab"/> and copies the hide flags of each Game Object
        /// in its hierarchy to the corresponding Game Object in the copy's hierarchy.
        /// </summary>
        /// <param name="prefab">The Prefab Game Object to make a copy of</param>
        /// <param name="parent">Optional parent that will be assigned to the clone of the original Game Object</param>
        /// <returns>The clone of the original Game Object</returns>
        public static GameObject ClonePrefabWithHideFlags(GameObject prefab, Transform parent = null)
        {
            var copy = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            CopyHideFlagsRecursively(prefab, copy);
            return copy;
        }
#endif

        static void CopyHideFlagsRecursively(GameObject copyFrom, GameObject copyTo)
        {
            copyTo.hideFlags = copyFrom.hideFlags;
            var copyFromTransform = copyFrom.transform;
            var copyToTransform = copyTo.transform;
            for (var i = 0; i < copyFromTransform.childCount; ++i)
            {
                CopyHideFlagsRecursively(copyFromTransform.GetChild(i).gameObject, copyToTransform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Searches for a component in a scene with a 3 step process, getting more comprehensive with each step
        /// At edit time will find *all* objects in the scene, even if they are disabled
        /// At play time, will be unable to find disabled objects that are not a child of desiredSource
        /// </summary>
        /// <typeparam name="T">The type of component to find in the scene</typeparam>
        /// <param name="desiredSource">The Game Object we expect to be a parent or owner of the component</param>
        /// <returns>A component of the desired type, or NULL if no component was located</returns>
        public static T ExhaustiveComponentSearch<T>(GameObject desiredSource) where T : Component
        {
            var foundObject = default(T);

            // We check in the following order
            // - Location we expect the object to be
            // - The entire scene
            // - All loaded assets (Editor Only)
            if (desiredSource != null)
                foundObject = desiredSource.GetComponentInChildren<T>(true);

            if (foundObject == null)
                foundObject = UnityObject.FindObjectOfType<T>();

            if (foundObject != null)
                return foundObject;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var matchingObjects = Resources.FindObjectsOfTypeAll<T>();
                foreach (var possibleMatch in matchingObjects)
                {
                    if (!EditorUtility.IsPersistent(possibleMatch))
                    {
                        foundObject = possibleMatch;
                        break;
                    }
                }
            }
#endif
            return foundObject;
        }

        /// <summary>
        /// Searches for a component in a scene with a 3 step process, getting more comprehensive with each step
        /// At edit time will find *all* objects in the scene, even if they are disabled
        /// At play time, will be unable to find disabled objects that are not a child of desiredSource
        /// </summary>
        /// <typeparam name="T">The type of component to find in the scene</typeparam>
        /// <param name="desiredSource">The GameObject we expect to be a parent or owner of the component</param>
        /// <param name="tag">The tag this component must have to match</param>
        /// <returns>A component of the desired type, or NULL if no component was located</returns>
        public static T ExhaustiveTaggedComponentSearch<T>(GameObject desiredSource, string tag) where T : Component
        {
            var foundObject = default(T);

            // We check in the following order
            // - Location we expect the object to be
            // - The entire scene
            // - All loaded assets (Editor Only)
            if (desiredSource != null)
            {
                var matchingObjects = desiredSource.GetComponentsInChildren<T>(true);
                foreach (var possibleMatch in matchingObjects)
                {
                    if (possibleMatch.gameObject.CompareTag(tag))
                    {
                        foundObject = possibleMatch;
                        break;
                    }
                }
            }

            if (foundObject == null)
            {
                var matchingObjects = GameObject.FindGameObjectsWithTag(tag);
                foreach (var possibleMatch in matchingObjects)
                {
                    foundObject = possibleMatch.GetComponent<T>();
                    if (foundObject != null)
                    {
                        break;
                    }
                }
            }

            if (foundObject == null)
                foundObject = UnityObject.FindObjectOfType<T>();

#if UNITY_EDITOR
            if (foundObject == null && !Application.isPlaying)
            {
                var loadedMatchingObjects = Resources.FindObjectsOfTypeAll<T>();
                foreach (var possibleMatch in loadedMatchingObjects)
                {
                    if (!EditorUtility.IsPersistent(possibleMatch) && possibleMatch.gameObject.CompareTag(tag))
                    {
                        foundObject = possibleMatch;
                        break;
                    }
                }
            }
#endif
            return foundObject;
        }

        /// <summary>
        /// Retrieves the first component of the given type in a scene
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve</typeparam>
        /// <param name="scene">The scene to search</param>
        /// <returns>The first component found in the active scene, or null if none exists</returns>
        public static T GetComponentInScene<T>(Scene scene) where T : Component
        {
            // k_GameObjects is cleared by GetRootGameObjects
            scene.GetRootGameObjects(k_GameObjects);
            foreach (var gameObject in k_GameObjects)
            {
                var component = gameObject.GetComponentInChildren<T>();
                if (component)
                    return component;
            }

            return null;
        }

        /// <summary>
        /// Retrieves all components of the given type in a scene
        /// </summary>
        /// <typeparam name="T">The type of components to retrieve</typeparam>
        /// <param name="scene">The scene to search</param>
        /// <param name="components">List that will be filled out with components retrieved</param>
        /// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
        public static void GetComponentsInScene<T>(Scene scene, List<T> components, bool includeInactive = false) where T : Component
        {
            // k_GameObjects is cleared by GetRootGameObjects
            scene.GetRootGameObjects(k_GameObjects);
            foreach (var gameObject in k_GameObjects)
            {
                if (!includeInactive && !gameObject.activeInHierarchy)
                    continue;

                components.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive));
            }
        }

        /// <summary>
        /// Retrieves the first component of the given type in the active scene
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve</typeparam>
        /// <returns>The first component found in the active scene, or null if none exists</returns>
        public static T GetComponentInActiveScene<T>() where T : Component
        {
            return GetComponentInScene<T>(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// Retrieves all components of the given type in the active scene
        /// </summary>
        /// <typeparam name="T">The type of components to retrieve</typeparam>
        /// <param name="components">List that will be filled out with components retrieved</param>
        /// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
        public static void GetComponentsInActiveScene<T>(List<T> components, bool includeInactive = false) where T : Component
        {
            GetComponentsInScene(SceneManager.GetActiveScene(), components, includeInactive);
        }

        /// <summary>
        /// Retrieves all components of the given type in all loaded scenes
        /// </summary>
        /// <typeparam name="T">The type of components to retrieve</typeparam>
        /// <param name="components">List that will be filled out with components retrieved</param>
        /// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
        public static void GetComponentsInAllScenes<T>(List<T> components, bool includeInactive = false) where T : Component
        {
            var loadedSceneCount = SceneManager.sceneCount;
            for (var i = 0; i < loadedSceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                GetComponentsInScene(scene, components, includeInactive);
            }
        }

        /// <summary>
        /// Get the direct children GameObjects of this GameObject.
        /// </summary>
        /// <param name="go">The parent GameObject that we will want to get the child GameObjects on.</param>
        /// <param name="childGameObjects">The direct children of a GameObject.</param>
        public static void GetChildGameObjects(this GameObject go, List<GameObject> childGameObjects)
        {
            var goTransform = go.transform;
            var childCount = goTransform.childCount;
            if (childCount == 0)
                return;

            childGameObjects.EnsureCapacity(childCount);
            for (var i = 0; i < childCount; i++)
            {
                childGameObjects.Add(goTransform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Gets a descendant GameObject with a specific name
        /// </summary>
        /// <param name="go">The parent object that is searched for a named child.</param>
        /// <param name="name">Name of child to be found.</param>
        /// <returns>The returned child GameObject or null if no child is found.</returns>
        public static GameObject GetNamedChild(this GameObject go, string name)
        {
            k_Transforms.Clear();
            go.GetComponentsInChildren(k_Transforms);
            var foundObject = k_Transforms.Find(currentTransform => currentTransform.name == name);
            k_Transforms.Clear();

            if (foundObject != null)
                return foundObject.gameObject;

            return null;
        }
    }
}
