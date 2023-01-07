using System;
using System.Collections.Generic;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// This object sends all metric events for the Services Core package.
    /// </summary>
    class CoreMetrics
    {
        internal const string PackageInitTimeMetricName = "package_init_time";
        internal const string AllPackagesInitSuccessMetricName = "all_packages_init_success";
        internal const string AllPackagesInitTimeMetricName = "all_packages_init_time";

        internal const string PackageInitializerNamesKeyFormat = "{0}.initializer-assembly-qualified-names";
        internal const char PackageInitializerNamesSeparator = ';';
        internal const string AllPackageNamesKey = "com.unity.services.core.all-package-names";
        internal const char AllPackageNamesSeparator = ';';

        public static CoreMetrics Instance { get; internal set; }

        internal IMetrics Metrics { get; set; }

        internal IDictionary<Type, IMetrics> AllPackageMetrics { get; }
            = new Dictionary<Type, IMetrics>();

        public void SendAllPackagesInitSuccessMetric()
        {
            if (Metrics is null)
            {
                CoreLogger.LogVerbose($"Can't send AllPackagesInitSuccess, Metrics is null.");
            }
            else
            {
                Metrics.SendSumMetric(AllPackagesInitSuccessMetricName);
            }
        }

        public void SendAllPackagesInitTimeMetric(double initTimeSeconds)
        {
            if (Metrics is null)
            {
                CoreLogger.LogVerbose($"Can't send AllPackagesInitTime, Metrics is null.");
            }
            else
            {
                Metrics.SendHistogramMetric(AllPackagesInitTimeMetricName, initTimeSeconds);
            }
        }

        public void SendInitTimeMetricForPackage(Type packageType, double initTimeSeconds)
        {
            if (AllPackageMetrics.TryGetValue(packageType, out var metric))
            {
                metric.SendHistogramMetric(PackageInitTimeMetricName, initTimeSeconds);
            }
            else
            {
                CoreLogger.LogVerbose($"There are no metrics for {packageType.Name}.");
            }
        }

        public void Initialize(IProjectConfiguration configuration, IMetricsFactory factory, Type corePackageType)
        {
            AllPackageMetrics.Clear();
            FindAndCacheAllPackageMetrics(configuration, factory);
            if (AllPackageMetrics.TryGetValue(corePackageType, out var coreMetrics))
            {
                Metrics = coreMetrics;
            }
            else
            {
                CoreLogger.LogVerbose("Metrics couldn't be created for Core package.");
            }
        }

        internal void FindAndCacheAllPackageMetrics(IProjectConfiguration configuration, IMetricsFactory factory)
        {
            var packageNames = configuration.GetString(AllPackageNamesKey, "");
            var splitNames = packageNames?.Split(AllPackageNamesSeparator) ?? Array.Empty<string>();
            foreach (var packageName in splitNames)
            {
                if (string.IsNullOrEmpty(packageName))
                    continue;

                var configKey = string.Format(PackageInitializerNamesKeyFormat, packageName);
                var joinedInitializerFullNames = configuration.GetString(configKey, "");
                if (string.IsNullOrEmpty(joinedInitializerFullNames))
                    continue;

                var initializerFullNames = joinedInitializerFullNames.Split(PackageInitializerNamesSeparator);
                foreach (var initializerFullName in initializerFullNames)
                {
                    var packageType = Type.GetType(initializerFullName);
                    if (packageType is null)
                    {
                        CoreLogger.LogVerbose($"'{initializerFullName}' not found. It may have been stripped.");
                        continue;
                    }

                    var metric = factory.Create(packageName);
                    AllPackageMetrics[packageType] = metric;
                }
            }
        }
    }
}
