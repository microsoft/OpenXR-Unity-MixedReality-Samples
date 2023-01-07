#if UNITY_2021_3_OR_NEWER
using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Newtonsoft.Json;
using UnityEditor.Build;
using UnityEngine;

namespace Unity.Services.Core.Configuration.Editor
{
    class ProjectConfigurationBuildInjectorWithPlayerProcessor : BuildPlayerProcessor
    {
        internal const string IoErrorMessage = "Service configuration file couldn't be created."
            + " Be sure you have read/write access to your project's Library folder.";

        internal static readonly string CoreLibraryFolderPath
            = Path.Combine(".", "Library", "com.unity.services.core");
        internal static readonly string ConfigCachePath
            = Path.Combine(CoreLibraryFolderPath, ConfigurationUtils.ConfigFileName);

        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            var config = ProjectConfigurationBuilder.CreateBuilderWithAllProvidersInProject()
                .BuildConfiguration();
            CreateProjectConfigFile(config);
            buildPlayerContext.AddAdditionalPathToStreamingAssets(ConfigCachePath);
        }

        internal static void CreateProjectConfigFile(SerializableProjectConfiguration config)
        {
            try
            {
                if (!Directory.Exists(CoreLibraryFolderPath))
                {
                    Directory.CreateDirectory(CoreLibraryFolderPath);
                }

                var serializedConfig = JsonConvert.SerializeObject(config);
                File.WriteAllText(ConfigCachePath, serializedConfig);
            }
            catch (SecurityException e)
                when (e.PermissionType == typeof(FileIOPermission)
                      && FakePredicateLogError())
            {
                // Never reached to avoid stack unwind.
            }
            catch (UnauthorizedAccessException)
                when (FakePredicateLogError())
            {
                // Never reached to avoid stack unwind.
            }

            bool FakePredicateLogError()
            {
                Debug.LogError(IoErrorMessage);
                return false;
            }
        }
    }
}
#endif
