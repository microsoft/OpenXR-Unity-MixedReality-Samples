using System;
using System.Collections.Generic;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Telemetry.Internal
{
    class MetricsFactory : IMetricsFactory
    {
        readonly IProjectConfiguration m_ProjectConfig;

        public IReadOnlyDictionary<string, string> CommonTags { get; }

        internal MetricsHandler Handler { get; }

        public MetricsFactory(MetricsHandler handler, IProjectConfiguration projectConfig)
        {
            Handler = handler;
            m_ProjectConfig = projectConfig;

            CommonTags = new Dictionary<string, string>(handler.Cache.Payload.CommonTags)
                .MergeAllowOverride(handler.Cache.Payload.MetricsCommonTags);
        }

        public IMetrics Create(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentNullException(nameof(packageName));

            var packageTags = FactoryUtils.CreatePackageTags(m_ProjectConfig, packageName);
            var metrics = new Metrics(Handler, packageTags);

            return metrics;
        }
    }
}
