using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR.Features
{
    internal class OpenXRChooseRuntimeLibraries : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public static string GetLoaderLibraryPath()
        {
            var extensions = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);

            // Loop over all the native plugin importers and find the custom loader
            var importers = PluginImporter.GetAllImporters();
            foreach (var importer in importers)
            {
                if (!importer.GetCompatibleWithEditor() || !importer.assetPath.Contains("openxr_loader"))
                    continue;

#if UNITY_EDITOR_WIN
                if (!importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64) || !importer.assetPath.EndsWith(".dll"))
                    continue;
#elif UNITY_EDITOR_OSX
                if (!importer.GetCompatibleWithPlatform(BuildTarget.StandaloneOSX) || !importer.assetPath.EndsWith(".dylib"))
                    continue;
#endif

                bool importerPartOfExtension = false;
                var root = Path.GetDirectoryName(importer.assetPath);
                foreach (var extInfo in extensions.Features)
                {
                    bool extensionContainsLoader = (root != null && root.Contains(extInfo.PluginPath));
                    importerPartOfExtension |= extensionContainsLoader;

                    bool customRuntimeLoaderOnEditorTarget = extInfo.Attribute.CustomRuntimeLoaderBuildTargets?.Intersect(
                        new[] {BuildTarget.StandaloneWindows64, BuildTarget.StandaloneOSX, BuildTarget.StandaloneLinux64}).Any() ?? false;

                    if (extensionContainsLoader &&
                        customRuntimeLoaderOnEditorTarget &&
                        extInfo.Feature.enabled)
                    {
                        return AssetPathToAbsolutePath(importer.assetPath);
                    }
                }

                // return default loader
                bool hasCustomLoader = extensions.CustomLoaderBuildTargets?.Length > 0;
                if (!importerPartOfExtension && !hasCustomLoader)
                    return AssetPathToAbsolutePath(importer.assetPath);
            }

            return "";
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            var path = assetPath.Replace('/', Path.DirectorySeparatorChar);
            if (assetPath.StartsWith("Packages"))
            {
                path = String.Join("" + Path.DirectorySeparatorChar, path.Split(Path.DirectorySeparatorChar).Skip(2));

                return Path.Combine(PackageManager.PackageInfo.FindForAssetPath(assetPath).resolvedPath, path);
            }

            return path;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            var enabled = BuildHelperUtils.HasLoader(report.summary.platformGroup, typeof(OpenXRLoaderBase));

            var extensions = FeatureHelpersInternal.GetAllFeatureInfo(report.summary.platformGroup);

            // Keep set of seen plugins, only disable plugins that haven't been seen.
            HashSet<string> seenPlugins = new HashSet<string>();

            // Loop over all the native plugin importers and only include the enabled ones in the build
            var importers = PluginImporter.GetAllImporters();
            foreach (var importer in importers)
            {
                if (!importer.GetCompatibleWithPlatform(report.summary.platform))
                    continue;
                bool loader = false;
                if (importer.assetPath.Contains("openxr_loader"))
                {
                    loader = true;
                    if (extensions.CustomLoaderBuildTargets?.Contains(report.summary.platform) ?? false)
                        importer.SetIncludeInBuildDelegate(path => false);
                    else
                        importer.SetIncludeInBuildDelegate(path => enabled);
                }

                if (importer.assetPath.Contains("UnityOpenXR"))
                {
                    importer.SetIncludeInBuildDelegate(path => enabled);
                }

                var root = Path.GetDirectoryName(importer.assetPath);
                foreach (var extInfo in extensions.Features)
                {
                    if (root != null && root.Contains(extInfo.PluginPath))
                    {
                        if (extInfo.Feature.enabled &&
                            (!loader || (extInfo.Attribute.CustomRuntimeLoaderBuildTargets?.Contains(report.summary.platform) ?? false)))
                        {
                            importer.SetIncludeInBuildDelegate(path => enabled);
                        }
                        else if (!seenPlugins.Contains(importer.assetPath))
                        {
                            importer.SetIncludeInBuildDelegate(path => false);
                        }
                        seenPlugins.Add(importer.assetPath);
                    }
                }
            }
        }

        [InitializeOnLoadMethod]
        static void InitializeOnLoad ()
        {
            var importers = PluginImporter.GetAllImporters();

            // fixes asset bundle building since IPreProcessBuildWithReport isn't called
            foreach (var importer in importers)
            {
                if (importer.assetPath.Contains("openxr_loader"))
                {
                    importer.SetIncludeInBuildDelegate(path => false);
                }
            }
        }
    }
}
