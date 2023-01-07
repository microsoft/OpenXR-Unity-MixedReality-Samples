using System.Collections.Generic;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Telemetry.Internal
{
    static class FactoryUtils
    {
        internal const string PackageVersionKeyFormat = "{0}.version";

        public static IDictionary<string, string> CreatePackageTags(
            IProjectConfiguration projectConfig, string packageName)
        {
            var packageVersion = projectConfig.GetString(
                string.Format(PackageVersionKeyFormat, packageName), string.Empty);
            if (string.IsNullOrEmpty(packageVersion))
            {
                CoreLogger.LogTelemetry($"No package version found for the package \"{packageName}\"");
            }

            return new Dictionary<string, string>
            {
                [TagKeys.PackageName] = packageName,
                [TagKeys.PackageVersion] = packageVersion,
            };
        }
    }
}
