using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unity.Services.Core.Telemetry.Internal
{
    [Serializable]
    struct DiagnosticsPayload : ITelemetryPayload
    {
        [JsonProperty("diagnostics")]
        public List<Diagnostic> Diagnostics;

        [JsonProperty("commonTags")]
        public Dictionary<string, string> CommonTags;

        [JsonProperty("diagnosticsCommonTags")]
        public Dictionary<string, string> DiagnosticsCommonTags;

        Dictionary<string, string> ITelemetryPayload.CommonTags => CommonTags;

        int ITelemetryPayload.Count => Diagnostics?.Count ?? 0;

        void ITelemetryPayload.Add(ITelemetryEvent telemetryEvent)
        {
            if (!(telemetryEvent is Diagnostic diagnostic))
                throw new ArgumentException("This payload accepts only Diagnostic events.");

            if (Diagnostics is null)
            {
                Diagnostics = new List<Diagnostic>(1);
            }

            Diagnostics.Add(diagnostic);
        }
    }
}
