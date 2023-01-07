using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for MonoBehaviour objects
    /// </summary>
    public static class MonoBehaviourExtensions
    {
#if UNITY_EDITOR
        /// <summary>
        /// Start this behaviour running in edit mode
        /// This sets runInEditMode to true, which, if the behaviour is enabled will call OnDisable and then OnEnable
        /// </summary>
        /// <param name="behaviour">The behaviour</param>
        public static void StartRunInEditMode(this MonoBehaviour behaviour)
        {
            behaviour.runInEditMode = true;
        }

        /// <summary>
        /// Stop this behaviour running in edit mode
        /// If the behaviour is enabled, we first disable it so that OnDisable is called. Then we set runInEditMode to false. Then, if the behaviour was enabled, we re-enable it
        /// </summary>
        /// <param name="behaviour">The behaviour</param>
        public static void StopRunInEditMode(this MonoBehaviour behaviour)
        {
            var wasEnabled = behaviour.enabled;
            if (wasEnabled)
                behaviour.enabled = false;

            behaviour.runInEditMode = false;

            if (wasEnabled)
                behaviour.enabled = true;
        }
#endif
    }
}
