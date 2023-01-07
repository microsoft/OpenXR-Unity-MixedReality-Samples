using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Services.Core.Configuration.Editor;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Telemetry.Internal;
using UnityEditor;
using UnityEditor.Build;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.Services.Core.Editor
{
    class OperatePackageVersionConfigProvider : IConfigurationProvider
    {
        static readonly OperatePackageVersionConfigProvider k_EditorInstance = new OperatePackageVersionConfigProvider
        {
            m_OperatePackages = CreateOperatePackagesConfigsForProject()
        };

        IEnumerable<PackageConfig> m_OperatePackages;

        int IOrderedCallback.callbackOrder { get; }

        void IConfigurationProvider.OnBuildingConfiguration(ConfigurationBuilder builder)
        {
            var operatePackages = BuildPipeline.isBuildingPlayer
                ? CreateOperatePackagesConfigsForProject()
                : k_EditorInstance.m_OperatePackages;

            ProvidePackageConfigs(builder, operatePackages);
        }

        static IEnumerable<PackageConfig> CreateOperatePackagesConfigsForProject()
            => CreatePackageConfigs(TypeCache.GetTypesDerivedFrom<IInitializablePackage>());

        internal static IEnumerable<PackageConfig> CreatePackageConfigs(IList<Type> packageTypes)
        {
            var packageInfoWithInitializers = new Dictionary<PackageInfo, List<Type>>(
                packageTypes.Count, new PackageInfoNameComparer());
            foreach (var packageType in packageTypes)
            {
                var packageInfo = PackageInfo.FindForAssembly(packageType.Assembly);
                if (packageInfo is null)
                    continue;

                if (packageInfoWithInitializers.TryGetValue(packageInfo, out var initializers))
                {
                    initializers.Add(packageType);
                }
                else
                {
                    packageInfoWithInitializers[packageInfo] = new List<Type>
                    {
                        packageType,
                    };
                }
            }

            return packageInfoWithInitializers.Select(x => new PackageConfig(x.Key, x.Value));
        }

        internal static void ProvidePackageConfigs(
            ConfigurationBuilder builder, IEnumerable<PackageConfig> operatePackages)
        {
            var allPackageNameBuilder = new StringBuilder();
            foreach (var packageInfo in operatePackages)
            {
                if (allPackageNameBuilder.Length > 0)
                {
                    allPackageNameBuilder.Append(CoreMetrics.AllPackageNamesSeparator);
                }

                allPackageNameBuilder.Append(packageInfo.Name);
                CreatePackageConfig(FactoryUtils.PackageVersionKeyFormat, packageInfo.Version);
                var joinedPackageInitializerNames = string.Join(
                    CoreMetrics.PackageInitializerNamesSeparator.ToString(), packageInfo.InitializerNames);
                CreatePackageConfig(CoreMetrics.PackageInitializerNamesKeyFormat, joinedPackageInitializerNames);

                void CreatePackageConfig(string keyFormat, string value)
                {
                    var configKey = string.Format(keyFormat, packageInfo.Name);
                    builder.SetString(configKey, value, true);
                }
            }

            builder.SetString(CoreMetrics.AllPackageNamesKey, allPackageNameBuilder.ToString());
        }
    }
}
