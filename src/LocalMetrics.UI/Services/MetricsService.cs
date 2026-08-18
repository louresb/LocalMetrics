using LocalMetrics.UI.Models;

namespace LocalMetrics.UI.Services;

public class MetricsService
{
    private readonly HttpClient _http;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(HttpClient http, ILogger<MetricsService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<SystemMetrics?> GetSystemMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<SystemMetrics>("api/systemmetrics", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to retrieve system metrics from the local API.");
            return null;
        }
    }
}
