using System.Diagnostics;
using System.Runtime.InteropServices;
using LocalMetrics.Api.Models;

namespace LocalMetrics.Api.Services;

public class SystemMetricsService
{
    public SystemMetrics GetCurrentMetrics()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //return GetMetricsWindows();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            //return GetMetricsLinux();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            //return GetMetricsMac();
        }

        throw new PlatformNotSupportedException("Operating system not supported.");
    }
}
