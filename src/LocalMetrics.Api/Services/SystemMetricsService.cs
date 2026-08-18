using LocalMetrics.Api.Config;
using LocalMetrics.Api.Models;
using Microsoft.Extensions.Options;

namespace LocalMetrics.Api.Services;

public class SystemMetricsService
{
    private readonly ISystemMetricsCollector _collector;
    private readonly TimeSpan _cacheDuration;
    private readonly TimeProvider _timeProvider;
    private readonly Lock _cacheLock = new();
    private SystemMetrics? _cachedMetrics;
    private DateTimeOffset _lastUpdated;

    public SystemMetricsService(
        ISystemMetricsCollector collector,
        IOptions<MetricsCacheSettings> settings,
        TimeProvider? timeProvider = null)
    {
        _collector = collector;
        _cacheDuration = TimeSpan.FromSeconds(settings.Value.DurationInSeconds);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SystemMetrics GetCurrentMetrics()
    {
        lock (_cacheLock)
        {
            var now = _timeProvider.GetUtcNow();
            if (_cachedMetrics != null && now - _lastUpdated < _cacheDuration)
            {
                return _cachedMetrics;
            }

            _cachedMetrics = _collector.GetMetrics();
            _lastUpdated = _timeProvider.GetUtcNow();
            return _cachedMetrics;
        }
    }
}
