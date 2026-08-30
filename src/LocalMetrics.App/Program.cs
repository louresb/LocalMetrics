using LocalMetrics.App.Components;
using LocalMetrics.App.Config;
using LocalMetrics.Core.Services;
using Microsoft.Extensions.Options;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MetricsCacheSettings>()
    .Bind(builder.Configuration.GetSection(MetricsCacheSettings.SectionName))
    .Validate(
        settings => double.IsFinite(settings.DurationSeconds) && settings.DurationSeconds >= 0,
        "Cache duration must be a finite, non-negative number.")
    .ValidateOnStart();

builder.Services.AddSingleton<ISystemMetricsCollector>(_ =>
{
    if (OperatingSystem.IsWindows())
    {
        return new WindowsSystemMetricsCollector();
    }

    if (OperatingSystem.IsLinux())
    {
        return new LinuxSystemMetricsCollector();
    }

    if (OperatingSystem.IsMacOS())
    {
        return new MacSystemMetricsCollector();
    }

    throw new PlatformNotSupportedException("LocalMetrics supports Windows, Linux, and macOS.");
});

builder.Services.AddSingleton(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<MetricsCacheSettings>>().Value;
    return new SystemMetricsService(
        serviceProvider.GetRequiredService<ISystemMetricsCollector>(),
        TimeSpan.FromSeconds(settings.DurationSeconds));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;
