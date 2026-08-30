using LocalMetrics.Core.Models;

namespace LocalMetrics.Core.Services;

public interface ISystemMetricsCollector
{
    SystemMetrics GetMetrics();
}
