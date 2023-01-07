using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unity.Services.Core.Telemetry.Internal
{
    [Serializable]
    struct Diagnostic : ITelemetryEvent
    {
        [JsonProperty("content")]
        public IDictionary<string, string> Content;
    }
}
