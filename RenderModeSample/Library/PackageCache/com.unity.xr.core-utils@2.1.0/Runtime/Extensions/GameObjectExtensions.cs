using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extensions methods for GameObjects
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Sets the hide flags on this GameObject and all of its descendants
        /// </summary>
        /// <param name="gameObject">The GameObject at the root of the hierarchy that will be modified</param>
        /// <param name="hideFlags">Should the GameObjects be hidden, saved with the scene or modifiable by the user?</param>
        public static void SetHideFlagsRecursively(this GameObject gameObject, HideFlags hideFlags)
        {
            gameObject.hideFlags = hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetHideFlagsRecursively(hideFlags);
            }
        }

        /// <summary>
        /// Adds <paramref name="hideFlags"/> to the hide flags on this GameObject and all of its descendants
        /// </summary>
        /// <param name="gameObject">The GameObject at the root of the hierarchy that will be modified</param>
        /// <param name="hideFlags">Should the GameObjects be hidden, saved with the scene or modifiable by the user?</param>
        public static void AddToHideFlagsRecursively(this GameObject gameObject, HideFlags hideFlags)
        {
            gameObject.hideFlags |= hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.AddToHideFlagsRecursively(hideFlags);
            }
        }

        /// <summary>
        /// Sets the layer of this GameObject and all of its descendants
        /// </summary>
        /// <param name="gameObject">The GameObject at the root of the hierarchy that will be modified</param>
        /// <param name="layer">The layer to recursively assign GameObjects to</param>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Sets the layer of this GameObject and adds to its HideFlags, and does the same for all of its descendants
        /// </summary>
        /// <param name="gameObject">The GameObject at the root of the hierarchy that will be modified</param>
        /// <param name="layer">The layer to recursively assign GameObjects to</param>
        /// <param name="hideFlags">Should the GameObjects be hidden, saved with the scene, or modifiable by the user?</param>
        public static void SetLayerAndAddToHideFlagsRecursively(this GameObject gameObject, int layer, HideFlags hideFlags)
        {
            gameObject.layer = layer;
            gameObject.hideFlags |= hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerAndAddToHideFlagsRecursively(layer, hideFlags);
            }
        }

        /// <summary>
        /// Sets the layer and HideFlags of this GameObject and all of its descendants
        /// </summary>
        /// <param name="gameObject">The GameObject at the root of the hierarchy that will be modified</param>
        /// <param name="layer">The layer to recursively assign GameObjects to</param>
        /// <param name="hideFlags">Should the GameObjects be hidden, saved with the scene, or modifiable by the user?</param>
        public static void SetLayerAndHideFlagsRecursively(this GameObject gameObject, int layer, HideFlags hideFlags)
        {
            gameObject.layer = layer;
            gameObject.hideFlags = hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerAndHideFlagsRecursively(layer, hideFlags);
            }
        }

        /// <summary>
        /// Set runInEditMode on all MonoBehaviours on this GameObject and its children
        /// </summary>
        /// <param name="gameObject">The GameObject at the root of the hierarchy that will be modified</param>
        /// <param name="enabled">The value to which runInEditMode will be assigned</param>
        public static void SetRunInEditModeRecursively(this GameObject gameObject, bool enabled)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
            foreach (var mb in monoBehaviours)
            {
                if (mb)
                {
                    if(enabled)
                        mb.StartRunInEditMode();
                    else
                        mb.StopRunInEditMode();
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetRunInEditModeRecursively(enabled);
            }
#endif
        }
    }
}
