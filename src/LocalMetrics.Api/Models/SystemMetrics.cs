namespace LocalMetrics.Api.Models;

public class SystemMetrics
{
    public float CpuUsage { get; set; }
    public float RamUsage { get; set; }
    public float DiskUsage { get; set; }
    public DateTime Timestamp { get; set; }
}
