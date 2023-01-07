using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.Serialization;
using UnityEngine.XR.Management;

[assembly: InternalsVisibleTo("Unity.XR.Management.Tests")]
[assembly: InternalsVisibleTo("Unity.XR.Management.EditorTests")]
namespace UnityEngine.XR.Management
{
    /// <summary>
    /// Class to handle active loader and subsystem management for XR. This class is to be added as a
    /// ScriptableObject asset in your project and should only be referenced by the <see cref="XRGeneralSettings"/>
    /// instance for its use.
    ///
    /// Given a list of loaders, it will attempt to load each loader in the given order. The first
    /// loader that is successful wins and all remaining loaders are ignored. The loader
    /// that succeeds is accessible through the <see cref="activeLoader"/> property on the manager.
    ///
    /// Depending on configuration the <see cref="XRManagerSettings"/> instance will automatically manage the active loader
    /// at correct points in the application lifecycle. The user can override certain points in the active loader lifecycle
    /// and manually manage them by toggling the <see cref="automaticLoading"/> and <see cref="automaticRunning"/>
    /// properties. Disabling <see cref="automaticLoading"/> implies the the user is responsible for the full lifecycle
    /// of the XR session normally handled by the <see cref="XRManagerSettings"/> instance. Toggling this to false also toggles
    /// <see cref="automaticRunning"/> false.
    ///
    /// Disabling <see cref="automaticRunning"/> only implies that the user is responsible for starting and stopping
    /// the <see cref="activeLoader"/> through the <see cref="StartSubsystems"/> and <see cref="StopSubsystems"/> APIs.
    ///
    /// Automatic lifecycle management is executed as follows
    ///
    /// * Runtime Initialize -> <see cref="InitializeLoader"/>. The loader list will be iterated over and the first successful loader will be set as the active loader.
    /// * Start -> <see cref="StartSubsystems"/>. Ask the active loader to start all subsystems.
    /// * OnDisable -> <see cref="StopSubsystems"/>. Ask the active loader to stop all subsystems.
    /// * OnDestroy -> <see cref="DeinitializeLoader"/>. Deinitialize and remove the active loader.
    /// </summary>
    public sealed class XRManagerSettings : ScriptableObject
    {
        [HideInInspector]
        bool m_InitializationComplete = false;

#pragma warning disable 414
        // This property is only used by the scriptable object editing part of the system and as such no one
        // directly references it. Have to manually disable the console warning here so that we can
        // get a clean console report.
        [HideInInspector]
        [SerializeField]
        bool m_RequiresSettingsUpdate = false;
#pragma warning restore 414

        [SerializeField]
        [Tooltip("Determines if the XR Manager instance is responsible for creating and destroying the appropriate loader instance.")]
        [FormerlySerializedAs("AutomaticLoading")]
        bool m_AutomaticLoading = false;

        /// <summary>
        /// Get and set Automatic Loading state for this manager. When this is true, the manager will automatically call
        /// <see cref="InitializeLoader"/> and <see cref="DeinitializeLoader"/> for you. When false <see cref="automaticRunning"/>
        /// is also set to false and remains that way. This means that disabling automatic loading disables all automatic behavior
        /// for the manager.
        /// </summary>
        public bool automaticLoading
        {
            get { return m_AutomaticLoading; }
            set { m_AutomaticLoading = value; }
        }

        [SerializeField]
        [Tooltip("Determines if the XR Manager instance is responsible for starting and stopping subsystems for the active loader instance.")]
        [FormerlySerializedAs("AutomaticRunning")]
        bool m_AutomaticRunning = false;

        /// <summary>
        /// Get and set automatic running state for this manager. When set to true the manager will call <see cref="StartSubsystems"/>
        /// and <see cref="StopSubsystems"/> APIs at appropriate times. When set to false, or when <see cref="automaticLoading"/> is false
        /// then it is up to the user of the manager to handle that same functionality.
        /// </summary>
        public bool automaticRunning
        {
            get { return m_AutomaticRunning; }
            set { m_AutomaticRunning = value; }
        }


        [SerializeField]
        [Tooltip("List of XR Loader instances arranged in desired load order.")]
        [FormerlySerializedAs("Loaders")]
        List<XRLoader> m_Loaders = new List<XRLoader>();

        // Maintains a list of registered loaders that is immutable at runtime.
        [SerializeField]
        [HideInInspector]
        HashSet<XRLoader> m_RegisteredLoaders = new HashSet<XRLoader>();

