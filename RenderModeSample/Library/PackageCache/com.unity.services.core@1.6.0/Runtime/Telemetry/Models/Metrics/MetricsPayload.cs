using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unity.Services.Core.Telemetry.Internal
{
    [Serializable]
    struct MetricsPayload : ITelemetryPayload
    {
        [JsonProperty("metrics")]
        public List<Metric> Metrics;

        [JsonProperty("commonTags")]
        public Dictionary<string, string> CommonTags;

        [JsonProperty("metricsCommonTags")]
        public Dictionary<string, string> MetricsCommonTags;

        Dictionary<string, string> ITelemetryPayload.CommonTags => CommonTags;

        int ITelemetryPayload.Count => Metrics?.Count ?? 0;

        void ITelemetryPayload.Add(ITelemetryEvent telemetryEvent)
        {
            if (!(telemetryEvent is Metric metric))
                throw new ArgumentException("This payload accepts only Metric events.");

            if (Metrics is null)
            {
                Metrics = new List<Metric>(1);
            }

            Metrics.Add(metric);
        }
    }
}
