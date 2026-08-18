using LocalMetrics.Api.Models;

namespace LocalMetrics.Api.Services;

public interface ISystemMetricsCollector
{
    SystemMetrics GetMetrics();
}
