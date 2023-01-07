using System.Runtime.Serialization;

namespace Unity.Services.Core.Telemetry.Internal
{
    enum MetricType
    {
        [EnumMember(Value = "GAUGE")]
        Gauge = 0,
        [EnumMember(Value = "SUM")]
        Sum = 1,
        [EnumMember(Value = "HISTOGRAM")]
        Histogram = 2,
    }
}
