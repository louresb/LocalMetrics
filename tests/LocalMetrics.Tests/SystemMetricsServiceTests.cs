using LocalMetrics.Core.Models;
using LocalMetrics.Core.Services;

namespace LocalMetrics.Tests;

[TestClass]
public sealed class SystemMetricsServiceTests
{
    [TestMethod]
    public void GetCurrentMetrics_WithinCacheDuration_ReturnsCachedSample()
    {
        var expected = CreateMetrics();
        var collector = new StubSystemMetricsCollector(() => expected);
        var service = CreateService(collector);

        var first = service.GetCurrentMetrics();
        var second = service.GetCurrentMetrics();

        Assert.AreSame(expected, first);
        Assert.AreSame(first, second);
        Assert.AreEqual(1, collector.CallCount);
    }

    [TestMethod]
    public void GetCurrentMetrics_WithZeroDuration_CollectsEveryTime()
    {
        var collector = new StubSystemMetricsCollector(() => CreateMetrics());
        var service = new SystemMetricsService(collector, TimeSpan.Zero);

        service.GetCurrentMetrics();
        service.GetCurrentMetrics();

        Assert.AreEqual(2, collector.CallCount);
    }

    [TestMethod]
    public void GetCurrentMetrics_AfterCacheExpires_CollectsFreshSample()
    {
        var clock = new ManualTimeProvider(
            new DateTimeOffset(2026, 8, 30, 12, 0, 0, TimeSpan.Zero));
        var sequence = 0;
        var collector = new StubSystemMetricsCollector(
            () => CreateMetrics(cpuUsage: ++sequence));
        var service = new SystemMetricsService(
            collector,
            TimeSpan.FromSeconds(5),
            clock);

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
        var service = CreateService(collector);
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
        Assert.IsTrue(results.All(sample => ReferenceEquals(expected, sample)));
    }

    [TestMethod]
    public void Constructor_WithNegativeCacheDuration_Throws()
    {
        var collector = new StubSystemMetricsCollector(() => CreateMetrics());

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SystemMetricsService(collector, TimeSpan.FromSeconds(-1)));
    }

    internal static SystemMetricsService CreateService(ISystemMetricsCollector collector) =>
        new(collector, TimeSpan.FromSeconds(5));

    internal static SystemMetrics CreateMetrics(float cpuUsage = 12.34f) => new()
    {
        CpuUsage = cpuUsage,
        RamUsage = 56.78f,
        DiskUsage = 90.12f,
        Timestamp = DateTime.UtcNow
    };

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
