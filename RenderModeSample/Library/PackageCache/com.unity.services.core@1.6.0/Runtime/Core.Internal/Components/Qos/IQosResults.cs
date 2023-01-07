using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core.Internal;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Services.Qos.Internal
{
    /// <summary>
    /// An interface that allows access to QoS measurements. For use by other Operate packages through the Core
    /// Services SDK.
    /// </summary>
#if UNITY_2020_2_OR_NEWER
    [RequireImplementors]
#endif
    public interface IQosResults : IServiceComponent
    {
        /// <summary>
        /// Gets sorted QoS measurements the specified service and regions.
        /// </summary>
        /// <remarks>
        /// `GetSortedQosResultsAsync` doesn't consider the returned regions until applying the services and regions filters.
        ///
        /// If you specify regions, it only includes those regions.
        /// </remarks>
        /// <param name="service">The service to query regions for QoS. `GetSortedQosResultsAsync` only uses measures
        /// regions configured for the specified service.</param>
        /// <param name="regions">The regions to query for QoS. If not null or empty, `GetSortedQosResultsAsync` only uses
        /// regions in the intersection of the specified service and the specified regions for measurements.</param>
        /// <returns>Returns the sorted list of QoS results, ordered from best to worst.</returns>
        Task<IList<QosResult>> GetSortedQosResultsAsync(string service, IList<string> regions);
    }

    /// <summary>
    /// Represents the results of QoS measurements for a given region.
    /// </summary>
    public struct QosResult
    {
        /// <summary>
        /// The identifier for the service's region used in this set of QoS measurements.
        /// </summary>
        /// <value>A string containing the region name.
        /// </value>
        public string Region;
        /// <summary>
        /// Average latency of QoS measurements to the region.
        /// </summary>
        /// <remarks>
        /// The latency is determined by measuring the time between sending a packet and receiving the response for that packet,
        /// then taking the average for all responses received. Only packets for which a response was received are
        /// considered in the calculation.
        /// </remarks>
        /// <value>A positive integer, in milliseconds.</value>
        public int AverageLatencyMs;
        /// <summary>
        /// Percentage of packet loss observed in QoS measurements to the region.
        /// </summary>
        /// <remarks>
        /// Packet loss is determined by counting the number of packets for which a response was received from the QoS server,
        /// then taking the percentage based on the total number of packets sent.
        /// </remarks>
        /// <value>A positive flow value. The range is 0.0f - 1.0f (0 - 100%).</value>
        public float PacketLossPercent;
    }
}
