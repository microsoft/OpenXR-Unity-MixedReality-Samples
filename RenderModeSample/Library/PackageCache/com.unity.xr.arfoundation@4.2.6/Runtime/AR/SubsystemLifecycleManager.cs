using System;
using System.Collections.Generic;
using UnityEngine.XR.Management;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A base class for subsystems whose lifetime is managed by a <c>MonoBehaviour</c>.
    /// </summary>
    /// <typeparam name="TSubsystem">The [Subsystem](xref:UnityEngine.Subsystem) which provides this manager data.</typeparam>
    /// <typeparam name="TProvider">The [provider](xref:UnityEngine.SubsystemsImplementation.SubsystemProvider) associated with this subsystem.</typeparam>
    /// <typeparam name="TSubsystemDescriptor">The <c>SubsystemDescriptor</c> required to create the Subsystem.</typeparam>
    public class SubsystemLifecycleManager<TSubsystem, TSubsystemDescriptor, TProvider> : MonoBehaviour
        where TSubsystem : SubsystemWithProvider<TSubsystem, TSubsystemDescriptor, TProvider>, new()
        where TSubsystemDescriptor : SubsystemDescriptorWithProvider<TSubsystem, TProvider>
        where TProvider : SubsystemProvider<TSubsystem>
    {
        /// <summary>
        /// Get the <c>TSubsystem</c> whose lifetime this component manages.
        /// </summary>
        public TSubsystem subsystem { get; private set; }

        /// <summary>
        /// The descriptor for the subsystem.
        /// </summary>
        /// <value>
        /// The descriptor for the subsystem.
        /// </value>
        public TSubsystemDescriptor descriptor => subsystem?.subsystemDescriptor;

        /// <summary>
        /// Returns the active <c>TSubsystem</c> instance if present, otherwise returns null.
        /// </summary>
        /// <returns>The active subsystem instance, or `null` if there isn't one.</returns>
        protected TSubsystem GetActiveSubsystemInstance()
        {
            TSubsystem activeSubsystem = null;

            // Query the currently active loader for the created subsystem, if one exists.
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                XRLoader loader = XRGeneralSettings.Instance.Manager.activeLoader;
                if (loader != null)
                    activeSubsystem = loader.GetLoadedSubsystem<TSubsystem>();
            }

            if (activeSubsystem == null)
                Debug.LogWarningFormat($"No active {typeof(TSubsystem).FullName} is available. Please ensure that a " +
                                       "valid loader configuration exists in the XR project settings.");

            return activeSubsystem;
        }

        /// <summary>
        /// Called by derived classes to initialize the subsystem is initialized before use
        /// </summary>
        protected void EnsureSubsystemInstanceSet()
        {
            subsystem = GetActiveSubsystemInstance();
        }

        /// <summary>
        /// Creates the <c>TSubsystem</c>.
        /// </summary>
        protected virtual void OnEnable()
        {
            EnsureSubsystemInstanceSet();

            if (subsystem != null)
            {
                OnBeforeStart();

                // The derived class may disable the
                // component if it has invalid state
                if (enabled)
                {
                    subsystem.Start();
                    OnAfterStart();
                }
            }
        }

        /// <summary>
        /// Stops the <c>TSubsystem</c>.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (subsystem != null)
                subsystem.Stop();
        }

        /// <summary>
        /// Destroys the <c>TSubsystem</c>.
        /// </summary>
        protected virtual void OnDestroy()
        {
            subsystem = null;
        }

        /// <summary>
        /// Invoked after creating the subsystem and before calling Start on it.
        /// The <see cref="subsystem"/> is not <c>null</c>.
        /// </summary>
        protected virtual void OnBeforeStart()
        { }

        /// <summary>
        /// Invoked after calling Start on it the Subsystem.
        /// The <see cref="subsystem"/> is not <c>null</c>.
        /// </summary>
        protected virtual void OnAfterStart()
        { }

        static List<TSubsystemDescriptor> s_SubsystemDescriptors =
            new List<TSubsystemDescriptor>();

        static List<TSubsystem> s_SubsystemInstances =
            new List<TSubsystem>();
    }
}
