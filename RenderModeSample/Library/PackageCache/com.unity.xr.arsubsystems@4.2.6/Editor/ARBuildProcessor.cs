using System;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.XR.Management;

namespace UnityEditor.XR.ARSubsystems
{
    /// <summary>
    /// A utility class to run AR build preprocessors.
    /// </summary>
    public static class ARBuildProcessor
    {
        /// <summary>
        /// Implement this interface to receive a callback when <see cref="PreprocessBuild"/> is called.
        /// </summary>
        public interface IPreprocessBuild : IOrderedCallback
        {
            /// <summary>
            /// Invoked when <see cref="PreprocessBuild"/> is called.
            /// </summary>
            /// <param name="buildEventArgs">The <see cref="PreprocessBuildEventArgs"/> associated with this
            /// preprocessor event.</param>
            void OnPreprocessBuild(PreprocessBuildEventArgs buildEventArgs);
        }

        /// <summary>
        /// Applies any preprocessing necessary to setup assets for a given build target.
        /// </summary>
        /// <remarks>
        /// This method instantiates all classes that derive from <see cref="IPreprocessBuild"/> and executes their
        /// <see cref="IPreprocessBuild.OnPreprocessBuild"/> methods.
        ///
        /// You should call this before building asset bundles (see
        /// [BuildPipeline.BuildAssetBundles](xref:UnityEditor.BuildPipeline.BuildAssetBundles(System.String,UnityEditor.BuildAssetBundleOptions,UnityEditor.BuildTarget)))
        /// in order to ensure that all AR assets have been correctly configured.
        /// </remarks>
        /// <param name="buildTarget">The build target for which assets should be preprocessed.</param>
        public static void PreprocessBuild(BuildTarget buildTarget)
        {
            // Get the list of active loaders for the given BuildTarget
            var settings = XRGeneralSettingsPerBuildTarget
                .XRGeneralSettingsForBuildTarget(BuildPipeline.GetBuildTargetGroup(buildTarget));
            var activeLoaders = settings && settings.Manager ? settings.Manager.activeLoaders : null;
            if (activeLoaders == null)
                return;

            // Clear the data store before asking packages to populate it.
            XRReferenceImageLibraryBuildProcessor.ClearDataStore();

            // Find and create all IPreprocessBuild objects
            var interfaces = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(type => Array.Exists(type.GetInterfaces(), i => i == typeof(IPreprocessBuild)))
                .Select(type => Activator.CreateInstance(type) as IPreprocessBuild)
                .Where(buildPreprocessor => buildPreprocessor != null)
                .ToList();

            // Sort by callback order
            interfaces.Sort((a, b) => a.callbackOrder.CompareTo(b.callbackOrder));

            // Invoke OnPreprocessBuild on each one
            var eventArgs = new PreprocessBuildEventArgs(buildTarget, activeLoaders);
            foreach (var @interface in interfaces)
            {
                @interface.OnPreprocessBuild(eventArgs);
            }
        }
    }
}
