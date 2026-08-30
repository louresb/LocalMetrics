using LocalMetrics.Core.Models;
using LocalMetrics.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalMetrics.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SystemMetricsController(SystemMetricsService metricsService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<SystemMetrics>(StatusCodes.Status200OK)]
    public ActionResult<SystemMetrics> Get()
    {
        return Ok(metricsService.GetCurrentMetrics());
    }
}
