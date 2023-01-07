using System;

namespace Unity.Services.Core.Telemetry.Internal
{
    [Serializable]
    class TelemetryConfig
    {
        public const int MaxMetricCountPerPayloadLimit = 295;

        public string TargetUrl;

        public string ServicePath;

        public double PayloadExpirationSeconds;

        public double PayloadSendingMaxIntervalSeconds;

        public double SafetyPersistenceIntervalSeconds;

        public int MaxMetricCountPerPayload;
    }
}
