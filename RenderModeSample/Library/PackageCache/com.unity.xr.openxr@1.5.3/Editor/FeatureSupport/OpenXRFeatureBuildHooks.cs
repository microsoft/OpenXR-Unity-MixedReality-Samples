using System;
using UnityEngine.XR.OpenXR.Features;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR.Features
{
    /// <summary>
    /// Inherit from this class to get callbacks to hook into the build process when your OpenXR Extension is enabled.
    /// </summary>
    public abstract class OpenXRFeatureBuildHooks : IPostGenerateGradleAndroidProject, IPostprocessBuildWithReport,
        IPreprocessBuildWithReport
    {
        private OpenXRFeature _ext = null;

        private bool IsExtensionEnabled(BuildTarget target, BuildTargetGroup group)
        {
            if (!BuildHelperUtils.HasLoader(group, typeof(OpenXRLoaderBase)))
                return false;

            if (OpenXRSettings.ActiveBuildTargetInstance == null || OpenXRSettings.ActiveBuildTargetInstance.features == null)
                return false;

            if (_ext == null || _ext.GetType() != featureType)
            {
                foreach (var ext in OpenXRSettings.ActiveBuildTargetInstance.features)
                {
                    if (featureType == ext.GetType())
                    {
                        _ext = ext;
                    }
                }
            }

            if (_ext == null || !_ext.enabled)
                return false;

            return true;
        }

        /// <summary>
        /// Returns the current callback order for build processing.
        /// </summary>
        /// <value>Int value denoting the callback oarder.</value>
        public abstract int callbackOrder { get; }

        /// <summary>
        /// Post process build step for checking if a feature is enabled. If so will call to the feature to run their build pre processing.
        /// </summary>
        /// <param name="report">Build report.</param>
        public virtual void OnPreprocessBuild(BuildReport report)
        {
            if (!IsExtensionEnabled(report.summary.platform, report.summary.platformGroup))
                return;

            OnPreprocessBuildExt(report);
        }

        /// <summary>
        /// Post process build step for checking if a feature is enabled for android builds. If so will call to the feature to run their build post processing for android builds.
        /// </summary>
        /// <param name="path">Path to gradle project.</param>
        public virtual void OnPostGenerateGradleAndroidProject(string path)
        {
            if (!IsExtensionEnabled(BuildTarget.Android, BuildTargetGroup.Android))
                return;

            OnPostGenerateGradleAndroidProjectExt(path);
        }

        /// <summary>
        /// Pre-process build step for checking if a feature is enabled. If so will call to the feature to run their build post processing.
        /// </summary>
        /// <param name="report">Build report.</param>
        public virtual void OnPostprocessBuild(BuildReport report)
        {
            if (!IsExtensionEnabled(report.summary.platform, report.summary.platformGroup))
                return;

            OnPostprocessBuildExt(report);
        }

        /// <summary>
        /// System.Type of the class that implements OpenXRFeature.
        /// </summary>
        public abstract Type featureType { get; }

        /// <summary>
        /// Called during the build process when the feature is enabled. Implement this function to receive a callback before the build starts.
        /// </summary>
        /// <param name="report">Report that contains information about the build, such as its target platform and output path.</param>
        protected abstract void OnPreprocessBuildExt(BuildReport report);

        /// <summary>
        /// Called during build process when extension is enabled. Implement this function to receive a callback after the Android Gradle project is generated and before building begins. Function is not called for Internal builds.
        /// </summary>
        /// <param name="path">The path to the root of the Gradle project. Note: When exporting the project, this parameter holds the path to the folder specified for export.</param>
        protected abstract void OnPostGenerateGradleAndroidProjectExt(string path);

        /// <summary>
        /// Called during the build process when extension is enabled. Implement this function to receive a callback after the build is complete.
        /// </summary>
        /// <param name="report">BuildReport that contains information about the build, such as the target platform and output path.</param>
        protected abstract void OnPostprocessBuildExt(BuildReport report);
    }
}
