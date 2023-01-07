using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Scheduler.Internal;

namespace Unity.Services.Core.Telemetry.Internal
{
    /// <summary>
    /// Handles common logic between all <see cref="Metrics"/> instances.
    /// </summary>
    class MetricsHandler : TelemetryHandler<MetricsPayload, Metric>
    {
        public MetricsHandler(
            TelemetryConfig config, CachedPayload<MetricsPayload> cache, IActionScheduler scheduler,
            ICachePersister<MetricsPayload> cachePersister, TelemetrySender sender)
            : base(config, cache, scheduler, cachePersister, sender)
        {
            // prevent .ctor of StringEnumConverter from being stripped
            AotHelper.EnsureType<StringEnumConverter>();
        }

        internal override void SendPersistedCache(CachedPayload<MetricsPayload> persistedCache)
        {
            if (!AreMetricsOutdated())
            {
                m_Sender.SendAsync(persistedCache.Payload);
            }

            m_CachePersister.Delete();

            bool AreMetricsOutdated()
            {
                var differenceFromUtcNow = DateTime.UtcNow - new DateTime(persistedCache.TimeOfOccurenceTicks);
                return differenceFromUtcNow.TotalSeconds > Config.PayloadExpirationSeconds;
            }
        }

        internal override void FetchSpecificCommonTags(ICloudProjectId cloudProjectId, IEnvironments environments)
        {
            Cache.Payload.MetricsCommonTags.Clear();
        }

        internal override void SendCachedPayload()
        {
            if (Cache.Payload.Metrics.Count <= 0)
                return;

            m_Sender.SendAsync(Cache.Payload);

            Cache.Payload.Metrics.Clear();
            Cache.TimeOfOccurenceTicks = 0;

            if (m_CachePersister.CanPersist)
            {
                m_CachePersister.Delete();
            }
        }
    }
}
