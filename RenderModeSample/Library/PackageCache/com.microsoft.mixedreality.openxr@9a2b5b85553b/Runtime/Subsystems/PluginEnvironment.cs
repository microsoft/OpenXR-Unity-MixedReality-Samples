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
    //Must match PluginEnvironment in PluginEnvironment.h
    enum PluginEnvironment
    {
        unityVersion = 1 << 0,
        openXRPluginVersion = 1 << 1,
        mrOpenXRPluginVersion = 1 << 2,
        graphicsAPI = 1 << 3,
        sessionCreationResult = 1 << 4,
        appName = 1 << 5,
        appVersion = 1 << 6,
        appMode = 1 << 7,
        openXRRuntimeName = 1 << 8,
        openXRRuntimeVersion = 1 << 9,
        apiVersion = 1 << 10
    };


    internal class PluginEnvironmentSubsystem
    {
        private static bool m_initialized = false;
        internal static void InitializePlugin()
        {
            if (!m_initialized)
            {
                m_initialized = true;
                NativeLib.SetPluginEnvironment(PluginEnvironment.unityVersion, Application.unityVersion);
                NativeLib.SetPluginEnvironment(PluginEnvironment.openXRPluginVersion, OpenXRRuntime.pluginVersion);
                NativeLib.SetPluginEnvironment(PluginEnvironment.mrOpenXRPluginVersion, typeof(OpenXRContext).Assembly.GetName().Version.ToString());
                NativeLib.InitializePlugin();
            }
        }

        internal static void OnSessionCreated()
        {
            string appMode = "undefined";

#if UNITY_EDITOR
            appMode = "PlayMode";
#else
            appMode = "AppMode";
#endif
            NativeLib.SetPluginEnvironment(PluginEnvironment.appName, Application.productName);
            NativeLib.SetPluginEnvironment(PluginEnvironment.appVersion, Application.version);
            NativeLib.SetPluginEnvironment(PluginEnvironment.appMode, appMode);
            NativeLib.SetPluginEnvironment(PluginEnvironment.openXRRuntimeName, OpenXRRuntime.name);
            NativeLib.SetPluginEnvironment(PluginEnvironment.openXRRuntimeVersion, OpenXRRuntime.version);
            NativeLib.SetPluginEnvironment(PluginEnvironment.apiVersion, OpenXRRuntime.apiVersion);
        }
    }

}