        /// <summary>
        /// List of loaders currently managed by this XR Manager instance.
        /// </summary>
        /// <remarks>
        /// Modifying the list of loaders at runtime is undefined behavior and could result in a crash or memory leak.
        /// Use <see cref="activeLoaders"/> to retrieve the currently ordered list of loaders. If you need to mutate
        /// the list at runtime, use <see cref="TryAddLoader"/>, <see cref="TryRemoveLoader"/>, and
        /// <see cref="TrySetLoaders"/>.
        /// </remarks>
        [Obsolete("'XRManagerSettings.loaders' property is obsolete. Use 'XRManagerSettings.activeLoaders' instead to get a list of the current loaders.")]
        public List<XRLoader> loaders
        {
            get { return m_Loaders; }
#if UNITY_EDITOR
            set { m_Loaders = value; }
#endif
        }

        /// <summary>
        /// A shallow copy of the list of loaders currently managed by this XR Manager instance.
        /// </summary>
        /// <remarks>
        /// This property returns a read only list. Any changes made to the list itself will not affect the list
        /// used by this XR Manager instance. To mutate the list of loaders currently managed by this instance,
        /// use <see cref="TryAddLoader"/>, <see cref="TryRemoveLoader"/>, and/or <see cref="TrySetLoaders"/>.
        /// </remarks>
        public IReadOnlyList<XRLoader> activeLoaders => m_Loaders;

        /// <summary>
        /// Read only boolean letting us know if initialization is completed. Because initialization is
        /// handled as a Coroutine, people taking advantage of the auto-lifecycle management of XRManager
        /// will need to wait for init to complete before checking for an ActiveLoader and calling StartSubsystems.
        /// </summary>
        public bool isInitializationComplete
        {
            get { return m_InitializationComplete; }
        }

        ///<summary>
        /// Return the current singleton active loader instance.
        ///</summary>
        [HideInInspector]
        public XRLoader activeLoader { get; private set; }

        /// <summary>
        /// Return the current active loader, cast to the requested type. Useful shortcut when you need
        /// to get the active loader as something less generic than XRLoader.
        /// </summary>
        ///
        /// <typeparam name="T">Requested type of the loader</typeparam>
        ///
        /// <returns>The active loader as requested type, or null.</returns>
        public T ActiveLoaderAs<T>() where T : XRLoader
        {
            return activeLoader as T;
        }

        /// <summary>
        /// Iterate over the configured list of loaders and attempt to initialize each one. The first one
        /// that succeeds is set as the active loader and initialization immediately terminates.
        ///
        /// When complete <see cref="isInitializationComplete"/> will be set to true. This will mark that it is safe to
        /// call other parts of the API. This does not guarantee that init successfully created a loader. For that
        /// you need to check that ActiveLoader is not null.
        ///
        /// Note that there can only be one active loader. Any attempt to initialize a new active loader with one
        /// already set will cause a warning to be logged and immediate exit of this function.
        ///
        /// This method is synchronous and on return all state should be immediately checkable.
        ///
        /// <b>If manual initialization of XR is being done, this method can not be called before Start completes
        /// as it depends on graphics initialization within Unity completing.</b>
        /// </summary>
        public void InitializeLoaderSync()
        {
            if (activeLoader != null)
            {
                Debug.LogWarning(
                    "XR Management has already initialized an active loader in this scene." +
                    " Please make sure to stop all subsystems and deinitialize the active loader before initializing a new one.");
                return;
            }

            foreach (var loader in currentLoaders)
            {
                if (loader != null)
                {
                    if (CheckGraphicsAPICompatibility(loader) && loader.Initialize())
                    {
                        activeLoader = loader;
                        m_InitializationComplete = true;
                        return;
                    }
                }
            }

            activeLoader = null;
        }

