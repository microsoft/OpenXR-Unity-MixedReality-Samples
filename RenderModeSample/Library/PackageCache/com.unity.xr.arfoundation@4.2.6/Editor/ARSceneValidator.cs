using System;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEngine.XR.ARFoundation;
using UnityEditor.XR.Management;

namespace UnityEditor.XR.ARFoundation
{
    class ARSceneValidator : IProcessSceneWithReport
    {
        [PostProcessBuild]
        static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (s_ScenesWithARTypes.Count > 0 && s_SessionCount == 0)
            {
                var scenes = "";
                foreach (var sceneName in s_ScenesWithARTypes)
                {
                    scenes += string.Format("\n\t{0}", sceneName);
                }

                Debug.LogWarningFormat(
                    "The following scenes contain AR components but no ARSession. The ARSession component controls the AR lifecycle, so these components will not do anything at runtime. Was this intended?{0}",
                    scenes);
            }

            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            if(generalSettings != null && generalSettings.Manager != null && generalSettings.Manager.activeLoaders != null)
            {
                 int loaderCount = generalSettings.Manager.activeLoaders.Count;
                 if(loaderCount <= 0 && s_SessionCount > 0)
                 {
                      Debug.LogWarning(
                    "There are scenes that contain an ARSession, but no XR plug-in providers have been selected for the current platform. To make a plug-in provider available at runtime go to Project Settings > XR Plug-in Management and enable at least one for the target platform.");
                 }
            }

            s_ScenesWithARTypes.Clear();
            s_SessionCount = 0;
        }

        int IOrderedCallback.callbackOrder => 0;

        void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report)
        {
            if (sceneContainsARTypes)
            {
                s_ScenesWithARTypes.Add(SceneManager.GetActiveScene().name);
            }

            s_SessionCount += UnityEngine.Object.FindObjectsOfType<ARSession>().Length;
        }

        static bool sceneContainsARTypes
        {
            get
            {
                foreach (var type in k_ARTypes)
                {
                    foreach (var component in UnityEngine.Object.FindObjectsOfType(type))
                    {
                        var monobehaviour = component as MonoBehaviour;
                        if (monobehaviour != null && monobehaviour.enabled)
                            return true;
                    }
                }

                return false;
            }
        }

        static List<string> s_ScenesWithARTypes = new List<string>();

        static int s_SessionCount;

        static readonly Type[] k_ARTypes = new Type[]
        {
            typeof(ARCameraBackground),
            typeof(ARPlaneManager),
            typeof(ARPointCloudManager),
            typeof(ARAnchorManager)
        };
    }
}
