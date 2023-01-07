using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine.XR.Management;


namespace UnityEditor.XR.Management.Metadata
{
    /// <summary>
    /// Provides an interface for describing specific loader metadata. Package authors should implement
    /// this interface for each loader they provide in their package.
    /// </summary>
    public interface IXRLoaderMetadata
    {
        /// <summary>
        /// The user facing name for this loader. Will be used to populate the
        /// list in the XR Plug-in Management UI.
        /// </summary>
        string loaderName { get; }

        /// <summary>
        /// The full type name for this loader. This is used to allow management to find and
        /// create instances of supported loaders for your package.
        ///
        /// When your package is first installed, the XR Plug-in Management system will
        /// use this information to create instances of your loaders in Assets/XR/Loaders.
        /// </summary>
        string loaderType { get; }

        /// <summary>
        /// The full list of supported buildtargets for this loader. This allows the UI to only show the
        /// loaders appropriate for a specific build target.
        ///
        /// Returning an empty list or a list containing just <a href="https://docs.unity3d.com/ScriptReference/BuildTargetGroup.Unknown.html">BuildTargetGroup.Unknown</a>. will make this
        /// loader invisible in the ui.
        /// </summary>
        List<BuildTargetGroup> supportedBuildTargets { get; }
    }

    /// <summary>
    /// Top level package metadata interface. Create an instance oif this interface to
    /// provide metadata information for your package.
    /// </summary>
    public interface IXRPackageMetadata
    {
        /// <summary>
        /// User facing package name. Should be the same as the value for the
        /// displayName keyword in the package.json file.
        /// </summary>
        string packageName { get; }

        /// <summary>
        /// The package id used to track and install the package. Must be the same value
        /// as the name keyword in the package.json file, otherwise installation will
        /// not be possible.
        /// </summary>
        string packageId { get; }

        /// <summary>
        /// This is the full type name for the settings type for your package.
        ///
        /// When your package is first installed, the XR Plug-in Management system will
        /// use this information to create an instance of your settings in Assets/XR/Settings.
        /// </summary>
        string settingsType { get; }

        /// <summary>
        /// List of <see cref="IXRLoaderMetadata"/> instances describing the data about the loaders
        /// your package supports.
        /// </summary>
        List<IXRLoaderMetadata> loaderMetadata { get; }
    }

}
