using System;

namespace UnityEditor.XR.OpenXR.Features
{
    /// <summary>
    /// Attribute used to describe a feature set to the OpenXR Editor components.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed public class OpenXRFeatureSetAttribute : System.Attribute
    {
        /// <summary>
        /// The list of feature ids that this feature set can enable or disable.
        /// </summary>
        public string[] FeatureIds;

        /// <summary>
        /// The string used to represent the feature set in the UI.
        /// </summary>
        public string UiName;

        /// <summary>
        /// Description of the feature set.
        /// </summary>
        public string Description;

        /// <summary>
        /// The id used to uniquely define this feature set. It is recommended to use reverse DNS naming for this id.
        /// </summary>
        public string FeatureSetId;

        /// <summary>
        /// The list of build targets that this feature set supports.
        /// </summary>
        public BuildTargetGroup[] SupportedBuildTargets;

        /// <summary>
        /// The list of feature ids that this feature set requires. The features in this list will be enabled (and the UI will not allow them to be disabled) whenever the feature set itself is enabled.
        ///
        /// Feature Ids are a subset of <see cref="FeatureIds"/>. Any feature id in this list and not also in <see cref="FeatureIds"/> will be ignored.
        /// </summary>
        public string[] RequiredFeatureIds;

        /// <summary>
        /// The list of feature ids that this feature set desires (but does not require). The features in this list will be enabled (but the UI will allow them to be disabled) whenever the feature set itself is enabled.
        ///
        /// Feature Ids are a subset of <see cref="FeatureIds"/>. Any feature id in this list and not also in <see cref="FeatureIds"/> will be ignored.
        /// </summary>
        public string[] DefaultFeatureIds;

    }
}
