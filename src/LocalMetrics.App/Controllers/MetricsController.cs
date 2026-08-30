using System.Globalization;
using System.Text;
using LocalMetrics.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalMetrics.App.Controllers;

[ApiController]
[Route("metrics")]
public sealed class MetricsController(SystemMetricsService metricsService) : ControllerBase
{
    [HttpGet]
    [Produces("text/plain")]
    public ContentResult Get()
    {
        var metrics = metricsService.GetCurrentMetrics();
        var output = new StringBuilder()
            .AppendLine("# HELP cpu_usage Current CPU usage percentage.")
            .AppendLine("# TYPE cpu_usage gauge")
            .AppendLine("cpu_usage " + Format(metrics.CpuUsage))
            .AppendLine("# HELP ram_usage Current memory usage percentage.")
            .AppendLine("# TYPE ram_usage gauge")
            .AppendLine("ram_usage " + Format(metrics.RamUsage))
            .AppendLine("# HELP disk_usage Current system disk usage percentage.")
            .AppendLine("# TYPE disk_usage gauge")
            .AppendLine("disk_usage " + Format(metrics.DiskUsage))
            .ToString();

        return Content(output, "text/plain; version=0.0.4; charset=utf-8");
    }

    private static string Format(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);
}
