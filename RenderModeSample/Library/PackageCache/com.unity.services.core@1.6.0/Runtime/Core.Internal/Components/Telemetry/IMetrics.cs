using System.Collections.Generic;

namespace Unity.Services.Core.Telemetry.Internal
{
    /// <summary>
    /// Object used to send metrics events to the backend.
    /// </summary>
    public interface IMetrics
    {
        /// <summary>
        /// Send a metric that can arbitrarily go up or down to the telemetry service.
        /// </summary>
        /// <param name="name">
        /// Name of the event.
        /// </param>
        /// <param name="value">
        /// Value of the metric.
        /// </param>
        /// <param name="tags">
        /// Event tags.
        /// </param>
        void SendGaugeMetric(string name, double value = 0, IDictionary<string, string> tags = null);

        /// <summary>
        /// Send a metric that lasts over time to the telemetry service.
        /// </summary>
        /// <param name="name">
        /// Name of the event.
        /// </param>
        /// <param name="time">
        /// Duration of the operation the event is tracking.
        /// </param>
        /// <param name="tags">
        /// Event tags.
        /// </param>
        void SendHistogramMetric(string name, double time, IDictionary<string, string> tags = null);

        /// <summary>
        /// Send a metric that can only be incremented to the telemetry service.
        /// </summary>
        /// <param name="name">
        /// Name of the event.
        /// </param>
        /// <param name="value">
        /// Value of the metric.
        /// </param>
        /// <param name="tags">
        /// Event tags.
        /// </param>
        void SendSumMetric(string name, double value = 1, IDictionary<string, string> tags = null);
    }
}
