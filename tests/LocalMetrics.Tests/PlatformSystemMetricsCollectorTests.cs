using LocalMetrics.Core.Services;

namespace LocalMetrics.Tests;

[TestClass]
public sealed class PlatformSystemMetricsCollectorTests
{
    [TestMethod]
    public void GetMetrics_OnCurrentPlatform_ReturnsBoundedSample()
    {
        ISystemMetricsCollector collector = CreateCollectorForCurrentPlatform();

        var metrics = collector.GetMetrics();

        AssertPercentage(metrics.CpuUsage, nameof(metrics.CpuUsage));
        AssertPercentage(metrics.RamUsage, nameof(metrics.RamUsage));
        AssertPercentage(metrics.DiskUsage, nameof(metrics.DiskUsage));
        Assert.IsTrue(metrics.Timestamp <= DateTime.UtcNow);
        Assert.IsTrue(metrics.Timestamp > DateTime.UtcNow.AddMinutes(-1));
    }

    private static ISystemMetricsCollector CreateCollectorForCurrentPlatform()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsSystemMetricsCollector();
        }

        if (OperatingSystem.IsLinux())
        {
            return new LinuxSystemMetricsCollector();
        }

        if (OperatingSystem.IsMacOS())
        {
            return new MacSystemMetricsCollector();
        }

        throw new PlatformNotSupportedException("Unsupported test runner operating system.");
    }

    private static void AssertPercentage(float value, string metricName)
    {
        Assert.IsTrue(float.IsFinite(value), $"{metricName} must be finite.");
        Assert.IsTrue(value is >= 0 and <= 100, $"{metricName} must be between 0 and 100.");
    }
}
