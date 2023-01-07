using System;

using UnityEditor;
using UnityEngine;

namespace UnityEditor.XR.Management
{
    /// <summary>
    /// Custom attribute that indicates a class supports the <see cref="IXRCustomLoaderUI"/> for a
    /// specific loader and build target group. Any class marked with this attribute will
    /// have <see cref="IXRCustomLoaderUI"/> methods called on it while the XR Plug-in Management UI is displaying
    /// the supported loader for the supported build target.
    ///
    /// Note that there can only be one custom loader for each (Loader Type, BuildTargetGroup) combination. If more than one loader exists,
    /// the system will fall back to built-in supported rendering and log a warning in the Console window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class XRCustomLoaderUIAttribute : Attribute
    {
        /// <summary>
        /// Supported build target group.
        /// </summary>
        /// <value>Supported build target group. </value>
        public BuildTargetGroup buildTargetGroup { get; set; }

        /// <summary>
        /// Supported loader type.
        /// </summary>
        /// <value>Supported loader type.</value>
        public string loaderTypeName { get; set; }

        private  XRCustomLoaderUIAttribute() {}

        /// <summary>
        /// Constructor for this attribute.
        /// </summary>
        /// <param name="loaderTypeName">Loader type name.</param>
        /// <param name="buildTargetGroup">Build Target Group.</param>
        public XRCustomLoaderUIAttribute(string loaderTypeName, BuildTargetGroup buildTargetGroup)
        {
            this.loaderTypeName = loaderTypeName;
            this.buildTargetGroup = buildTargetGroup;
        }
    }

    /// <summary>
    /// Custom interface provided by the package if the package uses
    /// its own UI in the XR Plug-in Management loader selection
    /// window.
    ///
    /// Any class that implements this interface must be tagged with the <see cref="XRCustomLoaderUIAttribute"/> attribute.
    /// </summary>
    public interface IXRCustomLoaderUI
    {
        /// <summary>
        /// Current enabled state of this loader.
        /// </summary>
        /// <value>True if the loader has been enabled, false otherwise.</value>
        bool IsLoaderEnabled { get; set; }

        /// <summary>
        /// Array of type names that are incompatible with the loader when it's enabled. Non-compatible loaders will be grayed out
        /// in the UI.
        /// </summary>
        /// <value>Array of type names to disable.</value>
        string[] IncompatibleLoaders { get; }

        /// <summary>
        /// The height of the area within the UI that this renderer will need to render its UI. This height will be the height of the rect passed into the <see cref="OnGUI"/> call.
        /// This should return a a non-zero value that's a multiple of the line height set in <see cref="SetRenderedLineHeight"/>.
        /// </summary>
        /// <value>Non-zero multiple of the line height set in <see cref="SetRenderedLineHeight"/>. If this is 0, the rect passed to <see cref="OnGUI"/> will be the default line height.</value>
        float RequiredRenderHeight { get; }

        /// <summary>
        /// The Rendering component passes the expected line height to the custom renderer. This allows the component to
        /// calculate the necessary area height required to render the custom UI into the component space. The calculated value should
        /// be returned from the <see cref="RequiredRenderHeight"/>.
        /// </summary>
        /// <param name="height"></param>
        void SetRenderedLineHeight(float height);

        /// <summary>
        ///  Allows XR Plug-in Management to tell the UI which build target group it's being used with.
        /// </summary>
        /// <value>Build target that this instance handles.</value>
        BuildTargetGroup ActiveBuildTargetGroup { get; set; }

        /// <summary>
        /// Call to render the UI for this custom loader.
        /// </summary>
        /// <param name="rect">The rect within which the UI should render into.</param>
        void OnGUI(Rect rect);
    }
}
