using System;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Behavior that fires a callback when it is destroyed.
    /// </summary>
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.0/api/Unity.XR.CoreUtils.OnDestroyNotifier.html")]
    public class OnDestroyNotifier : MonoBehaviour
    {
        /// <summary>
        /// Called when this behavior is destroyed.
        /// </summary>
        public Action<OnDestroyNotifier> Destroyed { private get; set; }

        void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }
    }
}
