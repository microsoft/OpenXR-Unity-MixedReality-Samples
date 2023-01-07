using System;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Samples.MeshingFeature;
using UnityEditor.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR.Samples.MeshingFeature
{
    /// <summary>
    /// Custom editor for meshing behaviour that ensures the meshing feature plugin can be loaded.
    ///
    /// For the meshing feature sample to be loaded the sample folder must be an immediate child of the 'Assets' folder and
    /// must be there on Unity startup.  This script will copy the sample folder to the required destination and attempt
    /// to force a restart after.
    ///
    /// Note that this script will self destruct after running by deleting the script asset to ensure it only ever runs once.
    ///
    /// Note that when developing samples this copying functionality may not be desired.  To disable this
    /// functionality add the UNITY_SAMPLE_DEV define to your project.
    /// </summary>
    [InitializeOnLoad]
    public class MeshingFeatureInstaller : Editor
    {
#if !UNITY_SAMPLE_DEV
        private const string k_MeshingFeaturePath = "Meshing Subsystem Feature/MeshingFeaturePlugin/UnitySubsystemsManifest.json";

        private const string k_ErrorMessage = "The Meshing Subsystem Feature Sample requires the `MeshingFeaturePlugin' folder be moved to the root 'Assets` folder to run properly.  An attempt was made to automatically move these files to the correct location but failed, please move these files manaually before running the sample.";

        private const string k_DialogText = "The Meshing Feature Sample requires that the MeshingFeature subsystem be registered, which will require Unity editor to be restarted.\n\nDo you want to *RESTART* the editor now?";

        static MeshingFeatureInstaller()
        {
            EditorApplication.update += Install;
        }

        private static void Install()
        {
            EditorApplication.update -= Install;

            // Automatically enable the feature
            FeatureHelpers.RefreshFeatures(BuildTargetGroup.Standalone);
            var feature = OpenXRSettings.Instance.GetFeature<MeshingTeapotFeature>();
            if (feature != null)
                feature.enabled = true;

            // Find the subsystem manifest to figure out where the sample was installed
            var source = AssetDatabase
                .FindAssets(Path.GetFileNameWithoutExtension(k_MeshingFeaturePath))
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(r => r.Contains(k_MeshingFeaturePath));

            if (string.IsNullOrEmpty(source))
            {
                Debug.LogError(k_ErrorMessage);
                return;
            }

            // Extract the `UnitySubsystemsManifest.json` from the path
            source = Path.GetDirectoryName(source);

            // Target path is 'Assets/<sample folder name>`
            var target = Path.Combine("Assets", Path.GetFileNameWithoutExtension(source));

            // Attempt to move the entire folder
            var moveResult = AssetDatabase.MoveAsset(source, target);
            if (!string.IsNullOrWhiteSpace(moveResult))
            {
                Debug.LogError(moveResult);
                return;
            }

            Debug.Log($"Moved '{source}' to '{target}");

            if (EditorUtility.DisplayDialog("Warning", k_DialogText, "Yes", "No"))
            {
                // Restart editor.
                var editorApplicationType = typeof(EditorApplication);
                var requestCloseAndRelaunchWithCurrentArgumentsMethod =
                    editorApplicationType.GetMethod("RequestCloseAndRelaunchWithCurrentArguments",
                        BindingFlags.NonPublic | BindingFlags.Static);

                if (requestCloseAndRelaunchWithCurrentArgumentsMethod == null)
                    throw new MissingMethodException(editorApplicationType.FullName, "RequestCloseAndRelaunchWithCurrentArguments");

                requestCloseAndRelaunchWithCurrentArgumentsMethod.Invoke(null, null);
            }

            // Self destruct
            AssetDatabase.DeleteAsset(Path.Combine(Path.GetDirectoryName(source), "Editor"));
        }
#endif
    }
}