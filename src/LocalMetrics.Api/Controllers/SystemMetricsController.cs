using LocalMetrics.Api.Models;
using LocalMetrics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocalMetrics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemMetricsController : ControllerBase
{
    private readonly SystemMetricsService _service;

    public SystemMetricsController(SystemMetricsService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<SystemMetrics> Get()
    {
        return Ok(_service.GetCurrentMetrics());
    }
}
