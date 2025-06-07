using LocalMetrics.Api.Config;
using LocalMetrics.Api.Models;
using Microsoft.Extensions.Options;

namespace LocalMetrics.Api.Services
{
    public class SystemMetricsService
    {
        private readonly ISystemMetricsCollector _collector;
        private readonly TimeSpan _cacheDuration;
        private SystemMetrics? _cachedMetrics;
        private DateTime _lastUpdated;

        public SystemMetricsService(
            ISystemMetricsCollector collector,
            IOptions<MetricsCacheSettings> settings)
        {
            _collector = collector;
            _cacheDuration = TimeSpan.FromSeconds(settings.Value.DurationInSeconds);
        }

        public SystemMetrics GetCurrentMetrics()
        {
            if (_cachedMetrics != null && DateTime.UtcNow - _lastUpdated < _cacheDuration)
            {
                return _cachedMetrics;
            }

            _cachedMetrics = _collector.GetMetrics();
            _lastUpdated = DateTime.UtcNow;
            return _cachedMetrics;
        }
    }
}
