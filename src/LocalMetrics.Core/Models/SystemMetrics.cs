namespace LocalMetrics.Core.Models;

public sealed class SystemMetrics
{
    public float CpuUsage { get; init; }
    public float RamUsage { get; init; }
    public float DiskUsage { get; init; }
    public DateTime Timestamp { get; init; }
}
