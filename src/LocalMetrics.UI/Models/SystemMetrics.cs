using System.Text.Json.Serialization;

namespace LocalMetrics.Web.Models;
public class SystemMetrics
{
    [JsonPropertyName("cpuUsage")]
    public float CpuUsage { get; set; }

    [JsonPropertyName("ramUsage")]
    public float RamUsage { get; set; }

    [JsonPropertyName("diskUsage")]
    public float DiskUsage { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
