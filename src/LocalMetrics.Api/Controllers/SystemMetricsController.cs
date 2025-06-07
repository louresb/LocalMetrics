using LocalMetrics.Api.Models;
using LocalMetrics.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LocalMetrics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemMetricsController : ControllerBase
{
    private readonly SystemMetricsService _metricsService;
    private readonly EncryptionService _encryptionService;

    public SystemMetricsController(SystemMetricsService metricsService, EncryptionService encryptionService)
    {
        _metricsService = metricsService;
        _encryptionService = encryptionService;
    }

    [HttpGet]
    public ActionResult<string> GetEncrypted()
    {
        var metrics = _metricsService.GetCurrentMetrics();
        var json = JsonSerializer.Serialize(metrics);
        var encrypted = _encryptionService.Encrypt(json);
        return Ok(encrypted);
    }

    [HttpPost("decrypt")]
    public ActionResult<SystemMetrics> Decrypt([FromBody] string encryptedBase64)
    {
        try
        {
            var decryptedJson = _encryptionService.Decrypt(encryptedBase64);
            var metrics = JsonSerializer.Deserialize<SystemMetrics>(decryptedJson);
            return Ok(metrics);
        }
        catch
        {
            return BadRequest("Invalid encrypted payload.");
        }
    }
}
