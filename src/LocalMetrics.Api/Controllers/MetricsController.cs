using LocalMetrics.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LocalMetrics.Api.Controllers;

[ApiController]
[Route("metrics")]
public class MetricsController : ControllerBase
{
    private readonly SystemMetricsService _service;

    public MetricsController(SystemMetricsService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var metrics = _service.GetCurrentMetrics();

        var sb = new StringBuilder();
        sb.AppendLine("# HELP cpu_usage CPU usage percentage");
        sb.AppendLine("# TYPE cpu_usage gauge");
        sb.AppendLine($"cpu_usage {metrics.CpuUsage.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");

        sb.AppendLine("# HELP ram_usage RAM usage percentage");
        sb.AppendLine("# TYPE ram_usage gauge");
        sb.AppendLine($"ram_usage {metrics.RamUsage.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");

        sb.AppendLine("# HELP disk_usage Disk usage percentage");
        sb.AppendLine("# TYPE disk_usage gauge");
        sb.AppendLine($"disk_usage {metrics.DiskUsage.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");

        return Content(sb.ToString(), "text/plain");
    }
}
