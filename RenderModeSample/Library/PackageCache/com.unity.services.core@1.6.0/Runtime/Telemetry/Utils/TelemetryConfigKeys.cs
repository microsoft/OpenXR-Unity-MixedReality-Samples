namespace Unity.Services.Core.Telemetry.Internal
{
    static class TelemetryConfigKeys
    {
        const string k_BaseKey = "com.unity.services.core.telemetry-";

        public const string TargetUrl = k_BaseKey + "target-url";

        public const string ServicePath = k_BaseKey + "service-path";

        public const string PayloadExpirationSeconds = k_BaseKey + "payload-expiration-seconds";

        public const string PayloadSendingMaxIntervalSeconds = k_BaseKey + "payload-sending-max-interval-seconds";

        public const string SafetyPersistenceIntervalSeconds = k_BaseKey + "safety-persistence-interval-seconds";

        public const string MaxMetricCountPerPayload = k_BaseKey + "max-metric-count-per-payload";
    }
}
