using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace UnityEditor.XR.ARSubsystems
{
    static class XRReferenceImageLibraryBuildProcessor
    {
        public static void ClearDataStore()
        {
            foreach (var library in XRReferenceImageLibrary.All())
            {
                library.ClearDataStore();
            }
        }

        class Preprocessor : IPreprocessBuildWithReport
        {
            // Should come before most other things
            public int callbackOrder => -1000;

            // Clear all native data and let each provider decide whether to populate it or not.
            public void OnPreprocessBuild(BuildReport report) => ClearDataStore();
        }

        class Postprocessor : IPostprocessBuildWithReport
        {
            // Should come after most other things
            public int callbackOrder => 1000;

            public void OnPostprocessBuild(BuildReport report) => ClearDataStore();
        }
    }
}
