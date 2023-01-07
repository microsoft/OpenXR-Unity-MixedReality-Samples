using System.Collections.Generic;

namespace Unity.Services.Core.Telemetry.Internal
{
    interface ITelemetryPayload
    {
        Dictionary<string, string> CommonTags { get; }

        int Count { get; }

        void Add(ITelemetryEvent telemetryEvent);
    }
}
