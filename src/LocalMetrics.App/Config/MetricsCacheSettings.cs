namespace LocalMetrics.App.Config;

public sealed class MetricsCacheSettings
{
    public const string SectionName = "MetricsCache";

    public double DurationSeconds { get; set; } = 5;
}
