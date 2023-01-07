using System.Collections.Generic;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Telemetry.Internal
{
    class Metrics : IMetrics
    {
        internal MetricsHandler Handler { get; }

        internal IDictionary<string, string> PackageTags { get; }

        public Metrics(MetricsHandler handler, IDictionary<string, string> packageTags)
        {
            Handler = handler;
            PackageTags = packageTags;
        }

        internal Metric CreateMetric(string name, double value, MetricType type, IDictionary<string, string> tags)
        {
            var metric = new Metric
            {
                Name = name,
                Value = value,
                Type = type,
                Tags = tags is null ? PackageTags : tags.MergeAllowOverride(PackageTags),
            };

            return metric;
        }

        void IMetrics.SendGaugeMetric(string name, double value, IDictionary<string, string> tags)
        {
            var metric = CreateMetric(name, value, MetricType.Gauge, tags);
            Handler.Register(metric);
        }

        void IMetrics.SendHistogramMetric(string name, double time, IDictionary<string, string> tags)
        {
            var metric = CreateMetric(name, time, MetricType.Histogram, tags);
            Handler.Register(metric);
        }

        void IMetrics.SendSumMetric(string name, double value, IDictionary<string, string> tags)
        {
            var metric = CreateMetric(name, value, MetricType.Sum, tags);
            Handler.Register(metric);
        }
    }
}
