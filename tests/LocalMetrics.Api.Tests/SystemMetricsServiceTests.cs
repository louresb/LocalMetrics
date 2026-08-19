using LocalMetrics.Api.Config;
using LocalMetrics.Api.Models;
using LocalMetrics.Api.Services;
using Microsoft.Extensions.Options;

namespace LocalMetrics.Api.Tests;

[TestClass]
public sealed class SystemMetricsServiceTests
{
    [TestMethod]
    public void GetCurrentMetrics_WithinCacheDuration_CollectsOnlyOnce()
    {
        var expected = CreateMetrics(cpuUsage: 12.5f);
        var collector = new StubSystemMetricsCollector(() => expected);
        var service = CreateService(collector, cacheDurationInSeconds: 5);

        var first = service.GetCurrentMetrics();
        var second = service.GetCurrentMetrics();

        Assert.AreSame(expected, first);
        Assert.AreSame(first, second);
        Assert.AreEqual(1, collector.CallCount);
    }

    [TestMethod]
    public void GetCurrentMetrics_WithZeroCacheDuration_CollectsEveryTime()
    {
        var sequence = 0;
        var collector = new StubSystemMetricsCollector(
            () => CreateMetrics(cpuUsage: ++sequence));
        var service = CreateService(collector, cacheDurationInSeconds: 0);

        var first = service.GetCurrentMetrics();
        var second = service.GetCurrentMetrics();

        Assert.AreEqual(1f, first.CpuUsage);
        Assert.AreEqual(2f, second.CpuUsage);
        Assert.AreEqual(2, collector.CallCount);
    }

    [TestMethod]
    public void GetCurrentMetrics_AfterCacheExpires_CollectsFreshMetrics()
    {
        var clock = new ManualTimeProvider(
            new DateTimeOffset(2026, 8, 18, 12, 30, 0, TimeSpan.Zero));
        var sequence = 0;
        var collector = new StubSystemMetricsCollector(
            () => CreateMetrics(cpuUsage: ++sequence));
        var service = CreateService(collector, cacheDurationInSeconds: 5, clock);

        var first = service.GetCurrentMetrics();
        clock.Advance(TimeSpan.FromSeconds(4));
        var cached = service.GetCurrentMetrics();
        clock.Advance(TimeSpan.FromSeconds(1));
        var refreshed = service.GetCurrentMetrics();

        Assert.AreSame(first, cached);
        Assert.AreEqual(1f, cached.CpuUsage);
        Assert.AreEqual(2f, refreshed.CpuUsage);
        Assert.AreEqual(2, collector.CallCount);
    }

    [TestMethod]
    public async Task GetCurrentMetrics_WhenCalledConcurrently_CollectsOnlyOnce()
    {
        var expected = CreateMetrics();
        var collector = new StubSystemMetricsCollector(() =>
        {
            Thread.Sleep(50);
            return expected;
        });
        var service = CreateService(collector, cacheDurationInSeconds: 5);
        using var start = new ManualResetEventSlim(initialState: false);

        var requests = Enumerable.Range(0, 16)
            .Select(_ => Task.Run(() =>
            {
                start.Wait();
                return service.GetCurrentMetrics();
            }))
            .ToArray();

        start.Set();
        var results = await Task.WhenAll(requests);

        Assert.AreEqual(1, collector.CallCount);
        Assert.IsTrue(results.All(metrics => ReferenceEquals(expected, metrics)));
    }

    internal static SystemMetricsService CreateService(
        ISystemMetricsCollector collector,
        int cacheDurationInSeconds = 5,
        TimeProvider? timeProvider = null)
    {
        var settings = Options.Create(new MetricsCacheSettings
        {
            DurationInSeconds = cacheDurationInSeconds
        });

        return new SystemMetricsService(collector, settings, timeProvider);
    }

    internal static SystemMetrics CreateMetrics(float cpuUsage = 12.34f)
    {
        return new SystemMetrics
        {
            CpuUsage = cpuUsage,
            RamUsage = 56.78f,
            DiskUsage = 90.12f,
            Timestamp = new DateTime(2026, 8, 18, 12, 30, 0, DateTimeKind.Utc)
        };
    }

    internal sealed class StubSystemMetricsCollector(Func<SystemMetrics> collect)
        : ISystemMetricsCollector
    {
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public SystemMetrics GetMetrics()
        {
            Interlocked.Increment(ref _callCount);
            return collect();
        }
    }

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan elapsed)
        {
            _utcNow += elapsed;
        }
    }
}
