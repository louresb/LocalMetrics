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

builder.Services.Configure<EncryptionSettings>(options =>
{
    var configKey = builder.Configuration["Encryption:Key"];
    var envKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");

    var key = !string.IsNullOrWhiteSpace(envKey) ? envKey : configKey;

    if (string.IsNullOrWhiteSpace(key))
        throw new InvalidOperationException("Encryption key is not configured.");

    options.Key = key;
});

builder.Services.AddSingleton<SystemMetricsService>();
builder.Services.AddSingleton<EncryptionService>();

builder.WebHost.UseUrls("http://0.0.0.0:5050");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
