using System.Collections.Generic;

using UnityEngine.XR.Management;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Manages the lifetime of the <c>XRInputSubsystem</c>. Add one of these to any <c>GameObject</c> in your scene
    /// if you want device pose information to be available. Read the input by using the <c>TrackedPoseDriver</c>
    /// </summary>
    [DefaultExecutionOrder(ARUpdateOrder.k_InputManager)]
    [DisallowMultipleComponent]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(ARInputManager) + ".html")]
    public sealed class ARInputManager : MonoBehaviour
    {
        /// <summary>
        /// Get the [`XRInputSubsystem`](https://docs.unity3d.com/ScriptReference/XR.XRInputSubsystem.html)
        /// whose lifetime this component manages.
        /// </summary>
        public XRInputSubsystem subsystem { get; private set; }

        void OnEnable()
        {
            subsystem = GetActiveSubsystemInstance();

            if (subsystem != null)
                subsystem.Start();
        }

        void OnDisable()
        {
            if (subsystem != null && subsystem.running)
                subsystem.Stop();
        }

        void OnDestroy()
        {
            subsystem = null;
        }

        XRInputSubsystem GetActiveSubsystemInstance()
        {
            XRInputSubsystem activeSubsystem = null;

            // Query the currently active loader for the created subsystem, if one exists.
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                XRLoader loader = XRGeneralSettings.Instance.Manager.activeLoader;
                if (loader != null)
                {
                    activeSubsystem = loader.GetLoadedSubsystem<XRInputSubsystem>();
                }
            }

            if (activeSubsystem == null)
            {
                Debug.LogWarning($"No active {typeof(XRInputSubsystem).FullName} is available. Please ensure that a " +
                    "valid loader configuration exists in the XR project settings.");
            }

            return activeSubsystem;
        }

        static List<XRInputSubsystemDescriptor> s_SubsystemDescriptors =
            new List<XRInputSubsystemDescriptor>();
    }
}
