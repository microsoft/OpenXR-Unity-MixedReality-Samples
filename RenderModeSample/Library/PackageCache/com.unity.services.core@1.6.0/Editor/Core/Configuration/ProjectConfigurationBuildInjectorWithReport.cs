#if !UNITY_2021_3_OR_NEWER
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.Services.Core.Configuration.Editor
{
    class ProjectConfigurationBuildInjectorWithReport : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public static string RuntimeConfigFullPath { get; }
            = Path.Combine(Application.streamingAssetsPath, ConfigurationUtils.ConfigFileName);

        public static void GenerateConfigFileInProject(ProjectConfigurationBuilder builder)
        {
            var config = builder.BuildConfiguration();
            var serializedConfig = JsonConvert.SerializeObject(config);
            AddConfigToProject(serializedConfig);
        }

        public static void AddConfigToProject(string config)
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            File.WriteAllText(RuntimeConfigFullPath, config);
            AssetDatabase.Refresh();
        }

        public static void RemoveConfigFromProject()
        {
            IoUtils.TryDeleteAssetFile(RuntimeConfigFullPath);
            IoUtils.TryDeleteStreamAssetsFolder();
        }

        int IOrderedCallback.callbackOrder { get; }

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            var builderWithAllProviders = ProjectConfigurationBuilder.CreateBuilderWithAllProvidersInProject();
            GenerateConfigFileInProject(builderWithAllProviders);

            EditorApplication.update += RemoveConfigFromProjectWhenBuildEnds;

            void RemoveConfigFromProjectWhenBuildEnds()
            {
                if (BuildPipeline.isBuildingPlayer)
                {
                    return;
                }

                EditorApplication.update -= RemoveConfigFromProjectWhenBuildEnds;
                RemoveConfigFromProject();
            }
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report) => RemoveConfigFromProject();
    }
}
#endif