        /// <summary>
        /// Iterate over the configured list of loaders and attempt to initialize each one. The first one
        /// that succeeds is set as the active loader and initialization immediately terminates.
        ///
        /// When complete <see cref="isInitializationComplete"/> will be set to true. This will mark that it is safe to
        /// call other parts of the API. This does not guarantee that init successfully created a loader. For that
        /// you need to check that ActiveLoader is not null.
        ///
        /// Note that there can only be one active loader. Any attempt to initialize a new active loader with one
        /// already set will cause a warning to be logged and immediate exit of this function.
        ///
        /// Iteration is done asynchronously and this method must be called within the context of a Coroutine.
        ///
        /// <b>If manual initialization of XR is being done, this method can not be called before Start completes
        /// as it depends on graphics initialization within Unity completing.</b>
        /// </summary>
        ///
        /// <returns>Enumerator marking the next spot to continue execution at.</returns>
        public IEnumerator InitializeLoader()
        {
            if (activeLoader != null)
            {
                Debug.LogWarning(
                    "XR Management has already initialized an active loader in this scene." +
                    " Please make sure to stop all subsystems and deinitialize the active loader before initializing a new one.");
                yield break;
            }

            foreach (var loader in currentLoaders)
            {
                if (loader != null)
                {
                    if (CheckGraphicsAPICompatibility(loader) && loader.Initialize())
                    {
                        activeLoader = loader;
                        m_InitializationComplete = true;
                        yield break;
                    }
                }

                yield return null;
            }

            activeLoader = null;
        }

        /// <summary>
        /// Attempts to append the given loader to the list of loaders at the given index.
        /// </summary>
        /// <param name="loader">
        /// The <see cref="XRLoader"/> to be added to this manager's instance of loaders.
        /// </param>
        /// <param name="index">
        /// The index at which the given <see cref="XRLoader"/> should be added. If you set a negative or otherwise
        /// invalid index, the loader will be appended to the end of the list.
        /// </param>
        /// <returns>
        /// <c>true</c> if the loader is not a duplicate and was added to the list successfully, <c>false</c>
        /// otherwise.
        /// </returns>
        /// <remarks>
        /// This method behaves differently in the Editor and during runtime/Play mode. While your app runs in the Editor and not in
        /// Play mode, attempting to add an <see cref="XRLoader"/> will always succeed and register that loader's type
        /// internally. Attempting to add a loader during runtime/Play mode will trigger a check to see whether a loader of
        /// that type was registered. If the check is successful, the loader is added. If not, the loader is not added and the method
        /// returns <c>false</c>.
        /// </remarks>
        public bool TryAddLoader(XRLoader loader, int index = -1)
        {
            if (loader == null || currentLoaders.Contains(loader))
                return false;

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying && !m_RegisteredLoaders.Contains(loader))
                m_RegisteredLoaders.Add(loader);
#endif
            if (!m_RegisteredLoaders.Contains(loader))
                return false;

            if (index < 0 || index >= currentLoaders.Count)
                currentLoaders.Add(loader);
            else
                currentLoaders.Insert(index, loader);

            return true;
        }

        /// <summary>
        /// Attempts to remove the first instance of a given loader from the list of loaders.
        /// </summary>
        /// <param name="loader">
        /// The <see cref="XRLoader"/> to be removed from this manager's instance of loaders.
        /// </param>
        /// <returns>
        /// <c>true</c> if the loader was successfully removed from the list, <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        /// This method behaves differently in the Editor and during runtime/Play mode. During runtime/Play mode, the loader
        /// will be removed with no additional side effects if it is in the list managed by this instance. While in the
        /// Editor and not in Play mode, the loader will be removed if it exists and
        /// it will be unregistered from this instance and any attempts to add it during
        /// runtime/Play mode will fail. You can re-add the loader in the Editor while not in Play mode.
        /// </remarks>
        public bool TryRemoveLoader(XRLoader loader)
        {
            var removedLoader = true;
            if (currentLoaders.Contains(loader))
                removedLoader = currentLoaders.Remove(loader);

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying && !currentLoaders.Contains(loader))
                m_RegisteredLoaders.Remove(loader);
#endif

            return removedLoader;
        }

        /// <summary>
        /// Attempts to set the given loader list as the list of loaders managed by this instance.
        /// </summary>
        /// <param name="reorderedLoaders">
        /// The list of <see cref="XRLoader"/>s to be managed by this manager instance.
        /// </param>
        /// <returns>
        /// <c>true</c> if the loader list was set successfully, <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        /// This method behaves differently in the Editor and during runtime/Play mode. While in the Editor and not in
        /// Play mode, any attempts to set the list of loaders will succeed without any additional checks. During
        /// runtime/Play mode, the new loader list will be validated against the registered <see cref="XRLoader"/> types.
        /// If any loaders exist in the list that were not registered at startup, the attempt will fail.
        /// </remarks>
        public bool TrySetLoaders(List<XRLoader> reorderedLoaders)
        {
            var originalLoaders = new List<XRLoader>(activeLoaders);
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                registeredLoaders.Clear();
                currentLoaders.Clear();
                foreach (var loader in reorderedLoaders)
                {
                    if (!TryAddLoader(loader))
                    {
                        TrySetLoaders(originalLoaders);
                        return false;
                    }
                }

                return true;
            }
