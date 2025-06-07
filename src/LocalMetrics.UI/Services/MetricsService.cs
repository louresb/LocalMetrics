using LocalMetrics.Web.Models;

namespace LocalMetrics.Web.Services;

public class MetricsService
{
    private readonly HttpClient _http;

    public MetricsService(HttpClient http)
    {
        _http = http;
    }

    public async Task<SystemMetrics?> GetSystemMetricsAsync()
    {
        try
        {
            var encrypted = await _http.GetStringAsync("api/systemmetrics");
            var response = await _http.PostAsJsonAsync("api/systemmetrics/decrypt", encrypted);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SystemMetrics>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MetricsService] Erro: {ex.Message}");
            return null;
        }
    }
}
