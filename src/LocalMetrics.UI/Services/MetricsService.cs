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

            var metrics = await response.Content.ReadFromJsonAsync<SystemMetrics>();
            return metrics;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MetricsService] Erro ao obter métricas: {ex.Message}");
            return null;
        }
    }

    public async Task<List<SystemMetrics>> GetSystemMetricsHistoryAsync(int count = 20)
    {
        var history = new List<SystemMetrics>();

        for (int i = 0; i < count; i++)
        {
            var metric = await GetSystemMetricsAsync();
            if (metric != null)
            {
                history.Add(metric);
                await Task.Delay(500); 
            }
        }

        return history;
    }
}
