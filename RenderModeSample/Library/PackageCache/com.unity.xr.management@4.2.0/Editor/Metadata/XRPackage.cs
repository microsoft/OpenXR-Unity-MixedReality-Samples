using UnityEngine;

namespace UnityEditor.XR.Management.Metadata
{

    /// <summary>
    /// Implement this interface to provide package level information and actions.
    ///
    /// XR Plug-in Management will reflect on all types in the project to find implementers
    /// of this interface. These instances are used to get information required to integrate
    /// your package with the XR Plug-in Management system.
    /// </summary>
    public interface IXRPackage
    {
        /// <summary>
        /// Returns an instance of <see cref="IXRPackageMetadata"/>. Information will be used
        /// to allow the XR Plug-in Management to provide settings and loaders through the settings UI.
        /// </summary>
        IXRPackageMetadata metadata { get; }

        /// <summary>
        /// Allows the package to configure new settings and/or port old settings to the instance passed
        /// in.
        ///
        /// </summary>
        /// <param name="obj">ScriptableObject instance representing an instance of the settings
        /// type provided by <see cref="IXRPackageMetadata.settingsType"/>.</param>
        /// <returns>True if the operation succeeded, false if not. If implementation is empty, just return true.</returns>
        bool PopulateNewSettingsInstance(ScriptableObject obj);
    }
}
