using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.XR.Management;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;


namespace UnityEditor.XR.Management.Legacy
{

    [InitializeOnLoad]
    class XRLegacyUninstaller
    {
        private static List<string> k_PackagesToBeRemoved = new List<string>(){
            "com.unity.xr.googlevr.android",
            "com.unity.xr.googlevr.ios",
            "com.unity.xr.oculus.android",
            "com.unity.xr.oculus.standalone",
            "com.unity.xr.openvr.standalone",
            "com.unity.xr.windowsmr.metro",
        };


        [Serializable]
        struct PackageRemovalRequest
        {
            [SerializeField]
            public string packageId;
            [SerializeField]
            public bool shouldCheckForInstallation;
        }

        [Serializable]
        struct PackageRemovalOperation
        {
            [SerializeField]
            public string packageId;

            [SerializeField]
            public RemoveRequest packageRemoveRequest;
            [SerializeField]
            public bool shouldCheckForInstallation;
        }

        [Serializable]
        struct LegacyPackageListRequest
        {
            [SerializeField]
            public ListRequest packageListRequest;
        }


        private static readonly string k_PackageToRemoveQueue = "Remove Package Queue";
        private static EditorWorkQueue<PackageRemovalOperation> s_PackageRemovelQueue => EditorWorkQueue<PackageRemovalOperation>.Instance;
        private static bool hasPackageBeingRemoved => s_PackageRemovelQueue.HasWorkItems;

        private static readonly string k_WaitingToRemoveQueue = "Waiting Remove Queue";
        private static EditorWorkQueue<PackageRemovalRequest> s_PackagesToRemoveQueue => EditorWorkQueue<PackageRemovalRequest>.Instance;
        private static bool hasPackagesToRemove => s_PackagesToRemoveQueue.HasWorkItems;

        private static readonly string k_LocalPackageListingQueryQueue = "Local Package List";
        private static EditorWorkQueue<LegacyPackageListRequest> s_PackageListQueue => EditorWorkQueue<LegacyPackageListRequest>.Instance;
        private static bool hasActivePackageListQuery => EditorWorkQueueBase.SessionStateHasStoredData(k_LocalPackageListingQueryQueue);


        internal static bool IsPackageInstalled(string package)
        {
            return File.Exists($"Packages/{package}/package.json");
        }

