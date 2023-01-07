using System;

using UnityEditor;
using UnityEngine;

using Codice.CM.Common;
using Unity.PlasticSCM.Editor.AssetMenu;
using Unity.PlasticSCM.Editor.AssetsOverlays;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.CollabMigration;
using Unity.PlasticSCM.Editor.Inspector;
using Unity.PlasticSCM.Editor.ProjectDownloader;
using Unity.PlasticSCM.Editor.SceneView;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor
{
    /// <summary>
    /// The Plastic SCM plugin for Unity editor.
    /// </summary>
    [InitializeOnLoad]
    public static class PlasticPlugin
    {
        /// <summary>
        /// Invoked when notification status changed.
        /// </summary>
        public static event Action OnNotificationUpdated = delegate { };

        internal static IAssetStatusCache AssetStatusCache 
        { 
            get { return sAssetStatusCache; } 
        }

        internal static WorkspaceOperationsMonitor WorkspaceOperationsMonitor 
        { 
            get { return sWorkspaceOperationsMonitor; } 
        }

        static PlasticPlugin()
        {
            CloudProjectDownloader.Initialize();
            MigrateCollabProject.Initialize();
            EditorDispatcher.Initialize();

            CooldownWindowDelayer cooldownInitializeAction = new CooldownWindowDelayer(
                Enable, UnityConstants.PLUGIN_DELAYED_INITIALIZE_INTERVAL);
            cooldownInitializeAction.Ping();
        }

        /// <summary>
        /// Get the plugin icon.
        /// </summary>
        public static Texture GetPluginIcon()
        {
            return PlasticNotification.GetIcon(sNotificationStatus);
        }

        internal static void Enable()
        {
            if (sIsEnabled)
                return;

            sIsEnabled = true;

            PlasticApp.InitializeIfNeeded();

            if (!FindWorkspace.HasWorkspace(ApplicationDataPath.Get()))
                return;

            EnableForWorkspace();
        }

        internal static void EnableForWorkspace()
        {
            if (sIsEnabledForWorkspace)
                return;

            WorkspaceInfo wkInfo = FindWorkspace.InfoForApplicationPath(
                ApplicationDataPath.Get(), PlasticGui.Plastic.API);

            if (wkInfo == null)
                return;

            sIsEnabledForWorkspace = true;

            PlasticApp.SetWorkspace(wkInfo);

            bool isGluonMode = PlasticGui.Plastic.API.IsGluonWorkspace(wkInfo);

            sAssetStatusCache = new AssetStatusCache(wkInfo, isGluonMode);

            PlasticAssetsProcessor plasticAssetsProcessor = new PlasticAssetsProcessor();

            sWorkspaceOperationsMonitor = BuildWorkspaceOperationsMonitor(
                plasticAssetsProcessor, isGluonMode);
            sWorkspaceOperationsMonitor.Start();

            AssetsProcessors.Enable(plasticAssetsProcessor, sAssetStatusCache);
            AssetMenuItems.Enable();
            DrawAssetOverlay.Enable();
            DrawInspectorOperations.Enable();
            DrawSceneOperations.Enable(sWorkspaceOperationsMonitor);
        }

        internal static void Disable()
        {
            try
            {
                PlasticApp.Dispose();

                if (!sIsEnabledForWorkspace)
                    return;

                sWorkspaceOperationsMonitor.Stop();

                AssetsProcessors.Disable();
                AssetMenuItems.Disable();
                DrawAssetOverlay.Disable();
                DrawInspectorOperations.Disable();
                DrawSceneOperations.Disable();
            }
            finally
            {
                sIsEnabled = false;
                sIsEnabledForWorkspace = false;
            }
        }

        internal static void SetNotificationStatus(
            PlasticWindow plasticWindow,
            PlasticNotification.Status status)
        {
            sNotificationStatus = status;

            plasticWindow.SetupWindowTitle(status);

            if (OnNotificationUpdated != null) 
                OnNotificationUpdated.Invoke();
        }

        static WorkspaceOperationsMonitor BuildWorkspaceOperationsMonitor(
            PlasticAssetsProcessor plasticAssetsProcessor,
            bool isGluonMode)
        {
            WorkspaceOperationsMonitor result = new WorkspaceOperationsMonitor(
                PlasticGui.Plastic.API, plasticAssetsProcessor, isGluonMode);
            plasticAssetsProcessor.SetWorkspaceOperationsMonitor(result);
            return result;
        }

        static PlasticNotification.Status sNotificationStatus;
        static AssetStatusCache sAssetStatusCache;
        static WorkspaceOperationsMonitor sWorkspaceOperationsMonitor;
        static bool sIsEnabled;
        static bool sIsEnabledForWorkspace;
    }
}