#endif
            currentLoaders.Clear();
            foreach (var loader in reorderedLoaders)
            {
                if (!TryAddLoader(loader))
                {
                    currentLoaders = originalLoaders;
                    return false;
                }
            }

            return true;
        }

        private bool CheckGraphicsAPICompatibility(XRLoader loader)
        {
            GraphicsDeviceType deviceType = SystemInfo.graphicsDeviceType;
            List<GraphicsDeviceType> supportedDeviceTypes = loader.GetSupportedGraphicsDeviceTypes(false);

            // To help with backward compatibility, if the compatibility list is empty we assume that it does not implement the GetSupportedGraphicsDeviceTypes method
            // Therefore we revert to the previous behavior of building or starting the loader regardless of gfx api settings.
            if (supportedDeviceTypes.Count > 0 && !supportedDeviceTypes.Contains(deviceType))
            {
                Debug.LogWarning(String.Format("The {0} does not support the initialized graphics device, {1}. Please change the preffered Graphics API in PlayerSettings. Attempting to start the next XR loader.", loader.name, deviceType.ToString()));
                return false;
            }

            return true;
        }

        /// <summary>
        /// If there is an active loader, this will request the loader to start all the subsystems that it
        /// is managing.
        ///
        /// You must wait for <see cref="isInitializationComplete"/> to be set to true prior to calling this API.
        /// </summary>
        public void StartSubsystems()
        {
            if (!m_InitializationComplete)
            {
                Debug.LogWarning(
                    "Call to StartSubsystems without an initialized manager." +
                    "Please make sure wait for initialization to complete before calling this API.");
                return;
            }

            if (activeLoader != null)
            {
                activeLoader.Start();
            }
        }

        /// <summary>
        /// If there is an active loader, this will request the loader to stop all the subsystems that it
        /// is managing.
        ///
        /// You must wait for <see cref="isInitializationComplete"/> to be set to tru prior to calling this API.
        /// </summary>
        public void StopSubsystems()
        {
            if (!m_InitializationComplete)
            {
                Debug.LogWarning(
                    "Call to StopSubsystems without an initialized manager." +
                    "Please make sure wait for initialization to complete before calling this API.");
                return;
            }

            if (activeLoader != null)
            {
                activeLoader.Stop();
            }
        }

        /// <summary>
        /// If there is an active loader, this function will deinitialize it and remove the active loader instance from
        /// management. We will automatically call <see cref="StopSubsystems"/> prior to deinitialization to make sure
        /// that things are cleaned up appropriately.
        ///
        /// You must wait for <see cref="isInitializationComplete"/> to be set to tru prior to calling this API.
        ///
        /// Upon return <see cref="isInitializationComplete"/> will be rest to false;
        /// </summary>
        public void DeinitializeLoader()
        {
            if (!m_InitializationComplete)
            {
                Debug.LogWarning(
                    "Call to DeinitializeLoader without an initialized manager." +
                    "Please make sure wait for initialization to complete before calling this API.");
                return;
            }

            StopSubsystems();
            if (activeLoader != null)
            {
                activeLoader.Deinitialize();
                activeLoader = null;
            }

            m_InitializationComplete = false;
        }

        // Use this for initialization
        void Start()
        {
            if (automaticLoading && automaticRunning)
            {
                StartSubsystems();
            }
        }

        void OnDisable()
        {
            if (automaticLoading && automaticRunning)
            {
                StopSubsystems();
            }
        }

        void OnDestroy()
        {
            if (automaticLoading)
            {
                DeinitializeLoader();
            }
        }

        // To modify the list of loaders internally use `currentLoaders` as it will return a list reference rather
        // than a shallow copy.
        // TODO @davidmo 10/12/2020: remove this in next major version bump and make 'loaders' internal.
        internal List<XRLoader> currentLoaders
        {
            get { return m_Loaders; }
            set { m_Loaders = value; }
        }

        // To modify the set of registered loaders use `registeredLoaders` as it will return a reference to the
        // hashset of loaders.
        internal HashSet<XRLoader> registeredLoaders
        {
            get { return m_RegisteredLoaders; }
        }
    }
}
