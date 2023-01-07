// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace Microsoft.MixedReality.OpenXR
{
    internal abstract class OpenXRFeaturePlugin<TPlugin>
        : OpenXRFeature, IOpenXRContext, ISubsystemPlugin where TPlugin : OpenXRFeaturePlugin<TPlugin>
    {
        internal static readonly NativeLibToken nativeLibToken;

        private List<SubsystemController> m_subsystemControllers = new List<SubsystemController>();
        public ulong Instance { get; private set; } = 0;
        public ulong SystemId { get; private set; } = 0;
        public ulong Session { get; private set; } = 0;
        public bool IsSessionRunning { get; private set; } = false;
        public XrSessionState SessionState { get; private set; } = XrSessionState.Unknown;
        public ulong SceneOriginSpace { get; private set; } = 0;

        public event OpenXRContextEvent InstanceCreated;       // after instance is created
        public event OpenXRContextEvent InstanceDestroying;    // before instance is destroyed
        public event OpenXRContextEvent SessionCreated;        // after session is created
        public event OpenXRContextEvent SessionDestroying;     // before session is destroyed
        public event OpenXRContextEvent SessionBegun;          // after session is begun
        public event OpenXRContextEvent SessionEnding;         // before session is ended

        public IntPtr PFN_xrGetInstanceProcAddr
        {
            get
            {
                return Instance == 0
                    ? IntPtr.Zero
                    : OpenXRFeature.xrGetInstanceProcAddr;
            }
        }

        static OpenXRFeaturePlugin()
        {
            NativeLibTokenAttribute attribute = typeof(TPlugin).GetCustomAttributes(
                typeof(NativeLibTokenAttribute), inherit: false).FirstOrDefault() as NativeLibTokenAttribute;
            if (attribute == null)
            {
                Debug.LogError($"{typeof(TPlugin).Name} lacks NativeLibToken attribute");
                return;
            }
            nativeLibToken = attribute.NativeLibToken;
        }

        internal NativeLibToken NativeLibToken => nativeLibToken;

        protected override void OnEnable()
        {
            base.OnEnable();
            PluginEnvironmentSubsystem.InitializePlugin();
        }

        protected void AddSubsystemController(SubsystemController subsystemController)
        {
            m_subsystemControllers.Add(subsystemController);
        }

        private bool IsExtensionEnabled(string extensionName, uint minimumRevision = 1)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(extensionName))
                return false;

            return OpenXRRuntime.GetExtensionVersion(extensionName) >= minimumRevision;
        }

        protected override void OnSubsystemCreate()
        {
            m_subsystemControllers.ForEach(controller => controller.OnSubsystemCreate(this));
        }

        protected override void OnSubsystemStart()
        {
            NativeLib.OnSubsystemsStarting(nativeLibToken);
            m_subsystemControllers.ForEach(controller => controller.OnSubsystemStart(this));
        }

        protected override void OnSubsystemStop()
        {
            m_subsystemControllers.ForEach(controller => controller.OnSubsystemStop(this));
            NativeLib.OnSubsystemsStopped(nativeLibToken);
        }

        protected override void OnSubsystemDestroy()
        {
            m_subsystemControllers.ForEach(controller => controller.OnSubsystemDestroy(this));
        }

        protected override bool OnInstanceCreate(ulong instance)
        {
            if (Instance != 0)
            {
                Debug.LogWarning("New instance was created without properly destroying the previous one.");
            }

            Instance = instance;

            string[] enabledExtensionNames = OpenXRRuntime.GetEnabledExtensions();
            NativeLib.OnInstanceCreated(nativeLibToken, instance, PFN_xrGetInstanceProcAddr, enabledExtensionNames, enabledExtensionNames.Length);

            InstanceCreated?.Invoke(this, EventArgs.Empty);
            return true;
        }

        protected override void OnInstanceDestroy(ulong instance)
        {
            if (Instance == 0)
            {
                // Unity might call destroy when instance handle was not successfully created
                // Ignore such cases since there's no resources associated with instance of 0.
                return;
            }

            if (SystemId != 0)
            {
                // Unity's OnSystemChange event won't trigger when destroying instance.
                // Reset resources associated with system before destroying the instance.
                SystemId = 0;
                NativeLib.SetXrSystemId(nativeLibToken, 0);
            }

            InstanceDestroying?.Invoke(this, EventArgs.Empty);
            Instance = 0;
            NativeLib.OnInstanceDestroyed(nativeLibToken);
        }

        protected override void OnSystemChange(ulong systemId)
        {
            SystemId = systemId;
            NativeLib.SetXrSystemId(nativeLibToken, systemId);
        }

        protected override void OnSessionCreate(ulong session)
        {
            Session = session;
            if (nativeLibToken == NativeLibToken.HoloLens)
            {
                PluginEnvironmentSubsystem.OnSessionCreated();
            }

            NativeLib.SetXrSession(nativeLibToken, session);
            SessionCreated?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnSessionBegin(ulong session)
        {
            NativeLib.SetXrSessionRunning(nativeLibToken, true);
            SessionBegun?.Invoke(this, EventArgs.Empty);
            IsSessionRunning = true;
        }

        protected override void OnSessionStateChange(int oldState, int newState)
        {
            SessionState = (XrSessionState)newState;
            NativeLib.SetSessionState(nativeLibToken, (uint)newState);
        }

        protected override void OnSessionEnd(ulong session)
        {
            IsSessionRunning = false;
            SessionEnding?.Invoke(this, EventArgs.Empty);
            NativeLib.SetXrSessionRunning(nativeLibToken, false);
        }

        protected override void OnSessionDestroy(ulong session)
        {
            SessionDestroying?.Invoke(this, EventArgs.Empty);
            Session = 0;
            NativeLib.SetXrSession(nativeLibToken, 0);
        }

        protected override void OnAppSpaceChange(ulong sceneOriginSpace)
        {
            SceneOriginSpace = sceneOriginSpace;
            NativeLib.SetSceneOriginSpace(nativeLibToken, sceneOriginSpace);
        }

        void ISubsystemPlugin.CreateSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id) =>
            base.CreateSubsystem<TDescriptor, TSubsystem>(descriptors, id);

        void ISubsystemPlugin.StartSubsystem<T>() => base.StartSubsystem<T>();

        void ISubsystemPlugin.StopSubsystem<T>() => base.StopSubsystem<T>();

        void ISubsystemPlugin.DestroySubsystem<T>() => base.DestroySubsystem<T>();
    }
}