using LocalMetrics.Api.Config;
using LocalMetrics.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (OperatingSystem.IsWindows())
{
    builder.Services.AddSingleton<ISystemMetricsCollector, WindowsSystemMetricsCollector>();
}
else if (OperatingSystem.IsLinux())
{
    builder.Services.AddSingleton<ISystemMetricsCollector, LinuxSystemMetricsCollector>();
}
else if (OperatingSystem.IsMacOS())
{
    builder.Services.AddSingleton<ISystemMetricsCollector, MacSystemMetricsCollector>();
}
else
{
    throw new PlatformNotSupportedException("Unsupported operating system.");
}

builder.Services.Configure<MetricsCacheSettings>(
    builder.Configuration.GetSection("MetricsCache"));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SystemMetricsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
