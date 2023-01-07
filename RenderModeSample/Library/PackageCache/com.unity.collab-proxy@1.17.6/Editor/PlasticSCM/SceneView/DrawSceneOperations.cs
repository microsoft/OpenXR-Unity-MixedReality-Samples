using System.Collections.Generic;

using UnityEngine;
using UnityEditor.VersionControl;

using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.AssetMenu;

namespace Unity.PlasticSCM.Editor.SceneView
{
    static class DrawSceneOperations
    {
        internal static void Enable(
            WorkspaceOperationsMonitor workspaceOperationsMonitor)
        {
            if (sIsEnabled)
                return;

            sWorkspaceOperationsMonitor = workspaceOperationsMonitor;
            sIsEnabled = true;

            Provider.preCheckoutCallback += Provider_preCheckoutCallback;
        }

        internal static void Disable()
        {
            sIsEnabled = false;
            sWorkspaceOperationsMonitor = null;

            Provider.preCheckoutCallback -= Provider_preCheckoutCallback;
        }

        static bool Provider_preCheckoutCallback(
            AssetList list,
            ref string changesetID,
            ref string changesetDescription)
        {
            if (!sIsEnabled)
                return true;

            if (!FindWorkspace.HasWorkspace(ApplicationDataPath.Get()))
            { 
                Disable();
                return true;
            }

            List<string> selectedPaths = GetSelectedPaths.ForOperation(
                list,
                PlasticPlugin.AssetStatusCache,
                AssetMenuOperations.Checkout);

            sWorkspaceOperationsMonitor.AddPathsToCheckout(selectedPaths);

            return true;
        }

        static bool sIsEnabled;
        static WorkspaceOperationsMonitor sWorkspaceOperationsMonitor;
    }
}
