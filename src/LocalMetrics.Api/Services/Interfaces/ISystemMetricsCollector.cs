using LocalMetrics.Api.Models;

public interface ISystemMetricsCollector
{
    SystemMetrics GetMetrics();
}
