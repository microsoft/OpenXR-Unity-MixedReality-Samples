using System.Collections.Generic;

namespace Unity.Services.Core.Telemetry.Internal
{
    class DisabledMetricsFactory : IMetricsFactory
    {
        IReadOnlyDictionary<string, string> IMetricsFactory.CommonTags { get; }
            = new Dictionary<string, string>();

        IMetrics IMetricsFactory.Create(string packageName) => new DisabledMetrics();
    }
}
