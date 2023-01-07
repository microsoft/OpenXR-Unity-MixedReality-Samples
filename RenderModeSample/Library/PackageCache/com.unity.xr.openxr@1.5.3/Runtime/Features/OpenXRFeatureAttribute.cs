using System;
using UnityEngine.XR.OpenXR;

#if UNITY_EDITOR
namespace UnityEditor.XR.OpenXR.Features
{

    public class FeatureCategory
    {
        public const string Default = "";
        public const string Feature = "Feature";
        public const string Interaction = "Interaction";
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OpenXRFeatureAttribute : Attribute
    {
        /// <summary>
        /// Feature name to show in the feature configuration UI.
        /// </summary>
        public string UiName;

        /// <summary>
        /// Hide this feature from the UI.
        /// </summary>
        public bool Hidden;

        /// <summary>
        /// Feature description to show in the UI.
        /// </summary>
        public string Desc;

        /// <summary>
        /// OpenXR runtime extension strings that need to be enabled to use this extension.
        /// If these extensions can't be enabled, a message will be logged, but execution will continue.
        /// Can contain multiple extensions separated by spaces.
        /// </summary>
        public string OpenxrExtensionStrings;

        /// <summary>
        /// Company that created the feature, shown in the feature configuration UI.
        /// </summary>
        public string Company;

        /// <summary>
        /// Link to the feature documentation. The help button in the UI opens this link in a web browser.
        /// </summary>
        public string DocumentationLink;

        /// <summary>
        /// Feature version.
        /// </summary>
        public string Version;

        /// <summary>
        /// BuildTargets in this list use a custom runtime loader (that is, openxr_loader.dll).
        /// Only one feature per platform can have a custom runtime loader.
        /// Unity will skip copying the default loader to the build and use this feature's loader instead on these platforms.
        /// Loader must be placed alongside the OpenXRFeature script or in a subfolder of it.
        /// </summary>
        public BuildTarget[] CustomRuntimeLoaderBuildTargets;

        /// <summary>
        /// BuildTargetsGroups that this feature supports. The feature will only be shown or included on these platforms.
        /// </summary>
        public BuildTargetGroup[] BuildTargetGroups;

        /// <summary>
        /// Feature category.
        /// </summary>
        public string Category = "";

        /// <summary>
        /// True fi this feature is required, false otherwise.
        /// Required features will cause the loader to fail to initialize if they fail to initialize or start.
        /// </summary>
        public bool Required = false;

        /// <summary>
        /// Determines the order in which the feature will be called in both the GetInstanceProcAddr hook list and
        /// when events such as OnInstanceCreate are called. Higher priority features will hook after lower priority features and
        /// be called first in the event list.
        /// </summary>
        public int Priority = 0;

        /// <summary>
        /// A well known string id for this feature. It is recommended that that id be in reverse DNS naming format (com.foo.bar.feature).
        /// </summary>
        public string FeatureId = "";


        internal static readonly System.Text.RegularExpressions.Regex k_PackageVersionRegex = new System.Text.RegularExpressions.Regex(@"(\d*\.\d*)\..*");

        /// <summary>
        /// This method returns the OpenXR internal documentation link.  This is necessary because the documentation link was made public in the
        /// Costants class which prevents it from being alterned in anything but a major revision.  This method will patch up the documentation links
        /// as needed as long as they are internal openxr documentation links.
        /// </summary>
        internal string InternalDocumentationLink
        {
            get
            {
                if (string.IsNullOrEmpty(DocumentationLink))
                    return DocumentationLink;

                // Update the version if needed
                if (DocumentationLink.StartsWith(Constants.k_DocumentationManualURL))
                {
                    var version = PackageManager.PackageInfo.FindForAssembly(typeof(OpenXRFeatureAttribute).Assembly)?.version;
                    var majorminor = k_PackageVersionRegex.Match(version).Groups[1].Value;
                    DocumentationLink = DocumentationLink.Replace("1.0", majorminor);
                }

                return DocumentationLink;
            }
        }
    }
}
#endif
