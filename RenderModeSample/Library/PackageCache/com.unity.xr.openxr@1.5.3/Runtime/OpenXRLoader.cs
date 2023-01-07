#if UNITY_EDITOR && ENABLE_TEST_SUPPORT
#define TEST_SUPPORT
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Scripting;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.XR.OpenXR.Features;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.OpenXR;

#endif

[assembly: Preserve]

[assembly:InternalsVisibleTo("Unity.XR.OpenXR.TestHelpers")]
[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Tests")]
[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Tests.Editor")]
[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Editor")]
namespace UnityEngine.XR.OpenXR
{
    /// <summary>
    /// Loader for the OpenXR Plug-in. Used by [XR Plug-in Management](https://docs.unity3d.com/Packages/com.unity.xr.management@latest) to manage OpenXR lifecycle.
    /// </summary>
#if UNITY_EDITOR
    [XRSupportedBuildTarget(BuildTargetGroup.Standalone, new BuildTarget[] {BuildTarget.StandaloneWindows64})]
    [XRSupportedBuildTarget(BuildTargetGroup.Android)]
    [XRSupportedBuildTarget(BuildTargetGroup.WSA)]
#endif
    public class OpenXRLoader : OpenXRLoaderBase
#if UNITY_EDITOR
        , IXRLoaderPreInit
#endif
    {
#if UNITY_EDITOR
        public string GetPreInitLibraryName(BuildTarget buildTarget, BuildTargetGroup buildTargetGroup)
        {
            return "UnityOpenXR";
        }
#endif
    }

    /// <summary>
    /// Base abstract class to hold common loader code.
    /// </summary>
#if UNITY_EDITOR
    // Hide this by default from the UI by setting to Unknown.
    [XRSupportedBuildTarget(BuildTargetGroup.Unknown)]
#endif
    public partial class OpenXRLoaderBase : XRLoaderHelper
    {
        const double k_IdlePollingWaitTimeInSeconds = 0.1;
        private static List<XRDisplaySubsystemDescriptor> s_DisplaySubsystemDescriptors =
            new List<XRDisplaySubsystemDescriptor>();
        private static List<XRInputSubsystemDescriptor> s_InputSubsystemDescriptors =
            new List<XRInputSubsystemDescriptor>();

        /// <summary>
        /// Represents the running OpenXRLoader instance. This value should be non null after calling
        /// Initialize until a subsequent call to DeInitialize is made.
        /// </summary>
        internal static OpenXRLoaderBase Instance { get; private set; }

        internal enum LoaderState
        {
            Uninitialized,
            InitializeAttempted,
            Initialized,
            StartAttempted,
            Started,
            StopAttempted,
            Stopped,
            DeinitializeAttempted
        }

        internal LoaderState currentLoaderState { get; private set; } = LoaderState.Uninitialized;

        List<LoaderState> validLoaderInitStates = new List<LoaderState>{LoaderState.Uninitialized, LoaderState.InitializeAttempted};
        List<LoaderState> validLoaderStartStates = new List<LoaderState>{LoaderState.Initialized, LoaderState.StartAttempted, LoaderState.Stopped};
        List<LoaderState> validLoaderStopStates = new List<LoaderState>{LoaderState.StartAttempted, LoaderState.Started, LoaderState.StopAttempted};
        List<LoaderState> validLoaderDeinitStates = new List<LoaderState>{LoaderState.InitializeAttempted, LoaderState.Initialized, LoaderState.Stopped, LoaderState.DeinitializeAttempted};

        List<LoaderState> runningStates = new List<LoaderState>()
        {
            LoaderState.Initialized,
            LoaderState.StartAttempted,
            LoaderState.Started
        };

#if TEST_SUPPORT
        [NonSerialized]
        internal LoaderState targetLoaderState;

        bool ShouldExitEarly()
        {
            return (currentLoaderState == targetLoaderState);
        }
#endif

        OpenXRFeature.NativeEvent currentOpenXRState;
        private bool actionSetsAttached;

        /// <summary>
        /// Reference to the current display subsystem if the loader is initialized, or null if the loader is not initialized.
        /// </summary>
        internal XRDisplaySubsystem displaySubsystem => GetLoadedSubsystem<XRDisplaySubsystem>();

        /// <summary>
        /// Reference to the current input subsystem if the loader is initialized, or null if the loader is not initialized.
        /// </summary>
        internal XRInputSubsystem inputSubsystem => Instance?.GetLoadedSubsystem<XRInputSubsystem>();

        /// <summary>
        /// True if the loader has been initialized, false otherwise.
        /// </summary>
        private bool isInitialized =>
            currentLoaderState != LoaderState.Uninitialized &&
            currentLoaderState != LoaderState.DeinitializeAttempted;

        /// <summary>
        /// True if the loader has been started, false otherwise.
        /// </summary>
        private bool isStarted => runningStates.Contains(currentLoaderState);

        private UnhandledExceptionEventHandler unhandledExceptionHandler = null;

        internal bool DisableValidationChecksOnEnteringPlaymode = false;

        static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var section = DiagnosticReport.GetSection("Unhandled Exception Report");
            DiagnosticReport.AddSectionEntry(section, "Is Terminating", $"{args.IsTerminating}");

            var e = (Exception)args.ExceptionObject;

            DiagnosticReport.AddSectionEntry(section, "Message", $"{e.Message}");
            DiagnosticReport.AddSectionEntry(section, "Source", $"{e.Source}");
            DiagnosticReport.AddSectionEntry(section, "Stack Trace", $"\n{e.StackTrace}");

            DiagnosticReport.DumpReport("Uncaught Exception");
        }

        /// <summary>
        /// See [XRLoader.Initialize](xref:UnityEngine.XR.Management.XRLoader.Initialize)
        /// </summary>
        /// <returns>True if initialized, false otherwise.</returns>
        public override bool Initialize()
        {
            if (currentLoaderState == LoaderState.Initialized)
                return true;

            if (!validLoaderInitStates.Contains(currentLoaderState))
                return false;

            if (Instance != null)
            {
                Debug.LogError("Only one OpenXRLoader can be initialized at any given time");
                return false;
            }

#if UNITY_EDITOR
            if (!DisableValidationChecksOnEnteringPlaymode)
            {
                if (OpenXRProjectValidation.LogPlaymodeValidationIssues())
                    return false;
            }
#endif

            DiagnosticReport.StartReport();

            // Wrap the initialization in a try catch block to ensure if any exceptions are thrown that
            // we cleanup, otherwise the user will not be able to run again until they restart the editor.
            try
            {
                if (InitializeInternal())
                    return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Deinitialize();
            Instance = null;
            OpenXRAnalytics.SendInitializeEvent(false);

            return false;
        }

        private bool InitializeInternal ()
        {
            Instance = this;

            currentLoaderState = LoaderState.InitializeAttempted;

#if TEST_SUPPORT
            if (ShouldExitEarly()) return false;
#endif

            OpenXRInput.RegisterLayouts();

            OpenXRFeature.Initialize();

            if (!LoadOpenXRSymbols())
            {
                Debug.LogError("Failed to load openxr runtime loader.");
                return false;
            }

            // Sort the features array by priority in descending order (highest priority first)
            OpenXRSettings.Instance.features = OpenXRSettings.Instance.features
                .Where(f => f != null)
                .OrderByDescending(f => f.priority)
                .ThenBy(f => f.nameUi)
                .ToArray();

            OpenXRFeature.HookGetInstanceProcAddr();

            if (!Internal_InitializeSession())
                return false;

            SetApplicationInfo();
            RequestOpenXRFeatures();
            RegisterOpenXRCallbacks();

            if(null != OpenXRSettings.Instance)
                OpenXRSettings.Instance.ApplySettings();

            if (!CreateSubsystems())
                return false;

            if (OpenXRFeature.requiredFeatureFailed)
                return false;

            OpenXRAnalytics.SendInitializeEvent(true);

            OpenXRFeature.ReceiveLoaderEvent(this, OpenXRFeature.LoaderEvent.SubsystemCreate);

            DebugLogEnabledSpecExtensions();

            Application.onBeforeRender += ProcessOpenXRMessageLoop;
            currentLoaderState = LoaderState.Initialized;
            return true;
        }

        private bool CreateSubsystems()
        {
            // NOTE: This function is only necessary to handle subsystems being lost after domain reload. If that issue is fixed
            // at the management level the code below can be folded back into Initialize
            // NOTE: Below we check to see if a subsystem is already created before creating it. This is done because we currently
            // re-create the subsystems after a domain reload to fix a deficiency in XR Managements handling of domain reload. To
            // ensure we properly handle a fix to that deficiency we first check to make sure the subsystems are not already created.

            if (displaySubsystem == null)
            {
                CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "OpenXR Display");
                if (displaySubsystem == null)
                    return false;
            }

            if (inputSubsystem == null)
            {
                CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "OpenXR Input");
                if (inputSubsystem == null)
                    return false;
            }

            return true;
        }

        private double lastPollCheckTime = 0;

        internal void ProcessOpenXRMessageLoop()
        {
            if (currentOpenXRState == OpenXRFeature.NativeEvent.XrIdle ||
                currentOpenXRState == OpenXRFeature.NativeEvent.XrStopping ||
                currentOpenXRState == OpenXRFeature.NativeEvent.XrExiting ||
                currentOpenXRState == OpenXRFeature.NativeEvent.XrLossPending ||
                currentOpenXRState == OpenXRFeature.NativeEvent.XrInstanceLossPending)
            {
                var time = Time.realtimeSinceStartup;

                if ((time - lastPollCheckTime) < k_IdlePollingWaitTimeInSeconds)
                    return;

                lastPollCheckTime = time;
            }

            Internal_PumpMessageLoop();
        }

        /// <summary>
        /// See [XRLoader.Start](xref:UnityEngine.XR.Management.XRLoader.Start)
        /// </summary>
        /// <returns>True if started, false otherwise.</returns>
        public override bool Start()
        {
            if (currentLoaderState == LoaderState.Started)
                return true;

            if (!validLoaderStartStates.Contains(currentLoaderState))
                return false;

            currentLoaderState = LoaderState.StartAttempted;

#if TEST_SUPPORT
            if (ShouldExitEarly()) return false;
#endif

            if (!StartInternal())
            {
                Stop();
                return false;
            }

            currentLoaderState = LoaderState.Started;

            return true;
        }


        private bool StartInternal()
        {
            // In order to get XrReady, we have to at least attempt to create
            // the session if it isn't already there.
            if (!Internal_CreateSessionIfNeeded())
                return false;

            if (currentOpenXRState != OpenXRFeature.NativeEvent.XrReady ||
                (currentLoaderState != LoaderState.StartAttempted && currentLoaderState != LoaderState.Started))
            {
                return true;
            }

            // Note: Display has to be started before Input so that Input can have access to the Session object
            StartSubsystem<XRDisplaySubsystem>();
            if (!displaySubsystem?.running ?? false)
                return false;

            // calls xrBeginSession
            Internal_BeginSession();

            if (!actionSetsAttached)
            {
                OpenXRInput.AttachActionSets();
                actionSetsAttached = true;
            }

            if (!displaySubsystem?.running ?? false)
                StartSubsystem<XRDisplaySubsystem>();

            if (!inputSubsystem?.running ?? false)
                StartSubsystem<XRInputSubsystem>();

            var inputRunning = inputSubsystem?.running ?? false;
            var displayRunning = displaySubsystem?.running ?? false;

            if (inputRunning && displayRunning)
            {
                OpenXRFeature.ReceiveLoaderEvent(this, OpenXRFeature.LoaderEvent.SubsystemStart);
                return true;
            }

            return false;
        }

        /// <summary>
        /// See [XRLoader.Stop](xref:UnityEngine.XR.Management.XRLoader.Stop)
        /// </summary>
        /// <returns>True if stopped, false otherwise.</returns>
        public override bool Stop()
        {
            if (currentLoaderState == LoaderState.Stopped)
                return true;

            if (!validLoaderStopStates.Contains(currentLoaderState))
                return false;

            currentLoaderState = LoaderState.StopAttempted;

#if TEST_SUPPORT
            if (ShouldExitEarly()) return false;
#endif

            var inputRunning = inputSubsystem?.running ?? false;
            var displayRunning = displaySubsystem?.running ?? false;

            if (inputRunning || displayRunning)
                OpenXRFeature.ReceiveLoaderEvent(this, OpenXRFeature.LoaderEvent.SubsystemStop);

            if (inputRunning)
                StopSubsystem<XRInputSubsystem>();

            if (displayRunning)
                StopSubsystem<XRDisplaySubsystem>();
            StopInternal();

            currentLoaderState = LoaderState.Stopped;

            return true;
        }

        private void StopInternal()
        {
            Internal_EndSession();

            ProcessOpenXRMessageLoop();
        }

        /// <summary>
        /// See [XRLoader.DeInitialize](xref:UnityEngine.XR.Management.XRLoader.Stop)
        /// </summary>
        /// <returns>True if deinitialized, false otherwise.</returns>
        public override bool Deinitialize()
        {
            if (currentLoaderState == LoaderState.Uninitialized)
                return true;

            if (!validLoaderDeinitStates.Contains(currentLoaderState))
                return false;

            currentLoaderState = LoaderState.DeinitializeAttempted;

            try
            {
#if TEST_SUPPORT
                if (ShouldExitEarly()) return false;

                // The test hook above will leave the loader in a half initialized state.  To work
                // around this we reset the instance pointer if it is missing.
                if (Instance == null)
                    Instance = this;
#endif
                Internal_RequestExitSession();

                Application.onBeforeRender -= ProcessOpenXRMessageLoop;

                ProcessOpenXRMessageLoop(); // Drain any remaining events.

                OpenXRFeature.ReceiveLoaderEvent(this, OpenXRFeature.LoaderEvent.SubsystemDestroy);

                DestroySubsystem<XRInputSubsystem>();
                DestroySubsystem<XRDisplaySubsystem>();

                DiagnosticReport.DumpReport("System Shutdown");

                Internal_DestroySession();

                ProcessOpenXRMessageLoop();

                Internal_UnloadOpenXRLibrary();

                currentLoaderState = LoaderState.Uninitialized;
                actionSetsAttached = false;

                if (unhandledExceptionHandler != null)
                {
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    currentDomain.UnhandledException -= unhandledExceptionHandler;
                    unhandledExceptionHandler = null;
                }

                return base.Deinitialize();
            }
            finally
            {
                // Ensure we always clear the instance reference even if some part of Deinitialize threw an exception
                Instance = null;
            }
        }

        internal new void CreateSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id)
            where TDescriptor : ISubsystemDescriptor
            where TSubsystem : ISubsystem
        {
            base.CreateSubsystem<TDescriptor, TSubsystem>(descriptors, id);
        }

        internal new void StartSubsystem<T>() where T : class, ISubsystem => base.StartSubsystem<T>();

        internal new void StopSubsystem<T>() where T : class, ISubsystem => base.StopSubsystem<T>();

        internal new void DestroySubsystem<T>() where T : class, ISubsystem => base.DestroySubsystem<T>();

        private void SetApplicationInfo()
        {
            var md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(Application.version));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            uint applicationVersionHash = BitConverter.ToUInt32(data, 0);

            Internal_SetApplicationInfo(Application.productName, Application.version, applicationVersionHash, Application.unityVersion);
        }

        internal static byte[] StringToWCHAR_T(string s)
        {
            var encoding = Environment.OSVersion.Platform == PlatformID.Unix ? Encoding.UTF32 : Encoding.Unicode;
            return encoding.GetBytes(s + '\0');
        }

        private bool LoadOpenXRSymbols()
        {
            string loaderPath = "openxr_loader";

#if UNITY_EDITOR_WIN
            loaderPath ="..\\..\\..\\RuntimeLoaders\\windows\\x64\\openxr_loader";
#elif UNITY_EDITOR_OSX
            // no loader for osx, use the mock by default
            loaderPath = "../../../Tests/osx/x64/openxr_loader";
#endif

#if UNITY_EDITOR
            // Pass down active loader path to plugin
            EditorBuildSettings.TryGetConfigObject<UnityEngine.Object>(Constants.k_SettingsKey, out var obj);
            if (obj != null && (obj is IPackageSettings packageSettings))
            {
                var extensionLoaderPath = packageSettings.GetActiveLoaderLibraryPath();
                if (!String.IsNullOrEmpty(extensionLoaderPath))
                    loaderPath = extensionLoaderPath;
            }
#endif
            if (!Internal_LoadOpenXRLibrary(StringToWCHAR_T(loaderPath)))
                return false;

            return true;
        }

        private void RequestOpenXRFeatures()
        {
            var instance = OpenXRSettings.Instance;
            if (instance == null || instance.features == null)
                return;

            StringBuilder requestedLog = new StringBuilder("");
            StringBuilder failedLog = new StringBuilder("");
            uint count = 0;
            uint failedCount = 0;
            foreach (var feature in instance.features)
            {
                if (feature == null || !feature.enabled)
                    continue;

                ++count;

                requestedLog.Append($"  {feature.nameUi}: Version={feature.version}, Company=\"{feature.company}\"");

                if (!string.IsNullOrEmpty(feature.openxrExtensionStrings))
                {
                    requestedLog.Append($", Extensions=\"{feature.openxrExtensionStrings}\"");

                    // Check to see if any of the required extensions are not supported by the runtime
                    foreach (var extensionString in feature.openxrExtensionStrings.Split(' '))
                    {
                        if (string.IsNullOrWhiteSpace(extensionString)) continue;
                        if (!Internal_RequestEnableExtensionString(extensionString))
                        {
                            ++failedCount;
                            failedLog.Append($"  {extensionString}: Feature=\"{feature.nameUi}\": Version={feature.version}, Company=\"{feature.company}\"\n");
                        }
                    }
                }

                requestedLog.Append("\n");
            }

            var section = DiagnosticReport.GetSection("OpenXR Runtime Info");
            DiagnosticReport.AddSectionBreak(section);
            DiagnosticReport.AddSectionEntry(section, "Features requested to be enabled", $"({count})\n{requestedLog.ToString()}");
            DiagnosticReport.AddSectionBreak(section);
            DiagnosticReport.AddSectionEntry(section, "Requested feature extensions not supported by runtime", $"({failedCount})\n{failedLog.ToString()}");
        }

        private static void DebugLogEnabledSpecExtensions()
        {
            var section = DiagnosticReport.GetSection("OpenXR Runtime Info");
            DiagnosticReport.AddSectionBreak(section);

            var extensions = OpenXRRuntime.GetEnabledExtensions();
            var log = new StringBuilder($"({extensions.Length})\n");
            foreach(var extension in extensions)
                log.Append($"  {extension}: Version={OpenXRRuntime.GetExtensionVersion(extension)}\n");

            DiagnosticReport.AddSectionEntry(section, "Runtime extensions enabled", log.ToString());
        }

        [AOT.MonoPInvokeCallback(typeof(ReceiveNativeEventDelegate))]
        private static void ReceiveNativeEvent(OpenXRFeature.NativeEvent e, ulong payload)
        {
            var loader = Instance;

            if (loader != null) loader.currentOpenXRState = e;

            switch (e)
            {
                case OpenXRFeature.NativeEvent.XrRestartRequested:
                    OpenXRRestarter.Instance.ShutdownAndRestart();
                    break;

                case OpenXRFeature.NativeEvent.XrReady:
                    loader.StartInternal();
                    break;

                case OpenXRFeature.NativeEvent.XrFocused:
                    DiagnosticReport.DumpReport("System Startup Completed");
                    break;

                case OpenXRFeature.NativeEvent.XrRequestRestartLoop:
                    OpenXRRestarter.Instance.PauseAndRestart();
                    break;

                case OpenXRFeature.NativeEvent.XrStopping:
                    loader.StopInternal();
                    break;

                default:
                    break;
            }

            OpenXRFeature.ReceiveNativeEvent(e, payload);

            if((loader == null || !loader.isStarted) && e != OpenXRFeature.NativeEvent.XrInstanceChanged)
                return;

            switch (e)
            {
                case OpenXRFeature.NativeEvent.XrExiting:
                    OpenXRRestarter.Instance.Shutdown();
                    break;

                case OpenXRFeature.NativeEvent.XrLossPending:
                    OpenXRRestarter.Instance.ShutdownAndRestart();
                    break;

                case OpenXRFeature.NativeEvent.XrInstanceLossPending:
                    OpenXRRestarter.Instance.Shutdown();
                    break;
                default:
                    break;
            }
        }

        internal delegate void ReceiveNativeEventDelegate(OpenXRFeature.NativeEvent e, ulong payload);

        internal static void RegisterOpenXRCallbacks()
        {
            Internal_SetCallbacks(ReceiveNativeEvent);
        }

#if UNITY_EDITOR
        private void OnAfterAssemblyReload()
        {
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

            // Recreate the subsystems.  Note that post domain reload this is more about
            // repopulating the subsystem instance map than it is about actually creating subsystems. At this
            // point the domain reload is finished and the SubsystemManager has already patched
            // all of the subsystem interop pointers so we just need to go out and get them again.
            CreateSubsystems();
        }

        private void OnEnable()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;

            if (unhandledExceptionHandler != null)
            {
                currentDomain.UnhandledException -= unhandledExceptionHandler;
                unhandledExceptionHandler = null;
            }

            unhandledExceptionHandler = new UnhandledExceptionEventHandler(ExceptionHandler);
            currentDomain.UnhandledException += unhandledExceptionHandler;

            // If the loader is already initialized then this is likely due to a domain
            // reload so we need patch u the running instance reference.
            if (isInitialized && Instance == null)
            {
                Instance = this;

                // Recreate subsystems after all assemblies are finished loading.  This cannot be done here
                // because the SubsystemManager is handling domain reload itself, but is called after
                // this call to OnEnable but before the afterAssemblyReload callback.  The SubsystemManager will
                // reset the subsystem instance list on domain reload and thus if we create the subsystems now they
                // will be invalidated immediately after by the domain reload and the list will be out of sync.  By waiting
                // until the afterAssemblyReload we can ensure the list has been created first.
                AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
                // Re-register the callbacks with the plugin to reflect the new class instance
                RegisterOpenXRCallbacks();

                // Hook ourself back into onBeforeRender.  While the onBeforeRender should no longer contain our
                // message loop hook we will remove it first just to be extra safe.
                Application.onBeforeRender -= ProcessOpenXRMessageLoop;
                Application.onBeforeRender += ProcessOpenXRMessageLoop;
            }

        }
#endif
    }
}