        static XRLegacyUninstaller()
        {
            s_PackageRemovelQueue.QueueName = k_PackageToRemoveQueue;
            s_PackageRemovelQueue.ProcessItemCallback += (PackageRemovalOperation prr) =>
            {

                if (prr.packageRemoveRequest == null || String.IsNullOrEmpty(prr.packageRemoveRequest.PackageIdOrName))
                {
                    return;
                }

                if (prr.packageRemoveRequest.Status == StatusCode.Failure)
                {
                    return;
                }

                if (!prr.packageRemoveRequest.IsCompleted || (prr.shouldCheckForInstallation && IsPackageInstalled(prr.packageId)))
                {
                    s_PackageRemovelQueue.QueueWorkItem(prr);
                    return;
                }


            };
            if (s_PackageRemovelQueue.HasWorkItems) s_PackageRemovelQueue.StartQueue();

            s_PackagesToRemoveQueue.QueueName = k_WaitingToRemoveQueue;
            s_PackagesToRemoveQueue.ProcessItemCallback += (PackageRemovalRequest prr) =>
            {
                if (s_PackageRemovelQueue.HasWorkItems)
                {
                    s_PackagesToRemoveQueue.QueueWorkItem(prr);
                    return;
                }

                if (!String.IsNullOrEmpty(prr.packageId) && IsPackageInstalled(prr.packageId))
                {
                    PackageRemovalOperation pro = new PackageRemovalOperation { packageId = prr.packageId, packageRemoveRequest = PackageManager.Client.Remove(prr.packageId), shouldCheckForInstallation=prr.shouldCheckForInstallation };
                    s_PackageRemovelQueue.QueueWorkItem(pro);
                }
            };
            if (s_PackagesToRemoveQueue.HasWorkItems) s_PackagesToRemoveQueue.StartQueue();

            var packageSettings = XRPackageInitializationSettings.Instance;
            if (packageSettings.HasSettings("ShouldQueryLegacyPackageRemoval"))
            {
                return;
            }

            if (!packageSettings.HasSettings("RemoveLegacyInputHelpersForReload"))
            {
                PackageRemovalRequest lihremreq = new PackageRemovalRequest();
                lihremreq.packageId = "com.unity.xr.legacyinputhelpers";
                lihremreq.shouldCheckForInstallation = false;
                s_PackagesToRemoveQueue.QueueWorkItem(lihremreq);
                packageSettings.AddSettings("RemoveLegacyInputHelpersForReload");
                packageSettings.SaveSettings();
            }

            s_PackageListQueue.QueueName = k_LocalPackageListingQueryQueue;
            s_PackageListQueue.ProcessItemCallback += (LegacyPackageListRequest plr) => {
                if (plr.packageListRequest == null)
                    return;

                if (!plr.packageListRequest.IsCompleted)
                {
                    s_PackageListQueue.QueueWorkItem(plr);
                }
                else
                {
                    if (plr.packageListRequest.Status == PackageManager.StatusCode.Success)
                    {
                        List<string> packageIdsToRemove = new List<string>();
                        string removeRequestText = $"{XRConstants.k_XRPluginManagement} has detected that this project is using built in VR. Built in VR is incompatible with the new XR Plug-in system and any built in packages should be removed.\nDo you want {XRConstants.k_XRPluginManagement} to remove the following packages for you?\n\n";
                        foreach (var package in plr.packageListRequest.Result)
                        {
                            if (k_PackagesToBeRemoved.Contains(package.name))
                            {
                                packageIdsToRemove.Add(package.name);
                                removeRequestText += $"{package.displayName}\n";
                            }
                        }

                        removeRequestText += "\nChoosing to cancel will require manual removal of built-in integration packages through the Package Manger window.";

                        if (packageIdsToRemove.Count > 0)
                        {
                            if (EditorUtility.DisplayDialog("Built in VR Detected", removeRequestText, "Ok", "Cancel"))
                            {
                                foreach (string packageId in packageIdsToRemove)
                                {
                                    PackageRemovalRequest remreq = new PackageRemovalRequest();
                                    remreq.packageId = packageId;
                                    remreq.shouldCheckForInstallation = true;
                                    s_PackagesToRemoveQueue.QueueWorkItem(remreq);
                                }
                            }

                            var packSettings = XRPackageInitializationSettings.Instance;
                            packSettings.AddSettings("ShouldQueryLegacyPackageRemoval");
                            packSettings.SaveSettings();
                        }

#if !UNITY_2020_2_OR_NEWER
                        bool virtualRealityEnabled = false;
                        foreach (var buildTargetGroup in (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup)))
                        {
#pragma warning disable CS0618
                            virtualRealityEnabled |= PlayerSettings.GetVirtualRealitySupported(buildTargetGroup);
#pragma warning restore CS0618
                        }

                        if (virtualRealityEnabled)
                        {
                            if (EditorUtility.DisplayDialog("Built in VR Enabled", $"{XRConstants.k_XRPluginManagement} has detected that Built-In VR is enabled. We must disable Built-In VR prior to using {XRConstants.k_XRPluginManagement}.", "Ok"))
                            {
                                foreach (var buildTargetGroup in (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup)))
                                {
#pragma warning disable CS0618
                                    if (PlayerSettings.GetVirtualRealitySupported(buildTargetGroup))
                                    {
                                        PlayerSettings.SetVirtualRealitySupported(buildTargetGroup, false);
                                        UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(buildTargetGroup, new string[]{});
                                    }
#pragma warning restore CS0618
                                }
                            }
                        }
#endif //!UNITY_2020_2_OR_NEWER
}
                }
            };

            LegacyPackageListRequest req = new LegacyPackageListRequest();
            req.packageListRequest = PackageManager.Client.List(true, true);
            s_PackageListQueue.QueueWorkItem(req);
        }

    }
}

