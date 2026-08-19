using LocalMetrics.Api.Models;
using LocalMetrics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalMetrics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemMetricsController : ControllerBase
{
    private readonly SystemMetricsService _metricsService;
    public SystemMetricsController(SystemMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet]
    public ActionResult<SystemMetrics> Get()
    {
        return Ok(_metricsService.GetCurrentMetrics());
    }
}
