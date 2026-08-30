using LocalMetrics.Core.Models;

namespace LocalMetrics.Core.Services;

public sealed class SystemMetricsService
{
    private readonly ISystemMetricsCollector _collector;
    private readonly TimeSpan _cacheDuration;
    private readonly TimeProvider _timeProvider;
    private readonly object _syncRoot = new();
    private SystemMetrics? _cachedMetrics;
    private DateTimeOffset _cachedAt;

    public SystemMetricsService(
        ISystemMetricsCollector collector,
        TimeSpan cacheDuration,
        TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(cacheDuration, TimeSpan.Zero);

        _collector = collector;
        _cacheDuration = cacheDuration;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SystemMetrics GetCurrentMetrics()
    {
        lock (_syncRoot)
        {
            var now = _timeProvider.GetUtcNow();

            if (_cachedMetrics is null || now - _cachedAt >= _cacheDuration)
            {
                _cachedMetrics = _collector.GetMetrics();
                _cachedAt = _timeProvider.GetUtcNow();
            }

            return _cachedMetrics;
        }
    }
}
